namespace OrderTaking.Api

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System.Text.Json
open OrderTaking.Domain
open OrderTaking.Common

// DTOの定義
type PlaceOrderRequest = {
    OrderId: string
    CustomerInfo: CustomerInfoDto
    ShippingAddress: AddressDto
    BillingAddress: AddressDto
    Lines: OrderLineDto list
}

and CustomerInfoDto = {
    FirstName: string
    LastName: string
    EmailAddress: string
}

and AddressDto = {
    AddressLine1: string
    AddressLine2: string
    City: string
    ZipCode: string
}

and OrderLineDto = {
    OrderLineId: string
    ProductCode: string
    Quantity: decimal
}

type PlaceOrderResponse = {
    OrderId: string
    Message: string
}

type ErrorResponse = {
    Error: string
    Details: string list
}

module OrderEndpoints =

    // DTOからドメインオブジェクトへの変換
    let toDomainOrder (request: PlaceOrderRequest) : UnvalidatedOrder = {
        OrderId = request.OrderId
        CustomerInfo = {
            FirstName = request.CustomerInfo.FirstName
            LastName = request.CustomerInfo.LastName
            EmailAddress = request.CustomerInfo.EmailAddress
        }
        ShippingAddress = {
            AddressLine1 = request.ShippingAddress.AddressLine1
            AddressLine2 = request.ShippingAddress.AddressLine2
            City = request.ShippingAddress.City
            ZipCode = request.ShippingAddress.ZipCode
        }
        BillingAddress = {
            AddressLine1 = request.BillingAddress.AddressLine1
            AddressLine2 = request.BillingAddress.AddressLine2
            City = request.BillingAddress.City
            ZipCode = request.BillingAddress.ZipCode
        }
        Lines = request.Lines |> List.map (fun line -> {
            OrderLineId = line.OrderLineId
            ProductCode = line.ProductCode
            Quantity = line.Quantity
        })
    }

    // エラーメッセージの生成
    let formatError (error: PlaceOrderError) =
        match error with
        | Validation (FieldIsMissing field) ->
            { Error = "検証エラー"; Details = [$"必須フィールド '{field}' が不足しています"] }
        | Validation (FieldOutOfRange (field, min, max)) ->
            { Error = "検証エラー"; Details = [$"フィールド '{field}' の値が範囲外です（{min}-{max}）"] }
        | Validation (FieldInvalidFormat field) ->
            { Error = "検証エラー"; Details = [$"フィールド '{field}' の形式が不正です"] }
        | Pricing (ProductNotFound productCode) ->
            let productCodeStr =
                match productCode with
                | Widget code -> WidgetCode.value code
                | Gizmo code -> GizmoCode.value code
            { Error = "価格計算エラー"; Details = [$"商品コード '{productCodeStr}' が見つかりません"] }
        | RemoteService error ->
            { Error = "外部サービスエラー"; Details = [error] }

    // 注文受付エンドポイントの実装
    let addOrderEndpoints
        (app: WebApplication)
        (placeOrderWorkflow: PlaceOrderWorkflow) =

        // POST /api/orders
        app.MapPost("/api/orders", Func<PlaceOrderRequest, System.Threading.Tasks.Task<IResult>>(fun request ->
            task {
                let unvalidatedOrder = toDomainOrder request

                let! result = placeOrderWorkflow unvalidatedOrder |> Async.StartAsTask

                match result with
                | Ok events ->
                    let orderPlacedEvent =
                        events
                        |> List.pick (function
                            | OrderPlaced pricedOrder -> Some pricedOrder
                            | _ -> None)

                    let response = {
                        OrderId = OrderId.value orderPlacedEvent.OrderId
                        Message = "注文が正常に受け付けられました"
                    }
                    return Results.Ok(response)

                | Error error ->
                    let errorResponse = formatError error
                    return Results.BadRequest(errorResponse)
            }
        )) |> ignore

        // GET /api/orders/{orderId}
        app.MapGet("/api/orders/{orderId}", Func<string, IResult>(fun orderId ->
            // 実装は将来のバージョンで追加
            Results.Problem("注文照会機能は未実装です", statusCode = 501)
        )) |> ignore

        app