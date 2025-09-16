namespace OrderTaking.Infrastructure

open Microsoft.EntityFrameworkCore
open OrderTaking.Domain
open OrderTaking.Common
open System.Threading.Tasks

// Entity Framework モデル
type OrderEntity() =
    member val OrderId = "" with get, set
    member val CustomerName = "" with get, set
    member val CustomerEmail = "" with get, set
    member val ShippingAddress = "" with get, set
    member val BillingAddress = "" with get, set
    member val AmountToBill = 0m with get, set

type OrderLineEntity() =
    member val OrderLineId = "" with get, set
    member val OrderId = "" with get, set
    member val ProductCode = "" with get, set
    member val Quantity = 0m with get, set
    member val LinePrice = 0m with get, set

// DbContext
type OrderContext(options: DbContextOptions<OrderContext>) =
    inherit DbContext(options)

    [<DefaultValue(false)>] val mutable orders: DbSet<OrderEntity>
    member this.Orders with get() = this.orders and set v = this.orders <- v

    [<DefaultValue(false)>] val mutable orderLines: DbSet<OrderLineEntity>
    member this.OrderLines with get() = this.orderLines and set v = this.orderLines <- v

    override this.OnModelCreating(modelBuilder: ModelBuilder) =
        modelBuilder.Entity<OrderEntity>()
            .HasKey("OrderId") |> ignore

        modelBuilder.Entity<OrderLineEntity>()
            .HasKey("OrderLineId") |> ignore

// リポジトリ実装
type IOrderRepository =
    abstract member SaveOrder: PricedOrder -> Task<unit>
    abstract member GetOrder: OrderId -> Task<PricedOrder option>

type OrderRepository(context: OrderContext) =
    interface IOrderRepository with
        member _.SaveOrder(order: PricedOrder) =
            task {
                let orderEntity = OrderEntity()
                orderEntity.OrderId <- OrderId.value order.OrderId
                orderEntity.CustomerName <- String50.value order.CustomerInfo.Name
                orderEntity.CustomerEmail <- EmailAddress.value order.CustomerInfo.Email
                orderEntity.ShippingAddress <-
                    sprintf "%s %s %s %s"
                        (String50.value order.ShippingAddress.AddressLine1)
                        (order.ShippingAddress.AddressLine2 |> Option.map String50.value |> Option.defaultValue "")
                        (String50.value order.ShippingAddress.City)
                        (String50.value order.ShippingAddress.ZipCode)
                orderEntity.BillingAddress <-
                    sprintf "%s %s %s %s"
                        (String50.value order.BillingAddress.AddressLine1)
                        (order.BillingAddress.AddressLine2 |> Option.map String50.value |> Option.defaultValue "")
                        (String50.value order.BillingAddress.City)
                        (String50.value order.BillingAddress.ZipCode)
                orderEntity.AmountToBill <- order.AmountToBill

                context.Orders.Add(orderEntity) |> ignore

                for line in order.Lines do
                    let lineEntity = OrderLineEntity()
                    lineEntity.OrderLineId <- line.OrderLineId
                    lineEntity.OrderId <- OrderId.value order.OrderId
                    lineEntity.ProductCode <-
                        match line.ProductCode with
                        | Widget code -> sprintf "Widget %s" (WidgetCode.value code)
                        | Gizmo code -> sprintf "Gizmo %s" (GizmoCode.value code)
                    lineEntity.Quantity <-
                        match line.Quantity with
                        | Unit qty -> decimal (UnitQuantity.value qty)
                        | Kilogram qty -> KilogramQuantity.value qty
                    lineEntity.LinePrice <- line.LinePrice

                    context.OrderLines.Add(lineEntity) |> ignore

                let! _ = context.SaveChangesAsync()
                return ()
            }

        member _.GetOrder(orderId: OrderId) =
            task {
                // 実装は将来のバージョンで追加
                return None
            }