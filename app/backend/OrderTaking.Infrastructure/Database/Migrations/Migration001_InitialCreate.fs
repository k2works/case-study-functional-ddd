namespace OrderTaking.Infrastructure.Migrations

open FluentMigrator

/// 初期データベーススキーマ作成マイグレーション
[<Migration(20251119001L)>]
type Migration001_InitialCreate() =
    inherit Migration()

    /// マイグレーション実行（Up）
    override this.Up() =
        // このマイグレーションは初期セットアップのプレースホルダー
        // 実際のテーブル作成は後続のマイグレーションで行う
        ()

    /// マイグレーションロールバック（Down）
    override this.Down() =
        // このマイグレーションは何も作成しないため、ロールバックも不要
        ()
