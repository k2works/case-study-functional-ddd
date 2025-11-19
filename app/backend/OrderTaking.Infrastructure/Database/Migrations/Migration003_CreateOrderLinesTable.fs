namespace OrderTaking.Infrastructure.Migrations

open FluentMigrator

/// OrderLines テーブル作成マイグレーション
[<Migration(20251119003L)>]
type Migration003_CreateOrderLinesTable() =
    inherit Migration()

    /// マイグレーション実行（Up）
    override this.Up() =
        this.Create
            .Table("OrderLines")
            .WithColumn("order_line_id")
            .AsString(50)
            .PrimaryKey()
            .WithColumn("order_id")
            .AsString(50)
            .NotNullable()
            .ForeignKey("FK_OrderLines_Orders", "Orders", "order_id")
            .WithColumn("product_code")
            .AsString(10)
            .NotNullable()
            .WithColumn("product_type")
            .AsString(10)
            .NotNullable()
            .WithColumn("quantity")
            .AsDecimal(10, 3)
            .NotNullable()
            .WithColumn("unit_price")
            .AsDecimal(10, 2)
            .Nullable()
            .WithColumn("line_total")
            .AsDecimal(10, 2)
            .Nullable()
            .WithColumn("line_order")
            .AsInt32()
            .NotNullable()
        |> ignore

        // インデックスの作成
        this.Create
            .Index("idx_orderlines_order_id")
            .OnTable("OrderLines")
            .OnColumn("order_id")
        |> ignore

        this.Create
            .Index("idx_orderlines_product_code")
            .OnTable("OrderLines")
            .OnColumn("product_code")
        |> ignore

    /// マイグレーションロールバック（Down）
    override this.Down() =
        this.Delete.Table("OrderLines") |> ignore
