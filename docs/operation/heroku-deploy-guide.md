# Heroku デプロイガイド

## 概要

プロジェクトでは Heroku を使用して .NET 9.0 F# アプリケーションをデプロイします。

## 前提条件

- Heroku CLI がインストールされていること
- Heroku アカウントがあること
- Git リポジトリが設定されていること

## Heroku アプリケーション

### 本番環境

- **アプリ名**: `case-study-function-ddd`
- **URL**: https://case-study-function-ddd-{hash}.herokuapp.com/
- **Git リモート**: heroku

### 開発環境

- **アプリ名**: `case-study-function-ddd-dev`
- **URL**: https://case-study-function-ddd-dev-e9b7ca1c1ec6.herokuapp.com/
- **Git リモート**: heroku-dev

## デプロイ設定

### Procfile

プロジェクトルートの `Procfile`:

```
web: cd app/backend/OrderTaking.WebApi/bin/Release/net9.0 && ./OrderTaking.WebApi
```

**重要**: Heroku ビルドパックは `heroku_output/` にビルド成果物を配置しますが、現在の Procfile は開発環境の構造に合わせています。

### global.json

.NET SDK バージョンを固定:

```json
{
  "sdk": {
    "version": "9.0.200"
  }
}
```

### ビルドパック

.NET Core ビルドパック:
```
https://github.com/jincod/dotnetcore-buildpack
```

## デプロイ方法

### 手動デプロイ（開発環境）

```bash
# Heroku CLI バージョン確認
heroku --version

# アプリ情報確認
heroku apps:info --app case-study-function-ddd-dev

# Git リモート追加（初回のみ）
git remote add heroku-dev https://git.heroku.com/case-study-function-ddd-dev.git

# デプロイ
git push heroku-dev development:main
```

### 手動デプロイ（本番環境）

```bash
# 本番環境へのデプロイ
git push heroku main:main
```

### GitHub Actions 自動デプロイ

`.github/workflows/deploy.yml` で自動デプロイを設定済み:

```yaml
- uses: akhileshns/heroku-deploy@v3.13.15
  with:
    heroku_api_key: ${{ secrets.HEROKU_API_KEY }}
    heroku_app_name: "case-study-function-ddd-dev"
    heroku_email: ${{ secrets.HEROKU_EMAIL }}
    branch: "development"
```

## デプロイフロー

```
1. git push heroku-dev development:main
   ↓
2. Heroku がソースを受信
   ↓
3. .NET Core ビルドパック実行
   - .NET 9.0 SDK インストール
   - dotnet restore
   - dotnet publish
   ↓
4. heroku_output/ に成果物配置
   ↓
5. Procfile の web コマンド実行
   ↓
6. アプリケーション起動
```

## デプロイ確認

### アプリステータス確認

```bash
# Dyno ステータス
heroku ps --app case-study-function-ddd-dev

# アプリ情報
heroku apps:info --app case-study-function-ddd-dev
```

**正常時の出力**:
```
=== web (Eco): cd app/backend/OrderTaking.WebApi/bin/Release/net9.0 && ./OrderTaking.WebApi (1)

web.1: up 2025/11/10 11:38:26 +0900 (~ 15m ago)
```

### ログ確認

```bash
# リアルタイムログ
heroku logs --tail --app case-study-function-ddd-dev

# 最新 100 行
heroku logs -n 100 --app case-study-function-ddd-dev
```

**正常起動時のログ**:
```
heroku[web.1]: Starting process with command `cd /app/heroku_output && ./OrderTaking.WebApi`
app[web.1]: info: Microsoft.Hosting.Lifetime[14]
app[web.1]: Now listening on: http://[::]:25202
app[web.1]: info: Microsoft.Hosting.Lifetime[0]
app[web.1]: Application started. Press Ctrl+C to shut down.
heroku[web.1]: State changed from starting to up
```

### Web アクセス確認

```bash
# ブラウザで開く
heroku open --app case-study-function-ddd-dev

# curl で確認
curl https://case-study-function-ddd-dev-e9b7ca1c1ec6.herokuapp.com/
```

## デプロイ実績（イテレーション 1）

### デプロイ成功確認

| 項目 | 結果 |
|------|------|
| **デプロイ日時** | 2025-11-10 11:53 JST |
| **Release バージョン** | v5 |
| **ビルド時間** | 約 30 秒 |
| **Slug サイズ** | 122 MB |
| **Dyno タイプ** | Eco |
| **Region** | US |
| **デプロイ方法** | 手動プッシュ (git push) |

### ビルドログ確認

```
remote: Building source:
remote: -----> Building on the Heroku-24 stack
remote: -----> Using buildpack: https://github.com/jincod/dotnetcore-buildpack
remote: -----> Core .NET app detected
remote: > Installing dotnet
remote: > publish /tmp/build.../OrderTaking.WebApi/OrderTaking.WebApi.fsproj for Release
remote: Welcome to .NET 9.0!
remote: SDK Version: 9.0.200
remote: OrderTaking.WebApi -> /tmp/build.../heroku_output/
remote: -----> Discovering process types
remote: Procfile declares types -> web
remote: -----> Compressing... Done: 122.8M
remote: -----> Launching... Released v5
remote: Verifying deploy... done.
```

## トラブルシューティング

### エラー 1: Application crashed

**症状**:
```
web.1: crashed 2025/11/10 11:53:33 +0900
```

**原因**:
- Procfile のパスが間違っている
- PORT 環境変数が設定されていない
- ビルド成果物の場所が間違っている

**対処法**:
```bash
# ログで詳細確認
heroku logs --tail --app case-study-function-ddd-dev

# Procfile のパス確認
cat Procfile

# Dyno 再起動
heroku restart --app case-study-function-ddd-dev
```

### エラー 2: ビルド失敗

**症状**:
```
remote: Build failed
```

**原因**:
- .NET SDK バージョン不一致
- 依存関係の問題

**対処法**:
```bash
# global.json で SDK バージョン確認
cat global.json

# ローカルでビルド確認
dotnet build --configuration Release
```

### エラー 3: 503 Service Unavailable

**症状**:
```
curl: (503) Service Unavailable
```

**原因**:
- アプリが起動していない（crashed）
- Eco dyno がスリープ中

**対処法**:
```bash
# Dyno ステータス確認
heroku ps --app case-study-function-ddd-dev

# 手動起動
heroku ps:scale web=1 --app case-study-function-ddd-dev
```

## Heroku ダッシュボード

Web UI でアプリを管理:
https://dashboard.heroku.com/apps/case-study-function-ddd-dev

**確認事項**:
- Dyno ステータス
- リソース使用状況
- ログ
- 設定変数
- ビルド履歴

## Eco Dyno の制限

### Eco プラン特性

- **月間無料時間**: 1000 時間
- **スリープ**: 30 分アクセスがないとスリープ
- **起動時間**: スリープ後の初回アクセスは遅い（数秒）
- **同時実行**: 1 dyno のみ

### スリープ対策

本番環境では Professional Dyno の使用を推奨:

```bash
# Professional Dyno にスケール
heroku ps:type professional --app case-study-function-ddd
```

## 環境変数

### 設定方法

```bash
# 環境変数設定
heroku config:set ASPNETCORE_ENVIRONMENT=Production --app case-study-function-ddd-dev

# 環境変数確認
heroku config --app case-study-function-ddd-dev
```

### 推奨設定

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:$PORT
```

## 参考資料

- [Heroku .NET ガイド](https://devcenter.heroku.com/articles/dotnet-core)
- [Heroku Buildpacks](https://devcenter.heroku.com/articles/buildpacks)
- [イテレーション 1 計画](../development/iteration_plan-1.md)

---

**作成日**: 2025-11-10
**最終更新**: 2025-11-10
**バージョン**: 1.0
