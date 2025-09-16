module OrderTaking.Tests.OrderTypesTests

open NUnit.Framework
open OrderTaking.Domain
open OrderTaking.Common

[<TestFixture>]
type CustomerInfoTests() =

    [<Test>]
    member _.``有効な顧客情報を作成できる``() =
        let name = String50.create "田中太郎" |> function | Ok v -> v | Error e -> failwith e
        let email = EmailAddress.create "taro@example.com" |> function | Ok v -> v | Error e -> failwith e

        let customer : CustomerInfo = {
            Name = name
            Email = email
        }

        Assert.That(String50.value customer.Name, Is.EqualTo("田中太郎"))
        Assert.That(EmailAddress.value customer.Email, Is.EqualTo("taro@example.com"))

[<TestFixture>]
type AddressTests() =

    [<Test>]
    member _.``有効な住所を作成できる``() =
        let line1 = String50.create "東京都渋谷区" |> function | Ok v -> v | Error e -> failwith e
        let line2 = String50.create "1-2-3" |> function | Ok v -> Some v | Error _ -> None
        let city = String50.create "渋谷区" |> function | Ok v -> v | Error e -> failwith e
        let zip = String50.create "150-0002" |> function | Ok v -> v | Error e -> failwith e

        let address : Address = {
            AddressLine1 = line1
            AddressLine2 = line2
            City = city
            ZipCode = zip
        }

        Assert.That(String50.value address.AddressLine1, Is.EqualTo("東京都渋谷区"))
        Assert.That(address.AddressLine2 |> Option.map String50.value, Is.EqualTo(Some "1-2-3"))
        Assert.That(String50.value address.City, Is.EqualTo("渋谷区"))
        Assert.That(String50.value address.ZipCode, Is.EqualTo("150-0002"))

[<TestFixture>]
type UnvalidatedOrderTests() =

    [<Test>]
    member _.``未検証注文を作成できる``() =
        let order : UnvalidatedOrder = {
            OrderId = "ORDER001"
            CustomerInfo = {
                FirstName = "太郎"
                LastName = "田中"
                EmailAddress = "taro@example.com"
            }
            ShippingAddress = {
                AddressLine1 = "東京都渋谷区"
                AddressLine2 = "1-2-3"
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
                    Quantity = 5.0m
                }
                {
                    OrderLineId = "LINE002"
                    ProductCode = "G123"
                    Quantity = 2.5m
                }
            ]
        }

        Assert.That(order.OrderId, Is.EqualTo("ORDER001"))
        Assert.That(order.CustomerInfo.FirstName, Is.EqualTo("太郎"))
        Assert.That(order.Lines.Length, Is.EqualTo(2))

[<TestFixture>]
type ValidatedOrderTests() =

    [<Test>]
    member _.``検証済み注文を作成できる``() =
        let orderId = OrderId.create "ORDER001" |> function | Ok v -> v | Error e -> failwith e
        let name = String50.create "田中太郎" |> function | Ok v -> v | Error e -> failwith e
        let email = EmailAddress.create "taro@example.com" |> function | Ok v -> v | Error e -> failwith e

        let address : Address = {
            AddressLine1 = String50.create "東京都渋谷区" |> function | Ok v -> v | Error e -> failwith e
            AddressLine2 = None
            City = String50.create "渋谷区" |> function | Ok v -> v | Error e -> failwith e
            ZipCode = String50.create "150-0002" |> function | Ok v -> v | Error e -> failwith e
        }

        let widgetCode = WidgetCode.create "W1234" |> function | Ok v -> v | Error e -> failwith e
        let quantity = UnitQuantity.create 5 |> function | Ok v -> v | Error e -> failwith e

        let order : ValidatedOrder = {
            OrderId = orderId
            CustomerInfo = {
                Name = name
                Email = email
            }
            ShippingAddress = address
            BillingAddress = address
            Lines = [
                {
                    OrderLineId = "LINE001"
                    ProductCode = Widget widgetCode
                    Quantity = Unit quantity
                }
            ]
        }

        Assert.That(OrderId.value order.OrderId, Is.EqualTo("ORDER001"))
        Assert.That(String50.value order.CustomerInfo.Name, Is.EqualTo("田中太郎"))
        Assert.That(order.Lines.Length, Is.EqualTo(1))

[<TestFixture>]
type PricedOrderTests() =

    [<Test>]
    member _.``価格付き注文を作成できる``() =
        let orderId = OrderId.create "ORDER001" |> function | Ok v -> v | Error e -> failwith e
        let name = String50.create "田中太郎" |> function | Ok v -> v | Error e -> failwith e
        let email = EmailAddress.create "taro@example.com" |> function | Ok v -> v | Error e -> failwith e

        let address : Address = {
            AddressLine1 = String50.create "東京都渋谷区" |> function | Ok v -> v | Error e -> failwith e
            AddressLine2 = None
            City = String50.create "渋谷区" |> function | Ok v -> v | Error e -> failwith e
            ZipCode = String50.create "150-0002" |> function | Ok v -> v | Error e -> failwith e
        }

        let widgetCode = WidgetCode.create "W1234" |> function | Ok v -> v | Error e -> failwith e
        let quantity = UnitQuantity.create 5 |> function | Ok v -> v | Error e -> failwith e

        let order : PricedOrder = {
            OrderId = orderId
            CustomerInfo = {
                Name = name
                Email = email
            }
            ShippingAddress = address
            BillingAddress = address
            Lines = [
                {
                    OrderLineId = "LINE001"
                    ProductCode = Widget widgetCode
                    Quantity = Unit quantity
                    LinePrice = 500.00m
                }
            ]
            AmountToBill = 500.00m
        }

        Assert.That(order.AmountToBill, Is.EqualTo(500.00m))
        Assert.That(order.Lines.Head.LinePrice, Is.EqualTo(500.00m))

[<TestFixture>]
type DomainEventTests() =

    [<Test>]
    member _.``OrderPlacedイベントを作成できる``() =
        let orderId = OrderId.create "ORDER001" |> function | Ok v -> v | Error e -> failwith e
        let name = String50.create "田中太郎" |> function | Ok v -> v | Error e -> failwith e
        let email = EmailAddress.create "taro@example.com" |> function | Ok v -> v | Error e -> failwith e

        let address : Address = {
            AddressLine1 = String50.create "東京都渋谷区" |> function | Ok v -> v | Error e -> failwith e
            AddressLine2 = None
            City = String50.create "渋谷区" |> function | Ok v -> v | Error e -> failwith e
            ZipCode = String50.create "150-0002" |> function | Ok v -> v | Error e -> failwith e
        }

        let pricedOrder : PricedOrder = {
            OrderId = orderId
            CustomerInfo = { Name = name; Email = email }
            ShippingAddress = address
            BillingAddress = address
            Lines = []
            AmountToBill = 1000.00m
        }

        let event = OrderPlaced pricedOrder

        match event with
        | OrderPlaced order ->
            Assert.That(order.AmountToBill, Is.EqualTo(1000.00m))
        | _ ->
            Assert.Fail("Wrong event type")

    [<Test>]
    member _.``BillableOrderPlacedイベントを作成できる``() =
        let orderId = OrderId.create "ORDER001" |> function | Ok v -> v | Error e -> failwith e
        let address : Address = {
            AddressLine1 = String50.create "東京都渋谷区" |> function | Ok v -> v | Error e -> failwith e
            AddressLine2 = None
            City = String50.create "渋谷区" |> function | Ok v -> v | Error e -> failwith e
            ZipCode = String50.create "150-0002" |> function | Ok v -> v | Error e -> failwith e
        }

        let billableOrder : BillableOrderPlaced = {
            OrderId = orderId
            BillingAddress = address
            AmountToBill = 2000.00m
        }

        let event = BillableOrderPlaced billableOrder

        match event with
        | BillableOrderPlaced order ->
            Assert.That(order.AmountToBill, Is.EqualTo(2000.00m))
            Assert.That(OrderId.value order.OrderId, Is.EqualTo("ORDER001"))
        | _ ->
            Assert.Fail("Wrong event type")

[<TestFixture>]
type ValidationErrorTests() =

    [<Test>]
    member _.``FieldIsMissingエラーを作成できる``() =
        let error = FieldIsMissing "CustomerName"
        match error with
        | FieldIsMissing field ->
            Assert.That(field, Is.EqualTo("CustomerName"))
        | _ ->
            Assert.Fail("Wrong error type")

    [<Test>]
    member _.``FieldOutOfRangeエラーを作成できる``() =
        let error = FieldOutOfRange ("Quantity", 0.0m, 100.0m)
        match error with
        | FieldOutOfRange (field, min, max) ->
            Assert.That(field, Is.EqualTo("Quantity"))
            Assert.That(min, Is.EqualTo(0.0m))
            Assert.That(max, Is.EqualTo(100.0m))
        | _ ->
            Assert.Fail("Wrong error type")

    [<Test>]
    member _.``FieldInvalidFormatエラーを作成できる``() =
        let error = FieldInvalidFormat "EmailAddress"
        match error with
        | FieldInvalidFormat field ->
            Assert.That(field, Is.EqualTo("EmailAddress"))
        | _ ->
            Assert.Fail("Wrong error type")