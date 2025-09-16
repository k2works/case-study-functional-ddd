module OrderTaking.Tests.OrderWorkflowsTests

open NUnit.Framework
open OrderTaking.Domain
open OrderTaking.Common
open System.Threading.Tasks

// テスト用のヘルパー関数とモック
module TestHelpers =

    let createValidUnvalidatedOrder() : UnvalidatedOrder = {
        OrderId = "ORDER001"
        CustomerInfo = {
            FirstName = "太郎"
            LastName = "田中"
            EmailAddress = "taro@example.com"
        }
        ShippingAddress = {
            AddressLine1 = "東京都渋谷区"
            AddressLine2 = ""
            City = "渋谷区"
            ZipCode = "150-0002"
        }
        BillingAddress = {
            AddressLine1 = "東京都港区"
            AddressLine2 = ""
            City = "港区"
            ZipCode = "106-0032"
        }
        Lines = [
            {
                OrderLineId = "LINE001"
                ProductCode = "W1234"
                Quantity = 5m
            }
        ]
    }

    let createValidAddress() : Address = {
        AddressLine1 = String50.create "東京都渋谷区" |> function | Ok v -> v | Error e -> failwith e
        AddressLine2 = None
        City = String50.create "渋谷区" |> function | Ok v -> v | Error e -> failwith e
        ZipCode = String50.create "150-0002" |> function | Ok v -> v | Error e -> failwith e
    }

    // モック関数の作成
    let mockCheckProductCodeExists: CheckProductCodeExists =
        fun productCode -> true

    let mockGetProductPrice: GetProductPrice =
        fun productCode ->
            match productCode with
            | Widget _ -> Some 100.00m
            | Gizmo _ -> Some 50.00m

    let mockCheckAddressExists: CheckAddressExists =
        fun unvalidatedAddress ->
            async {
                let line1Result = String50.create unvalidatedAddress.AddressLine1 |> Result.mapError (fun _ -> FieldIsMissing "AddressLine1")
                let cityResult = String50.create unvalidatedAddress.City |> Result.mapError (fun _ -> FieldIsMissing "City")
                let zipResult = String50.create unvalidatedAddress.ZipCode |> Result.mapError (fun _ -> FieldIsMissing "ZipCode")

                match line1Result, cityResult, zipResult with
                | Ok line1, Ok city, Ok zip ->
                    let line2 =
                        if System.String.IsNullOrWhiteSpace(unvalidatedAddress.AddressLine2)
                        then None
                        else String50.create unvalidatedAddress.AddressLine2 |> Result.toOption

                    return Ok {
                        AddressLine1 = line1
                        AddressLine2 = line2
                        City = city
                        ZipCode = zip
                    }
                | Error e, _, _ -> return Error e
                | _, Error e, _ -> return Error e
                | _, _, Error e -> return Error e
            }

    let mockSendOrderAcknowledgment: SendOrderAcknowledgment =
        fun pricedOrder ->
            async {
                return Ok {
                    OrderId = pricedOrder.OrderId
                    EmailAddress = pricedOrder.CustomerInfo.Email
                }
            }

[<TestFixture>]
type ValidateOrderTests() =

    [<Test>]
    member _.``有効な注文は正常に検証される``() =
        async {
            let unvalidatedOrder = TestHelpers.createValidUnvalidatedOrder()

            let! result = OrderWorkflows.validateOrder
                            TestHelpers.mockCheckProductCodeExists
                            TestHelpers.mockCheckAddressExists
                            unvalidatedOrder

            match result with
            | Ok validatedOrder ->
                Assert.That(OrderId.value validatedOrder.OrderId, Is.EqualTo("ORDER001"))
                Assert.That(String50.value validatedOrder.CustomerInfo.Name, Does.Contain("太郎"))
                Assert.That(validatedOrder.Lines.Length, Is.EqualTo(1))
            | Error error ->
                Assert.Fail($"検証に失敗しました: {error}")
        } |> Async.RunSynchronously

    [<Test>]
    member _.``無効な商品コードはエラーになる``() =
        async {
            let invalidOrder = {
                TestHelpers.createValidUnvalidatedOrder() with
                    Lines = [{ OrderLineId = "1"; ProductCode = "INVALID"; Quantity = 1m }]
            }

            let! result = OrderWorkflows.validateOrder
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
        let validatedOrder : ValidatedOrder = {
            OrderId = OrderId.create "ORDER001" |> function | Ok v -> v | Error e -> failwith e
            CustomerInfo = {
                Name = String50.create "田中太郎" |> function | Ok v -> v | Error e -> failwith e
                Email = EmailAddress.create "taro@example.com" |> function | Ok v -> v | Error e -> failwith e
            }
            ShippingAddress = TestHelpers.createValidAddress()
            BillingAddress = TestHelpers.createValidAddress()
            Lines = [
                {
                    OrderLineId = "LINE001"
                    ProductCode = Widget (WidgetCode.create "W1234" |> function | Ok v -> v | Error e -> failwith e)
                    Quantity = Unit (UnitQuantity.create 5 |> function | Ok v -> v | Error e -> failwith e)
                }
            ]
        }

        let result = OrderWorkflows.priceOrder TestHelpers.mockGetProductPrice validatedOrder

        match result with
        | Ok pricedOrder ->
            Assert.That(pricedOrder.AmountToBill, Is.EqualTo(500.00m))
            Assert.That(pricedOrder.Lines.Head.LinePrice, Is.EqualTo(500.00m))
        | Error error ->
            Assert.Fail($"価格計算に失敗しました: {error}")

    [<Test>]
    member _.``存在しない商品の価格計算はエラーになる``() =
        let mockGetProductPriceNotFound: GetProductPrice = fun _ -> None

        let validatedOrder : ValidatedOrder = {
            OrderId = OrderId.create "ORDER001" |> function | Ok v -> v | Error e -> failwith e
            CustomerInfo = {
                Name = String50.create "田中太郎" |> function | Ok v -> v | Error e -> failwith e
                Email = EmailAddress.create "taro@example.com" |> function | Ok v -> v | Error e -> failwith e
            }
            ShippingAddress = TestHelpers.createValidAddress()
            BillingAddress = TestHelpers.createValidAddress()
            Lines = [
                {
                    OrderLineId = "LINE001"
                    ProductCode = Widget (WidgetCode.create "W1234" |> function | Ok v -> v | Error e -> failwith e)
                    Quantity = Unit (UnitQuantity.create 5 |> function | Ok v -> v | Error e -> failwith e)
                }
            ]
        }

        let result = OrderWorkflows.priceOrder mockGetProductPriceNotFound validatedOrder

        match result with
        | Ok _ -> Assert.Fail("存在しない商品でも価格計算が成功しました")
        | Error (ProductNotFound _) -> Assert.Pass()

[<TestFixture>]
type CreateEventsTests() =

    [<Test>]
    member _.``イベントが正しく生成される``() =
        let pricedOrder : PricedOrder = {
            OrderId = OrderId.create "ORDER001" |> function | Ok v -> v | Error e -> failwith e
            CustomerInfo = {
                Name = String50.create "田中太郎" |> function | Ok v -> v | Error e -> failwith e
                Email = EmailAddress.create "taro@example.com" |> function | Ok v -> v | Error e -> failwith e
            }
            ShippingAddress = TestHelpers.createValidAddress()
            BillingAddress = TestHelpers.createValidAddress()
            Lines = []
            AmountToBill = 1000.00m
        }

        let acknowledgment = Some {
            OrderId = pricedOrder.OrderId
            EmailAddress = pricedOrder.CustomerInfo.Email
        }

        let events = OrderWorkflows.createEvents pricedOrder acknowledgment

        Assert.That(events.Length, Is.GreaterThanOrEqualTo(2))
        Assert.That(events |> List.exists (function OrderPlaced _ -> true | _ -> false), Is.True)
        Assert.That(events |> List.exists (function BillableOrderPlaced _ -> true | _ -> false), Is.True)
        Assert.That(events |> List.exists (function AcknowledgmentSent _ -> true | _ -> false), Is.True)

[<TestFixture>]
type PlaceOrderWorkflowTests() =

    [<Test>]
    member _.``完全なワークフローが正常に実行される``() =
        async {
            let unvalidatedOrder = TestHelpers.createValidUnvalidatedOrder()

            let workflow = OrderWorkflows.placeOrder
                            TestHelpers.mockCheckProductCodeExists
                            TestHelpers.mockGetProductPrice
                            TestHelpers.mockCheckAddressExists
                            TestHelpers.mockSendOrderAcknowledgment

            let! result = workflow unvalidatedOrder

            match result with
            | Ok events ->
                Assert.That(events.Length, Is.GreaterThan(0))
                Assert.That(
                    events |> List.exists (function OrderPlaced _ -> true | _ -> false),
                    Is.True,
                    "OrderPlacedイベントが生成されていません"
                )
            | Error error ->
                Assert.Fail($"ワークフローが失敗しました: {error}")
        } |> Async.RunSynchronously