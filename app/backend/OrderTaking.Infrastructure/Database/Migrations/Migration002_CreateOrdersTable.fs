namespace OrderTaking.Infrastructure.Migrations

open FluentMigrator

/// Orders テーブル作成マイグレーション
[<Migration(20251119002L)>]
type Migration002_CreateOrdersTable() =
    inherit Migration()

    /// マイグレーション実行（Up）
    override this.Up() =
        this.Create
            .Table("Orders")
            .WithColumn("order_id")
            .AsString(50)
            .PrimaryKey()
            .WithColumn("customer_first_name")
            .AsString(50)
            .NotNullable()
            .WithColumn("customer_last_name")
            .AsString(50)
            .NotNullable()
            .WithColumn("customer_email")
            .AsString(255)
            .NotNullable()
            .WithColumn("shipping_address_line1")
            .AsString(100)
            .NotNullable()
            .WithColumn("shipping_address_line2")
            .AsString(100)
            .Nullable()
            .WithColumn("shipping_address_city")
            .AsString(50)
            .NotNullable()
            .WithColumn("shipping_address_zip_code")
            .AsString(10)
            .NotNullable()
            .WithColumn("billing_address_line1")
            .AsString(100)
            .NotNullable()
            .WithColumn("billing_address_line2")
            .AsString(100)
            .Nullable()
            .WithColumn("billing_address_city")
            .AsString(50)
            .NotNullable()
            .WithColumn("billing_address_zip_code")
            .AsString(10)
            .NotNullable()
            .WithColumn("order_status")
            .AsString(20)
            .NotNullable()
            .WithColumn("total_amount")
            .AsDecimal(10, 2)
            .Nullable()
            .WithColumn("created_at")
            .AsDateTime()
            .NotNullable()
            .WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at")
            .AsDateTime()
            .NotNullable()
            .WithDefault(SystemMethods.CurrentDateTime)
        |> ignore

        // インデックスの作成
        this.Create
            .Index("idx_orders_customer_email")
            .OnTable("Orders")
            .OnColumn("customer_email")
        |> ignore

        this.Create
            .Index("idx_orders_order_status")
            .OnTable("Orders")
            .OnColumn("order_status")
        |> ignore

        this.Create
            .Index("idx_orders_created_at")
            .OnTable("Orders")
            .OnColumn("created_at")
        |> ignore

    /// マイグレーションロールバック（Down）
    override this.Down() = this.Delete.Table("Orders") |> ignore
