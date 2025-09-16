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

    let createValidPricedOrder() : 価格計算済注文 =
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

        {
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

    [<Test>]
    member _.``注文を正常に保存できる``() =
        async {
            use context = createInMemoryContext()
            let repository = 注文リポジトリ(context) :> I注文リポジトリ
            let pricedOrder = createValidPricedOrder()

            do! repository.注文を保存(pricedOrder) |> Async.AwaitTask

            let savedOrder = context.Orders.FirstOrDefault(fun o -> o.OrderId = "ORDER001")
            Assert.That(savedOrder, Is.Not.Null)
            Assert.That(savedOrder.CustomerName, Is.EqualTo("田中太郎"))
            Assert.That(savedOrder.CustomerEmail, Is.EqualTo("taro@example.com"))
            Assert.That(savedOrder.AmountToBill, Is.EqualTo(500.00m))

            let savedLine = context.OrderLines.FirstOrDefault(fun l -> l.OrderId = "ORDER001")
            Assert.That(savedLine, Is.Not.Null)
            Assert.That(savedLine.OrderLineId, Is.EqualTo("LINE001"))
            Assert.That(savedLine.ProductCode, Is.EqualTo("ウィジェット W1234"))
            Assert.That(savedLine.Quantity, Is.EqualTo(5m))
            Assert.That(savedLine.LinePrice, Is.EqualTo(500.00m))
        } |> Async.RunSynchronously

    [<Test>]
    member _.``複数の注文明細を持つ注文を保存できる``() =
        async {
            use context = createInMemoryContext()
            let repository = 注文リポジトリ(context) :> I注文リポジトリ

            let orderId = 注文ID.作成 "ORDER002" |> function | Ok v -> v | Error e -> failwith e
            let name = 文字列50.作成 "佐藤花子" |> function | Ok v -> v | Error e -> failwith e
            let email = メールアドレス.作成 "hanako@example.com" |> function | Ok v -> v | Error e -> failwith e

            let address : 住所 = {
                住所行1 = 文字列50.作成 "大阪府大阪市" |> function | Ok v -> v | Error e -> failwith e
                住所行2 = None
                都市 = 文字列50.作成 "大阪市" |> function | Ok v -> v | Error e -> failwith e
                郵便番号 = 文字列50.作成 "530-0001" |> function | Ok v -> v | Error e -> failwith e
            }

            let widgetCode = ウィジェットコード.作成 "W5678" |> function | Ok v -> v | Error e -> failwith e
            let gizmoCode = ギズモコード.作成 "G123" |> function | Ok v -> v | Error e -> failwith e
            let unitQuantity = 単位数量.作成 3 |> function | Ok v -> v | Error e -> failwith e
            let kgQuantity = キログラム数量.作成 2.5m |> function | Ok v -> v | Error e -> failwith e

            let pricedOrder : 価格計算済注文 = {
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
                        数量 = 単位 unitQuantity
                        明細価格 = 300.00m
                    }
                    {
                        注文明細ID = "LINE002"
                        商品コード = ギズモ gizmoCode
                        数量 = キログラム kgQuantity
                        明細価格 = 125.00m
                    }
                ]
                請求金額 = 425.00m
            }

            do! repository.注文を保存(pricedOrder) |> Async.AwaitTask

            let savedOrder = context.Orders.FirstOrDefault(fun o -> o.OrderId = "ORDER002")
            Assert.That(savedOrder, Is.Not.Null)
            Assert.That(savedOrder.CustomerName, Is.EqualTo("佐藤花子"))
            Assert.That(savedOrder.AmountToBill, Is.EqualTo(425.00m))

            let savedLines = context.OrderLines.Where(fun l -> l.OrderId = "ORDER002").ToArray()
            Assert.That(savedLines.Length, Is.EqualTo(2))

            let widgetLine = savedLines |> Seq.find (fun l -> l.OrderLineId = "LINE001")
            Assert.That(widgetLine.ProductCode, Is.EqualTo("ウィジェット W5678"))
            Assert.That(widgetLine.Quantity, Is.EqualTo(3m))
            Assert.That(widgetLine.LinePrice, Is.EqualTo(300.00m))

            let gizmoLine = savedLines |> Seq.find (fun l -> l.OrderLineId = "LINE002")
            Assert.That(gizmoLine.ProductCode, Is.EqualTo("ギズモ G123"))
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