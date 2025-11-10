# 静的解析ガイド

## 概要

プロジェクトでは以下の静的解析ツールを使用して、コード品質を自動的にチェックします：

1. **FSharpLint**: F# コードの品質チェック
2. **Fantomas**: F# コードのフォーマットチェック

## FSharpLint

### 概要

FSharpLint は F# コードの静的解析ツールで、コーディング規約違反や潜在的な問題を検出します。

### 設定ファイル

**`fsharplint.json`** (プロジェクトルート):

```json
{
  "ignoreFiles": ["**/obj/**", "**/bin/**"],
  "analysers": {
    "Hints": {
      "enabled": true
    },
    "Typography": {
      "enabled": true
    },
    "NestedStatements": {
      "enabled": true,
      "depth": 7
    }
  }
}
```

### 実行方法

#### 1. スタンドアロン実行

```bash
dotnet fsharplint lint OrderTaking.sln
```

#### 2. Cake タスク経由

```bash
# Lint タスク単独
dotnet cake --target=Lint

# Quality タスク（FormatCheck + Lint）
dotnet cake --target=Quality
```

#### 3. VSCode タスク

1. `Ctrl + Shift + P` → `Tasks: Run Task`
2. `Cake: Lint` または `Cake: Quality` を選択

### 実行結果

**成功時**:
```
========== Summary: 0 warnings ==========
```

**警告検出時**:
```
========== Linting C:\...\Library.fs ==========
Warning: [Hint] Line 10: Consider using 'List.isEmpty' instead of pattern matching
========== Finished: 1 warnings ==========
```

### 主なチェック項目

| カテゴリ | 内容 |
|---------|------|
| **Hints** | より良い書き方の提案 |
| **Typography** | スペース、インデント、命名規則 |
| **NestedStatements** | ネストの深さチェック (最大 7) |
| **Naming** | 変数名、関数名の規約チェック |
| **RaiseWithTooManyArguments** | raise の引数チェック |

## Fantomas

### 概要

Fantomas は F# コードのフォーマッターで、一貫したコードスタイルを維持します。

### 設定ファイル

#### `.fantomasignore` (プロジェクトルート)

```
**/obj/**
**/bin/**
**/.vs/**
**/.idea/**
**/.vscode/**
**/packages/**
**/paket-files/**
```

#### `.editorconfig` (オプション)

Fantomas は `.editorconfig` の設定も尊重します。

### 実行方法

#### 1. フォーマットチェック

```bash
# スタンドアロン
dotnet fantomas --check .

# Cake タスク
dotnet cake --target=FormatCheck

# VSCode タスク
Ctrl + Shift + P → Tasks: Run Task → Cake: FormatCheck
```

#### 2. 自動フォーマット

```bash
# スタンドアロン
dotnet fantomas .

# Cake タスク
dotnet cake --target=Format

# VSCode タスク
Ctrl + Shift + P → Tasks: Run Task → Cake: Format
```

### 実行結果

**フォーマット不要時**:
```
（出力なし - 成功）
```

**フォーマット必要時**:
```
OrderTaking.Domain/Library.fs needs formatting
OrderTaking.Application/Library.fs needs formatting
```

### フォーマット規則

| 項目 | ルール |
|------|--------|
| **インデント** | スペース 4 つ |
| **行の最大長** | 120 文字（推奨） |
| **パイプライン** | 適切な改行と配置 |
| **空行** | 関数間に 1 行 |
| **トレイリングスペース** | 削除 |

## Cake 統合

### Quality タスク

FormatCheck と Lint を一括実行：

```bash
dotnet cake --target=Quality
```

**build.cake の定義**:
```csharp
Task("Quality")
    .Description("Run all quality checks (format check and lint).")
    .IsDependentOn("FormatCheck")
    .IsDependentOn("Lint");
```

### CI パイプライン統合

GitHub Actions で自動実行：

```yaml
- name: Run Quality Checks
  run: dotnet cake --target=Quality
```

## ベストプラクティス

### 1. コミット前に必ずチェック

```bash
# 推奨フロー
dotnet cake --target=Format    # 自動フォーマット
dotnet cake --target=Quality   # 品質チェック
git add .
git commit -m "..."
```

### 2. Pre-commit フック（オプション）

`.git/hooks/pre-commit`:
```bash
#!/bin/sh
dotnet cake --target=Quality
if [ $? -ne 0 ]; then
    echo "Quality check failed. Run 'dotnet cake --target=Format' to fix."
    exit 1
fi
```

### 3. CI での厳格チェック

- ローカル: `Format` で自動修正可能
- CI: `FormatCheck` で厳格チェック（修正不可）

## トラブルシューティング

### Q: FSharpLint が「0 warnings」だが警告があるはず

A: `fsharplint.json` の設定を確認。アナライザーが無効になっている可能性があります。

### Q: Fantomas が obj/ や bin/ をチェックしてしまう

A: `.fantomasignore` ファイルが存在するか確認。存在しない場合は作成してください。

### Q: VSCode タスクが動作しない

A: `.vscode/tasks.json` が存在するか確認。TT-1 で作成済みのはずです。

### Q: CI で Quality タスクが失敗する

A: ローカルで `dotnet cake --target=Quality` を実行して問題を修正後、コミットしてください。

## メトリクス

### 目標

| メトリクス | 目標値 |
|-----------|--------|
| **FSharpLint 警告数** | 0 件 |
| **Fantomas フォーマット違反** | 0 件 |
| **Quality タスク成功率** | 100% |

### 実績（イテレーション 1）

| メトリクス | 実績 |
|-----------|------|
| **FSharpLint 警告数** | 0 件 ✅ |
| **Fantomas フォーマット違反** | 0 件 ✅ |
| **Quality タスク実行時間** | 13.3 秒 |

## 参考資料

- [FSharpLint 公式ドキュメント](https://fsprojects.github.io/FSharpLint/)
- [Fantomas 公式ドキュメント](https://fsprojects.github.io/fantomas/)
- [イテレーション 1 計画](../development/iteration_plan-1.md)
- [Cake ビルドスクリプト](../../app/backend/build.cake)

---

**作成日**: 2025-11-10
**最終更新**: 2025-11-10
**バージョン**: 1.0
