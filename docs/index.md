# 注文受付システム (OrderTaking) - 関数型ドメイン駆動設計ケーススタディ

## 概要

本プロジェクトは、F# の関数型プログラミングとドメイン駆動設計の原則を適用した注文受付システムです。「Domain Modeling Made Functional」の実践例として、型安全で保守性の高いソフトウェア開発のアプローチを学習できます。

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

- [アーキテクチャ決定ログ](./adr) - 重要な技術決定の記録と理由
- [開発日誌](./journal) - 開発の進捗と学習記録
- [運用ドキュメント](./operation) - システム運用とメンテナンス情報
