module OrderTaking.Tests.DatabaseTests

open NUnit.Framework
open OrderTaking.Infrastructure
open OrderTaking.Domain
open OrderTaking.Common
open Microsoft.EntityFrameworkCore
open System.Threading.Tasks
open System.Linq

[<TestFixture>]
type OrderContextTests() =

    let createInMemoryContext () =
        let options = DbContextOptionsBuilder<OrderContext>()
                        .UseInMemoryDatabase(databaseName = System.Guid.NewGuid().ToString())
                        .Options
        new OrderContext(options)

    [<Test>]
    member _.``DbContextが正常に作成される``() =
        use context = createInMemoryContext()
        Assert.That(context, Is.Not.Null)
        Assert.That(context.Orders, Is.Not.Null)
        Assert.That(context.OrderLines, Is.Not.Null)

[<TestFixture>]
type OrderRepositoryTests() =

    let createInMemoryContext () =
        let options = DbContextOptionsBuilder<OrderContext>()
                        .UseInMemoryDatabase(databaseName = System.Guid.NewGuid().ToString())
                        .Options
        new OrderContext(options)

    let createValidPricedOrder() : PricedOrder =
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

        {
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

    [<Test>]
    member _.``注文を正常に保存できる``() =
        async {
            use context = createInMemoryContext()
            let repository = OrderRepository(context) :> IOrderRepository
            let pricedOrder = createValidPricedOrder()

            do! repository.SaveOrder(pricedOrder) |> Async.AwaitTask

            let savedOrder = context.Orders.FirstOrDefault(fun o -> o.OrderId = "ORDER001")
            Assert.That(savedOrder, Is.Not.Null)
            Assert.That(savedOrder.CustomerName, Is.EqualTo("田中太郎"))
            Assert.That(savedOrder.CustomerEmail, Is.EqualTo("taro@example.com"))
            Assert.That(savedOrder.AmountToBill, Is.EqualTo(500.00m))

            let savedLine = context.OrderLines.FirstOrDefault(fun l -> l.OrderId = "ORDER001")
            Assert.That(savedLine, Is.Not.Null)
            Assert.That(savedLine.OrderLineId, Is.EqualTo("LINE001"))
            Assert.That(savedLine.ProductCode, Is.EqualTo("Widget W1234"))
            Assert.That(savedLine.Quantity, Is.EqualTo(5m))
            Assert.That(savedLine.LinePrice, Is.EqualTo(500.00m))
        } |> Async.RunSynchronously

    [<Test>]
    member _.``複数の注文明細を持つ注文を保存できる``() =
        async {
            use context = createInMemoryContext()
            let repository = OrderRepository(context) :> IOrderRepository

            let orderId = OrderId.create "ORDER002" |> function | Ok v -> v | Error e -> failwith e
            let name = String50.create "佐藤花子" |> function | Ok v -> v | Error e -> failwith e
            let email = EmailAddress.create "hanako@example.com" |> function | Ok v -> v | Error e -> failwith e

            let address : Address = {
                AddressLine1 = String50.create "大阪府大阪市" |> function | Ok v -> v | Error e -> failwith e
                AddressLine2 = None
                City = String50.create "大阪市" |> function | Ok v -> v | Error e -> failwith e
                ZipCode = String50.create "530-0001" |> function | Ok v -> v | Error e -> failwith e
            }

            let widgetCode = WidgetCode.create "W5678" |> function | Ok v -> v | Error e -> failwith e
            let gizmoCode = GizmoCode.create "G123" |> function | Ok v -> v | Error e -> failwith e
            let unitQuantity = UnitQuantity.create 3 |> function | Ok v -> v | Error e -> failwith e
            let kgQuantity = KilogramQuantity.create 2.5m |> function | Ok v -> v | Error e -> failwith e

            let pricedOrder : PricedOrder = {
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
                        Quantity = Unit unitQuantity
                        LinePrice = 300.00m
                    }
                    {
                        OrderLineId = "LINE002"
                        ProductCode = Gizmo gizmoCode
                        Quantity = Kilogram kgQuantity
                        LinePrice = 125.00m
                    }
                ]
                AmountToBill = 425.00m
            }

            do! repository.SaveOrder(pricedOrder) |> Async.AwaitTask

            let savedOrder = context.Orders.FirstOrDefault(fun o -> o.OrderId = "ORDER002")
            Assert.That(savedOrder, Is.Not.Null)
            Assert.That(savedOrder.CustomerName, Is.EqualTo("佐藤花子"))
            Assert.That(savedOrder.AmountToBill, Is.EqualTo(425.00m))

            let savedLines = context.OrderLines.Where(fun l -> l.OrderId = "ORDER002").ToArray()
            Assert.That(savedLines.Length, Is.EqualTo(2))

            let widgetLine = savedLines |> Seq.find (fun l -> l.OrderLineId = "LINE001")
            Assert.That(widgetLine.ProductCode, Is.EqualTo("Widget W5678"))
            Assert.That(widgetLine.Quantity, Is.EqualTo(3m))
            Assert.That(widgetLine.LinePrice, Is.EqualTo(300.00m))

            let gizmoLine = savedLines |> Seq.find (fun l -> l.OrderLineId = "LINE002")
            Assert.That(gizmoLine.ProductCode, Is.EqualTo("Gizmo G123"))
            Assert.That(gizmoLine.Quantity, Is.EqualTo(2.5m))
            Assert.That(gizmoLine.LinePrice, Is.EqualTo(125.00m))
        } |> Async.RunSynchronously

[<TestFixture>]
type OrderEntityTests() =

    [<Test>]
    member _.``OrderEntityが正常に作成される``() =
        let entity = OrderEntity()
        entity.OrderId <- "ORDER001"
        entity.CustomerName <- "テスト太郎"
        entity.CustomerEmail <- "test@example.com"
        entity.AmountToBill <- 1000.00m

        Assert.That(entity.OrderId, Is.EqualTo("ORDER001"))
        Assert.That(entity.CustomerName, Is.EqualTo("テスト太郎"))
        Assert.That(entity.CustomerEmail, Is.EqualTo("test@example.com"))
        Assert.That(entity.AmountToBill, Is.EqualTo(1000.00m))

    [<Test>]
    member _.``OrderLineEntityが正常に作成される``() =
        let entity = OrderLineEntity()
        entity.OrderLineId <- "LINE001"
        entity.OrderId <- "ORDER001"
        entity.ProductCode <- "W1234"
        entity.Quantity <- 5m
        entity.LinePrice <- 500.00m

        Assert.That(entity.OrderLineId, Is.EqualTo("LINE001"))
        Assert.That(entity.OrderId, Is.EqualTo("ORDER001"))
        Assert.That(entity.ProductCode, Is.EqualTo("W1234"))
        Assert.That(entity.Quantity, Is.EqualTo(5m))
        Assert.That(entity.LinePrice, Is.EqualTo(500.00m))