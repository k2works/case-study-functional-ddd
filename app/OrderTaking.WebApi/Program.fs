open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore
open OrderTaking.Domain
open OrderTaking.Infrastructure
open OrderTaking.Api
open OrderTaking.Common
open OrderTaking.Domain.OrderWorkflows

// 外部依存関数の実装
let checkProductCodeExists: CheckProductCodeExists =
    fun productCode ->
        // 簡易的な実装 - 実際のアプリケーションではデータベースを使用
        match productCode with
        | Widget code ->
            let codeStr = WidgetCode.value code
            codeStr.StartsWith("W")
        | Gizmo code ->
            let codeStr = GizmoCode.value code
            codeStr.StartsWith("G")

let getProductPrice: GetProductPrice =
    fun productCode ->
        // 簡易的な価格テーブル
        match productCode with
        | Widget code ->
            let codeStr = WidgetCode.value code
            match codeStr with
            | "W1234" -> Some 100.00m
            | "W5678" -> Some 150.00m
            | _ -> Some 120.00m // デフォルト価格
        | Gizmo code ->
            let codeStr = GizmoCode.value code
            match codeStr with
            | "G123" -> Some 50.00m
            | "G456" -> Some 75.00m
            | _ -> Some 60.00m // デフォルト価格

let checkAddressExists: CheckAddressExists =
    fun unvalidatedAddress ->
        AsyncResultBuilder.asyncResult {
            // 簡易的な住所検証 - 実際のアプリケーションでは外部APIを使用
            try
                let! line1 = String50.create unvalidatedAddress.AddressLine1 |> Result.mapError (fun _ -> FieldIsMissing "AddressLine1") |> AsyncResult.ofResult
                let! city = String50.create unvalidatedAddress.City |> Result.mapError (fun _ -> FieldIsMissing "City") |> AsyncResult.ofResult
                let! zip = String50.create unvalidatedAddress.ZipCode |> Result.mapError (fun _ -> FieldIsMissing "ZipCode") |> AsyncResult.ofResult

                let line2 =
                    if String.IsNullOrWhiteSpace(unvalidatedAddress.AddressLine2)
                    then None
                    else String50.create unvalidatedAddress.AddressLine2 |> Result.toOption

                return {
                    AddressLine1 = line1
                    AddressLine2 = line2
                    City = city
                    ZipCode = zip
                }
            with
            | ex -> return! AsyncResult.ofError (FieldInvalidFormat ex.Message)
        }

let sendOrderAcknowledgment: SendOrderAcknowledgment =
    fun pricedOrder ->
        async {
            // 簡易的なメール送信の模擬
            try
                // 実際のアプリケーションではメールサービスを使用
                return Ok {
                    OrderId = pricedOrder.OrderId
                    EmailAddress = pricedOrder.CustomerInfo.Email
                }
            with
            | ex -> return Error ex.Message
        }

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    // サービス登録
    builder.Services.AddDbContext<OrderContext>(fun options ->
        options.UseInMemoryDatabase("OrderTakingDb") |> ignore
    ) |> ignore

    builder.Services.AddScoped<IOrderRepository, OrderRepository>() |> ignore

    // Swagger設定
    builder.Services.AddEndpointsApiExplorer() |> ignore
    builder.Services.AddSwaggerDocument() |> ignore

    let app = builder.Build()

    // ミドルウェア設定
    if app.Environment.IsDevelopment() then
        app.UseOpenApi() |> ignore
        app.UseSwaggerUi() |> ignore

    // ワークフローの作成
    let placeOrderWorkflow = OrderWorkflows.placeOrder
                                checkProductCodeExists
                                getProductPrice
                                checkAddressExists
                                sendOrderAcknowledgment

    // エンドポイント設定
    OrderEndpoints.addOrderEndpoints app placeOrderWorkflow |> ignore

    app.MapGet("/", Func<string>(fun () -> "Order Taking API - F# Functional Domain Modeling")) |> ignore

    app.Run()

    0

