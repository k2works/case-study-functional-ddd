module OrderTaking.Tests.Migration

open Xunit
open FsUnit.Xunit
open FluentMigrator.Runner
open Microsoft.Extensions.DependencyInjection
open System.Data.SQLite

// ========================================
// FluentMigrator セットアップテスト
// ========================================

[<Fact>]
let ``Migration runner が正しく設定される`` () =
    // Arrange
    let serviceProvider =
        ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(fun rb ->
                rb
                    .AddSQLite()
                    .WithGlobalConnectionString("Data Source=:memory:")
                    .ScanIn(typeof<OrderTaking.Infrastructure.Migrations.Migration001_InitialCreate>.Assembly)
                    .For.Migrations()
                |> ignore)
            .AddLogging(fun lb -> lb.AddFluentMigratorConsole() |> ignore)
            .BuildServiceProvider(false)

    // Act
    use scope = serviceProvider.CreateScope()

    let runner =
        scope.ServiceProvider.GetRequiredService<IMigrationRunner>()

    // Assert
    runner |> should not' (equal null)

[<Fact>]
let ``マイグレーションが検出される`` () =
    // Arrange
    let serviceProvider =
        ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(fun rb ->
                rb
                    .AddSQLite()
                    .WithGlobalConnectionString("Data Source=:memory:")
                    .ScanIn(typeof<OrderTaking.Infrastructure.Migrations.Migration001_InitialCreate>.Assembly)
                    .For.Migrations()
                |> ignore)
            .AddLogging(fun lb -> lb.AddFluentMigratorConsole() |> ignore)
            .BuildServiceProvider(false)

    use scope = serviceProvider.CreateScope()

    let runner =
        scope.ServiceProvider.GetRequiredService<IMigrationRunner>()

    // Act & Assert
    // マイグレーションが登録されていることを確認
    // この時点ではマイグレーションクラスが存在しないため失敗する
    runner.MigrateUp()
    |> should not' (throw typeof<System.Exception>)

[<Fact>]
let ``Orders テーブルが作成される`` () =
    // Arrange
    let dbFile =
        System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"test_orders_{System.Guid.NewGuid()}.db")

    let connectionString =
        $"Data Source={dbFile}"

    try
        use serviceProvider =
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

        // Act
        do
            use scope = serviceProvider.CreateScope()

            let runner =
                scope.ServiceProvider.GetRequiredService<IMigrationRunner>()

            runner.MigrateUp()

        // Assert - Orders テーブルが存在することを確認
        use connection =
            new SQLiteConnection(connectionString)

        connection.Open()
        use command = connection.CreateCommand()
        command.CommandText <- "SELECT name FROM sqlite_master WHERE type='table' AND name='Orders'"
        let result = command.ExecuteScalar()

        result |> should not' (equal null)
        result.ToString() |> should equal "Orders"
    finally
        // クリーンアップ
        System.GC.Collect()
        System.GC.WaitForPendingFinalizers()

        if System.IO.File.Exists(dbFile) then
            try
                System.IO.File.Delete(dbFile)
            with :? System.IO.IOException ->
                () // ファイルが削除できない場合は無視

[<Fact>]
let ``OrderLines テーブルが作成される`` () =
    // Arrange
    let dbFile =
        System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"test_orderlines_{System.Guid.NewGuid()}.db")

    let connectionString =
        $"Data Source={dbFile}"

    try
        use serviceProvider =
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

        // Act
        do
            use scope = serviceProvider.CreateScope()

            let runner =
                scope.ServiceProvider.GetRequiredService<IMigrationRunner>()

            runner.MigrateUp()

        // Assert - OrderLines テーブルが存在することを確認
        use connection =
            new SQLiteConnection(connectionString)

        connection.Open()
        use command = connection.CreateCommand()
        command.CommandText <- "SELECT name FROM sqlite_master WHERE type='table' AND name='OrderLines'"
        let result = command.ExecuteScalar()

        result |> should not' (equal null)
        result.ToString() |> should equal "OrderLines"
    finally
        // クリーンアップ
        System.GC.Collect()
        System.GC.WaitForPendingFinalizers()

        if System.IO.File.Exists(dbFile) then
            try
                System.IO.File.Delete(dbFile)
            with :? System.IO.IOException ->
                () // ファイルが削除できない場合は無視

// ========================================
// マイグレーション統合テスト
// ========================================

[<Fact>]
let ``マイグレーションの Up と Down が正しく動作する`` () =
    // Arrange
    let dbFile =
        System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"test_updown_{System.Guid.NewGuid()}.db")

    let connectionString =
        $"Data Source={dbFile}"

    try
        use serviceProvider =
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

        // Act - Up マイグレーション
        do
            use scope = serviceProvider.CreateScope()

            let runner =
                scope.ServiceProvider.GetRequiredService<IMigrationRunner>()

            runner.MigrateUp()

        // Assert - テーブルが作成されていることを確認
        use connection =
            new SQLiteConnection(connectionString)

        connection.Open()

        use commandOrders =
            connection.CreateCommand()

        commandOrders.CommandText <- "SELECT name FROM sqlite_master WHERE type='table' AND name='Orders'"

        let resultOrders =
            commandOrders.ExecuteScalar()

        resultOrders |> should not' (equal null)

        use commandOrderLines =
            connection.CreateCommand()

        commandOrderLines.CommandText <- "SELECT name FROM sqlite_master WHERE type='table' AND name='OrderLines'"

        let resultOrderLines =
            commandOrderLines.ExecuteScalar()

        resultOrderLines |> should not' (equal null)

        connection.Close()

        // Act - Down マイグレーション
        do
            use scope = serviceProvider.CreateScope()

            let runner =
                scope.ServiceProvider.GetRequiredService<IMigrationRunner>()

            runner.MigrateDown(0L)

        // Assert - テーブルが削除されていることを確認
        connection.Open()

        use commandAfter =
            connection.CreateCommand()

        commandAfter.CommandText <-
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('Orders', 'OrderLines')"

        let count =
            commandAfter.ExecuteScalar() :?> int64

        count |> should equal 0L

    finally
        // クリーンアップ
        System.GC.Collect()
        System.GC.WaitForPendingFinalizers()

        if System.IO.File.Exists(dbFile) then
            try
                System.IO.File.Delete(dbFile)
            with :? System.IO.IOException ->
                () // ファイルが削除できない場合は無視

[<Fact>]
let ``外部キー制約が正しく機能する`` () =
    // Arrange
    let dbFile =
        System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"test_fk_{System.Guid.NewGuid()}.db")

    let connectionString =
        $"Data Source={dbFile}"

    try
        use serviceProvider =
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

        // Act
        do
            use scope = serviceProvider.CreateScope()

            let runner =
                scope.ServiceProvider.GetRequiredService<IMigrationRunner>()

            runner.MigrateUp()

        // Assert - 外部キー制約を確認
        use connection =
            new SQLiteConnection(connectionString)

        connection.Open()

        // 外部キーを有効化
        use enableFk = connection.CreateCommand()
        enableFk.CommandText <- "PRAGMA foreign_keys = ON"
        enableFk.ExecuteNonQuery() |> ignore

        // Orders に行を挿入
        use insertOrder = connection.CreateCommand()

        insertOrder.CommandText <-
            """
            INSERT INTO Orders (order_id, customer_first_name, customer_last_name, customer_email,
                shipping_address_line1, shipping_address_city, shipping_address_zip_code,
                billing_address_line1, billing_address_city, billing_address_zip_code,
                order_status, created_at, updated_at)
            VALUES ('order-1', 'John', 'Doe', 'john@example.com',
                '123 Main St', 'Tokyo', '100-0001',
                '123 Main St', 'Tokyo', '100-0001',
                'Validated', datetime('now'), datetime('now'))
        """

        insertOrder.ExecuteNonQuery() |> ignore

        // OrderLines に行を挿入（正常ケース）
        use insertLine = connection.CreateCommand()

        insertLine.CommandText <-
            """
            INSERT INTO OrderLines (order_line_id, order_id, product_code, product_type, quantity, line_order)
            VALUES ('line-1', 'order-1', 'W1234', 'Widget', 10.0, 1)
        """

        insertLine.ExecuteNonQuery() |> should equal 1

        // 存在しない order_id を参照しようとする（エラーケース）
        use insertInvalidLine =
            connection.CreateCommand()

        insertInvalidLine.CommandText <-
            """
            INSERT INTO OrderLines (order_line_id, order_id, product_code, product_type, quantity, line_order)
            VALUES ('line-2', 'invalid-order', 'W1234', 'Widget', 10.0, 2)
        """

        // 外部キー制約違反でエラーが発生することを確認
        (fun () -> insertInvalidLine.ExecuteNonQuery() |> ignore)
        |> should throw typeof<System.Data.SQLite.SQLiteException>

    finally
        // クリーンアップ
        System.GC.Collect()
        System.GC.WaitForPendingFinalizers()

        if System.IO.File.Exists(dbFile) then
            try
                System.IO.File.Delete(dbFile)
            with :? System.IO.IOException ->
                () // ファイルが削除できない場合は無視
