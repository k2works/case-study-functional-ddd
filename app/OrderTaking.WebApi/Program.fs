open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore
open OrderTaking.Domain
open OrderTaking.Infrastructure
open OrderTaking.Api
open OrderTaking.Common

// 外部依存関数の実装
let checkProductCodeExists: 商品コード存在確認 =
    fun 商品コード ->
        // 簡易的な実装 - 実際のアプリケーションではデータベースを使用
        match 商品コード with
        | ウィジェット コード ->
            let コード文字列 = ウィジェットコード.値 コード
            コード文字列.StartsWith("W")
        | ギズモ コード ->
            let コード文字列 = ギズモコード.値 コード
            コード文字列.StartsWith("G")

let getProductPrice: 商品価格取得 =
    fun 商品コード ->
        // 簡易的な価格テーブル
        match 商品コード with
        | ウィジェット コード ->
            let コード文字列 = ウィジェットコード.値 コード
            match コード文字列 with
            | "W1234" -> Some 100.00m
            | "W5678" -> Some 150.00m
            | _ -> Some 120.00m // デフォルト価格
        | ギズモ コード ->
            let コード文字列 = ギズモコード.値 コード
            match コード文字列 with
            | "G123" -> Some 50.00m
            | "G456" -> Some 75.00m
            | _ -> Some 60.00m // デフォルト価格

let checkAddressExists: 住所存在確認 =
    fun 未検証住所 ->
        非同期結果ビルダー.非同期結果 {
            // 簡易的な住所検証 - 実際のアプリケーションでは外部APIを使用
            try
                let! 住所行1 = 文字列50.作成 未検証住所.住所行1 |> Result.mapError (fun _ -> フィールド欠如 "AddressLine1") |> 非同期結果.結果から
                let! 都市 = 文字列50.作成 未検証住所.都市 |> Result.mapError (fun _ -> フィールド欠如 "City") |> 非同期結果.結果から
                let! 郵便番号 = 文字列50.作成 未検証住所.郵便番号 |> Result.mapError (fun _ -> フィールド欠如 "ZipCode") |> 非同期結果.結果から

                let 住所行2 =
                    if String.IsNullOrWhiteSpace(未検証住所.住所行2)
                    then None
                    else
                        match 文字列50.作成 未検証住所.住所行2 with
                        | Ok 値 -> Some 値
                        | Error _ -> None

                return {
                    住所行1 = 住所行1
                    住所行2 = 住所行2
                    都市 = 都市
                    郵便番号 = 郵便番号
                }
            with
            | 例外 -> return! 非同期結果.エラーから (フィールド形式不正 例外.Message)
        }

let sendOrderAcknowledgment: 注文確認送信 =
    fun 価格計算済注文 ->
        async {
            // 簡易的なメール送信の模擬
            try
                // 実際のアプリケーションではメールサービスを使用
                return Ok {
                    注文ID = 価格計算済注文.注文ID
                    メールアドレス = 価格計算済注文.顧客情報.メール
                }
            with
            | 例外 -> return Error 例外.Message
        }

[<EntryPoint>]
let main 引数 =
    let ビルダー = WebApplication.CreateBuilder(引数)

    // サービス登録
    ビルダー.Services.AddDbContext<OrderContext>(fun オプション ->
        オプション.UseInMemoryDatabase("OrderTakingDb") |> ignore
    ) |> ignore

    ビルダー.Services.AddScoped<I注文リポジトリ, 注文リポジトリ>() |> ignore

    // Swagger設定
    ビルダー.Services.AddEndpointsApiExplorer() |> ignore
    ビルダー.Services.AddSwaggerDocument() |> ignore

    let アプリ = ビルダー.Build()

    // ミドルウェア設定
    if アプリ.Environment.IsDevelopment() then
        アプリ.UseOpenApi() |> ignore
        アプリ.UseSwaggerUi() |> ignore

    // ワークフローの作成
    let 注文受付ワークフロー = 注文ワークフロー.注文を受け付け
                                checkProductCodeExists
                                getProductPrice
                                checkAddressExists
                                sendOrderAcknowledgment

    // エンドポイント設定
    注文エンドポイント.注文エンドポイントを追加 アプリ 注文受付ワークフロー |> ignore

    アプリ.MapGet("/", Func<string>(fun () -> "Order Taking API - F# Functional Domain Modeling")) |> ignore

    アプリ.Run()

    0

