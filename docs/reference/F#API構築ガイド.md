# F# API 構築ガイド

## 概要

本ガイドは、F# を使用した ASP.NET Core 最小 API の構築方法を、関数型ドメインモデリングの原則に基づいて解説します。「Domain Modeling Made Functional」のアプローチと ASP.NET Core の最小 API パターンを組み合わせ、型安全で保守性の高い API を構築する手法を提供します。

## 目次

1. [環境セットアップ](#環境セットアップ)
2. [プロジェクト構造](#プロジェクト構造)
3. [ドメインモデル設計](#ドメインモデル設計)
4. [API エンドポイント設計](#api-エンドポイント設計)
5. [エラーハンドリング](#エラーハンドリング)
6. [データベース統合](#データベース統合)
7. [テスト戦略](#テスト戦略)
8. [API ドキュメント生成](#api-ドキュメント生成)
9. [ベストプラクティス](#ベストプラクティス)

## 環境セットアップ

### 必須コンポーネント

- .NET 9.0 SDK
- Visual Studio Code + Ionide 拡張機能 または JetBrains Rider
- F# コンパイラ（.NET SDK に含まれる）

### プロジェクト作成

```bash
# ソリューション作成
dotnet new sln -o OrderTakingApi
cd OrderTakingApi

# F# Web API プロジェクト作成
dotnet new web -lang "F#" -o src/OrderTaking.WebApi
dotnet new gitignore

# ソリューションにプロジェクト追加
dotnet sln add src/OrderTaking.WebApi/OrderTaking.WebApi.fsproj

# 初期ビルド確認
dotnet build
```

### プロジェクト設定

**OrderTaking.WebApi.fsproj**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>

  <ItemGroup>
    <!-- F# ファイルの順序は重要 -->
    <Compile Include="Common/SimpleTypes.fs" />
    <Compile Include="Common/CompoundTypes.fs" />
    <Compile Include="Domain/OrderTypes.fs" />
    <Compile Include="Domain/OrderWorkflows.fs" />
    <Compile Include="Infrastructure/Database.fs" />
    <Compile Include="Api/OrderEndpoints.fs" />
    <Compile Include="Program.fs" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.3" />
    <PackageReference Include="Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore" Version="9.0.3" />
    <PackageReference Include="NSwag.AspNetCore" Version="14.2.0" />
    <PackageReference Include="System.Text.Json" Version="8.0.0" />
  </ItemGroup>
</Project>
```

## プロジェクト構造

```
src/OrderTaking.WebApi/
├── Common/                    # 共通型定義
│   ├── SimpleTypes.fs         # 基本制約付き型
│   └── CompoundTypes.fs       # 複合型
├── Domain/                    # ドメインロジック
│   ├── OrderTypes.fs          # 注文関連型
│   └── OrderWorkflows.fs      # ビジネスワークフロー
├── Infrastructure/            # インフラストラクチャ
│   └── Database.fs           # データアクセス
├── Api/                      # API エンドポイント
│   └── OrderEndpoints.fs     # 注文 API
├── Program.fs                # アプリケーション エントリーポイント
├── appsettings.json         # アプリケーション設定
└── OrderTaking.WebApi.fsproj # プロジェクトファイル
```

## ドメインモデル設計

### 制約付き基本型

**Common/SimpleTypes.fs**

```fsharp
namespace OrderTaking.Common

open System

/// 50文字以下の文字列
type String50 = private String50 of string

module String50 =
    let create str =
        if String.IsNullOrWhiteSpace(str) then
            Error "String50は必須です"
        elif str.Length > 50 then
            Error "String50は50文字以下である必要があります"
        else
            Ok (String50 str)

    let value (String50 str) = str

/// メールアドレス
type EmailAddress = private EmailAddress of string

module EmailAddress =
    let create str =
        if String.IsNullOrWhiteSpace(str) then
            Error "メールアドレスは必須です"
        elif not (str.Contains("@")) then
            Error "有効なメールアドレスを入力してください"
        else
            Ok (EmailAddress str)

    let value (EmailAddress email) = email

/// 注文ID
type OrderId = private OrderId of string

module OrderId =
    let create str =
        if String.IsNullOrWhiteSpace(str) then
            Error "注文IDは必須です"
        elif str.Length > 10 then
            Error "注文IDは10文字以下である必要があります"
        else
            Ok (OrderId str)

    let value (OrderId id) = id

/// 商品コード
type ProductCode =
    | Widget of WidgetCode
    | Gizmo of GizmoCode

and WidgetCode = private WidgetCode of string
and GizmoCode = private GizmoCode of string

module WidgetCode =
    let create str =
        if String.IsNullOrWhiteSpace(str) then
            Error "WidgetCodeは必須です"
        elif not (str.StartsWith("W") && str.Length = 5) then
            Error "WidgetCodeは'W'で始まる5文字である必要があります"
        else
            Ok (WidgetCode str)

    let value (WidgetCode code) = code

module GizmoCode =
    let create str =
        if String.IsNullOrWhiteSpace(str) then
            Error "GizmoCodeは必須です"
        elif not (str.StartsWith("G") && str.Length = 4) then
            Error "GizmoCodeは'G'で始まる4文字である必要があります"
        else
            Ok (GizmoCode str)

    let value (GizmoCode code) = code

/// 数量
type OrderQuantity =
    | Unit of UnitQuantity
    | Kilogram of KilogramQuantity

and UnitQuantity = private UnitQuantity of int
and KilogramQuantity = private KilogramQuantity of decimal

module UnitQuantity =
    let create qty =
        if qty < 1 || qty > 1000 then
            Error "Unit数量は1-1000の範囲である必要があります"
        else
            Ok (UnitQuantity qty)

    let value (UnitQuantity qty) = qty

module KilogramQuantity =
    let create qty =
        if qty < 0.05m || qty > 100.00m then
            Error "Kilogram数量は0.05-100.00の範囲である必要があります"
        else
            Ok (KilogramQuantity qty)

    let value (KilogramQuantity qty) = qty
```

### 複合型とドメインエンティティ

**Domain/OrderTypes.fs**

```fsharp
namespace OrderTaking.Domain

open OrderTaking.Common

/// 顧客情報
type CustomerInfo = {
    Name: String50
    Email: EmailAddress
}

/// 住所
type Address = {
    AddressLine1: String50
    AddressLine2: String50 option
    City: String50
    ZipCode: String50
}

/// 注文明細
type OrderLine = {
    OrderLineId: string
    ProductCode: ProductCode
    Quantity: OrderQuantity
}

/// 未検証注文（外部入力）
type UnvalidatedOrder = {
    OrderId: string
    CustomerInfo: UnvalidatedCustomerInfo
    ShippingAddress: UnvalidatedAddress
    BillingAddress: UnvalidatedAddress
    Lines: UnvalidatedOrderLine list
}

and UnvalidatedCustomerInfo = {
    FirstName: string
    LastName: string
    EmailAddress: string
}

and UnvalidatedAddress = {
    AddressLine1: string
    AddressLine2: string
    City: string
    ZipCode: string
}

and UnvalidatedOrderLine = {
    OrderLineId: string
    ProductCode: string
    Quantity: decimal
}

/// 検証済み注文
type ValidatedOrder = {
    OrderId: OrderId
    CustomerInfo: CustomerInfo
    ShippingAddress: Address
    BillingAddress: Address
    Lines: ValidatedOrderLine list
}

and ValidatedOrderLine = {
    OrderLineId: string
    ProductCode: ProductCode
    Quantity: OrderQuantity
}

/// 価格付き注文
type PricedOrder = {
    OrderId: OrderId
    CustomerInfo: CustomerInfo
    ShippingAddress: Address
    BillingAddress: Address
    Lines: PricedOrderLine list
    AmountToBill: decimal
}

and PricedOrderLine = {
    OrderLineId: string
    ProductCode: ProductCode
    Quantity: OrderQuantity
    LinePrice: decimal
}

/// ドメインイベント
type OrderEvent =
    | OrderPlaced of PricedOrder
    | BillableOrderPlaced of BillableOrderPlaced
    | AcknowledgmentSent of AcknowledgmentSent

and BillableOrderPlaced = {
    OrderId: OrderId
    BillingAddress: Address
    AmountToBill: decimal
}

and AcknowledgmentSent = {
    OrderId: OrderId
    EmailAddress: EmailAddress
}

/// ドメインエラー
type ValidationError =
    | FieldIsMissing of string
    | FieldOutOfRange of string * decimal * decimal
    | FieldInvalidFormat of string

type PricingError =
    | ProductNotFound of ProductCode

type PlaceOrderError =
    | Validation of ValidationError
    | Pricing of PricingError
    | RemoteService of string
```

### ビジネスワークフロー

**Domain/OrderWorkflows.fs**

```fsharp
namespace OrderTaking.Domain

open OrderTaking.Common

// 外部依存の抽象化
type CheckProductCodeExists = ProductCode -> bool
type GetProductPrice = ProductCode -> decimal option
type CheckAddressExists = UnvalidatedAddress -> AsyncResult<Address, ValidationError>
type SendOrderAcknowledgment = PricedOrder -> AsyncResult<AcknowledgmentSent, string>

// ワークフローの関数型定義
type ValidateOrder = CheckProductCodeExists -> CheckAddressExists -> UnvalidatedOrder -> AsyncResult<ValidatedOrder, ValidationError>
type PriceOrder = GetProductPrice -> ValidatedOrder -> Result<PricedOrder, PricingError>
type AcknowledgeOrder = SendOrderAcknowledgment -> PricedOrder -> AsyncResult<AcknowledgmentSent option, string>
type CreateEvents = PricedOrder -> AcknowledgmentSent option -> OrderEvent list

// メインワークフロー
type PlaceOrderWorkflow = UnvalidatedOrder -> AsyncResult<OrderEvent list, PlaceOrderError>

module OrderWorkflows =

    // 注文検証の実装
    let validateOrder: ValidateOrder =
        fun checkProductExists checkAddressExists unvalidatedOrder ->
            asyncResult {
                // 顧客情報の検証
                let! customerName =
                    String50.create (unvalidatedOrder.CustomerInfo.FirstName + " " + unvalidatedOrder.CustomerInfo.LastName)
                    |> Result.mapError (fun msg -> FieldInvalidFormat msg)
                    |> AsyncResult.ofResult

                let! customerEmail =
                    EmailAddress.create unvalidatedOrder.CustomerInfo.EmailAddress
                    |> Result.mapError (fun msg -> FieldInvalidFormat msg)
                    |> AsyncResult.ofResult

                // 住所の検証
                let! shippingAddress = checkAddressExists unvalidatedOrder.ShippingAddress
                let! billingAddress = checkAddressExists unvalidatedOrder.BillingAddress

                // 注文明細の検証
                let! validatedLines =
                    unvalidatedOrder.Lines
                    |> List.map (validateOrderLine checkProductExists)
                    |> AsyncResult.sequence

                let! orderId =
                    OrderId.create unvalidatedOrder.OrderId
                    |> Result.mapError (fun msg -> FieldInvalidFormat msg)
                    |> AsyncResult.ofResult

                return {
                    OrderId = orderId
                    CustomerInfo = {
                        Name = customerName
                        Email = customerEmail
                    }
                    ShippingAddress = shippingAddress
                    BillingAddress = billingAddress
                    Lines = validatedLines
                }
            }

    // 注文明細検証
    and validateOrderLine checkProductExists (line: UnvalidatedOrderLine) =
        asyncResult {
            // 商品コードの検証とパース
            let! productCode = parseProductCode line.ProductCode

            // 商品の存在確認
            let productExists = checkProductExists productCode
            if not productExists then
                return! AsyncResult.ofError (FieldIsMissing "ProductCode not found")

            // 数量の検証
            let! quantity = parseQuantity productCode line.Quantity

            return {
                OrderLineId = line.OrderLineId
                ProductCode = productCode
                Quantity = quantity
            }
        }

    // 商品コードパース
    and parseProductCode (codeStr: string) =
        if codeStr.StartsWith("W") then
            WidgetCode.create codeStr
            |> Result.map Widget
            |> Result.mapError (fun msg -> FieldInvalidFormat msg)
            |> AsyncResult.ofResult
        elif codeStr.StartsWith("G") then
            GizmoCode.create codeStr
            |> Result.map Gizmo
            |> Result.mapError (fun msg -> FieldInvalidFormat msg)
            |> AsyncResult.ofResult
        else
            AsyncResult.ofError (FieldInvalidFormat "無効な商品コード形式")

    // 数量パース
    and parseQuantity productCode qty =
        match productCode with
        | Widget _ ->
            UnitQuantity.create (int qty)
            |> Result.map Unit
            |> Result.mapError (fun msg -> FieldOutOfRange("Unit", qty, qty))
            |> AsyncResult.ofResult
        | Gizmo _ ->
            KilogramQuantity.create qty
            |> Result.map Kilogram
            |> Result.mapError (fun msg -> FieldOutOfRange("Kilogram", qty, qty))
            |> AsyncResult.ofResult

    // 価格計算の実装
    let priceOrder: PriceOrder =
        fun getProductPrice validatedOrder ->
            let pricedLines =
                validatedOrder.Lines
                |> List.map (fun line ->
                    match getProductPrice line.ProductCode with
                    | Some price ->
                        let qty =
                            match line.Quantity with
                            | Unit (UnitQuantity.value -> qty) -> decimal qty
                            | Kilogram (KilogramQuantity.value -> qty) -> qty
                        Ok {
                            OrderLineId = line.OrderLineId
                            ProductCode = line.ProductCode
                            Quantity = line.Quantity
                            LinePrice = price * qty
                        }
                    | None ->
                        Error (ProductNotFound line.ProductCode)
                )
                |> Result.sequence

            pricedLines
            |> Result.map (fun lines ->
                let totalAmount = lines |> List.sumBy (_.LinePrice)
                {
                    OrderId = validatedOrder.OrderId
                    CustomerInfo = validatedOrder.CustomerInfo
                    ShippingAddress = validatedOrder.ShippingAddress
                    BillingAddress = validatedOrder.BillingAddress
                    Lines = lines
                    AmountToBill = totalAmount
                }
            )

    // 確認送信の実装
    let acknowledgeOrder: AcknowledgeOrder =
        fun sendAcknowledgment pricedOrder ->
            asyncResult {
                let! acknowledgment = sendAcknowledgment pricedOrder
                return Some acknowledgment
            }
            |> AsyncResult.catch (fun _ -> AsyncResult.ofResult (Ok None))

    // イベント生成の実装
    let createEvents: CreateEvents =
        fun pricedOrder acknowledgmentOpt ->
            let events = [
                OrderPlaced pricedOrder

                if pricedOrder.AmountToBill > 0m then
                    BillableOrderPlaced {
                        OrderId = pricedOrder.OrderId
                        BillingAddress = pricedOrder.BillingAddress
                        AmountToBill = pricedOrder.AmountToBill
                    }

                match acknowledgmentOpt with
                | Some ack -> AcknowledgmentSent ack
                | None -> ()
            ]
            events |> List.choose id

    // メインワークフロー実装
    let placeOrder
        (checkProductExists: CheckProductCodeExists)
        (getProductPrice: GetProductPrice)
        (checkAddressExists: CheckAddressExists)
        (sendAcknowledgment: SendOrderAcknowledgment)
        : PlaceOrderWorkflow =
        fun unvalidatedOrder ->
            asyncResult {
                // 検証
                let! validatedOrder =
                    validateOrder checkProductExists checkAddressExists unvalidatedOrder
                    |> AsyncResult.mapError Validation

                // 価格計算
                let! pricedOrder =
                    priceOrder getProductPrice validatedOrder
                    |> Result.mapError Pricing
                    |> AsyncResult.ofResult

                // 確認送信
                let! acknowledgment =
                    acknowledgeOrder sendAcknowledgment pricedOrder
                    |> AsyncResult.mapError RemoteService

                // イベント生成
                let events = createEvents pricedOrder acknowledgment

                return events
            }

// 便利なヘルパーモジュール
and AsyncResult =
    let ofResult result =
        async { return result }

    let ofError error =
        async { return Error error }

    let map f asyncResult =
        async {
            let! result = asyncResult
            return Result.map f result
        }

    let mapError f asyncResult =
        async {
            let! result = asyncResult
            return Result.mapError f result
        }

    let bind f asyncResult =
        async {
            let! result = asyncResult
            match result with
            | Ok value -> return! f value
            | Error error -> return Error error
        }

    let sequence asyncResults =
        let rec loop acc remaining =
            async {
                match remaining with
                | [] -> return Ok (List.rev acc)
                | head :: tail ->
                    let! headResult = head
                    match headResult with
                    | Ok value ->
                        return! loop (value :: acc) tail
                    | Error error ->
                        return Error error
            }
        loop [] asyncResults

    let catch handler asyncResult =
        async {
            try
                return! asyncResult
            with
            | ex -> return! handler ex
        }

type AsyncResultBuilder() =
    member _.Return(value) = AsyncResult.ofResult (Ok value)
    member _.ReturnFrom(asyncResult) = asyncResult
    member _.Bind(asyncResult, f) = AsyncResult.bind f asyncResult
    member _.Zero() = AsyncResult.ofResult (Ok ())

let asyncResult = AsyncResultBuilder()
```

## API エンドポイント設計

**Api/OrderEndpoints.fs**

```fsharp
namespace OrderTaking.Api

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System.Text.Json
open OrderTaking.Domain
open OrderTaking.Common

// DTOの定義
type PlaceOrderRequest = {
    OrderId: string
    CustomerInfo: CustomerInfoDto
    ShippingAddress: AddressDto
    BillingAddress: AddressDto
    Lines: OrderLineDto list
}

and CustomerInfoDto = {
    FirstName: string
    LastName: string
    EmailAddress: string
}

and AddressDto = {
    AddressLine1: string
    AddressLine2: string
    City: string
    ZipCode: string
}

and OrderLineDto = {
    OrderLineId: string
    ProductCode: string
    Quantity: decimal
}

type PlaceOrderResponse = {
    OrderId: string
    Message: string
}

type ErrorResponse = {
    Error: string
    Details: string list
}

module OrderEndpoints =

    // DTOからドメインオブジェクトへの変換
    let toDomainOrder (request: PlaceOrderRequest) : UnvalidatedOrder = {
        OrderId = request.OrderId
        CustomerInfo = {
            FirstName = request.CustomerInfo.FirstName
            LastName = request.CustomerInfo.LastName
            EmailAddress = request.CustomerInfo.EmailAddress
        }
        ShippingAddress = {
            AddressLine1 = request.ShippingAddress.AddressLine1
            AddressLine2 = request.ShippingAddress.AddressLine2
            City = request.ShippingAddress.City
            ZipCode = request.ShippingAddress.ZipCode
        }
        BillingAddress = {
            AddressLine1 = request.BillingAddress.AddressLine1
            AddressLine2 = request.BillingAddress.AddressLine2
            City = request.BillingAddress.City
            ZipCode = request.BillingAddress.ZipCode
        }
        Lines = request.Lines |> List.map (fun line -> {
            OrderLineId = line.OrderLineId
            ProductCode = line.ProductCode
            Quantity = line.Quantity
        })
    }

    // エラーメッセージの生成
    let formatError (error: PlaceOrderError) =
        match error with
        | Validation (FieldIsMissing field) ->
            { Error = "検証エラー"; Details = [$"必須フィールド '{field}' が不足しています"] }
        | Validation (FieldOutOfRange (field, min, max)) ->
            { Error = "検証エラー"; Details = [$"フィールド '{field}' の値が範囲外です（{min}-{max}）"] }
        | Validation (FieldInvalidFormat field) ->
            { Error = "検証エラー"; Details = [$"フィールド '{field}' の形式が不正です"] }
        | Pricing (ProductNotFound productCode) ->
            { Error = "価格計算エラー"; Details = [$"商品コード '{productCode}' が見つかりません"] }
        | RemoteService error ->
            { Error = "外部サービスエラー"; Details = [error] }

    // 注文受付エンドポイントの実装
    let addOrderEndpoints
        (app: WebApplication)
        (placeOrderWorkflow: PlaceOrderWorkflow) =

        // POST /api/orders
        app.MapPost("/api/orders", Func<PlaceOrderRequest, Task<IResult>>(fun request ->
            task {
                let unvalidatedOrder = toDomainOrder request

                let! result = placeOrderWorkflow unvalidatedOrder

                match result with
                | Ok events ->
                    let orderPlacedEvent =
                        events
                        |> List.pick (function
                            | OrderPlaced pricedOrder -> Some pricedOrder
                            | _ -> None)

                    let response = {
                        OrderId = OrderId.value orderPlacedEvent.OrderId
                        Message = "注文が正常に受け付けられました"
                    }
                    return Results.Ok(response)

                | Error error ->
                    let errorResponse = formatError error
                    return Results.BadRequest(errorResponse)
            }
        )) |> ignore

        // GET /api/orders/{orderId}
        app.MapGet("/api/orders/{orderId}", Func<string, IResult>(fun orderId ->
            // 実装は将来のバージョンで追加
            Results.NotImplemented("注文照会機能は未実装です")
        )) |> ignore

        app
```

## エラーハンドリング

**Result 型の活用例**

```fsharp
// Result型の拡張
module Result =
    let sequence results =
        let rec loop acc remaining =
            match remaining with
            | [] -> Ok (List.rev acc)
            | (Ok value) :: tail -> loop (value :: acc) tail
            | (Error error) :: _ -> Error error
        loop [] results

    let catch handler result =
        match result with
        | Ok value -> Ok value
        | Error error -> handler error

// グローバルエラーハンドリングミドルウェア
type ErrorHandlingMiddleware(next: RequestDelegate, logger: ILogger<ErrorHandlingMiddleware>) =
    member _.InvokeAsync(context: HttpContext) =
        task {
            try
                do! next.Invoke(context)
            with
            | :? ValidationException as ex ->
                context.Response.StatusCode <- 400
                let response = {| error = "Validation failed"; message = ex.Message |}
                do! context.Response.WriteAsync(JsonSerializer.Serialize(response))
            | ex ->
                logger.LogError(ex, "未処理の例外が発生しました")
                context.Response.StatusCode <- 500
                let response = {| error = "Internal server error"; message = "予期しないエラーが発生しました" |}
                do! context.Response.WriteAsync(JsonSerializer.Serialize(response))
        }
```

## データベース統合

**Infrastructure/Database.fs**

```fsharp
namespace OrderTaking.Infrastructure

open Microsoft.EntityFrameworkCore
open OrderTaking.Domain
open OrderTaking.Common

// Entity Framework モデル
type OrderEntity() =
    member val OrderId = "" with get, set
    member val CustomerName = "" with get, set
    member val CustomerEmail = "" with get, set
    member val ShippingAddress = "" with get, set
    member val BillingAddress = "" with get, set
    member val AmountToBill = 0m with get, set

type OrderLineEntity() =
    member val OrderLineId = "" with get, set
    member val OrderId = "" with get, set
    member val ProductCode = "" with get, set
    member val Quantity = 0m with get, set
    member val LinePrice = 0m with get, set

// DbContext
type OrderContext(options: DbContextOptions<OrderContext>) =
    inherit DbContext(options)

    [<DefaultValue>] val mutable orders: DbSet<OrderEntity>
    member this.Orders with get() = this.orders and set v = this.orders <- v

    [<DefaultValue>] val mutable orderLines: DbSet<OrderLineEntity>
    member this.OrderLines with get() = this.orderLines and set v = this.orderLines <- v

    override this.OnModelCreating(modelBuilder: ModelBuilder) =
        modelBuilder.Entity<OrderEntity>()
            .HasKey(fun o -> o.OrderId) |> ignore

        modelBuilder.Entity<OrderLineEntity>()
            .HasKey(fun ol -> ol.OrderLineId) |> ignore

// リポジトリ実装
type IOrderRepository =
    abstract member SaveOrder: PricedOrder -> Task<unit>
    abstract member GetOrder: OrderId -> Task<PricedOrder option>

type OrderRepository(context: OrderContext) =
    interface IOrderRepository with
        member _.SaveOrder(order: PricedOrder) =
            task {
                let orderEntity = OrderEntity()
                orderEntity.OrderId <- OrderId.value order.OrderId
                orderEntity.CustomerName <- String50.value order.CustomerInfo.Name
                orderEntity.CustomerEmail <- EmailAddress.value order.CustomerInfo.Email
                orderEntity.AmountToBill <- order.AmountToBill

                context.Orders.Add(orderEntity) |> ignore

                for line in order.Lines do
                    let lineEntity = OrderLineEntity()
                    lineEntity.OrderLineId <- line.OrderLineId
                    lineEntity.OrderId <- OrderId.value order.OrderId
                    lineEntity.ProductCode <- string line.ProductCode
                    lineEntity.Quantity <-
                        match line.Quantity with
                        | Unit qty -> decimal (UnitQuantity.value qty)
                        | Kilogram qty -> KilogramQuantity.value qty
                    lineEntity.LinePrice <- line.LinePrice

                    context.OrderLines.Add(lineEntity) |> ignore

                do! context.SaveChangesAsync() |> Task.ignore
            }

        member _.GetOrder(orderId: OrderId) =
            task {
                // 実装は将来のバージョンで追加
                return None
            }
```

## テスト戦略

**ユニットテスト例**

```fsharp
module OrderWorkflowTests

open NUnit.Framework
open OrderTaking.Domain
open OrderTaking.Common

[<TestFixture>]
type ValidateOrderTests() =

    let mockCheckProductExists = fun _ -> true
    let mockCheckAddressExists = fun _ -> AsyncResult.ofResult (Ok createValidAddress())

    [<Test>]
    member _.``有効な注文は正常に検証される``() =
        async {
            let unvalidatedOrder = createValidUnvalidatedOrder()

            let! result = OrderWorkflows.validateOrder
                mockCheckProductExists
                mockCheckAddressExists
                unvalidatedOrder

            match result with
            | Ok validatedOrder ->
                Assert.IsNotNull(validatedOrder)
                Assert.AreEqual("W1234", string validatedOrder.Lines.Head.ProductCode)
            | Error error ->
                Assert.Fail($"検証に失敗しました: {error}")
        }

    [<Test>]
    member _.``無効な商品コードはエラーになる``() =
        async {
            let invalidOrder = { createValidUnvalidatedOrder() with
                Lines = [{ OrderLineId = "1"; ProductCode = "INVALID"; Quantity = 1m }] }

            let! result = OrderWorkflows.validateOrder
                mockCheckProductExists
                mockCheckAddressExists
                invalidOrder

            match result with
            | Ok _ -> Assert.Fail("無効な商品コードが検証をパスしました")
            | Error _ -> Assert.Pass()
        }

// テスト用ヘルパー関数
module TestHelpers =
    let createValidUnvalidatedOrder() = {
        OrderId = "ORDER001"
        CustomerInfo = {
            FirstName = "太郎"
            LastName = "田中"
            EmailAddress = "taro@example.com"
        }
        ShippingAddress = createValidUnvalidatedAddress()
        BillingAddress = createValidUnvalidatedAddress()
        Lines = [{
            OrderLineId = "1"
            ProductCode = "W1234"
            Quantity = 5m
        }]
    }

    let createValidUnvalidatedAddress() = {
        AddressLine1 = "東京都渋谷区"
        AddressLine2 = ""
        City = "渋谷区"
        ZipCode = "12345"
    }
```

## API ドキュメント生成

**Swagger/OpenAPI 設定**

```fsharp
// Program.fs での設定
let configureSwagger (builder: WebApplicationBuilder) =
    builder.Services.AddEndpointsApiExplorer() |> ignore
    builder.Services.AddOpenApiDocument(fun config ->
        config.DocumentName <- "OrderTakingAPI"
        config.Title <- "Order Taking API v1"
        config.Version <- "v1"
        config.Description <- "F# による関数型ドメインモデリングベースの注文受付API"
    ) |> ignore

let configureSwaggerUI (app: WebApplication) =
    if app.Environment.IsDevelopment() then
        app.UseOpenApi() |> ignore
        app.UseSwaggerUi(fun config ->
            config.DocumentTitle <- "Order Taking API"
            config.Path <- "/swagger"
            config.DocumentPath <- "/swagger/{documentName}/swagger.json"
            config.DocExpansion <- "list"
        ) |> ignore
    app
```

## ベストプラクティス

### 1. 型安全性の活用

```fsharp
// ❌ 避けるべき
let processOrder (orderId: string) (amount: decimal) = ...

// ✅ 推奨
let processOrder (orderId: OrderId) (amount: Money) = ...
```

### 2. Railway Oriented Programming

```fsharp
// ❌ 例外ベース
let validateAndProcess order =
    let validated = validateOrder order  // 例外が発生する可能性
    let priced = priceOrder validated    // 例外が発生する可能性
    priced

// ✅ Result型ベース
let validateAndProcess order =
    order
    |> validateOrder
    |> Result.bind priceOrder
    |> Result.bind saveOrder
```

### 3. 依存関係の注入

```fsharp
// 関数型依存注入
type Dependencies = {
    CheckProductExists: CheckProductCodeExists
    GetProductPrice: GetProductPrice
    CheckAddressExists: CheckAddressExists
    SendAcknowledgment: SendOrderAcknowledgment
}

let createPlaceOrderWorkflow (deps: Dependencies) =
    OrderWorkflows.placeOrder
        deps.CheckProductExists
        deps.GetProductPrice
        deps.CheckAddressExists
        deps.SendAcknowledgment
```

### 4. エラーメッセージの国際化

```fsharp
type ErrorMessage =
    | InvalidEmail
    | ProductNotFound of ProductCode
    | QuantityOutOfRange of min: decimal * max: decimal

module ErrorMessages =
    let toJapanese = function
        | InvalidEmail -> "有効なメールアドレスを入力してください"
        | ProductNotFound code -> $"商品コード '{code}' が見つかりません"
        | QuantityOutOfRange (min, max) -> $"数量は {min} から {max} の範囲で入力してください"
```

### 5. パフォーマンス最適化

```fsharp
// 非同期処理の並行実行
let validateOrderAsync checkProduct checkAddress order =
    async {
        let! customerTask = validateCustomerAsync order.CustomerInfo
        let! shippingTask = checkAddress order.ShippingAddress
        let! billingTask = checkAddress order.BillingAddress
        let! linesTask = validateLinesAsync checkProduct order.Lines

        let! customer = customerTask
        let! shipping = shippingTask
        let! billing = billingTask
        let! lines = linesTask

        return {
            OrderId = order.OrderId
            CustomerInfo = customer
            ShippingAddress = shipping
            BillingAddress = billing
            Lines = lines
        }
    }
```

## まとめ

本ガイドでは、F# を使用した ASP.NET Core 最小 API の構築方法を、関数型ドメインモデリングの原則に基づいて説明しました。

### 主要な利点

1. **型安全性**: コンパイル時エラー検出による実行時エラーの削減
2. **表現力**: ビジネスルールを型システムで表現
3. **保守性**: 不変性と純粋関数による副作用の最小化
4. **テスタビリティ**: 関数型設計による高いテスト容易性

### 次のステップ

- **パフォーマンス最適化**: 大量データ処理への対応
- **イベントソーシング**: より高度なイベント駆動アーキテクチャ
- **CQRS**: コマンドとクエリの完全分離
- **分散システム**: マイクロサービス化への拡張

F# による関数型プログラミングアプローチにより、堅牢で保守性の高い API システムを構築できます。