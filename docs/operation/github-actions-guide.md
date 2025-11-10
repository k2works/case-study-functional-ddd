# GitHub Actions CI/CD ガイド

## 概要

プロジェクトでは GitHub Actions を使用して CI/CD パイプラインを構築しています。

## ワークフロー構成

### CI ワークフロー (.github/workflows/ci.yml)

**トリガー**:
- `push` イベント: main, development ブランチ
- `pull_request` イベント: main, development ブランチ

**ジョブ構成**:

```yaml
jobs:
  build:    # ビルド・テスト
  quality:  # 品質チェック（build 後に実行）
```

## build ジョブ

### 実行内容

| ステップ | 内容 | 備考 |
|---------|------|------|
| 1. Checkout | リポジトリをチェックアウト | actions/checkout@v4 |
| 2. Setup .NET | .NET 9.0 をセットアップ | actions/setup-dotnet@v4 |
| 3. Restore tools | dotnet tool restore | Cake, Fantomas, FSharpLint |
| 4. Restore dependencies | dotnet restore | NuGet パッケージ |
| 5. Build | dotnet build | Release 構成 |
| 6. Test | dotnet test | 7 テスト実行 |
| 7. Format Check | dotnet fantomas --check . | コードフォーマット確認 |
| 8. Lint | dotnet dotnet-fsharplint lint | 静的解析 |

### 実行結果（イテレーション 1）

```
✓ build in 59s
  ✓ Set up job
  ✓ Run actions/checkout@v4
  ✓ Setup .NET
  ✓ Restore tools
  ✓ Restore dependencies
  ✓ Build
  ✓ Test
  ✓ Format Check
  ✓ Lint
  ✓ Complete job
```

**所要時間**: 59 秒

## quality ジョブ

### 実行内容

| ステップ | 内容 | 備考 |
|---------|------|------|
| 1. Checkout | リポジトリをチェックアウト | actions/checkout@v4 |
| 2. Setup .NET | .NET 9.0 をセットアップ | actions/setup-dotnet@v4 |
| 3. Restore tools | dotnet tool restore | Cake |
| 4. Run Cake Quality Task | dotnet cake --target=Quality | FormatCheck + Lint |

### 実行結果（イテレーション 1）

```
✓ quality in 33s
  ✓ Set up job
  ✓ Run actions/checkout@v4
  ✓ Setup .NET
  ✓ Restore tools
  ✓ Run Cake Quality Task
  ✓ Complete job
```

**所要時間**: 33 秒

## CI 実行の確認方法

### 方法 1: GitHub Web UI

1. GitHub リポジトリページを開く
2. "Actions" タブをクリック
3. ワークフロー実行履歴を確認

### 方法 2: GitHub CLI

#### 最新の実行を確認

```bash
gh run list --limit 5
```

**出力例**:
```
✓  [時間: 1.5h] TT-2, TT-3...  CI  development  push  19218858438  2m  2025-11-10
✓  [時間: 4.0h] TT-1...        CI  development  push  19218857123  2m  2025-11-10
```

#### 特定の実行を監視

```bash
gh run watch 19218858438
```

#### ログを表示

```bash
gh run view 19218858438 --log
```

### 方法 3: VSCode 拡張機能

**GitHub Actions** 拡張機能を使用：
- ワークフロー実行状況を VSCode 内で確認
- ログのリアルタイム表示

## プッシュ時の自動実行

### フロー

```
1. ローカルでコミット
   ↓
2. git push origin development
   ↓
3. GitHub Actions が自動実行
   ↓
4. build ジョブ実行 (59s)
   ↓
5. quality ジョブ実行 (33s)
   ↓
6. 成功 / 失敗の通知
```

### 実行例

```bash
# コミット
git add .
git commit -m "[時間: Xh] タスク内容"

# プッシュ（CI 自動実行）
git push origin development

# 実行状況確認
gh run list --limit 1

# 結果確認
gh run view <run-id>
```

## Pull Request での品質チェック

### PR 作成時

```bash
# ブランチ作成
git checkout -b feature/new-feature

# 変更をコミット
git add .
git commit -m "Add new feature"

# プッシュ
git push origin feature/new-feature

# PR 作成
gh pr create --title "Add new feature" --body "..."
```

### PR での CI 実行

PR 作成時に自動的に CI が実行され、結果が PR に表示されます：

```
✓ All checks have passed
  ✓ build (59s)
  ✓ quality (33s)
```

### マージ条件

- **必須**: build ジョブが成功
- **必須**: quality ジョブが成功

**注意**: ジョブが失敗した場合、PR はマージできません。

## エラー発生時の対応

### Format Check エラー

**エラー例**:
```
Error: Fantomas check failed.
OrderTaking.Domain/Library.fs needs formatting
```

**対応**:
```bash
# ローカルでフォーマット修正
dotnet cake --target=Format

# コミット
git add .
git commit -m "fix: フォーマット修正"

# プッシュして再実行
git push
```

### Lint エラー

**エラー例**:
```
Warning: [Hint] Line 10: Consider using 'List.isEmpty' instead
```

**対応**:
```bash
# コードを修正
# ...

# コミット・プッシュ
git add .
git commit -m "refactor: FSharpLint 警告対応"
git push
```

### Test エラー

**エラー例**:
```
Failed!  - Failed:     1, Passed:     6
```

**対応**:
```bash
# ローカルでテスト実行
dotnet test

# テスト修正
# ...

# コミット・プッシュ
git add .
git commit -m "fix: テスト修正"
git push
```

## CI パイプラインのカスタマイズ

### カバレッジ追加（今後）

`.github/workflows/ci.yml` に追加:

```yaml
- name: Run Tests with Coverage
  run: dotnet cake --target=Coverage

- name: Upload Coverage Report
  uses: codecov/codecov-action@v3
  with:
    files: ./TestResults/**/coverage.cobertura.xml
```

### カバレッジバッジ

README.md に追加:

```markdown
[![CI](https://github.com/k2works/case-study-functional-ddd/actions/workflows/ci.yml/badge.svg)](https://github.com/k2works/case-study-functional-ddd/actions)
```

## ベストプラクティス

### 1. ローカルで事前確認

CI 実行前にローカルでチェック：

```bash
# ビルド・テスト
dotnet cake

# 品質チェック
dotnet cake --target=Quality

# すべて成功してからプッシュ
git push
```

### 2. 小さく頻繁なコミット

- 1 コミット = 1 論理的変更
- CI 実行時間を短縮
- 失敗時のデバッグが容易

### 3. ブランチ保護

main ブランチの保護設定：

- PR 経由でのみマージ可能
- CI 成功が必須
- レビュー承認が必須

## トラブルシューティング

### Q: CI が実行されない

A: 以下を確認：
1. `.github/workflows/ci.yml` が存在するか
2. トリガー設定（push, pull_request）が正しいか
3. ブランチ名が一致しているか

### Q: CI が長時間実行される

A: タイムアウト設定を確認：
```yaml
jobs:
  build:
    timeout-minutes: 10
```

### Q: 並行実行数の制限

A: GitHub の並行実行数制限に注意：
- Free: 20 並行ジョブ
- Pro: 40 並行ジョブ

## メトリクス（イテレーション 1）

| メトリクス | 実績 |
|-----------|------|
| **CI 実行時間** | 92 秒 (build: 59s + quality: 33s) |
| **CI 成功率** | 100% |
| **ワークフロー実行回数** | 1 回（テスト実行） |
| **エラー検出数** | 0 件 |

## 参考資料

- [GitHub Actions 公式ドキュメント](https://docs.github.com/ja/actions)
- [.NET での GitHub Actions](https://docs.microsoft.com/ja-jp/dotnet/devops/github-actions-overview)
- [イテレーション 1 計画](../development/iteration_plan-1.md)
- [CI ワークフローファイル](../../.github/workflows/ci.yml)

---

**作成日**: 2025-11-10
**最終更新**: 2025-11-10
**バージョン**: 1.0
