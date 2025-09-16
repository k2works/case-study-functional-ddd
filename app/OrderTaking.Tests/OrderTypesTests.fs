module OrderTaking.Tests.OrderTypesTests

open NUnit.Framework
open OrderTaking.Domain
open OrderTaking.Common

[<TestFixture>]
type CustomerInfoTests() =

    [<Test>]
    member _.``有効な顧客情報を作成できる``() =
        let 名前 = 文字列50.作成 "田中太郎" |> function | Ok 値 -> 値 | Error エラー -> failwith エラー
        let メール = メールアドレス.作成 "taro@example.com" |> function | Ok 値 -> 値 | Error エラー -> failwith エラー

        let 顧客 : 顧客情報 = {
            名前 = 名前
            メール = メール
        }

        Assert.That(文字列50.値 顧客.名前, Is.EqualTo("田中太郎"))
        Assert.That(メールアドレス.値 顧客.メール, Is.EqualTo("taro@example.com"))

[<TestFixture>]
type AddressTests() =

    [<Test>]
    member _.``有効な住所を作成できる``() =
        let 住所行1 = 文字列50.作成 "東京都渋谷区" |> function | Ok 値 -> 値 | Error エラー -> failwith エラー
        let 住所行2 = 文字列50.作成 "1-2-3" |> function | Ok 値 -> Some 値 | Error _ -> None
        let 都市 = 文字列50.作成 "渋谷区" |> function | Ok 値 -> 値 | Error エラー -> failwith エラー
        let 郵便番号 = 文字列50.作成 "150-0002" |> function | Ok 値 -> 値 | Error エラー -> failwith エラー

        let 住所 : 住所 = {
            住所行1 = 住所行1
            住所行2 = 住所行2
            都市 = 都市
            郵便番号 = 郵便番号
        }

        Assert.That(文字列50.値 住所.住所行1, Is.EqualTo("東京都渋谷区"))
        Assert.That(住所.住所行2 |> Option.map 文字列50.値, Is.EqualTo(Some "1-2-3"))
        Assert.That(文字列50.値 住所.都市, Is.EqualTo("渋谷区"))
        Assert.That(文字列50.値 住所.郵便番号, Is.EqualTo("150-0002"))

[<TestFixture>]
type UnvalidatedOrderTests() =

    [<Test>]
    member _.``未検証注文を作成できる``() =
        let 注文 : 未検証注文 = {
            注文ID = "ORDER001"
            顧客情報 = {
                名 = "太郎"
                姓 = "田中"
                メールアドレス = "taro@example.com"
            }
            配送先住所 = {
                住所行1 = "東京都渋谷区"
                住所行2 = "1-2-3"
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
                    数量 = 5.0m
                }
                {
                    注文明細ID = "LINE002"
                    商品コード = "G123"
                    数量 = 2.5m
                }
            ]
        }

        Assert.That(注文.注文ID, Is.EqualTo("ORDER001"))
        Assert.That(注文.顧客情報.名, Is.EqualTo("太郎"))
        Assert.That(注文.明細.Length, Is.EqualTo(2))

[<TestFixture>]
type ValidatedOrderTests() =

    [<Test>]
    member _.``検証済み注文を作成できる``() =
        let orderId = 注文ID.作成 "ORDER001" |> function | Ok v -> v | Error e -> failwith e
        let name = 文字列50.作成 "田中太郎" |> function | Ok v -> v | Error e -> failwith e
        let email = メールアドレス.作成 "taro@example.com" |> function | Ok v -> v | Error e -> failwith e

        let address : 住所 = {
            住所行1 = 文字列50.作成 "東京都渋谷区" |> function | Ok v -> v | Error e -> failwith e
            住所行2 = None
            都市 = 文字列50.作成 "渋谷区" |> function | Ok v -> v | Error e -> failwith e
            郵便番号 = 文字列50.作成 "150-0002" |> function | Ok v -> v | Error e -> failwith e
        }

        let widgetCode = ウィジェットコード.作成 "W1234" |> function | Ok v -> v | Error e -> failwith e
        let quantity = 単位数量.作成 5 |> function | Ok v -> v | Error e -> failwith e

        let order : 検証済注文 = {
            注文ID = orderId
            顧客情報 = {
                名前 = name
                メール = email
            }
            配送先住所 = address
            請求先住所 = address
            明細 = [
                {
                    注文明細ID = "LINE001"
                    商品コード = ウィジェット widgetCode
                    数量 = 単位 quantity
                }
            ]
        }

        Assert.That(注文ID.値 order.注文ID, Is.EqualTo("ORDER001"))
        Assert.That(文字列50.値 order.顧客情報.名前, Is.EqualTo("田中太郎"))
        Assert.That(order.明細.Length, Is.EqualTo(1))

[<TestFixture>]
type PricedOrderTests() =

    [<Test>]
    member _.``価格付き注文を作成できる``() =
        let orderId = 注文ID.作成 "ORDER001" |> function | Ok v -> v | Error e -> failwith e
        let name = 文字列50.作成 "田中太郎" |> function | Ok v -> v | Error e -> failwith e
        let email = メールアドレス.作成 "taro@example.com" |> function | Ok v -> v | Error e -> failwith e

        let address : 住所 = {
            住所行1 = 文字列50.作成 "東京都渋谷区" |> function | Ok v -> v | Error e -> failwith e
            住所行2 = None
            都市 = 文字列50.作成 "渋谷区" |> function | Ok v -> v | Error e -> failwith e
            郵便番号 = 文字列50.作成 "150-0002" |> function | Ok v -> v | Error e -> failwith e
        }

        let widgetCode = ウィジェットコード.作成 "W1234" |> function | Ok v -> v | Error e -> failwith e
        let quantity = 単位数量.作成 5 |> function | Ok v -> v | Error e -> failwith e

        let order : 価格計算済注文 = {
            注文ID = orderId
            顧客情報 = {
                名前 = name
                メール = email
            }
            配送先住所 = address
            請求先住所 = address
            明細 = [
                {
                    注文明細ID = "LINE001"
                    商品コード = ウィジェット widgetCode
                    数量 = 単位 quantity
                    明細価格 = 500.00m
                }
            ]
            請求金額 = 500.00m
        }

        Assert.That(order.請求金額, Is.EqualTo(500.00m))
        Assert.That(order.明細.Head.明細価格, Is.EqualTo(500.00m))

[<TestFixture>]
type DomainEventTests() =

    [<Test>]
    member _.``OrderPlacedイベントを作成できる``() =
        let orderId = 注文ID.作成 "ORDER001" |> function | Ok v -> v | Error e -> failwith e
        let name = 文字列50.作成 "田中太郎" |> function | Ok v -> v | Error e -> failwith e
        let email = メールアドレス.作成 "taro@example.com" |> function | Ok v -> v | Error e -> failwith e

        let address : 住所 = {
            住所行1 = 文字列50.作成 "東京都渋谷区" |> function | Ok v -> v | Error e -> failwith e
            住所行2 = None
            都市 = 文字列50.作成 "渋谷区" |> function | Ok v -> v | Error e -> failwith e
            郵便番号 = 文字列50.作成 "150-0002" |> function | Ok v -> v | Error e -> failwith e
        }

        let pricedOrder : 価格計算済注文 = {
            注文ID = orderId
            顧客情報 = { 名前 = name; メール = email }
            配送先住所 = address
            請求先住所 = address
            明細 = []
            請求金額 = 1000.00m
        }

        let event = 注文受付 pricedOrder

        match event with
        | 注文受付 order ->
            Assert.That(order.請求金額, Is.EqualTo(1000.00m))
        | _ ->
            Assert.Fail("Wrong event type")

    [<Test>]
    member _.``BillableOrderPlacedイベントを作成できる``() =
        let orderId = 注文ID.作成 "ORDER001" |> function | Ok v -> v | Error e -> failwith e
        let address : 住所 = {
            住所行1 = 文字列50.作成 "東京都渋谷区" |> function | Ok v -> v | Error e -> failwith e
            住所行2 = None
            都市 = 文字列50.作成 "渋谷区" |> function | Ok v -> v | Error e -> failwith e
            郵便番号 = 文字列50.作成 "150-0002" |> function | Ok v -> v | Error e -> failwith e
        }

        let billableOrder : 請求対象注文受付 = {
            注文ID = orderId
            請求先住所 = address
            請求金額 = 2000.00m
        }

        let event = 請求対象注文受付 billableOrder

        match event with
        | 請求対象注文受付 order ->
            Assert.That(order.請求金額, Is.EqualTo(2000.00m))
            Assert.That(注文ID.値 order.注文ID, Is.EqualTo("ORDER001"))
        | _ ->
            Assert.Fail("Wrong event type")

[<TestFixture>]
type ValidationErrorTests() =

    [<Test>]
    member _.``FieldIsMissingエラーを作成できる``() =
        let error = フィールド欠如 "CustomerName"
        match error with
        | フィールド欠如 field ->
            Assert.That(field, Is.EqualTo("CustomerName"))
        | _ ->
            Assert.Fail("Wrong error type")

    [<Test>]
    member _.``FieldOutOfRangeエラーを作成できる``() =
        let error = フィールド範囲外 ("Quantity", 0.0m, 100.0m)
        match error with
        | フィールド範囲外 (field, min, max) ->
            Assert.That(field, Is.EqualTo("Quantity"))
            Assert.That(min, Is.EqualTo(0.0m))
            Assert.That(max, Is.EqualTo(100.0m))
        | _ ->
            Assert.Fail("Wrong error type")

    [<Test>]
    member _.``FieldInvalidFormatエラーを作成できる``() =
        let error = フィールド形式不正 "EmailAddress"
        match error with
        | フィールド形式不正 field ->
            Assert.That(field, Is.EqualTo("EmailAddress"))
        | _ ->
            Assert.Fail("Wrong error type")