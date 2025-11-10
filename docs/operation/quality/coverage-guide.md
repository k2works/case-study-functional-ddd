# カバレッジガイド

## 概要

プロジェクトでは **coverlet** を使用してコードカバレッジを測定します。カバレッジ目標は **80% 以上** です。

## coverlet とは

coverlet は .NET 向けのクロスプラットフォームなコードカバレッジツールです。

- XPlat Code Coverage 形式をサポート
- Cobertura XML レポート生成
- CI/CD パイプライン統合が容易

## カバレッジ測定方法

### 1. Cake タスク経由（推奨）

```bash
dotnet cake --target=Coverage
```

### 2. dotnet test 直接実行

```bash
dotnet test --collect:"XPlat Code Coverage" \
            --results-directory:./TestResults
```

### 3. VSCode タスク

1. `Ctrl + Shift + P` → `Tasks: Run Task`
2. `Cake: Coverage` を選択

## カバレッジレポート

### 生成場所

```
TestResults/
└── {GUID}/
    └── coverage.cobertura.xml
```

### レポート内容

**coverage.cobertura.xml** (XML 形式):
```xml
<coverage line-rate="0.85" branch-rate="0.92" ...>
  <packages>
    <package name="OrderTaking.Domain" line-rate="0.90" ...>
      <classes>
        <class name="OrderTaking.Domain.Types" ...>
          <lines>
            <line number="10" hits="5" branch="False" />
            <line number="11" hits="0" branch="False" />
          </lines>
        </class>
      </classes>
    </package>
  </packages>
</coverage>
```

## レポート可視化

### 方法 1: reportgenerator ツール

#### インストール

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

#### HTML レポート生成

```bash
reportgenerator \
  -reports:./TestResults/**/coverage.cobertura.xml \
  -targetdir:./TestResults/CoverageReport \
  -reporttypes:Html
```

#### レポート表示

```bash
# Windows
start ./TestResults/CoverageReport/index.html

# macOS
open ./TestResults/CoverageReport/index.html

# Linux
xdg-open ./TestResults/CoverageReport/index.html
```

### 方法 2: VSCode 拡張機能

**Coverage Gutters** 拡張機能を使用：

1. VSCode で Coverage Gutters をインストール
2. Cobertura XML ファイルを読み込み
3. エディタ上でカバレッジを色分け表示

## カバレッジメトリクス

### line-rate（行カバレッジ）

```
line-rate = lines-covered / lines-valid
```

**例**:
- lines-covered: 85 行
- lines-valid: 100 行
- **line-rate: 0.85 (85%)**

### branch-rate（分岐カバレッジ）

```
branch-rate = branches-covered / branches-valid
```

**例**:
- branches-covered: 18 分岐
- branches-valid: 20 分岐
- **branch-rate: 0.90 (90%)**

## カバレッジ目標

### プロジェクト全体

| メトリクス | 目標値 | 判定 |
|-----------|--------|------|
| **Line Coverage** | 80% 以上 | 必須 |
| **Branch Coverage** | 75% 以上 | 推奨 |

### レイヤー別

| レイヤー | Line Coverage 目標 |
|---------|-------------------|
| **Domain** | 90% 以上 |
| **Application** | 85% 以上 |
| **Infrastructure** | 70% 以上 |
| **WebApi** | 60% 以上 |

## イテレーション 1 の状況

### 現在のカバレッジ

| メトリクス | 実績 | 理由 |
|-----------|------|------|
| **Line Coverage** | 0% | プロダクションコード未実装 |
| **Branch Coverage** | N/A | プロダクションコード未実装 |

### 理由

イテレーション 1 は環境構築フェーズです：

- **既存コード**: サンプル Library.fs のみ（未テスト）
- **テストコード**: テストフレームワーク動作確認用のサンプルテスト
- **Story 1.1**: 設計のみ（実装はイテレーション 2）

### カバレッジレポート機能確認

✅ **動作確認完了**:
- Coverage タスク実行成功
- coverage.cobertura.xml 生成成功
- レポート形式正常

## カバレッジ向上戦略

### 1. ユニットテストの追加

**Domain レイヤー** (最優先):
```fsharp
// 制約付き型のテスト例
[<Fact>]
let ``String50.create should accept valid string`` () =
    let result = String50.create "Valid"
    result |> should be (ofCase <@ Result.Ok @>)

[<Fact>]
let ``String50.create should reject too long string`` () =
    let longString = String.replicate 51 "a"
    let result = String50.create longString
    result |> should be (ofCase <@ Result.Error @>)
```

### 2. プロパティベーステスト

**FsCheck を活用**:
```fsharp
[<Property>]
let ``EmailAddress round-trip`` (NonNull email) =
    match EmailAddress.create email with
    | Ok addr -> EmailAddress.value addr = email
    | Error _ -> true
```

### 3. 統合テスト

**Application レイヤー**:
```fsharp
[<Fact>]
let ``PlaceOrder workflow should process valid order`` () =
    // Arrange
    let validOrder = ...

    // Act
    let result = PlaceOrder.execute validOrder

    // Assert
    result |> should be (ofCase <@ Result.Ok @>)
```

## CI/CD 統合

### GitHub Actions

```yaml
- name: Run Tests with Coverage
  run: dotnet cake --target=Coverage

- name: Generate Coverage Report
  run: |
    dotnet tool install -g dotnet-reportgenerator-globaltool
    reportgenerator \
      -reports:./TestResults/**/coverage.cobertura.xml \
      -targetdir:./TestResults/CoverageReport \
      -reporttypes:Html;Cobertura

- name: Check Coverage Threshold
  run: |
    coverage=$(grep -oP 'line-rate="\K[0-9.]+' ./TestResults/**/coverage.cobertura.xml | head -1 | awk '{print $1 * 100}')
    if (( $(echo "$coverage < 80" | bc -l) )); then
      echo "Coverage $coverage% is below 80% threshold"
      exit 1
    fi
```

### カバレッジバッジ

README.md にバッジを追加（オプション）:
```markdown
[![Coverage](https://img.shields.io/badge/coverage-85%25-green.svg)](./TestResults/CoverageReport)
```

## ベストプラクティス

### 1. テスト駆動開発 (TDD)

```
Red → Green → Refactor
```

1. **Red**: 失敗するテストを書く
2. **Green**: テストを通す最小限のコードを書く
3. **Refactor**: コードを改善、テストは通ったまま

### 2. カバレッジ 100% を目指さない

- 100% は非現実的かつ不要
- 80% で十分な品質
- 重要なビジネスロジックを優先

### 3. テストしにくいコードを避ける

- 副作用を分離
- 依存性注入
- 純粋関数を優先

## トラブルシューティング

### Q: カバレッジが 0% のまま

A: 以下を確認：
1. テストがプロダクションコードを呼び出しているか
2. `dotnet test` が成功しているか
3. TestResults/ ディレクトリにファイルが生成されているか

### Q: カバレッジレポートが見つからない

A: `--results-directory` オプションで出力先を指定：
```bash
dotnet test --collect:"XPlat Code Coverage" \
            --results-directory:./TestResults
```

### Q: 一部のファイルがカバレッジ対象外

A: `coverlet.collector` の設定を確認。除外設定がないか確認。

## 参考資料

- [coverlet 公式ドキュメント](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator](https://github.com/danielpalme/ReportGenerator)
- [イテレーション 1 計画](../development/iteration_plan-1.md)
- [Cake ビルドスクリプト](../../app/backend/build.cake)

---

**作成日**: 2025-11-10
**最終更新**: 2025-11-10
**バージョン**: 1.0
