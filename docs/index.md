# 注文受付システム (OrderTaking) - 関数型ドメイン駆動設計ケーススタディ

## 概要

本プロジェクトは、F# の関数型プログラミングとドメイン駆動設計の原則を適用した注文受付システムです。「Domain Modeling Made Functional」の実践例として、型安全で保守性の高いソフトウェア開発のアプローチを学習できます。

## 🎯 プロジェクト状況（2025-11-19）

### イテレーション 3 完了（100%）

**達成率**: Phase 4, 5, 6, 8 完了（全 148 テスト成功 100%）

**主な成果**:
- ✅ **Dapper + FluentMigrator 基盤構築完了**（Phase 4-5）
  - FluentMigrator によるマイグレーション管理（3 マイグレーション、6 テスト）
  - Dapper による OrderRepository 実装（386 行、7 統合テスト）
  - SQLite (開発) / PostgreSQL (本番) 両対応
- ✅ **Story 1.4「注文の確定処理」完全実装**（Phase 6）
  - PlaceOrder ワークフローのデータベース統合
  - Web API のデータベース統合（環境別マイグレーション実行）
  - E2E テスト完全対応（139 → 148 テスト）
- ✅ **API 改善**（Phase 8）
  - Swagger レスポンススキーマ、構造化エラーレスポンス
  - README.md に包括的な API ドキュメント追加
- ✅ コード品質: Fantomas、pre-commit フック 100% 合格、警告 0・エラー 0

**実装完了**:
- Migration001-003（Orders/OrderLines テーブル）
- OrderRepository.fs（IOrderRepository インターフェース、Dapper 実装）
- PlaceOrder ワークフローのデータベース統合（DatabaseError 追加、Async 化）
- TestWebApplicationFactory（テスト環境のデータベース分離）
- ResponseTypes.fs（4 種類のレスポンス型定義）
- ErrorResponseHelper モジュール（構造化エラーレスポンス）

**技術基盤**:
- データベース: SQLite (開発)、PostgreSQL (本番)
- マイグレーション: FluentMigrator 6.2.0
- データアクセス: Dapper 2.1.35
- テスト: 148 テスト（単体 + プロパティ + 統合 + データベース + API）

**開発プロセス**:
- イテレーション計画: [iteration_plan-3.md](./development/iteration_plan-3.md)
- レトロスペクティブ: [retrospective-3.md](./development/retrospective-3.md)
- 改善アクション: [iteration_4_improvements.md](./development/iteration_4_improvements.md) - 5 件の改善アクション定義

### イテレーション 2 完了（100%）

**達成率**: 全 29 タスク完了（100%）、全 124 テスト成功（100%）

**主な成果**:
- ✅ Story 1.1「基本的な注文受付」完全実装
- ✅ Phase 1-10 すべて完了（ドメイン層 + インフラ層 + Web API）
- ✅ テストカバレッジ: 80%+ 達成（124 テスト: 単体 + プロパティ + 統合 + API）
- ✅ Web API with Swagger UI 公開（プロダクション対応）
- ✅ コード品質: Fantomas、FSharpLint、pre-commit フック統合
- ✅ 見積もり精度: 100%（イテレーション平均 98.7%）

**実装完了**:
- 制約付き型 13 種類、複合値オブジェクト 5 種類、エンティティ 3 種類
- ドメインサービス 4 種類、PlaceOrder ワークフロー
- インフラストラクチャレイヤー、JSON シリアライゼーション（F# 対応）
- POST /api/orders エンドポイント、Swagger UI

**開発プロセス**:
- イテレーション計画: [iteration_plan-2.md](./development/iteration_plan-2.md)
- レトロスペクティブ: [retrospective-2.md](./development/retrospective-2.md)
- KPT 分析と 7 件の改善アクション定義

### イテレーション 1 完了（100%）

**達成率**: 計画時間 38.25h に対し実績 37.25h（97.4% の見積もり精度）

**主な成果**:
- ✅ 制約付き型（11種類）の設計・実装完了
- ✅ テストカバレッジ: 29 テスト（日本語表記）すべて成功
- ✅ コード品質: Fantomas フォーマット準拠、FSharpLint 警告ゼロ
- ✅ ドキュメント整備: プロセス、品質ガイド、レトロスペクティブ完備

**開発プロセス**:
- イテレーション計画: [iteration_plan-1.md](./development/iteration_plan-1.md)
- レトロスペクティブ: [retrospective-1.md](./development/retrospective-1.md)
- 13 件の改善アクションを特定（イテレーション 2 で実施）

## まず読むべきドキュメント

### 🎯 プロジェクト理解
- [要件定義](./requirements/requirements_definition.md) - システム全体の要件と価値提案。プロジェクトの基盤となる重要なドキュメント
- [ビジネスユースケース](./requirements/business_usecase.md) - ビジネスプロセスレベルでの注文受付業務フロー
- [システムユースケース](./requirements/system_usecase.md) - システムレベルでの機能要件と技術仕様
- [ユーザーストーリー](./requirements/user_story.md) - アジャイル開発のためのユーザー視点の要件

### 🏗️ アーキテクチャ理解
- [アーキテクチャ設計](./design/architecture.md) - ヘキサゴナルアーキテクチャとF#モジュール構成の詳細
- [ドメインモデル設計](./design/domain_model.md) - 関数型ドメインモデリングの実践的実装
- [技術スタック](./design/tech_stack.md) - F# 9.0、ASP.NET Core、Entity Framework Coreによる技術構成
- [データモデル設計](./design/data-model.md) - ドメインモデルからリレーショナルDBへのマッピング
- [インフラ設計](./design/architecture_infrastructure.md) - Heroku + InMemory DB によるモノリシック構成

### 💻 実装ガイド
- [F# API構築ガイド](./reference/F#API構築ガイド.md) - ASP.NET Core最小APIとF#による実装手順
- [テスト戦略](./design/test_strategy.md) - TDD、プロパティベーステスト、統合テストの戦略

## 学習パス

### 🔰 初学者向け
1. [よいソフトウェアとは](./reference/よいソフトウェアとは.md) - 開発理念の理解
2. [F# TDD入門 第1部](./reference/テスト駆動開発から始めるFSharp入門1.md) - F#基礎とTDD
3. [要件定義](./requirements/requirements_definition.md) - システム要件の理解

### 🎯 実装者向け
1. [アーキテクチャ設計](./design/architecture.md) - 全体アーキテクチャ
2. [ドメインモデル設計](./design/domain_model.md) - ドメインロジック実装
3. [F# API構築ガイド](./reference/F#API構築ガイド.md) - 具体的実装手順

### 📚 F# 学習教材
- [F# TDD入門 第1部](./reference/テスト駆動開発から始めるFSharp入門1.md) - 基礎概念とセットアップ
- [F# TDD入門 第2部](./reference/テスト駆動開発から始めるFSharp入門2.md) - 型システムとドメインモデリング
- [F# TDD入門 第3部](./reference/テスト駆動開発から始めるFSharp入門3.md) - 関数型プログラミングパターン
- [F# TDD入門 第4部](./reference/テスト駆動開発から始めるFSharp入門4.md) - 実践的なWebAPI実装

## プロジェクト特徴

- **言語**: F# 9.0 (.NET 9.0)
- **アーキテクチャ**: ポートとアダプター（ヘキサゴナルアーキテクチャ）
- **ドメイン**: 顧客からの注文受付、検証、価格計算、確定処理
- **設計手法**: ドメイン駆動設計 + 関数型プログラミング
- **開発手法**: テスト駆動開発（TDD）

## プロジェクト管理

### 📅 リリース計画

**計画期間**: 6.5 ヶ月（26 週間、13 イテレーション）
**総ストーリーポイント**: 87 SP
**計画ベロシティ**: 18 SP / イテレーション

#### マイルストーン

| リリース | 目標日 | 主要機能 | 状況 |
|---------|--------|---------|------|
| **Iteration 0** | 2025-01-19 | 環境構築 | ✅ 完了 |
| **Iteration 1** | 2025-02-02 | 制約付き型、開発基盤 | ✅ 完了（100%） |
| **Release 1.0 (MVP)** | 2025-03-15 | 注文受付・検証・価格計算・確定 | 🔄 進行中 |
| **Release 1.1** | 2025-04-26 | 部門連携通知、イベントストア | 📋 計画中 |
| **Release 1.2** | 2025-06-07 | エラーハンドリング強化 | 📋 計画中 |
| **Release 2.0** | 2025-07-05 | 運用機能、ステータス照会 API | 📋 計画中 |

**詳細**: [リリース計画](./development/release_plan.md)

#### 主要機能リリース計画

**Release 1.0 - MVP** (6 イテレーション、47 SP):
- ✅ Story 1.1: 基本的な注文受付（8 SP）- Iteration 1-2（完了）
- ✅ Story 1.2: 注文内容の検証（13 SP）- Iteration 1-2（完了）
- ✅ Story 1.3: 価格の自動計算（5 SP）- Iteration 2（完了）
- ✅ Story 1.4: 注文の確定処理（8 SP）- Iteration 3（完了）
- 📋 Story 3.1: 商品コードの管理（3 SP）- Iteration 4
- 📋 Story 4.1: 注文受付 API（5 SP）- Iteration 5（一部完了：Web API 実装済み）
- 📋 Story 5.1: 顧客への確認メール（5 SP）- Iteration 5-6

### 📋 開発プロセス
- [イテレーション計画](./development/) - スプリント計画とタスク管理
  - [Iteration 0 計画](./development/iteration_plan-0.md) - 完了（環境構築）
  - [Iteration 1 計画](./development/iteration_plan-1.md) - 完了（100%、97.4% 精度）
  - [Iteration 2 計画](./development/iteration_plan-2.md) - 完了（100%、全 29 タスク）
  - [Iteration 3 計画](./development/iteration_plan-3.md) - 完了（100%、Phase 4, 5, 6, 8、Story 1.4 完了）
- [レトロスペクティブ](./development/) - イテレーション振り返りと改善
  - [Retrospective 0](./development/retrospective-0.md) - 環境構築の振り返り
  - [Retrospective 1](./development/retrospective-1.md) - KPT 分析と 13 件の改善アクション
  - [Retrospective 2](./development/retrospective-2.md) - KPT 分析と 7 件の改善アクション（Story 1.1 完了）
  - [Retrospective 3](./development/retrospective-3.md) - KPT 分析と 5 件の改善アクション（Phase 4, 5, 6, 8 完了、Story 1.4 完了）
- [デイリースタンドアップ記録](./operation/process/standup-logs/) - 日次進捗と課題管理
  - [2025年1月](./operation/process/standup-logs/2025-01.md)

### 📐 設計決定
- [アーキテクチャ決定ログ](./adr) - 重要な技術決定の記録と理由
- [開発日誌](./journal) - 開発の進捗と学習記録

### ⚙️ 運用ドキュメント
- [環境セットアップガイド](./operation/environment-setup-guide.md) - 開発環境構築手順
- [トラブルシューティングガイド](./operation/troubleshooting-guide.md) - よくある問題と解決方法
- [GitHub Actions ガイド](./operation/github-actions-guide.md) - CI/CD パイプライン設定
- [Heroku デプロイガイド](./operation/heroku-deploy-guide.md) - 本番環境へのデプロイ手順
- [プロセスガイド](./operation/process/) - 開発プロセスとワークフロー
  - [デイリースタンドアップガイド](./operation/process/daily-standup-guide.md)
  - [時間記録ガイド](./operation/process/time-tracking-guide.md)
- [品質管理](./operation/quality/) - コード品質とテストカバレッジ
  - [カバレッジガイド](./operation/quality/coverage-guide.md)
  - [静的解析ガイド](./operation/quality/static-analysis-guide.md)
