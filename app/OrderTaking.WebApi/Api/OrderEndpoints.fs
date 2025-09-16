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

module 注文エンドポイント =

    // DTOからドメインオブジェクトへの変換
    let ドメイン注文へ変換 (request: PlaceOrderRequest) : 未検証注文 = {
        注文ID = request.OrderId
        顧客情報 = {
            名 = request.CustomerInfo.FirstName
            姓 = request.CustomerInfo.LastName
            メールアドレス = request.CustomerInfo.EmailAddress
        }
        配送先住所 = {
            住所行1 = request.ShippingAddress.AddressLine1
            住所行2 = request.ShippingAddress.AddressLine2
            都市 = request.ShippingAddress.City
            郵便番号 = request.ShippingAddress.ZipCode
        }
        請求先住所 = {
            住所行1 = request.BillingAddress.AddressLine1
            住所行2 = request.BillingAddress.AddressLine2
            都市 = request.BillingAddress.City
            郵便番号 = request.BillingAddress.ZipCode
        }
        明細 = request.Lines |> List.map (fun 明細 -> {
            注文明細ID = 明細.OrderLineId
            商品コード = 明細.ProductCode
            数量 = 明細.Quantity
        })
    }

    // エラーメッセージの生成
    let エラーをフォーマット (error: 注文受付エラー) =
        match error with
        | 検証エラー (フィールド欠如 field) ->
            { Error = "検証エラー"; Details = [$"必須フィールド '{field}' が不足しています"] }
        | 検証エラー (フィールド範囲外 (field, min, max)) ->
            { Error = "検証エラー"; Details = [$"フィールド '{field}' の値が範囲外です（{min}-{max}）"] }
        | 検証エラー (フィールド形式不正 field) ->
            { Error = "検証エラー"; Details = [$"フィールド '{field}' の形式が不正です"] }
        | 価格計算エラー (商品が見つからない 商品コード) ->
            let 商品コード文字列 =
                match 商品コード with
                | ウィジェット コード -> ウィジェットコード.値 コード
                | ギズモ コード -> ギズモコード.値 コード
            { Error = "価格計算エラー"; Details = [$"商品コード '{商品コード文字列}' が見つかりません"] }
        | 外部サービスエラー エラー ->
            { Error = "外部サービスエラー"; Details = [エラー] }

    // 注文受付エンドポイントの実装
    let 注文エンドポイントを追加
        (app: WebApplication)
        (placeOrderWorkflow: 注文受付ワークフロー) =

        // POST /api/orders
        app.MapPost("/api/orders", Func<PlaceOrderRequest, System.Threading.Tasks.Task<IResult>>(fun 要求 ->
            task {
                let 未検証注文 = ドメイン注文へ変換 要求

                let! 結果 = placeOrderWorkflow 未検証注文 |> Async.StartAsTask

                match 結果 with
                | Ok イベント ->
                    let 注文受付イベント =
                        イベント
                        |> List.pick (function
                            | 注文受付 価格計算済注文 -> Some 価格計算済注文
                            | _ -> None)

                    let 応答 = {
                        OrderId = 注文ID.値 注文受付イベント.注文ID
                        Message = "注文が正常に受け付けられました"
                    }
                    return Results.Ok(応答)

                | Error エラー ->
                    let エラー応答 = エラーをフォーマット エラー
                    return Results.BadRequest(エラー応答)
            }
        )) |> ignore

        // GET /api/orders/{orderId}
        app.MapGet("/api/orders/{orderId}", Func<string, IResult>(fun orderId ->
            // 実装は将来のバージョンで追加
            Results.Problem("注文照会機能は未実装です", statusCode = 501)
        )) |> ignore

        app