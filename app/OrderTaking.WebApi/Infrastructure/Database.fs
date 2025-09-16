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
type I注文リポジトリ =
    abstract member 注文を保存: 価格計算済注文 -> Task<unit>
    abstract member 注文を取得: 注文ID -> Task<価格計算済注文 option>

type 注文リポジトリ(context: OrderContext) =
    interface I注文リポジトリ with
        member _.注文を保存(order: 価格計算済注文) =
            task {
                let 注文エンティティ = OrderEntity()
                注文エンティティ.OrderId <- 注文ID.値 order.注文ID
                注文エンティティ.CustomerName <- 文字列50.値 order.顧客情報.名前
                注文エンティティ.CustomerEmail <- メールアドレス.値 order.顧客情報.メール
                注文エンティティ.ShippingAddress <-
                    sprintf "%s %s %s %s"
                        (文字列50.値 order.配送先住所.住所行1)
                        (order.配送先住所.住所行2 |> Option.map 文字列50.値 |> Option.defaultValue "")
                        (文字列50.値 order.配送先住所.都市)
                        (文字列50.値 order.配送先住所.郵便番号)
                注文エンティティ.BillingAddress <-
                    sprintf "%s %s %s %s"
                        (文字列50.値 order.請求先住所.住所行1)
                        (order.請求先住所.住所行2 |> Option.map 文字列50.値 |> Option.defaultValue "")
                        (文字列50.値 order.請求先住所.都市)
                        (文字列50.値 order.請求先住所.郵便番号)
                注文エンティティ.AmountToBill <- order.請求金額

                context.Orders.Add(注文エンティティ) |> ignore

                for 明細 in order.明細 do
                    let 明細エンティティ = OrderLineEntity()
                    明細エンティティ.OrderLineId <- 明細.注文明細ID
                    明細エンティティ.OrderId <- 注文ID.値 order.注文ID
                    明細エンティティ.ProductCode <-
                        match 明細.商品コード with
                        | ウィジェット コード -> sprintf "ウィジェット %s" (ウィジェットコード.値 コード)
                        | ギズモ コード -> sprintf "ギズモ %s" (ギズモコード.値 コード)
                    明細エンティティ.Quantity <-
                        match 明細.数量 with
                        | 単位 数量値 -> decimal (単位数量.値 数量値)
                        | キログラム 数量値 -> キログラム数量.値 数量値
                    明細エンティティ.LinePrice <- 明細.明細価格

                    context.OrderLines.Add(明細エンティティ) |> ignore

                let! 結果 = context.SaveChangesAsync()
                return ()
            }

        member _.注文を取得(orderId: 注文ID) =
            task {
                // 実装は将来のバージョンで追加
                return None
            }