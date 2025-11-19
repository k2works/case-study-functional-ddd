namespace OrderTaking.Infrastructure

open OrderTaking.Domain.ConstrainedTypes
open OrderTaking.Domain.Entities
open System.Data
open System.Data.SQLite
open Dapper
open OrderTaking.Domain.CompoundTypes

/// 注文リポジトリのインターフェース
type IOrderRepository =
    /// 価格計算済み注文を保存する
    abstract member SaveAsync: PricedOrder -> Async<OrderId>

    /// 注文 ID で注文を取得する
    abstract member GetByIdAsync: OrderId -> Async<PricedOrder option>

    /// 注文ステータスを更新する
    abstract member UpdateStatusAsync: OrderId -> string -> Async<unit>

/// データベース行を表すレコード型
[<CLIMutable>]
type OrderRow =
    { order_id: string
      customer_first_name: string
      customer_last_name: string
      customer_email: string
      shipping_address_line1: string
      shipping_address_line2: string
      shipping_address_city: string
      shipping_address_zip_code: string
      billing_address_line1: string
      billing_address_line2: string
      billing_address_city: string
      billing_address_zip_code: string
      total_amount: decimal }

[<CLIMutable>]
type OrderLineRow =
    { order_line_id: string
      product_code: string
      product_type: string
      quantity: decimal
      unit_price: decimal
      line_total: decimal }

/// Dapper を使用した注文リポジトリの実装
type OrderRepository(connectionString: string) =

    /// データベース接続を作成する
    let createConnection () =
        let connection =
            new SQLiteConnection(connectionString)

        connection.Open()
        connection :> IDbConnection

    /// ProductCode を文字列とタイプに分解する
    let deconstructProductCode productCode =
        match productCode with
        | Widget code -> (WidgetCode.value code, "Widget")
        | Gizmo code -> (GizmoCode.value code, "Gizmo")

    /// OrderQuantity を値とタイプに分解する
    let deconstructQuantity quantity =
        match quantity with
        | Unit qty -> (UnitQuantity.value qty |> decimal, "Unit")
        | Kilogram qty -> (KilogramQuantity.value qty, "Kilogram")

    /// CustomerInfo を分解する
    let deconstructCustomerInfo customerInfo =
        let (name, email) =
            CustomerInfo.value customerInfo

        let (firstName, lastName) =
            PersonalName.value name

        (firstName, lastName, email)

    /// Address を分解する
    let deconstructAddress address = Address.value address

    interface IOrderRepository with

        /// 価格計算済み注文を保存する
        member _.SaveAsync(pricedOrder: PricedOrder) =
            async {
                use connection = createConnection ()

                use transaction =
                    connection.BeginTransaction()

                try
                    // Orders テーブルに挿入
                    let insertOrderSql =
                        """
                        INSERT INTO Orders (
                            order_id,
                            customer_first_name,
                            customer_last_name,
                            customer_email,
                            shipping_address_line1,
                            shipping_address_line2,
                            shipping_address_city,
                            shipping_address_zip_code,
                            billing_address_line1,
                            billing_address_line2,
                            billing_address_city,
                            billing_address_zip_code,
                            order_status,
                            total_amount,
                            created_at,
                            updated_at
                        ) VALUES (
                            @OrderId,
                            @CustomerFirstName,
                            @CustomerLastName,
                            @CustomerEmail,
                            @ShippingAddressLine1,
                            @ShippingAddressLine2,
                            @ShippingAddressCity,
                            @ShippingAddressZipCode,
                            @BillingAddressLine1,
                            @BillingAddressLine2,
                            @BillingAddressCity,
                            @BillingAddressZipCode,
                            @OrderStatus,
                            @TotalAmount,
                            @CreatedAt,
                            @UpdatedAt
                        )
                        """

                    let (firstName, lastName, email) =
                        deconstructCustomerInfo pricedOrder.CustomerInfo

                    let (shippingLine1, shippingLine2, shippingCity, shippingZip) =
                        deconstructAddress pricedOrder.ShippingAddress

                    let (billingLine1, billingLine2, billingCity, billingZip) =
                        deconstructAddress pricedOrder.BillingAddress

                    let orderParams =
                        {| OrderId = OrderId.value pricedOrder.OrderId |> string
                           CustomerFirstName = String50.value firstName
                           CustomerLastName = String50.value lastName
                           CustomerEmail = EmailAddress.value email
                           ShippingAddressLine1 = String50.value shippingLine1
                           ShippingAddressLine2 =
                            shippingLine2
                            |> Option.map String50.value
                            |> Option.toObj
                           ShippingAddressCity = String50.value shippingCity
                           ShippingAddressZipCode = ZipCode.value shippingZip
                           BillingAddressLine1 = String50.value billingLine1
                           BillingAddressLine2 =
                            billingLine2
                            |> Option.map String50.value
                            |> Option.toObj
                           BillingAddressCity = String50.value billingCity
                           BillingAddressZipCode = ZipCode.value billingZip
                           OrderStatus = "Priced"
                           TotalAmount = BillingAmount.value pricedOrder.AmountToBill
                           CreatedAt = System.DateTime.UtcNow
                           UpdatedAt = System.DateTime.UtcNow |}

                    do!
                        connection.ExecuteAsync(insertOrderSql, orderParams, transaction)
                        |> Async.AwaitTask
                        |> Async.Ignore

                    // OrderLines テーブルに挿入
                    let insertOrderLineSql =
                        """
                        INSERT INTO OrderLines (
                            order_line_id,
                            order_id,
                            product_code,
                            product_type,
                            quantity,
                            unit_price,
                            line_total,
                            line_order
                        ) VALUES (
                            @OrderLineId,
                            @OrderId,
                            @ProductCode,
                            @ProductType,
                            @Quantity,
                            @UnitPrice,
                            @LineTotal,
                            @LineOrder
                        )
                        """

                    for index, line in pricedOrder.Lines |> List.indexed do
                        let (productCode, productType) =
                            deconstructProductCode line.ProductCode

                        let (quantity, _) =
                            deconstructQuantity line.Quantity

                        let lineParams =
                            {| OrderLineId = OrderLineId.value line.OrderLineId |> string
                               OrderId = OrderId.value pricedOrder.OrderId |> string
                               ProductCode = productCode
                               ProductType = productType
                               Quantity = quantity
                               UnitPrice = Price.value line.Price
                               LineTotal = Price.value line.LinePrice
                               LineOrder = index + 1 |}

                        do!
                            connection.ExecuteAsync(insertOrderLineSql, lineParams, transaction)
                            |> Async.AwaitTask
                            |> Async.Ignore

                    transaction.Commit()

                    return pricedOrder.OrderId

                with ex ->
                    transaction.Rollback()
                    return raise ex
            }

        /// 注文 ID で注文を取得する
        member _.GetByIdAsync(orderId: OrderId) =
            async {
                use connection = createConnection ()

                // Orders テーブルから注文を取得
                let selectOrderSql =
                    """
                    SELECT
                        order_id,
                        customer_first_name,
                        customer_last_name,
                        customer_email,
                        shipping_address_line1,
                        shipping_address_line2,
                        shipping_address_city,
                        shipping_address_zip_code,
                        billing_address_line1,
                        billing_address_line2,
                        billing_address_city,
                        billing_address_zip_code,
                        total_amount
                    FROM Orders
                    WHERE order_id = @OrderId
                    """

                let orderIdStr =
                    OrderId.value orderId |> string

                let! orderResult =
                    connection.QueryFirstOrDefaultAsync<OrderRow>(selectOrderSql, {| OrderId = orderIdStr |})
                    |> Async.AwaitTask

                if isNull (box orderResult) then
                    return None
                else
                    // OrderLines テーブルから注文明細を取得
                    let selectLinesSql =
                        """
                        SELECT
                            order_line_id,
                            product_code,
                            product_type,
                            quantity,
                            unit_price,
                            line_total,
                            line_order
                        FROM OrderLines
                        WHERE order_id = @OrderId
                        ORDER BY line_order
                        """

                    let! linesResult =
                        connection.QueryAsync<OrderLineRow>(selectLinesSql, {| OrderId = orderIdStr |})
                        |> Async.AwaitTask

                    // CustomerInfo を再構成
                    let customerInfo =
                        CustomerInfo.create
                            orderResult.customer_first_name
                            orderResult.customer_last_name
                            orderResult.customer_email
                        |> Result.defaultWith (fun err -> failwith $"Failed to recreate CustomerInfo: {err}")

                    // ShippingAddress を再構成
                    let shippingAddress =
                        Address.create
                            orderResult.shipping_address_line1
                            (if isNull orderResult.shipping_address_line2 then
                                 None
                             else
                                 Some orderResult.shipping_address_line2)
                            orderResult.shipping_address_city
                            orderResult.shipping_address_zip_code
                        |> Result.defaultWith (fun err -> failwith $"Failed to recreate ShippingAddress: {err}")

                    // BillingAddress を再構成
                    let billingAddress =
                        Address.create
                            orderResult.billing_address_line1
                            (if isNull orderResult.billing_address_line2 then
                                 None
                             else
                                 Some orderResult.billing_address_line2)
                            orderResult.billing_address_city
                            orderResult.billing_address_zip_code
                        |> Result.defaultWith (fun err -> failwith $"Failed to recreate BillingAddress: {err}")

                    // OrderLines を再構成
                    let lines =
                        linesResult
                        |> Seq.map (fun line ->
                            let productCode =
                                match line.product_type with
                                | "Widget" ->
                                    WidgetCode.create "ProductCode" line.product_code
                                    |> Result.defaultWith (fun err -> failwith $"Failed to recreate WidgetCode: {err}")
                                    |> Widget
                                | "Gizmo" ->
                                    GizmoCode.create "ProductCode" line.product_code
                                    |> Result.defaultWith (fun err -> failwith $"Failed to recreate GizmoCode: {err}")
                                    |> Gizmo
                                | _ -> failwith $"Unknown product type: {line.product_type}"

                            let quantity =
                                let qty = line.quantity
                                // 数量タイプを推測（簡略化のため、10以下なら Unit、それ以外なら Kilogram と仮定）
                                if qty <= 10.0M then
                                    UnitQuantity.create "Quantity" (int qty)
                                    |> Result.defaultWith (fun err ->
                                        failwith $"Failed to recreate UnitQuantity: {err}")
                                    |> Unit
                                else
                                    KilogramQuantity.create "Quantity" qty
                                    |> Result.defaultWith (fun err ->
                                        failwith $"Failed to recreate KilogramQuantity: {err}")
                                    |> Kilogram

                            let price =
                                Price.create "Price" line.unit_price
                                |> Result.defaultWith (fun err -> failwith $"Failed to recreate Price: {err}")

                            let linePrice =
                                Price.create "LinePrice" line.line_total
                                |> Result.defaultWith (fun err -> failwith $"Failed to recreate LinePrice: {err}")

                            let orderLineId =
                                System.Guid.Parse(line.order_line_id)
                                |> OrderLineId.create

                            { OrderLineId = orderLineId
                              ProductCode = productCode
                              Quantity = quantity
                              Price = price
                              LinePrice = linePrice })
                        |> Seq.toList

                    // AmountToBill を再構成
                    let amountToBill =
                        BillingAmount.create "AmountToBill" orderResult.total_amount
                        |> Result.defaultWith (fun err -> failwith $"Failed to recreate BillingAmount: {err}")

                    // PricedOrder を返す
                    let pricedOrder =
                        { OrderId = orderId
                          CustomerInfo = customerInfo
                          ShippingAddress = shippingAddress
                          BillingAddress = billingAddress
                          Lines = lines
                          AmountToBill = amountToBill }

                    return Some pricedOrder
            }

        /// 注文ステータスを更新する
        member _.UpdateStatusAsync (orderId: OrderId) (status: string) =
            async {
                // TODO: 実装
                return ()
            }
