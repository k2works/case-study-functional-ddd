module OrderTaking.Tests.OrderRepository

open Xunit
open FsUnit.Xunit
open OrderTaking.Domain.ConstrainedTypes
open OrderTaking.Domain.Entities
open System.Data.SQLite
open FluentMigrator.Runner
open Microsoft.Extensions.DependencyInjection
open OrderTaking.Domain.CompoundTypes

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

/// テスト用のデータベースとマイグレーションを準備する
let setupTestDatabase () =
    let dbFile =
        System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"test_repo_{System.Guid.NewGuid()}.db")

    let connectionString =
        $"Data Source={dbFile}"

    let serviceProvider =
        ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(fun rb ->
                rb
                    .AddSQLite()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof<OrderTaking.Infrastructure.Migrations.Migration001_InitialCreate>.Assembly)
                    .For.Migrations()
                |> ignore)
            .AddLogging(fun lb -> lb.AddFluentMigratorConsole() |> ignore)
            .BuildServiceProvider(false)

    use scope = serviceProvider.CreateScope()

    let runner =
        scope.ServiceProvider.GetRequiredService<IMigrationRunner>()

    runner.MigrateUp()

    (connectionString, dbFile)

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
    let (connectionString, dbFile) =
        setupTestDatabase ()

    try
        let pricedOrder = createTestPricedOrder ()

        let repository =
            OrderTaking.Infrastructure.OrderRepository(connectionString) :> OrderTaking.Infrastructure.IOrderRepository

        // Act
        let savedOrderId =
            repository.SaveAsync pricedOrder
            |> Async.RunSynchronously

        // Assert
        savedOrderId |> should equal pricedOrder.OrderId

        // Verify - データベースに保存されているか確認
        use connection =
            new SQLiteConnection(connectionString)

        connection.Open()

        use command = connection.CreateCommand()

        command.CommandText <- "SELECT COUNT(*) FROM Orders WHERE order_id = @OrderId"

        command.Parameters.AddWithValue("@OrderId", OrderId.value pricedOrder.OrderId |> string)
        |> ignore

        let count =
            command.ExecuteScalar() :?> int64

        count |> should equal 1L

    finally
        // クリーンアップ
        System.GC.Collect()
        System.GC.WaitForPendingFinalizers()

        if System.IO.File.Exists(dbFile) then
            try
                System.IO.File.Delete(dbFile)
            with :? System.IO.IOException ->
                ()

[<Fact>]
let ``SaveAsync が Orders と OrderLines の両方を保存する`` () =
    // Arrange
    let (connectionString, dbFile) =
        setupTestDatabase ()

    try
        let pricedOrder = createTestPricedOrder ()

        let repository =
            OrderTaking.Infrastructure.OrderRepository(connectionString) :> OrderTaking.Infrastructure.IOrderRepository

        // Act
        let _ =
            repository.SaveAsync pricedOrder
            |> Async.RunSynchronously

        // Assert - OrderLines も保存されているか確認
        use connection =
            new SQLiteConnection(connectionString)

        connection.Open()

        use command = connection.CreateCommand()

        command.CommandText <- "SELECT COUNT(*) FROM OrderLines WHERE order_id = @OrderId"

        command.Parameters.AddWithValue("@OrderId", OrderId.value pricedOrder.OrderId |> string)
        |> ignore

        let count =
            command.ExecuteScalar() :?> int64

        count
        |> should equal (int64 pricedOrder.Lines.Length)

    finally
        // クリーンアップ
        System.GC.Collect()
        System.GC.WaitForPendingFinalizers()

        if System.IO.File.Exists(dbFile) then
            try
                System.IO.File.Delete(dbFile)
            with :? System.IO.IOException ->
                ()

// ========================================
// GetByIdAsync 実装テスト
// ========================================

[<Fact>]
let ``GetByIdAsync が存在する OrderId で PricedOrder を返す`` () =
    // Arrange
    let (connectionString, dbFile) =
        setupTestDatabase ()

    try
        let pricedOrder = createTestPricedOrder ()

        let repository =
            OrderTaking.Infrastructure.OrderRepository(connectionString) :> OrderTaking.Infrastructure.IOrderRepository

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

    finally
        // クリーンアップ
        System.GC.Collect()
        System.GC.WaitForPendingFinalizers()

        if System.IO.File.Exists(dbFile) then
            try
                System.IO.File.Delete(dbFile)
            with :? System.IO.IOException ->
                ()

[<Fact>]
let ``GetByIdAsync が存在しない OrderId で None を返す`` () =
    // Arrange
    let (connectionString, dbFile) =
        setupTestDatabase ()

    try
        let repository =
            OrderTaking.Infrastructure.OrderRepository(connectionString) :> OrderTaking.Infrastructure.IOrderRepository

        let nonExistentId = OrderId.generate ()

        // Act
        let retrievedOrder =
            repository.GetByIdAsync nonExistentId
            |> Async.RunSynchronously

        // Assert
        retrievedOrder |> should equal None

    finally
        // クリーンアップ
        System.GC.Collect()
        System.GC.WaitForPendingFinalizers()

        if System.IO.File.Exists(dbFile) then
            try
                System.IO.File.Delete(dbFile)
            with :? System.IO.IOException ->
                ()
