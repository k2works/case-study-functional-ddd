module OrderTaking.Tests.Api

open System.Net
open System.Net.Http
open System.Text
open Xunit
open FsUnit.Xunit
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open FluentMigrator.Runner

// ========================================
// Test Web Application Factory
// ========================================

type TestWebApplicationFactory() =
    inherit WebApplicationFactory<OrderTaking.WebApi.Program>()

    // 各テストインスタンスごとに一意の一時データベースパスを生成
    let uniqueDbPath =
        System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"test_api_{System.Guid.NewGuid()}.db")

    override this.ConfigureWebHost(builder: IWebHostBuilder) =
        // テスト環境として設定し、Program.fsでマイグレーション登録をスキップさせる
        builder.UseEnvironment("Testing") |> ignore

        builder.ConfigureServices(fun services ->
            // 既存のIOrderRepositoryを削除して、テスト用のものを登録
            let descriptor =
                services
                |> Seq.tryFind (fun d -> d.ServiceType = typeof<OrderTaking.Infrastructure.IOrderRepository>)

            match descriptor with
            | Some d -> services.Remove(d) |> ignore
            | None -> ()

            // テスト用の接続文字列で新しいRepositoryを登録
            let testConnectionString =
                $"Data Source={uniqueDbPath}"

            let testRepository =
                OrderTaking.Infrastructure.OrderRepository(testConnectionString)
                :> OrderTaking.Infrastructure.IOrderRepository

            services.AddSingleton<OrderTaking.Infrastructure.IOrderRepository>(testRepository)
            |> ignore

            // 依存性も更新
            let depsDescriptor =
                services
                |> Seq.tryFind (fun d ->
                    d.ServiceType = typeof<OrderTaking.Infrastructure.DependencyContainer.PlaceOrderDependencies>)

            match depsDescriptor with
            | Some d -> services.Remove(d) |> ignore
            | None -> ()

            let testDependencies =
                OrderTaking.Infrastructure.DependencyContainer.createDependenciesWithRepository testRepository

            services.AddSingleton<OrderTaking.Infrastructure.DependencyContainer.PlaceOrderDependencies>(
                testDependencies
            )
            |> ignore

            // テスト用データベースのマイグレーション設定
            // 一時的なサービスプロバイダーを作成してマイグレーションを実行
            let tempServices = ServiceCollection()

            tempServices
                .AddFluentMigratorCore()
                .ConfigureRunner(fun rb ->
                    rb
                        .AddSQLite()
                        .WithGlobalConnectionString(testConnectionString)
                        .ScanIn(typeof<OrderTaking.Infrastructure.Migrations.Migration001_InitialCreate>.Assembly)
                        .For.Migrations()
                    |> ignore)
            |> ignore

            use tempProvider =
                tempServices.BuildServiceProvider()

            let runner =
                tempProvider.GetRequiredService<IMigrationRunner>()

            runner.MigrateUp())
        |> ignore

    interface System.IDisposable with
        member this.Dispose() =
            // テスト終了後に一時データベースファイルを削除
            try
                if System.IO.File.Exists(uniqueDbPath) then
                    System.IO.File.Delete(uniqueDbPath)
            with _ ->
                ()

            (this :> WebApplicationFactory<OrderTaking.WebApi.Program>).Dispose()

// ========================================
// API Integration Tests
// ========================================

[<Fact>]
let ``POST /api/orders with valid order returns 200 OK`` () =
    task {
        use factory =
            new TestWebApplicationFactory()

        use client = factory.CreateClient()

        let json =
            """
            {
                "orderId": "12345678-1234-1234-1234-123456789012",
                "customerInfo": {
                    "firstName": "John",
                    "lastName": "Doe",
                    "emailAddress": "john@example.com"
                },
                "shippingAddress": {
                    "addressLine1": "123 Main St",
                    "addressLine2": null,
                    "city": "Springfield",
                    "zipCode": "12345"
                },
                "billingAddress": {
                    "addressLine1": "123 Main St",
                    "addressLine2": null,
                    "city": "Springfield",
                    "zipCode": "12345"
                },
                "lines": [
                    {
                        "orderLineId": "87654321-4321-4321-4321-210987654321",
                        "productCode": "W1234",
                        "quantity": 2.0
                    }
                ]
            }
            """

        use content =
            new StringContent(json, Encoding.UTF8, "application/json")

        let! response = client.PostAsync("/api/orders", content)

        response.StatusCode
        |> should equal HttpStatusCode.OK

        let! responseBody = response.Content.ReadAsStringAsync()
        Assert.Contains("events", responseBody)
        Assert.Contains("OrderPlaced", responseBody)
    }

[<Fact>]
let ``POST /api/orders with invalid order returns 400 BadRequest`` () =
    task {
        use factory =
            new TestWebApplicationFactory()

        use client = factory.CreateClient()

        let json =
            """
            {
                "orderId": "invalid-guid",
                "customerInfo": {
                    "firstName": "",
                    "lastName": "",
                    "emailAddress": "invalid-email"
                },
                "shippingAddress": {
                    "addressLine1": "",
                    "addressLine2": null,
                    "city": "",
                    "zipCode": ""
                },
                "billingAddress": {
                    "addressLine1": "",
                    "addressLine2": null,
                    "city": "",
                    "zipCode": ""
                },
                "lines": []
            }
            """

        use content =
            new StringContent(json, Encoding.UTF8, "application/json")

        let! response = client.PostAsync("/api/orders", content)

        response.StatusCode
        |> should equal HttpStatusCode.BadRequest

        let! responseBody = response.Content.ReadAsStringAsync()
        Assert.Contains("error", responseBody)
    }

[<Fact>]
let ``POST /api/orders with empty body returns 400 BadRequest`` () =
    task {
        use factory =
            new TestWebApplicationFactory()

        use client = factory.CreateClient()

        use content =
            new StringContent("", Encoding.UTF8, "application/json")

        let! response = client.PostAsync("/api/orders", content)

        response.StatusCode
        |> should equal HttpStatusCode.BadRequest

        let! responseBody = response.Content.ReadAsStringAsync()
        Assert.Contains("error", responseBody)
    }

[<Fact>]
let ``POST /api/orders with invalid JSON returns 400 BadRequest`` () =
    task {
        use factory =
            new TestWebApplicationFactory()

        use client = factory.CreateClient()

        let invalidJson = "{invalid json"

        use content =
            new StringContent(invalidJson, Encoding.UTF8, "application/json")

        let! response = client.PostAsync("/api/orders", content)

        response.StatusCode
        |> should equal HttpStatusCode.BadRequest

        let! responseBody = response.Content.ReadAsStringAsync()
        Assert.Contains("error", responseBody)
    }

[<Fact>]
let ``POST /api/orders with product code not found returns 400 BadRequest`` () =
    task {
        use factory =
            new TestWebApplicationFactory()

        use client = factory.CreateClient()

        let json =
            """
            {
                "orderId": "12345678-1234-1234-1234-123456789012",
                "customerInfo": {
                    "firstName": "John",
                    "lastName": "Doe",
                    "emailAddress": "john@example.com"
                },
                "shippingAddress": {
                    "addressLine1": "123 Main St",
                    "addressLine2": null,
                    "city": "Springfield",
                    "zipCode": "12345"
                },
                "billingAddress": {
                    "addressLine1": "123 Main St",
                    "addressLine2": null,
                    "city": "Springfield",
                    "zipCode": "12345"
                },
                "lines": [
                    {
                        "orderLineId": "87654321-4321-4321-4321-210987654321",
                        "productCode": "INVALID",
                        "quantity": 2.0
                    }
                ]
            }
            """

        use content =
            new StringContent(json, Encoding.UTF8, "application/json")

        let! response = client.PostAsync("/api/orders", content)

        response.StatusCode
        |> should equal HttpStatusCode.BadRequest

        let! responseBody = response.Content.ReadAsStringAsync()
        Assert.Contains("error", responseBody)
        Assert.Contains("Product code not found", responseBody)
    }
