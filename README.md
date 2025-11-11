# 関数型ドメイン駆動設計ケーススタディ

## 概要

本プロジェクトは、F# の関数型プログラミングとドメイン駆動設計の原則を適用した注文受付システムです。「Domain Modeling Made Functional」の実践例として、型安全で保守性の高いソフトウェア開発のアプローチを学習できます。

### 目的

- 関数型プログラミングによるドメインモデリングの実践
- 型システムを活用したビジネスルールの表現
- Railway Oriented Programming によるエラーハンドリング
- イベント駆動アーキテクチャの実装
- テスト駆動開発（TDD）の実践

### システム特徴

- **ドメイン**: 顧客からの注文受付、検証、価格計算、確定処理
- **アーキテクチャ**: ポートとアダプター（ヘキサゴナルアーキテクチャ）
- **実装言語**: F# 9.0 (.NET 9.0)
- **Web フレームワーク**: ASP.NET Core 9.0（最小API）
- **データベース**: Entity Framework Core 9.0（InMemory）

### 前提条件

| ソフトウェア | バージョン | 用途 |
| :----------- | :--------- | :--- |
| .NET SDK | 9.0+ | アプリケーション開発・実行 |
| F# | 9.0+ | プログラミング言語 |
| Node.js | 18.0+ | ドキュメント生成・開発支援 |

## アーキテクチャ

### レイヤー構成

```
app/backend/
├── OrderTaking.Domain/
│   ├── Common.SimpleTypes.fs       # 基本制約型（String50, EmailAddress等）
│   ├── Common.CompoundTypes.fs     # 複合値オブジェクト（Address, CustomerInfo等）
│   ├── PlaceOrder.PublicTypes.fs   # パブリック型定義（API境界）
│   └── PlaceOrder.Implementation.fs # ビジネスワークフロー実装
├── OrderTaking.Application/        # アプリケーション層
├── OrderTaking.Infrastructure/     # インフラストラクチャ層
│   └── PlaceOrder.Api.fs           # HTTP API 層
├── OrderTaking.WebApi/             # Web API プロジェクト
│   └── PlaceOrder.Dto.fs           # データ転送オブジェクト
└── OrderTaking.Tests/              # 単体・統合テスト
```

### ドメインモデル

#### 注文処理フロー
1. **未検証注文** → **検証済み注文** → **価格付き注文** → **イベント生成**
2. 各段階で型安全性を保証し、不正な状態遷移を防止

#### 主要ドメインオブジェクト
- **商品コード**: Widget（W1234）、Gizmo（G123）の2タイプ
- **注文数量**: Unit（1-1000個）、Kilogram（0.05-100.00kg）
- **価格**: 制約付き金額型（0.0-1000.00）

## 構成

- [クイックスタート](#クイックスタート)
- [構築](#構築)
- [開発](#開発)
- [テスト](#テスト)
- [ドキュメント](#ドキュメント)

## クイックスタート

### 基本セットアップ

```bash
# プロジェクトのクローン
git clone <repository-url>
cd case-study-functional-ddd

# 依存関係のインストール
npm install

# 開発サーバーの起動
npm start
```

### F# プロジェクトの実行

```bash
# バックエンドディレクトリに移動
cd app/backend

# .NET プロジェクトのビルド
dotnet build

# テストの実行
dotnet test

# API サーバーの起動
dotnet run --project OrderTaking.WebApi/
```

## 構築

### プロジェクト初期化

```bash
# バックエンドディレクトリに移動
cd app/backend

# F# プロジェクトの作成
dotnet new classlib -lang F# -n OrderTaking.Domain
dotnet new classlib -lang F# -n OrderTaking.Application
dotnet new classlib -lang F# -n OrderTaking.Infrastructure
dotnet new web -lang F# -n OrderTaking.WebApi

# テストプロジェクトの作成
dotnet new xunit -lang F# -n OrderTaking.Tests

# プロジェクト参照の追加
dotnet add OrderTaking.Application reference OrderTaking.Domain
dotnet add OrderTaking.Infrastructure reference OrderTaking.Domain
dotnet add OrderTaking.WebApi reference OrderTaking.Domain
dotnet add OrderTaking.WebApi reference OrderTaking.Application
dotnet add OrderTaking.WebApi reference OrderTaking.Infrastructure
dotnet add OrderTaking.Tests reference OrderTaking.Domain
dotnet add OrderTaking.Tests reference OrderTaking.Application
dotnet add OrderTaking.Tests reference OrderTaking.Infrastructure
dotnet add OrderTaking.Tests reference OrderTaking.WebApi

# ソリューションファイルの作成
dotnet new sln -n OrderTaking
dotnet sln add OrderTaking.Domain/OrderTaking.Domain.fsproj
dotnet sln add OrderTaking.Application/OrderTaking.Application.fsproj
dotnet sln add OrderTaking.Infrastructure/OrderTaking.Infrastructure.fsproj
dotnet sln add OrderTaking.WebApi/OrderTaking.WebApi.fsproj
dotnet sln add OrderTaking.Tests/OrderTaking.Tests.fsproj
```

### 開発環境の準備

```bash
# Claude MCP 設定
claude mcp add github npx @modelcontextprotocol/server-github -e GITHUB_PERSONAL_ACCESS_TOKEN=xxxxxxxxxxxxxxx
claude mcp add --transport http byterover-mcp --scope user https://mcp.byterover.dev/v2/mcp
claude mcp add github npx -y @modelcontextprotocol/server-github -s project
```

### Git Hooks のセットアップ

コミット前に品質チェックを自動実行する Git hooks を設定します：

```bash
# Linux/Mac
bash scripts/setup-hooks.sh

# Windows (PowerShell)
powershell scripts/setup-hooks.ps1
```

Pre-commit フックは以下をチェックします：
1. **Format Check**: Fantomas によるコードフォーマット検証
2. **Build**: 警告ゼロでのビルド成功
3. **Tests**: 全テストの成功

フックをスキップする場合：
```bash
git commit --no-verify
```

### 開発ツール

- **IDE**: Visual Studio Code + Ionide / JetBrains Rider
- **F# 対話環境**: F# Interactive（FSI）
- **静的解析**: FSharpLint
- **フォーマッター**: Fantomas

### 参考実装について

現在、参考実装は `docs/reference/DomainModelingMadeFunctional/` 以下にあります：

```bash
docs/reference/DomainModelingMadeFunctional/
├── OrderTaking/                    # 基本実装
└── OrderTakingEvolved/            # 進化版実装
```

これらを参考に `app/backend/src/OrderTaking/` に実際のアプリケーションを構築します。

**[⬆ back to top](#構成)**

## 開発

### 開発方針

本プロジェクトは「よいソフトウェア」の原則に従います：
- **変更を楽に安全にできて役に立つソフトウェア**を目標
- テスト駆動開発（TDD）の実践
- 継続的リファクタリング
- 小さく頻繁なコミット

### コーディング規約

```fsharp
// 制約付き型の例
type String50 = private String50 of string

module String50 =
    let create fieldName str =
        if String.IsNullOrEmpty(str) then
            Error $"{fieldName} must not be null or empty"
        elif str.Length > 50 then
            Error $"{fieldName} must not be more than 50 chars"
        else
            Ok (String50 str)
```

### ワークフロー例

```fsharp
// 注文受付ワークフロー
let placeOrder : PlaceOrder =
    fun checkProductExists checkAddressExists getProductPrice unvalidatedOrder ->
        asyncResult {
            let! validatedOrder = validateOrder checkProductExists checkAddressExists unvalidatedOrder
            let! pricedOrder = priceOrder getProductPrice validatedOrder
            let events = createEvents pricedOrder
            return events
        }
```

**[⬆ back to top](#構成)**

## テスト

### テスト戦略

- **単体テスト**: 純粋関数とドメインロジック
- **統合テスト**: ワークフロー全体の動作確認
- **プロパティベーステスト**: 型の制約確認

### テスト実行

```bash
# バックエンドディレクトリで作業
cd app/backend

# 全テストの実行
dotnet test

# 特定テストの実行
dotnet test --filter "TestCategory=Unit"

# カバレッジレポート生成
dotnet test --collect:"XPlat Code Coverage"
```

**[⬆ back to top](#構成)**

## ドキュメント

### ドキュメント構成

```bash
docs/
├── requirements/          # 要件定義
│   ├── requirements_definition.md
│   ├── business_usecase.md
│   ├── system_usecase.md
│   └── user_story.md
├── design/               # 設計書
│   ├── architecture.md
│   ├── domain_model.md
│   └── tech_stack.md
└── reference/            # 参考資料
    ├── よいソフトウェアとは.md
    ├── 開発ガイド.md
    └── テスト駆動開発からはじめるFSharp入門*.md
```

### ドキュメント生成

```bash
# ドキュメントサーバーの起動
npm run docs:serve

# ドキュメントのビルド
npm run docs:build

# ジャーナル生成
npm run journal
```

**[⬆ back to top](#構成)**

## 学習ポイント

### 関数型プログラミング

- **不変性**: データの予期しない変更を防止
- **純粋関数**: 副作用のない関数による予測可能な動作
- **関数合成**: 小さな関数の組み合わせによる複雑なロジック構築

### ドメイン駆動設計

- **ユビキタス言語**: ビジネス用語をそのままコードに反映
- **境界付けられたコンテキスト**: 明確なドメイン境界の定義
- **集約**: データ整合性を保証する境界の設計

### 型安全性

- **Make Illegal States Unrepresentable**: 不正な状態を型で排除
- **Parse, Don't Validate**: 検証済みデータを型で保証
- **Railway Oriented Programming**: Result型による安全なエラーハンドリング

## 参照

- **書籍**: [Domain Modeling Made Functional](https://pragprog.com/titles/swdddf/domain-modeling-made-functional/)
- **F# ドキュメント**: [Microsoft F# Guide](https://docs.microsoft.com/en-us/dotnet/fsharp/)
- **ドメイン駆動設計**: [Domain-Driven Design](https://domainlanguage.com/ddd/)
- **関数型プログラミング**: [F# for Fun and Profit](https://fsharpforfunandprofit.com/)
