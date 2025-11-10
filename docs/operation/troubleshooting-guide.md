# トラブルシューティングガイド

## 概要

このガイドでは、開発中に発生する可能性のある問題とその解決策をまとめています。

## 目次

1. [.NET SDK とツール](#1-net-sdk-とツール)
2. [ビルドとテスト](#2-ビルドとテスト)
3. [Cake タスク](#3-cake-タスク)
4. [静的解析ツール](#4-静的解析ツール)
5. [カバレッジ](#5-カバレッジ)
6. [CI/CD](#6-cicd)
7. [Heroku デプロイ](#7-heroku-デプロイ)
8. [Git](#8-git)
9. [IDE/エディタ](#9-ideエディタ)

---

## 1. .NET SDK とツール

### Q1: `dotnet` コマンドが見つからない

**症状**:
```
'dotnet' is not recognized as an internal or external command
```

**原因**: .NET SDK がインストールされていない、または PATH が設定されていない

**対処法**:

1. .NET SDK がインストールされているか確認:
```bash
# Windows
where dotnet

# macOS/Linux
which dotnet
```

2. インストールされていない場合は、[環境構築ガイド](environment-setup-guide.md#1-net-sdk-のインストール) を参照してインストール

3. インストールされているが PATH が設定されていない場合:
   - **Windows**: コマンドプロンプトを再起動
   - **macOS/Linux**: ターミナルを再起動
   - それでも解決しない場合は PC を再起動

### Q2: `dotnet tool restore` が失敗する

**症状**:
```
error NU1301: Unable to load the service index for source
```

**原因**: ネットワーク接続の問題、または NuGet キャッシュの破損

**対処法**:

1. NuGet キャッシュをクリア:
```bash
dotnet nuget locals all --clear
```

2. 再度リストア:
```bash
dotnet tool restore
```

3. それでも失敗する場合は、プロキシ設定を確認:
```bash
# プロキシ設定の確認
dotnet nuget config get http_proxy
```

### Q3: `dotnet cake` が実行できない

**症状**:
```
No executable found matching command "dotnet-cake"
```

**原因**: Cake ツールがローカルにインストールされていない

**対処法**:

1. ツールをリストア:
```bash
dotnet tool restore
```

2. それでも失敗する場合は、グローバルにインストール:
```bash
dotnet tool install -g Cake.Tool
```

3. `.config/dotnet-tools.json` ファイルが存在することを確認:
```bash
cat .config/dotnet-tools.json
```

---

## 2. ビルドとテスト

### Q4: ビルドが失敗する

**症状**:
```
error CS0246: The type or namespace name 'XXX' could not be found
```

**原因**: 依存関係がリストアされていない、または破損している

**対処法**:

1. Clean してから再ビルド:
```bash
dotnet cake --target=Clean
dotnet cake --target=Restore
dotnet cake --target=Build
```

2. それでも失敗する場合は、obj/ と bin/ を手動削除:
```bash
# Windows PowerShell
Get-ChildItem -Recurse -Directory -Filter obj | Remove-Item -Recurse -Force
Get-ChildItem -Recurse -Directory -Filter bin | Remove-Item -Recurse -Force

# macOS/Linux
find . -type d -name "obj" -exec rm -rf {} +
find . -type d -name "bin" -exec rm -rf {} +
```

3. 再度ビルド:
```bash
dotnet restore
dotnet build
```

### Q5: テストが失敗する

**症状**:
```
Failed!  - Failed:     1, Passed:     6, Skipped:     0, Total:     7
```

**原因**: コードの変更によりテストが期待する動作と異なる

**対処法**:

1. テスト結果の詳細を確認:
```bash
dotnet test --logger "console;verbosity=detailed"
```

2. 特定のテストのみ実行:
```bash
dotnet test --filter "FullyQualifiedName~TestNamespace.TestClass.TestMethod"
```

3. テストをデバッグモードで実行:
```bash
dotnet test --logger "console;verbosity=diagnostic"
```

4. テストコードまたはプロダクションコードを修正

### Q6: ビルドは成功するがテストが失敗する

**症状**: ローカルビルドは成功するが、テスト実行時にエラー

**原因**: テストプロジェクトの依存関係の問題

**対処法**:

1. テストプロジェクトの依存関係を確認:
```bash
cd OrderTaking.Tests
dotnet restore
```

2. Clean してから再ビルド:
```bash
dotnet cake --target=Clean
dotnet cake --target=Build
dotnet cake --target=Test
```

---

## 3. Cake タスク

### Q7: Cake タスクが見つからない

**症状**:
```
Error: The target 'XXX' was not found.
```

**原因**: タスク名が間違っている、または build.cake に定義されていない

**対処法**:

1. 利用可能なタスク一覧を確認:
```bash
dotnet cake --description
```

2. build.cake ファイルでタスクが定義されているか確認:
```bash
cat build.cake | grep "Task("
```

3. タスク名のスペルミスを確認

### Q8: Cake タスクがエラーで終了する

**症状**:
```
An error occurred when executing task 'XXX'.
```

**原因**: タスク内のコマンドが失敗している

**対処法**:

1. 詳細ログを有効にして実行:
```bash
dotnet cake --target=XXX --verbosity=diagnostic
```

2. エラーメッセージを確認し、該当するコマンドを手動で実行して問題を特定

3. build.cake ファイルのタスク定義を確認し、必要に応じて修正

---

## 4. 静的解析ツール

### Q9: FSharpLint が「0 warnings」だが警告があるはず

**症状**: FSharpLint が警告を検出しない

**原因**: `fsharplint.json` の設定でアナライザーが無効になっている

**対処法**:

1. `fsharplint.json` の設定を確認:
```bash
cat app/backend/fsharplint.json
```

2. アナライザーが有効になっているか確認:
```json
{
  "analysers": {
    "Hints": {
      "enabled": true
    },
    "Typography": {
      "enabled": true
    }
  }
}
```

3. 設定を修正して再実行:
```bash
dotnet cake --target=Lint
```

### Q10: Fantomas が obj/ や bin/ をチェックしてしまう

**症状**:
```
Checking 'obj/Debug/net9.0/...'
```

**原因**: `.fantomasignore` ファイルが存在しない、または正しく設定されていない

**対処法**:

1. `.fantomasignore` ファイルが存在するか確認:
```bash
ls -la app/backend/.fantomasignore
```

2. 存在しない場合は作成:
```bash
cat > app/backend/.fantomasignore << 'EOF'
**/obj/**
**/bin/**
**/.vs/**
**/.idea/**
**/.vscode/**
**/packages/**
**/paket-files/**
EOF
```

3. Fantomas を再実行:
```bash
dotnet cake --target=FormatCheck
```

### Q11: Fantomas のフォーマット結果が期待と異なる

**症状**: Fantomas がコードを意図しない形式にフォーマットする

**原因**: Fantomas の設定が適切でない

**対処法**:

1. `.editorconfig` ファイルで Fantomas の設定をカスタマイズ:
```ini
[*.fs]
indent_size = 4
max_line_length = 120
```

2. 特定のコードブロックをフォーマットから除外する場合は、コメントを使用:
```fsharp
// fantomas-ignore
let myCode = ...
```

---

## 5. カバレッジ

### Q12: カバレッジが 0% のまま

**症状**: カバレッジレポートが生成されるが、すべて 0%

**原因**: テストがプロダクションコードを呼び出していない、または coverlet が正しく動作していない

**対処法**:

1. テストがプロダクションコードを呼び出しているか確認:
```bash
dotnet test --logger "console;verbosity=detailed"
```

2. coverlet が正しくインストールされているか確認:
```bash
dotnet list package | grep coverlet
```

3. Coverage タスクを再実行:
```bash
dotnet cake --target=Coverage
```

4. TestResults/ ディレクトリにファイルが生成されているか確認:
```bash
ls -la TestResults/
```

### Q13: カバレッジレポートが見つからない

**症状**: Coverage タスクは成功するが、レポートファイルが見つからない

**原因**: `--results-directory` オプションが指定されていない

**対処法**:

1. build.cake の Coverage タスク定義を確認:
```csharp
ArgumentCustomization = args => args
    .Append("--collect:\"XPlat Code Coverage\"")
    .Append("--results-directory:./TestResults")
```

2. TestResults/ ディレクトリが作成されているか確認:
```bash
ls -la TestResults/
```

3. GUID 形式のサブディレクトリ内に `coverage.cobertura.xml` が存在するか確認:
```bash
find TestResults/ -name "coverage.cobertura.xml"
```

### Q14: 一部のファイルがカバレッジ対象外

**症状**: 特定のファイルがカバレッジレポートに含まれない

**原因**: coverlet の除外設定、またはテストが該当ファイルを呼び出していない

**対処法**:

1. coverlet の設定を確認（カスタム設定がある場合）

2. テストが該当ファイルのコードを実行しているか確認

3. 該当ファイルに対するテストを追加

---

## 6. CI/CD

### Q15: GitHub Actions CI が失敗する

**症状**: ローカルでは成功するが CI で失敗する

**原因**: 環境の違い、または依存関係の問題

**対処法**:

1. CI のログを確認:
```bash
gh run list --limit 5
gh run view <run-id> --log
```

2. ローカルで品質チェックを実行:
```bash
dotnet cake --target=Quality
```

3. すべてのテストが成功することを確認:
```bash
dotnet cake --target=Test
```

4. CI 環境と同じ .NET バージョンを使用しているか確認:
```bash
dotnet --version
# 期待値: 9.0.200
```

### Q16: GitHub Actions でタイムアウトが発生する

**症状**:
```
Error: The job running on runner has exceeded the maximum execution time
```

**原因**: CI ジョブの実行時間が制限時間を超えている

**対処法**:

1. `.github/workflows/ci.yml` でタイムアウト設定を確認:
```yaml
jobs:
  build:
    timeout-minutes: 10  # 必要に応じて増やす
```

2. 不要なステップを削除してジョブを最適化

3. 並列実行を検討

### Q17: GitHub Actions で環境変数が設定されない

**症状**: CI で環境変数にアクセスできない

**原因**: GitHub Secrets が設定されていない、または workflow ファイルで参照されていない

**対処法**:

1. GitHub リポジトリの Settings → Secrets and variables → Actions で Secrets を確認

2. workflow ファイルで Secrets を正しく参照:
```yaml
env:
  HEROKU_API_KEY: ${{ secrets.HEROKU_API_KEY }}
```

---

## 7. Heroku デプロイ

### Q18: Heroku デプロイ後にアプリがクラッシュする

**症状**:
```
heroku ps --app case-study-function-ddd-dev
web.1: crashed
```

**原因**: Procfile のパスが間違っている、または PORT 環境変数が設定されていない

**対処法**:

1. Heroku ログで詳細を確認:
```bash
heroku logs --tail --app case-study-function-ddd-dev
```

2. Procfile のパスを確認:
```bash
cat Procfile
# 期待値: web: cd app/backend/OrderTaking.WebApi/bin/Release/net9.0 && ./OrderTaking.WebApi
```

3. アプリが PORT 環境変数をリッスンしているか確認

4. Dyno を再起動:
```bash
heroku restart --app case-study-function-ddd-dev
```

### Q19: Heroku ビルドが失敗する

**症状**:
```
remote: Build failed
```

**原因**: .NET SDK バージョン不一致、または依存関係の問題

**対処法**:

1. `global.json` で SDK バージョンを確認:
```bash
cat global.json
```

2. ローカルでビルドが成功することを確認:
```bash
dotnet build --configuration Release
```

3. Heroku ビルドパックが正しく設定されているか確認:
```bash
heroku buildpacks --app case-study-function-ddd-dev
# 期待値: https://github.com/jincod/dotnetcore-buildpack
```

4. 必要に応じてビルドパックを再設定:
```bash
heroku buildpacks:set https://github.com/jincod/dotnetcore-buildpack --app case-study-function-ddd-dev
```

### Q20: Heroku で 503 Service Unavailable

**症状**:
```
curl https://case-study-function-ddd-dev-e9b7ca1c1ec6.herokuapp.com/
HTTP/1.1 503 Service Unavailable
```

**原因**: アプリが起動していない（crashed）、または Eco dyno がスリープ中

**対処法**:

1. Dyno ステータスを確認:
```bash
heroku ps --app case-study-function-ddd-dev
```

2. アプリが crashed している場合:
```bash
# ログを確認
heroku logs --tail --app case-study-function-ddd-dev

# 再起動
heroku restart --app case-study-function-ddd-dev
```

3. Eco dyno がスリープしている場合:
   - 数秒待つとアプリが起動する
   - 本番環境では Professional Dyno の使用を推奨

### Q21: Heroku CLI が見つからない

**症状**:
```
'heroku' is not recognized as an internal or external command
```

**原因**: Heroku CLI がインストールされていない、または PATH が設定されていない

**対処法**:

1. Heroku CLI がインストールされているか確認:
```bash
# Windows
where heroku

# macOS/Linux
which heroku
```

2. インストールされていない場合は、[環境構築ガイド](environment-setup-guide.md#3-heroku-cli-のインストール) を参照してインストール

3. PATH が設定されていない場合は、ターミナルを再起動

---

## 8. Git

### Q22: git push が拒否される

**症状**:
```
error: failed to push some refs to 'origin'
```

**原因**: リモートブランチがローカルより進んでいる

**対処法**:

1. リモートの変更を取得:
```bash
git fetch origin
```

2. リモートの変更をマージまたはリベース:
```bash
# マージ
git pull origin development

# または リベース
git pull --rebase origin development
```

3. 再度プッシュ:
```bash
git push origin development
```

### Q23: git commit が pre-commit フックで失敗する

**症状**:
```
Quality check failed. Run 'dotnet cake --target=Format' to fix.
```

**原因**: フォーマットチェックまたは Lint が失敗している

**対処法**:

1. フォーマットを自動修正:
```bash
dotnet cake --target=Format
```

2. Lint の警告を確認:
```bash
dotnet cake --target=Lint
```

3. 必要に応じてコードを修正

4. 再度コミット:
```bash
git add .
git commit -m "fix: フォーマット修正"
```

### Q24: マージコンフリクトが発生する

**症状**:
```
CONFLICT (content): Merge conflict in XXX
```

**原因**: 同じファイルの同じ箇所が異なるブランチで編集されている

**対処法**:

1. コンフリクトが発生しているファイルを確認:
```bash
git status
```

2. エディタでコンフリクトマーカーを探して手動解決:
```
<<<<<<< HEAD
your changes
=======
their changes
>>>>>>> branch-name
```

3. 解決後、ファイルを追加:
```bash
git add <resolved-file>
```

4. マージを完了:
```bash
git commit
```

---

## 9. IDE/エディタ

### Q25: VSCode で F# ファイルがハイライトされない

**症状**: F# ファイルがプレーンテキストとして表示される

**原因**: Ionide 拡張機能がインストールされていない

**対処法**:

1. Ionide-fsharp 拡張機能をインストール:
   - `Ctrl + Shift + X` (Windows/Linux) または `Cmd + Shift + X` (macOS)
   - "Ionide-fsharp" を検索してインストール

2. VSCode を再起動

### Q26: VSCode タスクが動作しない

**症状**: `Tasks: Run Task` でタスクが表示されない

**原因**: `.vscode/tasks.json` が存在しない、または正しく設定されていない

**対処法**:

1. `.vscode/tasks.json` ファイルが存在するか確認:
```bash
ls -la app/backend/.vscode/tasks.json
```

2. 存在しない場合は、TT-1 で作成されたファイルを確認

3. VSCode を再起動

### Q27: Rider で F# プロジェクトがビルドできない

**症状**: Rider で "Build failed" エラー

**原因**: .NET SDK バージョンの不一致

**対処法**:

1. Rider の設定で .NET SDK のパスを確認:
   - Settings → Build, Execution, Deployment → Toolset and Build
   - .NET SDK version: 9.0.200

2. ソリューションをリロード:
   - File → Reload All Projects

3. キャッシュをクリア:
   - File → Invalidate Caches / Restart

---

## 10. その他の問題

### Q28: ディスク容量不足エラー

**症状**:
```
Error: No space left on device
```

**原因**: ディスクの空き容量が不足している

**対処法**:

1. 不要な obj/ と bin/ ディレクトリを削除:
```bash
dotnet cake --target=Clean
```

2. NuGet キャッシュをクリア:
```bash
dotnet nuget locals all --clear
```

3. Docker イメージとコンテナを削除（使用している場合）:
```bash
docker system prune -a
```

### Q29: メモリ不足エラー

**症状**:
```
System.OutOfMemoryException
```

**原因**: メモリが不足している、またはメモリリークが発生している

**対処法**:

1. 不要なアプリケーションを終了

2. VSCode の拡張機能を無効化（一時的に）

3. ビルドを並列実行しないように設定:
```bash
dotnet build --no-parallel
```

### Q30: ポートがすでに使用されている

**症状**:
```
Error: Address already in use
```

**原因**: 指定されたポートがすでに他のプロセスで使用されている

**対処法**:

1. 使用中のプロセスを確認:
```bash
# Windows
netstat -ano | findstr :<PORT>

# macOS/Linux
lsof -i :<PORT>
```

2. プロセスを終了:
```bash
# Windows
taskkill /PID <PID> /F

# macOS/Linux
kill -9 <PID>
```

3. 別のポートを使用するように設定を変更

---

## サポート

上記の対処法で解決しない場合は、以下を参照してください：

### プロジェクト内ドキュメント

- [環境構築ガイド](environment-setup-guide.md)
- [静的解析ガイド](../quality/static-analysis-guide.md)
- [カバレッジガイド](../quality/coverage-guide.md)
- [GitHub Actions CI/CD ガイド](../ci-cd/github-actions-guide.md)
- [Heroku デプロイガイド](../deployment/heroku-deploy-guide.md)

### 外部リソース

- [.NET 公式ドキュメント](https://docs.microsoft.com/ja-jp/dotnet/)
- [F# ガイド](https://docs.microsoft.com/ja-jp/dotnet/fsharp/)
- [Cake ドキュメント](https://cakebuild.net/docs/)
- [Heroku .NET ガイド](https://devcenter.heroku.com/articles/dotnet-core)
- [GitHub Actions ドキュメント](https://docs.github.com/ja/actions)

### GitHub Issue

問題が解決しない場合は、GitHub Issue を作成してください：
- リポジトリ: https://github.com/k2works/case-study-functional-ddd/issues
- テンプレート: Bug Report

---

**作成日**: 2025-11-10
**最終更新**: 2025-11-10
**バージョン**: 1.0
