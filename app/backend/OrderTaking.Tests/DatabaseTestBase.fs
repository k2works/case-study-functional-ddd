module OrderTaking.Tests.DatabaseTestBase

open System
open System.Data.SQLite
open FluentMigrator.Runner
open Microsoft.Extensions.DependencyInjection
open Xunit
open FsUnit.Xunit

/// データベーステストの基底クラス
/// 各テスト実行前にデータベースを自動的にセットアップし、
/// テスト完了後にクリーンアップする
[<AbstractClass>]
type DatabaseTestBase() =
    let dbFile =
        IO.Path.Combine(IO.Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.db")

    let connectionString =
        $"Data Source={dbFile}"

    /// テスト用データベースのセットアップとマイグレーション実行
    do
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

    /// テスト用接続文字列を取得
    member _.ConnectionString = connectionString

    /// テスト用データベースファイルパスを取得
    member _.DbFile = dbFile

    /// データベース内の全データを削除する（テーブル構造は維持）
    member this.ClearAllData() =
        use connection =
            new SQLiteConnection(this.ConnectionString)

        connection.Open()

        // 外部キー制約を一時的に無効化
        use disableFk = connection.CreateCommand()
        disableFk.CommandText <- "PRAGMA foreign_keys = OFF"
        disableFk.ExecuteNonQuery() |> ignore

        // OrderLines を先に削除（外部キー制約のため）
        use deleteOrderLines =
            connection.CreateCommand()

        deleteOrderLines.CommandText <- "DELETE FROM OrderLines"
        deleteOrderLines.ExecuteNonQuery() |> ignore

        // Orders を削除
        use deleteOrders =
            connection.CreateCommand()

        deleteOrders.CommandText <- "DELETE FROM Orders"
        deleteOrders.ExecuteNonQuery() |> ignore

        // 外部キー制約を再度有効化
        use enableFk = connection.CreateCommand()
        enableFk.CommandText <- "PRAGMA foreign_keys = ON"
        enableFk.ExecuteNonQuery() |> ignore

    /// トランザクションを開始する
    /// テスト内で明示的にトランザクション制御が必要な場合に使用
    member this.BeginTransaction() =
        let connection =
            new SQLiteConnection(this.ConnectionString)

        connection.Open()
        connection.BeginTransaction()

    /// テスト完了後のクリーンアップ
    interface IDisposable with
        member _.Dispose() =
            // SQLite 接続プールをクリア
            SQLiteConnection.ClearAllPools()

            // GC を強制実行してファイルハンドルを解放
            GC.Collect()
            GC.WaitForPendingFinalizers()
            GC.Collect()

            if IO.File.Exists(dbFile) then
                try
                    IO.File.Delete(dbFile)
                with :? IO.IOException ->
                    // ファイルがロックされている場合は少し待ってから再試行
                    Threading.Thread.Sleep(100)

                    try
                        IO.File.Delete(dbFile)
                    with :? IO.IOException ->
                        ()


/// DatabaseTestBase のテスト
module DatabaseTestBaseTests =

    [<Fact>]
    let ``DatabaseTestBase がデータベースを自動作成する`` () =
        // Arrange & Act
        use testBase = { new DatabaseTestBase() }

        // Assert
        IO.File.Exists(testBase.DbFile)
        |> should equal true

    [<Fact>]
    let ``DatabaseTestBase が接続文字列を提供する`` () =
        // Arrange & Act
        use testBase = { new DatabaseTestBase() }

        // Assert
        testBase.ConnectionString.Contains(testBase.DbFile)
        |> should equal true

    [<Fact>]
    let ``DatabaseTestBase がマイグレーションを自動実行する`` () =
        // Arrange
        use testBase = { new DatabaseTestBase() }

        // Act - Orders テーブルが存在するか確認
        use connection =
            new SQLiteConnection(testBase.ConnectionString)

        connection.Open()

        use command = connection.CreateCommand()

        command.CommandText <- "SELECT name FROM sqlite_master WHERE type='table' AND name='Orders'"

        let result = command.ExecuteScalar()

        // Assert
        result |> should not' (equal null)
        result |> should equal (box "Orders")

    [<Fact>]
    let ``ClearAllData がデータベース内の全データを削除する`` () =
        // Arrange
        use testBase = { new DatabaseTestBase() }

        // テストデータを挿入
        use connection =
            new SQLiteConnection(testBase.ConnectionString)

        connection.Open()

        use insertOrder = connection.CreateCommand()

        insertOrder.CommandText <-
            """
            INSERT INTO Orders (
                order_id, customer_first_name, customer_last_name, customer_email,
                shipping_address_line1, shipping_address_line2, shipping_address_city, shipping_address_zip_code,
                billing_address_line1, billing_address_line2, billing_address_city, billing_address_zip_code,
                order_status, total_amount
            ) VALUES (
                '12345', 'John', 'Doe', 'john@example.com',
                '123 Main St', NULL, 'Tokyo', '10000',
                '123 Main St', NULL, 'Tokyo', '10000',
                'Placed', 1000.00
            )
            """

        insertOrder.ExecuteNonQuery() |> ignore

        // Act - データをクリア
        testBase.ClearAllData()

        // Assert - データが削除されたことを確認
        use countCommand =
            connection.CreateCommand()

        countCommand.CommandText <- "SELECT COUNT(*) FROM Orders"

        let count =
            countCommand.ExecuteScalar() :?> int64

        count |> should equal 0L

    [<Fact>]
    let ``DatabaseTestBase が IDisposable を実装している`` () =
        // Arrange & Act
        use testBase = { new DatabaseTestBase() }

        // Assert - IDisposable インターフェースを実装していることを確認
        let disposable = testBase :> IDisposable
        disposable |> should not' (equal null)

    [<Fact>]
    let ``BeginTransaction がトランザクションを開始する`` () =
        // Arrange
        use testBase = { new DatabaseTestBase() }

        // Act
        let transaction =
            testBase.BeginTransaction()

        // Assert
        transaction |> should not' (equal null)
        transaction.Dispose()

    [<Fact>]
    let ``トランザクション内のデータ変更がコミット前にロールバックできる`` () =
        // Arrange
        use testBase = { new DatabaseTestBase() }

        use transaction =
            testBase.BeginTransaction()

        let connection = transaction.Connection

        use insertOrder = connection.CreateCommand()
        insertOrder.Transaction <- transaction

        insertOrder.CommandText <-
            """
            INSERT INTO Orders (
                order_id, customer_first_name, customer_last_name, customer_email,
                shipping_address_line1, shipping_address_line2, shipping_address_city, shipping_address_zip_code,
                billing_address_line1, billing_address_line2, billing_address_city, billing_address_zip_code,
                order_status, total_amount
            ) VALUES (
                '99999', 'Jane', 'Smith', 'jane@example.com',
                '456 Oak St', NULL, 'Osaka', '20000',
                '456 Oak St', NULL, 'Osaka', '20000',
                'Placed', 2000.00
            )
            """

        insertOrder.ExecuteNonQuery() |> ignore

        // Act - ロールバック
        transaction.Rollback()

        // Assert - データが存在しないことを確認（新しい接続で確認）
        use verifyConnection =
            new SQLiteConnection(testBase.ConnectionString)

        verifyConnection.Open()

        use countCommand =
            verifyConnection.CreateCommand()

        countCommand.CommandText <- "SELECT COUNT(*) FROM Orders WHERE order_id = '99999'"

        let count =
            countCommand.ExecuteScalar() :?> int64

        count |> should equal 0L
