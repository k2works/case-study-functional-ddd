module OrderTaking.Tests.OrderRepository

open Xunit
open FsUnit.Xunit
open OrderTaking.Domain.ConstrainedTypes
open OrderTaking.Domain.Entities
open System.Data.SQLite
open OrderTaking.Domain.CompoundTypes
open OrderTaking.Tests.DatabaseTestBase

// ========================================
// テストヘルパー関数
// ========================================

/// Result から Ok の値を取得する（テスト用）
let getOk result =
    match result with
    | Ok value -> value
    | Error err -> failwith $"Expected Ok but got Error: {err}"

/// テスト用の PricedOrder を作成する
let createTestPricedOrder () =
    let orderId = OrderId.generate ()

    let customerInfo =
        CustomerInfo.create "John" "Doe" "john.doe@example.com"
        |> getOk

    let shippingAddress =
        Address.create "123 Main St" None "Tokyo" "10000"
        |> getOk

    let billingAddress = shippingAddress

    let productCode =
        WidgetCode.create "ProductCode" "W1234"
        |> getOk
        |> Widget

    let quantity =
        UnitQuantity.create "Quantity" 10 |> getOk |> Unit

    let price =
        Price.create "Price" 100.0M |> getOk

    let linePrice =
        Price.create "LinePrice" 1000.0M |> getOk

    let orderLine =
        { OrderLineId = OrderLineId.generate ()
          ProductCode = productCode
          Quantity = quantity
          Price = price
          LinePrice = linePrice }

    let amountToBill =
        BillingAmount.create "AmountToBill" 1000.0M
        |> getOk

    { OrderId = orderId
      CustomerInfo = customerInfo
      ShippingAddress = shippingAddress
      BillingAddress = billingAddress
      Lines = [ orderLine ]
      AmountToBill = amountToBill }

// ========================================
// IOrderRepository インターフェーステスト
// ========================================

[<Fact>]
let ``IOrderRepository インターフェースが定義されている`` () =
    // Arrange & Act
    let interfaceType =
        typeof<OrderTaking.Infrastructure.IOrderRepository>

    // Assert
    interfaceType.IsInterface |> should equal true

[<Fact>]
let ``IOrderRepository に SaveAsync メソッドが存在する`` () =
    // Arrange
    let interfaceType =
        typeof<OrderTaking.Infrastructure.IOrderRepository>

    // Act
    let methods = interfaceType.GetMethods()

    let saveMethod =
        methods
        |> Array.tryFind (fun m -> m.Name = "SaveAsync")

    // Assert
    saveMethod |> should not' (equal None)

[<Fact>]
let ``IOrderRepository に GetByIdAsync メソッドが存在する`` () =
    // Arrange
    let interfaceType =
        typeof<OrderTaking.Infrastructure.IOrderRepository>

    // Act
    let methods = interfaceType.GetMethods()

    let getMethod =
        methods
        |> Array.tryFind (fun m -> m.Name = "GetByIdAsync")

    // Assert
    getMethod |> should not' (equal None)

// ========================================
// SaveAsync 実装テスト
// ========================================

[<Fact>]
let ``SaveAsync が PricedOrder を保存して OrderId を返す`` () =
    // Arrange
    use testBase = { new DatabaseTestBase() }

    let pricedOrder = createTestPricedOrder ()

    let repository =
        OrderTaking.Infrastructure.OrderRepository(testBase.ConnectionString)
        :> OrderTaking.Infrastructure.IOrderRepository

    // Act
    let savedOrderId =
        repository.SaveAsync pricedOrder
        |> Async.RunSynchronously

    // Assert
    savedOrderId |> should equal pricedOrder.OrderId

    // Verify - データベースに保存されているか確認
    use connection =
        new SQLiteConnection(testBase.ConnectionString)

    connection.Open()

    use command = connection.CreateCommand()

    command.CommandText <- "SELECT COUNT(*) FROM Orders WHERE order_id = @OrderId"

    command.Parameters.AddWithValue("@OrderId", OrderId.value pricedOrder.OrderId |> string)
    |> ignore

    let count =
        command.ExecuteScalar() :?> int64

    count |> should equal 1L

[<Fact>]
let ``SaveAsync が Orders と OrderLines の両方を保存する`` () =
    // Arrange
    use testBase = { new DatabaseTestBase() }

    let pricedOrder = createTestPricedOrder ()

    let repository =
        OrderTaking.Infrastructure.OrderRepository(testBase.ConnectionString)
        :> OrderTaking.Infrastructure.IOrderRepository

    // Act
    let _ =
        repository.SaveAsync pricedOrder
        |> Async.RunSynchronously

    // Assert - OrderLines も保存されているか確認
    use connection =
        new SQLiteConnection(testBase.ConnectionString)

    connection.Open()

    use command = connection.CreateCommand()

    command.CommandText <- "SELECT COUNT(*) FROM OrderLines WHERE order_id = @OrderId"

    command.Parameters.AddWithValue("@OrderId", OrderId.value pricedOrder.OrderId |> string)
    |> ignore

    let count =
        command.ExecuteScalar() :?> int64

    count
    |> should equal (int64 pricedOrder.Lines.Length)

// ========================================
// GetByIdAsync 実装テスト
// ========================================

[<Fact>]
let ``GetByIdAsync が存在する OrderId で PricedOrder を返す`` () =
    // Arrange
    use testBase = { new DatabaseTestBase() }

    let pricedOrder = createTestPricedOrder ()

    let repository =
        OrderTaking.Infrastructure.OrderRepository(testBase.ConnectionString)
        :> OrderTaking.Infrastructure.IOrderRepository

    // Act - まず保存
    let _ =
        repository.SaveAsync pricedOrder
        |> Async.RunSynchronously

    // Act - 取得
    let retrievedOrder =
        repository.GetByIdAsync pricedOrder.OrderId
        |> Async.RunSynchronously

    // Assert
    retrievedOrder |> should not' (equal None)
    let order = retrievedOrder.Value
    order.OrderId |> should equal pricedOrder.OrderId

    order.Lines.Length
    |> should equal pricedOrder.Lines.Length

[<Fact>]
let ``GetByIdAsync が存在しない OrderId で None を返す`` () =
    // Arrange
    use testBase = { new DatabaseTestBase() }

    let repository =
        OrderTaking.Infrastructure.OrderRepository(testBase.ConnectionString)
        :> OrderTaking.Infrastructure.IOrderRepository

    let nonExistentId = OrderId.generate ()

    // Act
    let retrievedOrder =
        repository.GetByIdAsync nonExistentId
        |> Async.RunSynchronously

    // Assert
    retrievedOrder |> should equal None

// ========================================
// Transaction Tests
// ========================================

[<Fact>]
let ``SaveAsync rolls back transaction on database error`` () =
    // Arrange
    use testBase = { new DatabaseTestBase() }

    use connection =
        new SQLiteConnection(testBase.ConnectionString)

    connection.Open()

    // OrderLines テーブルを削除してエラーを発生させる
    use dropCmd = connection.CreateCommand()
    dropCmd.CommandText <- "DROP TABLE IF EXISTS OrderLines"
    dropCmd.ExecuteNonQuery() |> ignore

    let repository =
        OrderTaking.Infrastructure.OrderRepository(testBase.ConnectionString)
        :> OrderTaking.Infrastructure.IOrderRepository

    let order = createTestPricedOrder ()

    // Act - OrderLines が存在しないのでエラーが発生するはず
    try
        let _ =
            repository.SaveAsync order
            |> Async.RunSynchronously

        // エラーが発生しなかった場合はテスト失敗
        Assert.True(false, "Expected exception but SaveAsync succeeded")
    with ex ->
        // Assert - Orders テーブルにデータが残っていないことを確認（ロールバック確認）
        use checkCmd = connection.CreateCommand()

        checkCmd.CommandText <- "SELECT COUNT(*) FROM Orders WHERE order_id = @OrderId"

        checkCmd.Parameters.AddWithValue("@OrderId", OrderId.value order.OrderId |> string)
        |> ignore

        let count =
            checkCmd.ExecuteScalar() :?> int64

        count |> should equal 0L

[<Fact>]
let ``SaveAsync does not allow duplicate order IDs`` () =
    // Arrange
    use testBase = { new DatabaseTestBase() }

    let repository =
        OrderTaking.Infrastructure.OrderRepository(testBase.ConnectionString)
        :> OrderTaking.Infrastructure.IOrderRepository

    let order = createTestPricedOrder ()

    // Act - 最初の保存
    let savedOrderId =
        repository.SaveAsync order
        |> Async.RunSynchronously

    savedOrderId |> should equal order.OrderId

    // Act - 同じ OrderId で再度保存を試みる
    try
        let _ =
            repository.SaveAsync order
            |> Async.RunSynchronously

        // エラーが発生しなかった場合はテスト失敗
        Assert.True(false, "Expected exception for duplicate order ID")
    with ex ->
        // Assert - constraint 違反のエラーが発生することを確認
        ex.Message.ToLower()
        |> should haveSubstring "constraint"

// ========================================
// Error Case Tests
// ========================================

[<Fact>]
let ``SaveAsync handles empty order lines list`` () =
    // Arrange
    use testBase = { new DatabaseTestBase() }

    let repository =
        OrderTaking.Infrastructure.OrderRepository(testBase.ConnectionString)
        :> OrderTaking.Infrastructure.IOrderRepository

    let order =
        { createTestPricedOrder () with
            Lines = [] }

    // Act
    let savedOrderId =
        repository.SaveAsync order
        |> Async.RunSynchronously

    // Assert
    savedOrderId |> should equal order.OrderId

    // Verify - データベースから取得して確認
    let retrievedOrder =
        repository.GetByIdAsync order.OrderId
        |> Async.RunSynchronously

    match retrievedOrder with
    | Some retrieved ->
        retrieved.OrderId |> should equal order.OrderId
        retrieved.Lines |> should be Empty
    | None -> Assert.True(false, "Order should have been retrieved")

[<Fact>]
let ``SaveAsync handles order with null addressLine2`` () =
    // Arrange
    use testBase = { new DatabaseTestBase() }

    let repository =
        OrderTaking.Infrastructure.OrderRepository(testBase.ConnectionString)
        :> OrderTaking.Infrastructure.IOrderRepository

    let shippingAddress =
        Address.create "123 Main St" None "Springfield" "12345"
        |> getOk

    let billingAddress =
        Address.create "456 Elm St" None "Shelbyville" "54321"
        |> getOk

    let order =
        { createTestPricedOrder () with
            ShippingAddress = shippingAddress
            BillingAddress = billingAddress }

    // Act
    let savedOrderId =
        repository.SaveAsync order
        |> Async.RunSynchronously

    // Assert
    savedOrderId |> should equal order.OrderId

    // Verify - addressLine2 が None として正しく復元されることを確認
    let retrievedOrder =
        repository.GetByIdAsync order.OrderId
        |> Async.RunSynchronously

    match retrievedOrder with
    | Some retrieved ->
        retrieved.OrderId |> should equal order.OrderId

        let (_, line2, _, _) =
            Address.value retrieved.ShippingAddress

        line2 |> should equal None
    | None -> Assert.True(false, "Order should have been retrieved")

// ========================================
// Edge Case Tests
// ========================================

[<Fact>]
let ``SaveAsync handles Widget and Gizmo products correctly`` () =
    // Arrange
    use testBase = { new DatabaseTestBase() }

    let repository =
        OrderTaking.Infrastructure.OrderRepository(testBase.ConnectionString)
        :> OrderTaking.Infrastructure.IOrderRepository

    let widgetCode =
        WidgetCode.create "ProductCode" "W1234" |> getOk

    let gizmoCode =
        GizmoCode.create "ProductCode" "G123" |> getOk

    let widgetQuantity =
        UnitQuantity.create "Quantity" 5 |> getOk |> Unit

    let gizmoQuantity =
        KilogramQuantity.create "Quantity" 12.5M
        |> getOk
        |> Kilogram

    let price =
        Price.create "Price" 10.0M |> getOk

    let widgetLine =
        { OrderLineId = OrderLineId.generate ()
          ProductCode = Widget widgetCode
          Quantity = widgetQuantity
          Price = price
          LinePrice = price }

    let gizmoLine =
        { OrderLineId = OrderLineId.generate ()
          ProductCode = Gizmo gizmoCode
          Quantity = gizmoQuantity
          Price = price
          LinePrice = price }

    let order =
        { createTestPricedOrder () with
            Lines = [ widgetLine; gizmoLine ] }

    // Act
    let savedOrderId =
        repository.SaveAsync order
        |> Async.RunSynchronously

    // Assert
    savedOrderId |> should equal order.OrderId

    // Verify - 両方の製品タイプが正しく保存・復元されることを確認
    let retrievedOrder =
        repository.GetByIdAsync order.OrderId
        |> Async.RunSynchronously

    match retrievedOrder with
    | Some retrieved ->
        retrieved.Lines |> should haveLength 2

        match retrieved.Lines.[0].ProductCode with
        | Widget _ -> ()
        | _ -> Assert.True(false, "First line should be Widget")

        match retrieved.Lines.[1].ProductCode with
        | Gizmo _ -> ()
        | _ -> Assert.True(false, "Second line should be Gizmo")
    | None -> Assert.True(false, "Order should have been retrieved")

[<Fact>]
let ``SaveAsync preserves order line sequence`` () =
    // Arrange
    use testBase = { new DatabaseTestBase() }

    let repository =
        OrderTaking.Infrastructure.OrderRepository(testBase.ConnectionString)
        :> OrderTaking.Infrastructure.IOrderRepository

    let createOrderLine index =
        let productCode =
            WidgetCode.create "ProductCode" $"W123{index}"
            |> getOk
            |> Widget

        let quantity =
            UnitQuantity.create "Quantity" index
            |> getOk
            |> Unit

        let price =
            Price.create "Price" (decimal index * 10.0M)
            |> getOk

        { OrderLineId = OrderLineId.generate ()
          ProductCode = productCode
          Quantity = quantity
          Price = price
          LinePrice = price }

    let lines =
        [ 1; 2; 3; 4; 5 ] |> List.map createOrderLine

    let order =
        { createTestPricedOrder () with
            Lines = lines }

    // Act
    let savedOrderId =
        repository.SaveAsync order
        |> Async.RunSynchronously

    // Assert
    savedOrderId |> should equal order.OrderId

    // Verify - 注文明細が正しい順序で復元されることを確認
    let retrievedOrder =
        repository.GetByIdAsync order.OrderId
        |> Async.RunSynchronously

    match retrievedOrder with
    | Some retrieved ->
        retrieved.Lines |> should haveLength 5

        for i in 0..4 do
            let expectedQuantity =
                UnitQuantity.create "Quantity" (i + 1)
                |> getOk
                |> Unit

            retrieved.Lines.[i].Quantity
            |> should equal expectedQuantity
    | None -> Assert.True(false, "Order should have been retrieved")

// ========================================
// Concurrency Tests
// ========================================

[<Fact>]
let ``SaveAsync handles concurrent saves correctly`` () =
    // Arrange
    use testBase = { new DatabaseTestBase() }

    let repository =
        OrderTaking.Infrastructure.OrderRepository(testBase.ConnectionString)
        :> OrderTaking.Infrastructure.IOrderRepository

    // 10個の異なる注文を作成
    let orders =
        [ 1..10 ]
        |> List.map (fun _ -> createTestPricedOrder ())

    // Act - 並行して保存
    let saveOrder order =
        async {
            let! result = repository.SaveAsync order
            return result
        }

    let results =
        orders
        |> List.map saveOrder
        |> Async.Parallel
        |> Async.RunSynchronously

    // Assert - 全ての注文が保存されたことを確認
    results |> should haveLength 10

    for i in 0..9 do
        results.[i] |> should equal orders.[i].OrderId

    // Verify - データベースに10件保存されていることを確認
    use connection =
        new SQLiteConnection(testBase.ConnectionString)

    connection.Open()

    use command = connection.CreateCommand()
    command.CommandText <- "SELECT COUNT(*) FROM Orders"

    let count =
        command.ExecuteScalar() :?> int64

    count |> should equal 10L

[<Fact>]
let ``GetByIdAsync handles concurrent reads correctly`` () =
    // Arrange
    use testBase = { new DatabaseTestBase() }

    let repository =
        OrderTaking.Infrastructure.OrderRepository(testBase.ConnectionString)
        :> OrderTaking.Infrastructure.IOrderRepository

    let order = createTestPricedOrder ()

    // 注文を保存
    let _ =
        repository.SaveAsync order
        |> Async.RunSynchronously

    // Act - 同じ注文を並行して10回取得
    let getOrder () =
        async {
            let! result = repository.GetByIdAsync order.OrderId

            return result
        }

    let results =
        [ 1..10 ]
        |> List.map (fun _ -> getOrder ())
        |> Async.Parallel
        |> Async.RunSynchronously

    // Assert - 全ての取得が成功していることを確認
    results |> should haveLength 10

    for result in results do
        match result with
        | Some retrieved -> retrieved.OrderId |> should equal order.OrderId
        | None -> Assert.True(false, "Order should have been retrieved")

[<Fact>]
let ``SaveAsync and GetByIdAsync handle concurrent saves and reads correctly`` () =
    // Arrange
    use testBase = { new DatabaseTestBase() }

    let repository =
        OrderTaking.Infrastructure.OrderRepository(testBase.ConnectionString)
        :> OrderTaking.Infrastructure.IOrderRepository

    // 5個の異なる注文を作成
    let orders =
        [ 1..5 ]
        |> List.map (fun _ -> createTestPricedOrder ())

    // Act - まず全て保存してから並行読み取り
    let saveResults =
        orders
        |> List.map (fun order -> repository.SaveAsync order)
        |> Async.Parallel
        |> Async.RunSynchronously

    // 保存完了後に並行読み取り
    let readResults =
        orders
        |> List.map (fun order ->
            async {
                let! result = repository.GetByIdAsync order.OrderId
                return result
            })
        |> Async.Parallel
        |> Async.RunSynchronously

    // Assert - 保存操作が成功していることを確認
    saveResults |> should haveLength 5

    for i in 0..4 do
        saveResults.[i] |> should equal orders.[i].OrderId

    // Assert - 読み取り操作が成功していることを確認
    readResults |> should haveLength 5

    for i in 0..4 do
        match readResults.[i] with
        | Some retrieved ->
            retrieved.OrderId
            |> should equal orders.[i].OrderId
        | None -> Assert.True(false, "Order should have been retrieved")
