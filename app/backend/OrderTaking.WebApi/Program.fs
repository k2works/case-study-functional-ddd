namespace OrderTaking.WebApi

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open OrderTaking.Domain.Entities
open OrderTaking.Domain.DomainServices
open OrderTaking.Infrastructure
open OrderTaking.Infrastructure.DependencyContainer
open OrderTaking.Infrastructure.JsonSerialization
open FluentMigrator.Runner

// WebApplicationFactory から参照可能にするためのダミークラス
type Program() = class end

module Main =
    [<EntryPoint>]
    let main args =
        let builder =
            WebApplication.CreateBuilder(args)

        // Swagger/OpenAPI の設定
        builder.Services.AddEndpointsApiExplorer()
        |> ignore

        builder.Services.AddSwaggerGen() |> ignore

        // JSON シリアライゼーションの設定
        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>
            (fun (options: Microsoft.AspNetCore.Http.Json.JsonOptions) ->
                options.SerializerOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
                options.SerializerOptions.WriteIndented <- true
                options.SerializerOptions.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull
                options.SerializerOptions.Converters.Add(JsonFSharpConverter()))
        |> ignore

        // 接続文字列を環境変数または設定から取得
        let connectionString =
            let envConnectionString =
                Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")

            if String.IsNullOrWhiteSpace(envConnectionString) then
                // デフォルトの接続文字列 (SQLite)
                let defaultConnection =
                    builder.Configuration.GetConnectionString("DefaultConnection")

                if String.IsNullOrWhiteSpace(defaultConnection) then
                    // テスト環境では一時ファイルデータベースを使用
                    let tempDbPath =
                        System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"test_orders_{System.Guid.NewGuid()}.db")

                    $"Data Source={tempDbPath}"
                else
                    defaultConnection
            else
                envConnectionString

        // DI: OrderRepository の登録とマイグレーション実行
        let environment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")

        let repository, runMigrations =
            match environment with
            | "Production" ->
                // 本番環境では手動でマイグレーション実行を想定
                eprintfn "Production environment detected - migrations must be run manually"

                let repo =
                    OrderRepository(connectionString) :> IOrderRepository

                (repo, false)
            | "Testing" ->
                // テスト環境では TestWebApplicationFactory がマイグレーションを管理
                eprintfn "Testing environment detected - migrations managed by test factory"

                let repo =
                    OrderRepository(connectionString) :> IOrderRepository

                (repo, false)
            | _ ->
                // それ以外（Development など）では自動でマイグレーションを実行
                eprintfn "Non-production environment detected - will run migrations"

                let repo =
                    OrderRepository(connectionString) :> IOrderRepository

                (repo, true)

        builder.Services.AddSingleton<IOrderRepository>(repository)
        |> ignore

        // マイグレーション設定を DI に登録
        if runMigrations then
            builder.Services
                .AddFluentMigratorCore()
                .ConfigureRunner(fun rb ->
                    rb
                        .AddSQLite()
                        .WithGlobalConnectionString(connectionString)
                        .ScanIn(typeof<OrderTaking.Infrastructure.Migrations.Migration001_InitialCreate>.Assembly)
                        .For.Migrations()
                    |> ignore)
            |> ignore

        // DI: PlaceOrderDependencies の登録
        let dependencies =
            createDependenciesWithRepository repository

        builder.Services.AddSingleton<PlaceOrderDependencies>(dependencies)
        |> ignore

        let app = builder.Build()

        // インメモリデータベースの場合はマイグレーションを実行
        if runMigrations then
            try
                use scope = app.Services.CreateScope()

                let runner =
                    scope.ServiceProvider.GetRequiredService<FluentMigrator.Runner.IMigrationRunner>()

                eprintfn "Running migrations on database: %s" connectionString
                runner.MigrateUp()
                eprintfn "Migrations completed successfully"
            with ex ->
                eprintfn "Migration failed: %s" ex.Message
                raise ex

        // Swagger UI の設定
        app.UseSwagger() |> ignore
        app.UseSwaggerUI() |> ignore

        // ルートから Swagger UI へリダイレクト
        app
            .MapGet("/", Func<IResult>(fun () -> Results.Redirect("/swagger")))
            .ExcludeFromDescription()
        |> ignore

        // POST /api/orders エンドポイント
        app
            .MapPost(
                "/api/orders",
                Func<HttpRequest, PlaceOrderDependencies, Task<IResult>>
                    (fun (request: HttpRequest) (deps: PlaceOrderDependencies) ->
                        task {
                            use reader = new StreamReader(request.Body)
                            let! json = reader.ReadToEndAsync()

                            // 空のボディをチェック
                            if System.String.IsNullOrWhiteSpace(json) then
                                return Results.BadRequest({| error = "Request body is empty" |})
                            else
                                match deserializeUnvalidatedOrder json with
                                | Error deserializeError -> return Results.BadRequest({| error = deserializeError |})
                                | Ok unvalidatedOrder ->
                                    let! result =
                                        PlaceOrderWorkflow.placeOrder
                                            deps.CheckProductCodeExists
                                            deps.CheckAddressExists
                                            deps.GetProductPrice
                                            deps.SaveOrder
                                            deps.SendOrderAcknowledgment
                                            unvalidatedOrder
                                        |> Async.StartAsTask

                                    match result with
                                    | Error error ->
                                        let errorMessage =
                                            PlaceOrderError.toString error

                                        return Results.BadRequest({| error = errorMessage |})
                                    | Ok events ->
                                        let eventJsons =
                                            events |> List.map serializePlaceOrderEvent

                                        return Results.Ok({| events = eventJsons |})
                        })
            )
            .Accepts<UnvalidatedOrder>("application/json")
            .WithName("PlaceOrder")
            .WithTags("Orders")
            .WithSummary("Place a new order")
            .WithDescription(
                """Submit a new order with customer information, shipping address, and order lines.

**Example Request:**
```json
{
  "orderId": "12345678-1234-1234-1234-123456789012",
  "customerInfo": {
    "firstName": "John",
    "lastName": "Doe",
    "emailAddress": "john@example.com"
  },
  "shippingAddress": {
    "addressLine1": "123 Main St",
    "addressLine2": "Apt 1",
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
    },
    {
      "orderLineId": "11111111-1111-1111-1111-111111111111",
      "productCode": "G123",
      "quantity": 0.5
    }
  ]
}
```

**Valid Product Codes:**
- Widget codes: W1234, W5678, W9012
- Gizmo codes: G123, G1234, G5678, G9012"""
            )
        |> ignore

        app.Run()

        0 // Exit code
