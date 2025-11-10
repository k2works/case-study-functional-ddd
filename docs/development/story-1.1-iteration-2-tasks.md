# Story 1.1: 基本的な注文受付 - イテレーション 2 タスク分解

## 概要

イテレーション 1 で完了した設計を基に、Story 1.1 の完全な実装を行います。

## Story 1.1 受け入れ基準（再掲）

- [x] ドメインモデルの型定義が設計されている（イテレーション 1 完了）
- [x] 制約付き型（String50, EmailAddress など）のサンプル実装が完了（イテレーション 1 完了）
- [x] ワークフロー関数のシグネチャが定義されている（既存ドキュメント）
- [x] タスク分解が完了している（このドキュメント）
- [x] 設計ドキュメントが作成されている（domain_model.md）

## イテレーション 1 での成果

### 完了事項

1. **ドメインモデル設計**: `docs/design/domain_model.md` に詳細設計完了
2. **制約付き型サンプル実装**: `OrderTaking.Domain/ConstrainedTypes.fs` 作成
   - String50, EmailAddress, ZipCode
   - WidgetCode, GizmoCode, ProductCode
   - UnitQuantity, KilogramQuantity, OrderQuantity
   - Price, BillingAmount
   - OrderId, OrderLineId
3. **基本テスト**: `OrderTaking.Tests/ConstrainedTypesTests.fs` 作成（29 テスト）

### 残課題

- 制約付き型のテスト修正（FsUnit 互換性問題）
- 複合値オブジェクトの実装（PersonalName, CustomerInfo, Address）
- エンティティの実装（UnvalidatedOrder, ValidatedOrder, PricedOrder）
- ドメインサービスの実装（検証、価格計算、確認）
- ワークフローの実装（PlaceOrder）
- 統合テストの実装

## イテレーション 2 タスク分解

### Phase 1: 制約付き型の完成とテスト修正 (8h)

#### T1.1: 制約付き型のテスト修正 (2h)
**優先度**: 最高
**担当**: 開発者 A

**タスク**:
- FsUnit の `contain` 問題を修正
- すべてのテストが成功することを確認
- テストカバレッジ 100% を達成

**受け入れ基準**:
- [ ] 29 個のテストすべてが成功する
- [ ] ConstrainedTypes.fs のカバレッジが 100% である

#### T1.2: 追加の制約付き型の実装 (3h)
**優先度**: 高
**担当**: 開発者 A

**タスク**:
- 欠落している型の実装
  - String100（顧客名などに使用）
  - String255（住所行などに使用）
- バリデーション強化
  - EmailAddress: 正規表現による厳密な検証
  - ZipCode: 国別フォーマット対応（オプション）

**受け入れ基準**:
- [ ] String100, String255 が実装されている
- [ ] EmailAddress のバリデーションが強化されている
- [ ] すべての新規型にテストがある

#### T1.3: プロパティベーステストの追加 (3h)
**優先度**: 中
**担当**: 開発者 B

**タスク**:
- FsCheck を使用したプロパティベーステスト追加
- ラウンドトリップテスト（create → value → create）
- 境界値テスト

**受け入れ基準**:
- [ ] 各制約付き型に 2 つ以上のプロパティテストがある
- [ ] すべてのプロパティテストが成功する

### Phase 2: 複合値オブジェクトの実装 (6h)

#### T2.1: PersonalName と CustomerInfo の実装 (2h)
**優先度**: 高
**担当**: 開発者 A

**タスク**:
- PersonalName レコード型の実装
  - FirstName: String50
  - LastName: String50
- CustomerInfo レコード型の実装
  - Name: PersonalName
  - EmailAddress: EmailAddress
- 作成関数とテストの実装

**受け入れ基準**:
- [ ] PersonalName レコード型が実装されている
- [ ] CustomerInfo レコード型が実装されている
- [ ] 作成関数にバリデーションがある
- [ ] テストカバレッジ 100% を達成

#### T2.2: Address の実装 (2h)
**優先度**: 高
**担当**: 開発者 A

**タスク**:
- Address レコード型の実装
  - AddressLine1: String50
  - AddressLine2: String50 option
  - AddressLine3: String50 option
  - AddressLine4: String50 option
  - City: String50
  - ZipCode: ZipCode
- 作成関数とテストの実装

**受け入れ基準**:
- [ ] Address レコード型が実装されている
- [ ] option 型の適切な処理がある
- [ ] テストカバレッジ 100% を達成

#### T2.3: UnvalidatedOrder 型の実装 (2h)
**優先度**: 高
**担当**: 開発者 B

**タスク**:
- UnvalidatedOrder レコード型の実装
  - OrderId: string（未検証）
  - CustomerInfo: UnvalidatedCustomerInfo
  - ShippingAddress: UnvalidatedAddress
  - BillingAddress: UnvalidatedAddress
  - Lines: UnvalidatedOrderLine list
- Unvalidated 型の定義（string ベース）

**受け入れ基準**:
- [ ] UnvalidatedOrder レコード型が実装されている
- [ ] Unvalidated 系の型が定義されている
- [ ] JSON デシリアライゼーション対応

### Phase 3: エンティティとドメインサービスの実装 (12h)

#### T3.1: ValidatedOrder と ValidatedOrderLine の実装 (3h)
**優先度**: 高
**担当**: 開発者 A

**タスク**:
- ValidatedOrderLine レコード型の実装
  - OrderLineId: OrderLineId
  - ProductCode: ProductCode
  - Quantity: OrderQuantity
- ValidatedOrder レコード型の実装
  - OrderId: OrderId
  - CustomerInfo: CustomerInfo
  - ShippingAddress: Address
  - BillingAddress: Address
  - Lines: ValidatedOrderLine list

**受け入れ基準**:
- [ ] ValidatedOrderLine レコード型が実装されている
- [ ] ValidatedOrder レコード型が実装されている
- [ ] 型安全性が保証されている

#### T3.2: PricedOrder と PricedOrderLine の実装 (2h)
**優先度**: 高
**担当**: 開発者 A

**タスク**:
- PricedOrderLine レコード型の実装
  - OrderLineId: OrderLineId
  - ProductCode: ProductCode
  - Quantity: OrderQuantity
  - LinePrice: Price
- PricedOrder レコード型の実装
  - OrderId: OrderId
  - CustomerInfo: CustomerInfo
  - ShippingAddress: Address
  - BillingAddress: Address
  - AmountToBill: BillingAmount
  - Lines: PricedOrderLine list

**受け入れ基準**:
- [ ] PricedOrderLine レコード型が実装されている
- [ ] PricedOrder レコード型が実装されている
- [ ] 集約ルートとしての整合性ルールが実装されている

#### T3.3: 検証サービスの実装 (3h)
**優先度**: 高
**担当**: 開発者 B

**タスク**:
- CheckProductCodeExists 関数型の実装
- CheckAddressExists 関数型の実装
- ValidateOrder サービスの実装
  - 顧客情報の検証
  - 住所の検証（並列実行）
  - 注文明細の検証

**受け入れ基準**:
- [ ] CheckProductCodeExists が実装されている
- [ ] CheckAddressExists が実装されている（非同期）
- [ ] ValidateOrder が実装されている
- [ ] エラーハンドリングが適切である

#### T3.4: 価格計算サービスの実装 (2h)
**優先度**: 高
**担当**: 開発者 B

**タスク**:
- GetProductPrice 関数型の実装
- PriceOrder サービスの実装
  - 各明細の価格計算
  - 合計金額の計算
  - AmountToBill の設定

**受け入れ基準**:
- [ ] GetProductPrice が実装されている
- [ ] PriceOrder が実装されている
- [ ] 価格計算が正確である

#### T3.5: 確認サービスの実装 (2h)
**優先度**: 中
**担当**: 開発者 B

**タスク**:
- CreateOrderAcknowledgmentLetter 関数の実装
- SendOrderAcknowledgment 関数の実装
- AcknowledgeOrder サービスの実装

**受け入れ基準**:
- [ ] CreateOrderAcknowledgmentLetter が実装されている
- [ ] SendOrderAcknowledgment が実装されている
- [ ] AcknowledgeOrder が実装されている
- [ ] 送信失敗時も処理を継続する

### Phase 4: ワークフロー実装 (8h)

#### T4.1: PlaceOrder ワークフローの骨格実装 (3h)
**優先度**: 最高
**担当**: 開発者 A

**タスク**:
- PlaceOrder 関数型の実装
  - 型シグネチャ: `UnvalidatedOrder -> AsyncResult<PlaceOrderEvent list, PlaceOrderError>`
- 依存関係注入の実装
  - CheckProductCodeExists
  - CheckAddressExists
  - GetProductPrice
  - CreateOrderAcknowledgmentLetter
  - SendOrderAcknowledgment

**受け入れ基準**:
- [ ] PlaceOrder 関数型が実装されている
- [ ] 依存関係注入が機能している
- [ ] 型安全性が保証されている

#### T4.2: ワークフローフェーズの実装 (3h)
**優先度**: 最高
**担当**: 開発者 A

**タスク**:
- 検証フェーズの実装
- 価格計算フェーズの実装
- 確認フェーズの実装
- エラー変換の実装

**受け入れ基準**:
- [ ] 3 つのフェーズが実装されている
- [ ] エラーハンドリングが適切である
- [ ] AsyncResult が正しく使用されている

#### T4.3: ドメインイベントの実装 (2h)
**優先度**: 高
**担当**: 開発者 B

**タスク**:
- OrderPlaced イベント型の実装
- BillableOrderPlaced イベント型の実装
- OrderAcknowledgmentSent イベント型の実装
- PlaceOrderEvent 判別可能共用体の実装
- CreateEvents 関数の実装

**受け入れ基準**:
- [ ] 3 つのイベント型が実装されている
- [ ] CreateEvents が実装されている
- [ ] イベント生成ロジックが正確である

### Phase 5: インフラストラクチャとアダプター (6h)

#### T5.1: ダミーアダプターの実装 (2h)
**優先度**: 高
**担当**: 開発者 B

**タスク**:
- CheckProductCodeExists のダミー実装
- CheckAddressExists のダミー実装
- GetProductPrice のダミー実装
- 確認サービスのダミー実装

**受け入れ基準**:
- [ ] すべてのダミーアダプターが実装されている
- [ ] テストで使用できる

#### T5.2: 依存関係コンテナの実装 (2h)
**優先度**: 高
**担当**: 開発者 A

**タスク**:
- DependencyContainer モジュールの実装
- 依存関係の組み立て
- 設定ファイルからの読み込み（オプション）

**受け入れ基準**:
- [ ] DependencyContainer が実装されている
- [ ] すべての依存関係が解決される

#### T5.3: JSON シリアライゼーションの実装 (2h)
**優先度**: 中
**担当**: 開発者 B

**タスク**:
- UnvalidatedOrder の JSON デシリアライゼーション
- PlaceOrderEvent の JSON シリアライゼーション
- System.Text.Json または Newtonsoft.Json の使用

**受け入れ基準**:
- [ ] JSON シリアライゼーションが機能する
- [ ] F# 判別可能共用体が正しく処理される

### Phase 6: 統合テストとエンドツーエンドテスト (8h)

#### T6.1: ワークフロー統合テスト (3h)
**優先度**: 高
**担当**: 開発者 A

**タスク**:
- 正常系テストの実装
  - 有効な注文 → 3 つのイベント生成
- 異常系テストの実装
  - 無効な顧客情報
  - 無効な商品コード
  - 無効な住所

**受け入れ基準**:
- [ ] 正常系テストが成功する
- [ ] 異常系テストがすべて成功する
- [ ] エラーメッセージが適切である

#### T6.2: エンドツーエンドテスト (3h)
**優先度**: 中
**担当**: 開発者 B

**タスク**:
- API 経由のテスト実装（WebApi プロジェクト連携）
- ダミーアダプターを使用したテスト
- 実データを使用したテスト

**受け入れ基準**:
- [ ] API 経由のテストが成功する
- [ ] 実データテストが成功する

#### T6.3: パフォーマンステスト (2h)
**優先度**: 低
**担当**: 開発者 B

**タスク**:
- 注文処理時間の計測
- 並列処理のテスト
- メモリ使用量の計測

**受け入れ基準**:
- [ ] 1 注文あたり < 100ms
- [ ] メモリリークなし

### Phase 7: ドキュメント整備とレビュー (4h)

#### T7.1: API ドキュメントの作成 (2h)
**優先度**: 中
**担当**: 開発者 A

**タスク**:
- XML ドキュメントコメントの追加
- 使用例の追加
- README の更新

**受け入れ基準**:
- [ ] すべての公開関数に XML コメントがある
- [ ] 使用例が追加されている

#### T7.2: コードレビューと改善 (2h)
**優先度**: 高
**担当**: 開発者 A, B

**タスク**:
- コードレビュー実施
- FSharpLint 警告の解決
- Fantomas によるフォーマット
- パフォーマンス改善

**受け入れ基準**:
- [ ] FSharpLint 警告 0 件
- [ ] Fantomas フォーマット違反 0 件
- [ ] コードレビュー完了

## 見積もりサマリー

| Phase | タスク数 | 見積時間 | 優先度 |
|-------|----------|----------|--------|
| Phase 1: 制約付き型の完成 | 3 | 8h | 最高 |
| Phase 2: 複合値オブジェクト | 3 | 6h | 高 |
| Phase 3: エンティティとサービス | 5 | 12h | 高 |
| Phase 4: ワークフロー | 3 | 8h | 最高 |
| Phase 5: インフラストラクチャ | 3 | 6h | 高 |
| Phase 6: 統合テスト | 3 | 8h | 高 |
| Phase 7: ドキュメント | 2 | 4h | 中 |
| **合計** | **22** | **52h** | - |

**チーム理想時間**: 2 名 × 5 時間/日 × 10 日 = **100 理想時間**
**バッファ**: 100h - 52h = **48 時間 (48%)**

## リスクと依存関係

### 技術的リスク

| リスク | 影響度 | 確率 | 軽減策 |
|--------|--------|------|--------|
| FsUnit 互換性問題 | 中 | 高 | 代替アサーションライブラリの使用 |
| 非同期処理の複雑性 | 中 | 中 | AsyncResult パターンの徹底 |
| JSON シリアライゼーション | 低 | 中 | テストによる早期検証 |

### 依存関係

- Phase 2 → Phase 1 完了後
- Phase 3 → Phase 2 完了後
- Phase 4 → Phase 3 完了後
- Phase 5 → Phase 4 完了後（並行可能）
- Phase 6 → Phase 5 完了後
- Phase 7 → Phase 6 完了後（一部並行可能）

## Definition of Done (Story 1.1)

- [ ] すべてのコードが実装されている
- [ ] すべてのテストが成功する（単体・統合・E2E）
- [ ] テストカバレッジが 80% 以上である
- [ ] FSharpLint 警告が 0 件である
- [ ] Fantomas フォーマット違反が 0 件である
- [ ] API ドキュメントが完成している
- [ ] コードレビューが完了している
- [ ] CI パイプラインがすべて成功している
- [ ] デプロイが成功している

## 参考資料

- [ドメインモデル設計](../design/domain_model.md)
- [イテレーション 1 計画](./iteration_plan-1.md)
- [Domain Modeling Made Functional](https://pragprog.com/titles/swdddf/domain-modeling-made-functional/)

---

**作成日**: 2025-11-10
**作成者**: Claude (AI Assistant)
**バージョン**: 1.0
