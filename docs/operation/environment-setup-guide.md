# 環境構築ガイド

## 概要

このガイドでは、プロジェクトの開発環境を構築するための手順を説明します。

## 前提条件

- インターネット接続
- 管理者権限（一部インストールで必要）
- 約 5GB の空きディスク容量

## 対応 OS

- Windows 10/11
- macOS 12 以降
- Ubuntu 20.04 LTS 以降

## 1. .NET SDK のインストール

### 必須バージョン

- **.NET 9.0 SDK** (バージョン 9.0.200)

### Windows

#### 方法 1: インストーラー経由（推奨）

1. [.NET 公式サイト](https://dotnet.microsoft.com/download/dotnet/9.0) にアクセス
2. **.NET 9.0 SDK** をダウンロード
3. `dotnet-sdk-9.0.200-win-x64.exe` を実行
4. インストールウィザードに従ってインストール

#### 方法 2: winget 経由

```powershell
winget install Microsoft.DotNet.SDK.9
```

#### インストール確認

```powershell
dotnet --version
# 出力: 9.0.200
```

### macOS

#### 方法 1: インストーラー経由（推奨）

1. [.NET 公式サイト](https://dotnet.microsoft.com/download/dotnet/9.0) にアクセス
2. **.NET 9.0 SDK** (macOS Installer) をダウンロード
3. `dotnet-sdk-9.0.200-osx-x64.pkg` を実行
4. インストールウィザードに従ってインストール

#### 方法 2: Homebrew 経由

```bash
brew install --cask dotnet-sdk
```

#### インストール確認

```bash
dotnet --version
# 出力: 9.0.200
```

### Linux (Ubuntu)

#### APT 経由でのインストール

```bash
# Microsoft パッケージリポジトリを追加
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# .NET SDK 9.0 をインストール
sudo apt-get update
sudo apt-get install -y dotnet-sdk-9.0
```

#### インストール確認

```bash
dotnet --version
# 出力: 9.0.200
```

## 2. Git のインストール

### Windows

#### 方法 1: インストーラー経由（推奨）

1. [Git for Windows](https://git-scm.com/download/win) をダウンロード
2. インストーラーを実行
3. デフォルト設定で「Next」を選択してインストール

#### 方法 2: winget 経由

```powershell
winget install Git.Git
```

### macOS

```bash
# Xcode Command Line Tools 経由
xcode-select --install

# または Homebrew 経由
brew install git
```

### Linux (Ubuntu)

```bash
sudo apt-get update
sudo apt-get install -y git
```

### Git 初期設定

```bash
# ユーザー名とメールアドレスを設定
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"

# デフォルトブランチ名を設定
git config --global init.defaultBranch main

# 改行コード設定（Windows）
git config --global core.autocrlf true

# 改行コード設定（macOS/Linux）
git config --global core.autocrlf input

# 設定確認
git config --list
```

## 3. Heroku CLI のインストール

### Windows

```powershell
# インストーラーをダウンロードして実行
# https://devcenter.heroku.com/articles/heroku-cli
```

または

```powershell
winget install Heroku.HerokuCLI
```

### macOS

```bash
brew tap heroku/brew && brew install heroku
```

### Linux (Ubuntu)

```bash
curl https://cli-assets.heroku.com/install.sh | sh
```

### インストール確認

```bash
heroku --version
# 出力: heroku/10.0.0 win32-x64 node-v20.18.0
```

### Heroku ログイン

```bash
heroku login
# ブラウザが開き、Heroku にログインします
```

## 4. プロジェクトのクローン

### リポジトリのクローン

```bash
# SSH 経由（推奨）
git clone git@github.com:k2works/case-study-functional-ddd.git

# または HTTPS 経由
git clone https://github.com/k2works/case-study-functional-ddd.git

# プロジェクトディレクトリに移動
cd case-study-functional-ddd
```

### ブランチの確認

```bash
# 現在のブランチを確認
git branch

# development ブランチに切り替え
git checkout development
```

## 5. .NET ツールのインストール

プロジェクトで使用する .NET ツールをインストールします。

### app/backend ディレクトリに移動

```bash
cd app/backend
```

### ツールのリストア

```bash
dotnet tool restore
```

これにより、以下のツールがインストールされます：
- **Cake.Tool** (4.2.0): ビルド自動化
- **fantomas** (7.0.0-alpha-001): F# コードフォーマッター
- **dotnet-fsharplint** (0.24.3): F# 静的解析ツール

### インストール確認

```bash
# Cake
dotnet cake --version
# 出力: Cake version 4.2.0

# Fantomas
dotnet fantomas --version
# 出力: Fantomas v7.0.0-alpha-001

# FSharpLint
dotnet fsharplint --version
# 出力: dotnet-fsharplint version 0.24.3
```

## 6. 依存関係のリストア

### NuGet パッケージのリストア

```bash
dotnet restore
```

これにより、以下のパッケージがリストアされます：
- FSharp.Core
- Microsoft.NET.Test.Sdk
- xunit
- FsUnit.xUnit
- coverlet.collector

## 7. ビルドとテストの実行

### ビルド

```bash
# Cake 経由でビルド
dotnet cake --target=Build
```

**成功時の出力**:
```
========================================
Build
========================================
...
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### テスト実行

```bash
# Cake 経由でテスト実行
dotnet cake --target=Test
```

**成功時の出力**:
```
========================================
Test
========================================
...
Test run for OrderTaking.Tests
Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7
```

### 品質チェック

```bash
# フォーマットチェック + Lint
dotnet cake --target=Quality
```

**成功時の出力**:
```
========================================
FormatCheck
========================================
（出力なし - 成功）

========================================
Lint
========================================
========== Summary: 0 warnings ==========
```

### カバレッジ測定

```bash
# テスト + カバレッジ測定
dotnet cake --target=Coverage
```

**成功時の出力**:
```
========================================
Coverage
========================================
...
Coverage report generated in ./TestResults directory
```

## 8. VSCode のセットアップ（オプション）

### VSCode のインストール

- [Visual Studio Code](https://code.visualstudio.com/) をダウンロードしてインストール

### 推奨拡張機能

プロジェクトルートで VSCode を開くと、推奨拡張機能のインストールが提案されます：

```bash
code .
```

推奨拡張機能（`.vscode/extensions.json` で定義）:
- **Ionide.Ionide-fsharp**: F# 言語サポート
- **ms-dotnettools.csharp**: C# サポート
- **editorconfig.editorconfig**: EditorConfig サポート
- **GitHub.vscode-pull-request-github**: GitHub 統合

### VSCode タスク

`.vscode/tasks.json` で定義されている Cake タスクを実行できます：

1. `Ctrl + Shift + P` (Windows/Linux) または `Cmd + Shift + P` (macOS)
2. `Tasks: Run Task` を選択
3. 実行したいタスクを選択:
   - `Cake: Build`
   - `Cake: Test`
   - `Cake: Quality`
   - `Cake: Coverage`
   - など

### デフォルトビルドタスク

`Ctrl + Shift + B` (Windows/Linux) または `Cmd + Shift + B` (macOS) で `Cake: Build` を実行できます。

## 9. 環境確認チェックリスト

以下のコマンドを実行して、すべてが正しくインストールされているか確認してください：

```bash
# .NET SDK
dotnet --version
# ✅ 9.0.200

# Git
git --version
# ✅ git version 2.x.x

# Heroku CLI
heroku --version
# ✅ heroku/10.0.0 または以降

# Cake
dotnet cake --version
# ✅ Cake version 4.2.0

# Fantomas
dotnet fantomas --version
# ✅ Fantomas v7.0.0-alpha-001

# FSharpLint
dotnet fsharplint --version
# ✅ dotnet-fsharplint version 0.24.3

# ビルド成功
dotnet cake --target=Build
# ✅ Build succeeded

# テスト成功
dotnet cake --target=Test
# ✅ Passed: 7

# 品質チェック成功
dotnet cake --target=Quality
# ✅ 0 warnings
```

## 10. トラブルシューティング

### .NET SDK がインストールされているが dotnet コマンドが見つからない

**原因**: PATH 環境変数が設定されていない

**対処法**:
- コマンドプロンプト / ターミナルを再起動
- PC を再起動

### dotnet restore が失敗する

**原因**: ネットワーク接続の問題、または NuGet キャッシュの破損

**対処法**:
```bash
# NuGet キャッシュをクリア
dotnet nuget locals all --clear

# 再度リストア
dotnet restore
```

### dotnet cake が実行できない

**原因**: ツールがローカルにインストールされていない

**対処法**:
```bash
# ツールをリストア
dotnet tool restore

# それでも失敗する場合は、グローバルにインストール
dotnet tool install -g Cake.Tool
```

### Fantomas が obj/ や bin/ をフォーマットしようとする

**原因**: `.fantomasignore` ファイルが存在しない

**対処法**:
`.fantomasignore` ファイルが `app/backend/` ディレクトリに存在することを確認してください。

### ビルドは成功するがテストが失敗する

**原因**: テストプロジェクトの依存関係の問題

**対処法**:
```bash
# Clean してから再ビルド
dotnet cake --target=Clean
dotnet cake --target=Build
dotnet cake --target=Test
```

### GitHub Actions CI が失敗する

**原因**: ローカルでは成功するが CI で失敗する場合、環境の違いが原因

**対処法**:
- CI のログを確認（`gh run view <run-id> --log`）
- ローカルで `dotnet cake --target=Quality` を実行して品質チェックを事前に行う

### Heroku デプロイが失敗する

**原因**: Procfile のパス設定や buildpack の問題

**対処法**:
- [Heroku デプロイガイド](../deployment/heroku-deploy-guide.md) を参照
- `heroku logs --tail --app case-study-function-ddd-dev` でログを確認

## 11. 次のステップ

環境構築が完了したら、以下のドキュメントを参照してください：

### 開発フロー
- [コーディングとテストガイド](../reference/コーディングとテストガイド.md)
- [リリース・イテレーション計画ガイド](../reference/リリース・イテレーション計画ガイド.md)

### 品質管理
- [静的解析ガイド](../quality/static-analysis-guide.md)
- [カバレッジガイド](../quality/coverage-guide.md)

### CI/CD
- [GitHub Actions CI/CD ガイド](../ci-cd/github-actions-guide.md)
- [Heroku デプロイガイド](../deployment/heroku-deploy-guide.md)

### プロセス
- [時間記録ガイド](../process/time-tracking-guide.md)
- [デイリースタンドアップガイド](../process/daily-standup-guide.md)

## 12. 推奨開発環境

### エディタ

- **Visual Studio Code** (推奨)
- JetBrains Rider
- Visual Studio 2022

### ターミナル

- **Windows**: PowerShell 7+ または Windows Terminal
- **macOS**: Terminal.app または iTerm2
- **Linux**: Bash

### Git クライアント

- **コマンドライン** (推奨)
- GitHub Desktop
- Sourcetree

## 参考資料

- [.NET 公式ドキュメント](https://docs.microsoft.com/ja-jp/dotnet/)
- [F# ガイド](https://docs.microsoft.com/ja-jp/dotnet/fsharp/)
- [Cake ドキュメント](https://cakebuild.net/docs/)
- [Heroku .NET ガイド](https://devcenter.heroku.com/articles/dotnet-core)
- [イテレーション 1 計画](../development/iteration_plan-1.md)

---

**作成日**: 2025-11-10
**最終更新**: 2025-11-10
**バージョン**: 1.0
