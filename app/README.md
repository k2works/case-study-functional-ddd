# OrderTaking - 注文受付システム

F# の関数型プログラミングとドメイン駆動設計による注文受付システム。

## 📋 概要

本プロジェクトは「Domain Modeling Made Functional」の原則に基づいた、F# による関数型ドメイン駆動設計のケーススタディです。ヘキサゴナルアーキテクチャを採用し、テスト駆動開発（TDD）による高品質なソフトウェア開発を実践します。

## 🛠️ 技術スタック

### 言語・フレームワーク
- **F# 9.0** / **.NET 10.0 (RC)**
- **ASP.NET Core 10.0** - Minimal API
- **Entity Framework Core 10.0** - InMemory Provider

### ビルド・品質管理
- **Cake 5.0.0** - ビルド自動化
- **Fantomas 6.3.13** - コードフォーマッター
- **FSharpLint 0.26.4** - 静的解析

### テスト
- **xUnit 2.6.2** - テストフレームワーク
- **FsUnit.xUnit 6.0.0** - BDD スタイルテスト
- **FsCheck 2.16.6** - プロパティベーステスト
- **coverlet.collector 6.0.2** - カバレッジ収集

### インフラ
- **Heroku** - ホスティング（Standard-1X Dyno）
- **GitHub Actions** - CI/CD

## 📦 前提条件

### 必須
- [.NET 10.0 SDK (RC)](https://dotnet.microsoft.com/download/dotnet/10.0) または互換バージョン
- Git

### 推奨
- [Visual Studio Code](https://code.visualstudio.com/) + [Ionide](https://ionide.io/)
- または [JetBrains Rider](https://www.jetbrains.com/rider/)

## 🚀 開発環境セットアップ

### 1. リポジトリのクローン

```bash
git clone https://github.com/k2works/case-study-functional-ddd.git
cd case-study-functional-ddd/app
```

### 2. .NET SDK の確認

```bash
dotnet --version
# 10.0.100-rc.2.25502.107 または互換バージョン
```

### 3. ローカルツールの復元

```bash
dotnet tool restore
```

インストールされるツール：
- Cake.Tool 5.0.0
- Fantomas 6.3.13
- dotnet-fsharplint 0.26.4

### 4. 依存関係の復元

```bash
dotnet restore
```

### 5. ビルド

```bash
dotnet build
```

## 🏗️ ビルド・テスト手順

### Cake タスクを使用（推奨）

```bash
# すべて実行（Clean → Restore → Build → Test）
dotnet cake

# 個別タスク
dotnet cake --target=Clean         # クリーンアップ
dotnet cake --target=Restore       # 依存関係復元
dotnet cake --target=Build         # ビルド
dotnet cake --target=Test          # テスト実行
```

### dotnet CLI を直接使用

```bash
# ビルド
dotnet build --configuration Release

# テスト実行
dotnet test --configuration Release
```

## ✨ コード品質管理

### コードフォーマット

```bash
# フォーマット実行
dotnet cake --target=Format

# フォーマットチェック（CI 用）
dotnet cake --target=FormatCheck
```

または直接実行：

```bash
# フォーマット
dotnet fantomas src/ tests/

# チェックのみ
dotnet fantomas --check src/ tests/
```

### 静的解析

```bash
# リント実行
dotnet cake --target=Lint

# すべての品質チェック（FormatCheck + Lint）
dotnet cake --target=Quality
```

または直接実行：

```bash
dotnet dotnet-fsharplint lint OrderTaking.sln
```

## 📊 テスト

### テスト実行

```bash
# すべてのテスト
dotnet test

# 詳細出力
dotnet test --verbosity normal

# カバレッジ付き
dotnet test --collect:"XPlat Code Coverage"
```

### テストの種類

プロジェクトには以下のテストが含まれています：

1. **xUnit テスト** - 基本的な単体テスト
2. **FsUnit テスト** - BDD スタイルの読みやすいテスト
3. **FsCheck テスト** - プロパティベーステスト

例：
```fsharp
// xUnit
[<Fact>]
let ``Basic xUnit test`` () = Assert.True(true)

// FsUnit (BDD スタイル)
[<Fact>]
let ``FsUnit: List should contain elements`` () =
    [ 1; 2; 3 ] |> should contain 2

// FsCheck (プロパティベース)
[<Property>]
let ``List reverse twice is original`` (xs: int list) =
    List.rev (List.rev xs) = xs
```

## 🚢 デプロイ

### Heroku へのデプロイ

#### 前提条件
- Heroku アカウント
- Heroku CLI インストール

#### 初回セットアップ

```bash
# Heroku にログイン
heroku login

# アプリケーション作成
heroku create <your-app-name>

# Buildpack 設定
heroku buildpacks:set https://github.com/jincod/dotnetcore-buildpack

# デプロイ
git push heroku main
```

#### GitHub Actions による自動デプロイ

main ブランチへのプッシュで自動的にデプロイされます。

**必要な GitHub Secrets:**
- `HEROKU_API_KEY` - Heroku API キー
- `HEROKU_APP_NAME` - Heroku アプリ名
- `HEROKU_EMAIL` - Heroku アカウントメール

設定方法：
1. GitHub リポジトリ → Settings → Secrets and variables → Actions
2. New repository secret で上記 3 つを追加

## 📁 プロジェクト構造

```
app/
├── OrderTaking.sln              # ソリューションファイル
├── build.cake                   # Cake ビルドスクリプト
├── .editorconfig               # エディタ設定
├── .config/
│   └── dotnet-tools.json       # ローカルツール定義
├── src/
│   ├── OrderTaking.Domain/         # ドメイン層
│   ├── OrderTaking.Application/    # アプリケーション層
│   ├── OrderTaking.Infrastructure/ # インフラ層
│   └── OrderTaking.WebApi/         # WebAPI 層
└── tests/
    └── OrderTaking.Tests/          # テストプロジェクト
```

### アーキテクチャ

ヘキサゴナルアーキテクチャ（ポートとアダプター）を採用：

```
┌─────────────────────────────────────┐
│         WebApi (Adapter)            │
├─────────────────────────────────────┤
│       Application (Port)            │
├─────────────────────────────────────┤
│          Domain (Core)              │
├─────────────────────────────────────┤
│    Infrastructure (Adapter)         │
└─────────────────────────────────────┘
```

**依存関係:**
- WebApi → Application, Infrastructure
- Application → Domain
- Infrastructure → Domain
- Tests → すべて

## 🔄 CI/CD

### GitHub Actions

#### CI ワークフロー
- トリガー: push/PR to main, development
- ステップ:
  1. .NET 10.0 セットアップ
  2. ツール復元
  3. ビルド
  4. テスト
  5. フォーマットチェック
  6. リント

#### Deploy ワークフロー
- トリガー: push to main
- ステップ: Heroku へ自動デプロイ

## 📚 参照ドキュメント

### プロジェクトドキュメント
- [要件定義](../docs/requirements/requirements_definition.md)
- [アーキテクチャ設計](../docs/design/architecture.md)
- [ドメインモデル設計](../docs/design/domain_model.md)
- [インフラ設計](../docs/design/architecture_infrastructure.md)
- [技術スタック](../docs/design/tech_stack.md)
- [テスト戦略](../docs/design/test_strategy.md)

### リリース計画
- [リリース計画](../docs/development/release_plan.md)
- [イテレーション 0 計画](../docs/development/iteration_plan-0.md)

### F# 学習教材
- [F# TDD 入門 第1部](../docs/reference/テスト駆動開発から始めるFSharp入門1.md)
- [F# TDD 入門 第2部](../docs/reference/テスト駆動開発から始めるFSharp入門2.md)
- [F# TDD 入門 第3部](../docs/reference/テスト駆動開発から始めるFSharp入門3.md)
- [F# TDD 入門 第4部](../docs/reference/テスト駆動開発から始めるFSharp入門4.md)

## 🤝 コントリビューション

プロジェクトへの貢献を歓迎します。

### 開発ワークフロー

1. フィーチャーブランチを作成
2. 変更を実装
3. テストを追加・実行
4. フォーマットチェック: `dotnet cake --target=FormatCheck`
5. プルリクエストを作成

### コミット規約

Conventional Commits に準拠：

```
feat: 新機能追加
fix: バグ修正
docs: ドキュメント更新
style: フォーマット変更
refactor: リファクタリング
test: テスト追加・修正
chore: ビルド・ツール関連
```

## 📄 ライセンス

このプロジェクトは学習・研究目的のケーススタディです。

## 🙏 謝辞

- [Domain Modeling Made Functional](https://pragprog.com/titles/swdddf/domain-modeling-made-functional/) by Scott Wlaschin
- F# コミュニティ
