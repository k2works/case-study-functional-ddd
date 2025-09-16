module OrderTaking.Tests.OrderWorkflowsTests

open NUnit.Framework
open OrderTaking.Domain
open OrderTaking.Common
open System.Threading.Tasks

// テスト用のヘルパー関数とモック
module TestHelpers =

    let createValidUnvalidatedOrder() : 未検証注文 = {
        注文ID = "ORDER001"
        顧客情報 = {
            名 = "太郎"
            姓 = "田中"
            メールアドレス = "taro@example.com"
        }
        配送先住所 = {
            住所行1 = "東京都渋谷区"
            住所行2 = ""
            都市 = "渋谷区"
            郵便番号 = "150-0002"
        }
        請求先住所 = {
            住所行1 = "東京都港区"
            住所行2 = ""
            都市 = "港区"
            郵便番号 = "106-0032"
        }
        明細 = [
            {
                注文明細ID = "LINE001"
                商品コード = "W1234"
                数量 = 5m
            }
        ]
    }

    let createValidAddress() : 住所 = {
        住所行1 = 文字列50.作成 "東京都渋谷区" |> function | Ok v -> v | Error e -> failwith e
        住所行2 = None
        都市 = 文字列50.作成 "渋谷区" |> function | Ok v -> v | Error e -> failwith e
        郵便番号 = 文字列50.作成 "150-0002" |> function | Ok v -> v | Error e -> failwith e
    }

    // モック関数の作成
    let mockCheckProductCodeExists: 商品コード存在確認 =
        fun productCode -> true

    let mockGetProductPrice: 商品価格取得 =
        fun productCode ->
            match productCode with
            | ウィジェット _ -> Some 100.00m
            | ギズモ _ -> Some 50.00m

    let mockCheckAddressExists: 住所存在確認 =
        fun unvalidatedAddress ->
            async {
                let line1Result = 文字列50.作成 unvalidatedAddress.住所行1 |> Result.mapError (fun _ -> フィールド欠如 "AddressLine1")
                let cityResult = 文字列50.作成 unvalidatedAddress.都市 |> Result.mapError (fun _ -> フィールド欠如 "City")
                let zipResult = 文字列50.作成 unvalidatedAddress.郵便番号 |> Result.mapError (fun _ -> フィールド欠如 "ZipCode")

                match line1Result, cityResult, zipResult with
                | Ok line1, Ok city, Ok zip ->
                    let line2 =
                        if System.String.IsNullOrWhiteSpace(unvalidatedAddress.住所行2)
                        then None
                        else
                            match 文字列50.作成 unvalidatedAddress.住所行2 with
                            | Ok value -> Some value
                            | Error _ -> None

                    let 住所: 住所 = {
                        住所行1 = line1
                        住所行2 = line2
                        都市 = city
                        郵便番号 = zip
                    }
                    return Ok 住所
                | Error e, _, _ -> return Error e
                | _, Error e, _ -> return Error e
                | _, _, Error e -> return Error e
            }

    let mockSendOrderAcknowledgment: 注文確認送信 =
        fun pricedOrder ->
            async {
                let 確認: 確認送信完了 = {
                    注文ID = pricedOrder.注文ID
                    メールアドレス = pricedOrder.顧客情報.メール
                }
                return Ok 確認
            }

[<TestFixture>]
type ValidateOrderTests() =

    [<Test>]
    member _.``有効な注文は正常に検証される``() =
        async {
            let unvalidatedOrder = TestHelpers.createValidUnvalidatedOrder()

            let! result = 注文ワークフロー.注文を検証
                            TestHelpers.mockCheckProductCodeExists
                            TestHelpers.mockCheckAddressExists
                            unvalidatedOrder

            match result with
            | Ok validatedOrder ->
                Assert.That(注文ID.値 validatedOrder.注文ID, Is.EqualTo("ORDER001"))
                Assert.That(文字列50.値 validatedOrder.顧客情報.名前, Does.Contain("太郎"))
                Assert.That(validatedOrder.明細.Length, Is.EqualTo(1))
            | Error error ->
                Assert.Fail($"検証に失敗しました: {error}")
        } |> Async.RunSynchronously

    [<Test>]
    member _.``無効な商品コードはエラーになる``() =
        async {
            let invalidOrder = {
                TestHelpers.createValidUnvalidatedOrder() with
                    明細 = [{ 注文明細ID = "1"; 商品コード = "INVALID"; 数量 = 1m }]
            }

            let! result = 注文ワークフロー.注文を検証
                            TestHelpers.mockCheckProductCodeExists
                            TestHelpers.mockCheckAddressExists
                            invalidOrder

            match result with
            | Ok _ -> Assert.Fail("無効な商品コードが検証をパスしました")
            | Error error ->
                Assert.That(error.ToString(), Does.Contain("無効な商品コード"))
        } |> Async.RunSynchronously

[<TestFixture>]
type PriceOrderTests() =

    [<Test>]
    member _.``注文の価格計算が正しく行われる``() =
        let validatedOrder : 検証済注文 = {
            注文ID = 注文ID.作成 "ORDER001" |> function | Ok v -> v | Error e -> failwith e
            顧客情報 = {
                名前 = 文字列50.作成 "田中太郎" |> function | Ok v -> v | Error e -> failwith e
                メール = メールアドレス.作成 "taro@example.com" |> function | Ok v -> v | Error e -> failwith e
            }
            配送先住所 = TestHelpers.createValidAddress()
            請求先住所 = TestHelpers.createValidAddress()
            明細 = [
                {
                    注文明細ID = "LINE001"
                    商品コード = ウィジェット (ウィジェットコード.作成 "W1234" |> function | Ok v -> v | Error e -> failwith e)
                    数量 = 単位 (単位数量.作成 5 |> function | Ok v -> v | Error e -> failwith e)
                }
            ]
        }

        let result = 注文ワークフロー.注文価格を計算 TestHelpers.mockGetProductPrice validatedOrder

        match result with
        | Ok pricedOrder ->
            Assert.That(pricedOrder.請求金額, Is.EqualTo(500.00m))
            Assert.That(pricedOrder.明細.Head.明細価格, Is.EqualTo(500.00m))
        | Error error ->
            Assert.Fail($"価格計算に失敗しました: {error}")

    [<Test>]
    member _.``存在しない商品の価格計算はエラーになる``() =
        let mockGetProductPriceNotFound: 商品価格取得 = fun _ -> None

        let validatedOrder : 検証済注文 = {
            注文ID = 注文ID.作成 "ORDER001" |> function | Ok v -> v | Error e -> failwith e
            顧客情報 = {
                名前 = 文字列50.作成 "田中太郎" |> function | Ok v -> v | Error e -> failwith e
                メール = メールアドレス.作成 "taro@example.com" |> function | Ok v -> v | Error e -> failwith e
            }
            配送先住所 = TestHelpers.createValidAddress()
            請求先住所 = TestHelpers.createValidAddress()
            明細 = [
                {
                    注文明細ID = "LINE001"
                    商品コード = ウィジェット (ウィジェットコード.作成 "W1234" |> function | Ok v -> v | Error e -> failwith e)
                    数量 = 単位 (単位数量.作成 5 |> function | Ok v -> v | Error e -> failwith e)
                }
            ]
        }

        let result = 注文ワークフロー.注文価格を計算 mockGetProductPriceNotFound validatedOrder

        match result with
        | Ok _ -> Assert.Fail("存在しない商品でも価格計算が成功しました")
        | Error (商品が見つからない _) -> Assert.Pass()

[<TestFixture>]
type CreateEventsTests() =

    [<Test>]
    member _.``イベントが正しく生成される``() =
        let pricedOrder : 価格計算済注文 = {
            注文ID = 注文ID.作成 "ORDER001" |> function | Ok v -> v | Error e -> failwith e
            顧客情報 = {
                名前 = 文字列50.作成 "田中太郎" |> function | Ok v -> v | Error e -> failwith e
                メール = メールアドレス.作成 "taro@example.com" |> function | Ok v -> v | Error e -> failwith e
            }
            配送先住所 = TestHelpers.createValidAddress()
            請求先住所 = TestHelpers.createValidAddress()
            明細 = []
            請求金額 = 1000.00m
        }

        let acknowledgment = Some {
            注文ID = pricedOrder.注文ID
            メールアドレス = pricedOrder.顧客情報.メール
        }

        let events = 注文ワークフロー.イベントを作成 pricedOrder acknowledgment

        Assert.That(events.Length, Is.GreaterThanOrEqualTo(2))
        Assert.That(events |> List.exists (function 注文受付 _ -> true | _ -> false), Is.True)
        Assert.That(events |> List.exists (function 請求対象注文受付 _ -> true | _ -> false), Is.True)
        Assert.That(events |> List.exists (function 確認送信完了 _ -> true | _ -> false), Is.True)

[<TestFixture>]
type PlaceOrderWorkflowTests() =

    [<Test>]
    member _.``完全なワークフローが正常に実行される``() =
        async {
            let unvalidatedOrder = TestHelpers.createValidUnvalidatedOrder()

            let workflow = 注文ワークフロー.注文を受け付け
                            TestHelpers.mockCheckProductCodeExists
                            TestHelpers.mockGetProductPrice
                            TestHelpers.mockCheckAddressExists
                            TestHelpers.mockSendOrderAcknowledgment

            let! result = workflow unvalidatedOrder

            match result with
            | Ok events ->
                Assert.That(events.Length, Is.GreaterThan(0))
                Assert.That(
                    events |> List.exists (function 注文受付 _ -> true | _ -> false),
                    Is.True,
                    "注文受付イベントが生成されていません"
                )
            | Error error ->
                Assert.Fail($"ワークフローが失敗しました: {error}")
        } |> Async.RunSynchronously