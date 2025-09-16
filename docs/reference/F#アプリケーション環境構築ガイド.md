# F# アプリケーション環境構築ガイド

## 概要

本ガイドは「Domain Modeling Made Functional」の F# による関数型ドメインモデリングを基に、ASP.NET Core 最小 API を組み合わせた注文受付システムの開発環境構築手順を説明します。

## 前提条件

- Windows 10/11 または macOS 10.15+ または Linux
- インターネット接続環境
- 管理者権限（インストール時）

## 1. 基本環境のセットアップ

### 1.1 .NET 9.0 SDK のインストール

#### Windows の場合

1. [.NET 公式サイト](https://dotnet.microsoft.com/download) から .NET 9.0 SDK をダウンロード
2. インストーラーを実行し、指示に従ってインストール
3. コマンドプロンプトまたは PowerShell で確認：

```powershell
dotnet --version
```

#### macOS の場合

```bash
# Homebrew を使用
brew install --cask dotnet

# または公式インストーラーを使用
# https://dotnet.microsoft.com/download からダウンロード
```

#### Linux の場合

```bash
# Ubuntu/Debian の場合
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update && sudo apt-get install -y dotnet-sdk-9.0
```

### 1.2 インストールの確認

```bash
dotnet --info
```

以下の出力が表示されることを確認：
- .NET SDK: 9.0.x
- Runtime: Microsoft.NETCore.App 9.0.x

## 2. 開発ツールのセットアップ

### 2.1 推奨 IDE

#### Visual Studio Code + Ionide（推奨）

1. [Visual Studio Code](https://code.visualstudio.com/) をインストール
2. Ionide 拡張機能をインストール：
   ```bash
   code --install-extension Ionide.Ionide-fsharp
   ```

3. 推奨拡張機能の追加インストール：
   ```bash
   code --install-extension ms-dotnettools.vscode-dotnet-runtime
   code --install-extension humao.rest-client
   code --install-extension ms-vscode.vscode-json
   ```

#### JetBrains Rider（代替案）

- [Rider](https://www.jetbrains.com/rider/) をダウンロード・インストール
- F# プラグインが標準で含まれているため追加設定不要

### 2.2 F# 特有の設定

#### .editorconfig ファイルの作成

プロジェクトルートに `.editorconfig` ファイルを作成：

```ini
root = true

[*]
charset = utf-8
end_of_line = crlf
insert_final_newline = true
indent_style = space
indent_size = 4
trim_trailing_whitespace = true

[*.fs]
indent_size = 4

[*.fsx]
indent_size = 4

[*.{json,yml,yaml}]
indent_size = 2
```

## 3. プロジェクト構造のセットアップ

### 3.1 ソリューションとプロジェクトの作成

```bash
# ソリューションの作成
dotnet new sln -n DomainModelingMadeFunctional

# ディレクトリ構造の作成
mkdir src
mkdir tests
mkdir docs

# メインプロジェクト（ドメインライブラリ）
cd src
dotnet new classlib -lang "F#" -n OrderTaking

# Web API プロジェクト
dotnet new web -lang "F#" -n OrderTaking.WebApi

# テストプロジェクト
cd ../tests
dotnet new xunit -lang "F#" -n OrderTaking.Tests

# ソリューションにプロジェクトを追加
cd ..
dotnet sln add src/OrderTaking/OrderTaking.fsproj
dotnet sln add src/OrderTaking.WebApi/OrderTaking.WebApi.fsproj
dotnet sln add tests/OrderTaking.Tests/OrderTaking.Tests.fsproj
```

### 3.2 プロジェクト構造

```
DomainModelingMadeFunctional/
├── src/
│   ├── OrderTaking/                    # ドメインライブラリ
│   │   ├── Common.SimpleTypes.fs       # 基本型定義
│   │   ├── Common.CompoundTypes.fs     # 複合型定義
│   │   ├── PlaceOrder.PublicTypes.fs   # パブリック型
│   │   ├── PlaceOrder.Implementation.fs # ビジネスロジック
│   │   ├── PlaceOrder.Dto.fs           # データ転送オブジェクト
│   │   └── PlaceOrder.Api.fs           # API 層
│   └── OrderTaking.WebApi/             # Web API プロジェクト
│       ├── Program.fs                  # エントリーポイント
│       └── Controllers/                # API コントローラー
├── tests/
│   └── OrderTaking.Tests/              # テストプロジェクト
├── docs/                               # ドキュメント
└── DomainModelingMadeFunctional.sln    # ソリューション
```

## 4. 依存関係の設定

### 4.1 ドメインライブラリ（OrderTaking）

`src/OrderTaking/OrderTaking.fsproj` を編集：

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="Common.SimpleTypes.fs" />
    <Compile Include="Common.CompoundTypes.fs" />
    <Compile Include="PlaceOrder.PublicTypes.fs" />
    <Compile Include="PlaceOrder.Implementation.fs" />
    <Compile Include="PlaceOrder.Dto.fs" />
    <Compile Include="PlaceOrder.Api.fs" />
  </ItemGroup>

</Project>
```

### 4.2 Web API プロジェクト（OrderTaking.WebApi）

`src/OrderTaking.WebApi/OrderTaking.WebApi.fsproj` を編集：

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="Program.fs" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.3" />
    <PackageReference Include="NSwag.AspNetCore" Version="14.2.0" />
    <PackageReference Include="System.Text.Json" Version="5.0.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../OrderTaking/OrderTaking.fsproj" />
  </ItemGroup>

</Project>
```

### 4.3 テストプロジェクト（OrderTaking.Tests）

`tests/OrderTaking.Tests/OrderTaking.Tests.fsproj` を編集：

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <GenerateDocumentationFile>false</GenerateDocumentationFile>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="Tests.fs" />
    <Compile Include="Program.fs" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="FsUnit.xUnit" Version="6.0.1" />
    <PackageReference Include="coverlet.collector" Version="6.0.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../../src/OrderTaking/OrderTaking.fsproj" />
  </ItemGroup>

</Project>
```

## 5. F# 特有の設定

### 5.1 F# 9.0 機能の有効化

各プロジェクトファイルで以下の設定を確認：

```xml
<PropertyGroup>
  <TargetFramework>net9.0</TargetFramework>
  <Nullable>enable</Nullable>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>
```

### 5.2 F# ファイルの編集順序

F# はコンパイル順序が重要です。依存関係を考慮した順序で `<Compile Include="">` を設定してください。

## 6. 開発ツールの設定

### 6.1 テスト自動化の設定

#### test-automation.json の作成

```json
{
  "testFramework": "xunit",
  "testProjects": [
    "tests/OrderTaking.Tests/OrderTaking.Tests.fsproj"
  ],
  "coverage": {
    "enabled": true,
    "tool": "coverlet",
    "formats": ["html", "cobertura"]
  },
  "linting": {
    "tool": "FSharpLint",
    "configFile": "fsharplint.json"
  }
}
```

#### fsharplint.json の作成

```json
{
  "typographyRules": {
    "indentation": {
      "enabled": true,
      "numberOfIndentationSpaces": 4
    },
    "maxLinesInFile": {
      "enabled": true,
      "maxLines": 500
    }
  },
  "conventionRules": {
    "naming": {
      "enabled": true,
      "rules": [
        {
          "pattern": "^[A-Z][a-zA-Z0-9]*$",
          "applies": "modules"
        }
      ]
    }
  }
}
```

### 6.2 コードフォーマッターの設定

#### .config/dotnet-tools.json

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "fantomas": {
      "version": "6.3.9",
      "commands": [
        "fantomas"
      ]
    },
    "fsharplint": {
      "version": "0.21.7",
      "commands": [
        "fsharplint"
      ]
    }
  }
}
```

#### .editorconfig での Fantomas 設定

```ini
[*.fs]
fsharp_space_before_parameter = true
fsharp_space_before_lowercase_invocation = true
fsharp_space_before_uppercase_invocation = false
fsharp_space_before_class_constructor = false
fsharp_space_before_member = false
fsharp_space_before_colon = false
fsharp_space_after_comma = true
fsharp_space_before_semicolon = false
fsharp_space_after_semicolon = true
fsharp_indent_on_try_with = false
fsharp_space_around_delimiter = true
```

## 7. Git 設定

### 7.1 .gitignore ファイル

```gitignore
## Ignore Visual Studio temporary files, build results, and
## files generated by popular Visual Studio add-ons.

# User-specific files
*.suo
*.user
*.userosscache
*.sln.docstates

# Build results
[Dd]ebug/
[Dd]ebugPublic/
[Rr]elease/
[Rr]eleases/
x64/
x86/
build/
bld/
[Bb]in/
[Oo]bj/

# .NET Core
project.lock.json
project.fragment.lock.json
artifacts/
**/Properties/launchSettings.json

# StyleCop
StyleCopReport.xml

# Files built by Visual Studio
*_i.c
*_p.c
*_i.h
*.ilk
*.meta
*.obj
*.pch
*.pdb
*.pgc
*.pgd
*.rsp
*.sbr
*.tlb
*.tli
*.tlh
*.tmp
*.tmp_proj
*.log
*.vspscc
*.vssscc
.builds
*.pidb
*.svclog
*.scc

# Chutzpah Test files
_Chutzpah*

# Visual C++ cache files
ipch/
*.aps
*.ncb
*.opendb
*.opensdf
*.sdf
*.cachefile
*.VC.db
*.VC.VC.opendb

# Visual Studio profiler
*.psess
*.vsp
*.vspx
*.sap

# TFS 2012 Local Workspace
$tf/

# Guidance Automation Toolkit
*.gpState

# ReSharper is a .NET coding add-in
_ReSharper*/
*.[Rr]e[Ss]harper
*.DotSettings.user

# JustCode is a .NET coding add-in
.JustCode

# TeamCity is a build add-in
_TeamCity*

# DotCover is a Code Coverage Tool
*.dotCover

# NCrunch
_NCrunch_*
.*crunch*.local.xml
nCrunchTemp_*

# MightyMoose
*.mm.*
AutoTest.Net/

# Web workbench (sass)
.sass-cache/

# Installshield output folder
[Ee]xpress/

# DocProject is a documentation generator add-in
DocProject/buildhelp/
DocProject/Help/*.HxT
DocProject/Help/*.HxC
DocProject/Help/*.hhc
DocProject/Help/*.hhk
DocProject/Help/*.hhp
DocProject/Help/Html2
DocProject/Help/html

# Click-Once directory
publish/

# Publish Web Output
*.[Pp]ublish.xml
*.azurePubxml
# TODO: Comment the next line if you want to checkin your web deploy settings
# but database connection strings (with potential passwords) will be unencrypted
*.pubxml
*.publishproj

# Microsoft Azure Web App publish settings. Comment the next line if you want to
# checkin your Azure Web App publish settings, but sensitive information contained
# in these files may be inadvertently shared
*.azurePubxml

# NuGet Packages
*.nupkg
# The packages folder can be ignored because of Package Restore
**/packages/*
# except build/, which is used as an MSBuild target.
!**/packages/build/
# Uncomment if necessary however generally it will be regenerated when needed
#!**/packages/repositories.config
# NuGet v3's project.json files produces more ignorable files
*.nuget.props
*.nuget.targets

# Microsoft Azure Build Output
csx/
*.build.csdef

# Microsoft Azure Emulator
ecf/
rcf/

# Windows Store app package directories and files
AppPackages/
BundleArtifacts/
Package.StoreAssociation.xml
_pkginfo.txt

# Visual Studio cache files
# files ending in .cache can be ignored
*.[Cc]ache
# but keep track of directories ending in .cache
!*.[Cc]ache/

# Others
ClientBin/
~$*
*~
*.dbmdl
*.dbproj.schemaview
*.pfx
*.publishsettings
orleans.codegen.cs

# Since there are multiple workflows, uncomment the next line to ignore bower_components
# (https://github.com/github/gitignore/pull/1529#issuecomment-104372622)
#bower_components/

# RIA/Silverlight projects
Generated_Code/

# Backup & report files from converting an old project file
# to a newer Visual Studio version. Backup files are not needed,
# because we have git ;-)
_UpgradeReport_Files/
Backup*/
UpgradeLog*.XML
UpgradeLog*.htm

# SQL Server files
*.mdf
*.ldf

# Business Intelligence projects
*.rdl.data
*.bim.layout
*.bim_*.settings

# Microsoft Fakes
FakesAssemblies/

# GhostDoc plugin setting file
*.GhostDoc.xml

# Node.js Tools for Visual Studio
.ntvs_analysis.dat
node_modules/

# Typescript v1 declaration files
typings/

# Visual Studio 6 build log
*.plg

# Visual Studio 6 workspace options file
*.opt

# Visual Studio 6 auto-generated workspace file (contains which files were open etc.)
*.vbw

# Visual Studio LightSwitch build output
**/*.HTMLClient/GeneratedArtifacts
**/*.DesktopClient/GeneratedArtifacts
**/*.DesktopClient/ModelManifest.xml
**/*.Server/GeneratedArtifacts
**/*.Server/ModelManifest.xml
_Pvt_Extensions

# Paket dependency manager
.paket/paket.exe
paket-files/

# FAKE - F# Make
.fake/

# JetBrains Rider
.idea/
*.sln.iml

# CodeRush
.cr/

# Python Tools for Visual Studio (PTVS)
__pycache__/
*.pyc

# Cake - Uncomment if you are using it
# tools/**
# !tools/packages.config

# Telerik's JustMock configuration file
*.jmconfig

# BizTalk build output
*.btp.cs
*.btm.cs
*.odx.cs
*.xsd.cs

# F# project-specific
.ionide/
```

### 7.2 初期コミット

```bash
git init
git add .
git commit -m "feat: initial F# project setup with domain modeling structure"
```

## 8. ビルドとテストの実行

### 8.1 依存関係の復元

```bash
dotnet restore
```

### 8.2 ビルド

```bash
dotnet build
```

### 8.3 テスト実行

```bash
dotnet test
```

### 8.4 フォーマットとリント

```bash
# コードフォーマット
dotnet fantomas . --recurse

# リント実行
dotnet fsharplint lint --format msbuild
```

## 9. サンプルコードの実装

### 9.1 基本型の定義（Common.SimpleTypes.fs）

```fsharp
namespace OrderTaking.Common

// 制約付き基本型
type String50 = private String50 of string

module String50 =
    let create str =
        if String.length str <= 50 && not (String.IsNullOrWhiteSpace str) then
            Ok (String50 str)
        else
            Error "String must be 1-50 characters"

    let value (String50 str) = str

type EmailAddress = private EmailAddress of string

module EmailAddress =
    let create str =
        if str |> String.contains "@" && str |> String.contains "." then
            Ok (EmailAddress str)
        else
            Error "Invalid email format"

    let value (EmailAddress email) = email

type OrderId = private OrderId of string

module OrderId =
    let create str =
        if not (String.IsNullOrWhiteSpace str) then
            Ok (OrderId str)
        else
            Error "OrderId cannot be empty"

    let value (OrderId id) = id
```

### 9.2 Program.fs（最小 API）

```fsharp
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

let builder = WebApplication.CreateBuilder()

// サービス設定
builder.Services.AddOpenApi() |> ignore

let app = builder.Build()

// 開発環境でのみ Swagger UI を有効化
if app.Environment.IsDevelopment() then
    app.MapOpenApi() |> ignore

// ヘルスチェックエンドポイント
app.MapGet("/health", fun () -> task { return "OK" }) |> ignore

// 注文エンドポイント（プレースホルダー）
app.MapGet("/orders", fun () -> task { return [] }) |> ignore

app.Run()
```

## 10. 動作確認

### 10.1 アプリケーションの起動

```bash
cd src/OrderTaking.WebApi
dotnet run
```

### 10.2 エンドポイントの確認

ブラウザまたは curl で以下にアクセス：

```bash
# ヘルスチェック
curl http://localhost:5000/health

# Swagger UI（開発環境）
# http://localhost:5000/swagger にアクセス
```

## 11. トラブルシューティング

### 11.1 よくある問題

#### F# コンパイルエラー

```
error FS0039: The field, constructor or member 'X' is not defined.
```

**解決策**: ファイルの順序を確認し、依存関係順に並べる

#### NuGet パッケージエラー

```bash
# パッケージキャッシュのクリア
dotnet nuget locals all --clear

# 依存関係の再復元
dotnet restore --force
```

### 11.2 パフォーマンス最適化

- プロジェクトファイル内でのコンパイル順序最適化
- 不要な依存関係の除去
- F# Interactive (FSI) の活用

## まとめ

本ガイドに従って環境構築を行うことで、F# による関数型ドメインモデリングと ASP.NET Core 最小 API を組み合わせた開発環境が整います。

次のステップとして、以下のドキュメントを参照してください：

- [ドメインモデル設計ガイド](ドメインモデル設計ガイド.md)
- [テスト戦略ガイド](テスト戦略ガイド.md)
- [アーキテクチャ設計ガイド](アーキテクチャ設計ガイド.md)

環境構築で問題が発生した場合は、本ガイドのトラブルシューティングセクションを参照するか、プロジェクトの Issue トラッカーに報告してください。