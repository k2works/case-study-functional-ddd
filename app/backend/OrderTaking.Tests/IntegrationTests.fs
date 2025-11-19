module OrderTaking.Tests.Integration

open Xunit
open FsUnit.Xunit
open OrderTaking.Domain.ConstrainedTypes
open OrderTaking.Domain.CompoundTypes
open OrderTaking.Domain.Entities
open OrderTaking.Domain.DomainServices

// ========================================
// T7.1: Happy Path の統合テスト
// ========================================

[<Fact>]
let ``E2E: 複数明細を含む有効な注文が正常に処理される`` () =
    // Arrange - 複数の明細（Widget と Gizmo 混在）を含む注文
    let orderId =
        System.Guid.NewGuid().ToString()

    let unvalidatedOrder =
        UnvalidatedOrder.create
            orderId
            (UnvalidatedCustomerInfo.create "Alice" "Smith" "alice@example.com")
            (UnvalidatedAddress.create "456 Oak Ave" (Some "Suite 200") "Osaka" "54321")
            (UnvalidatedAddress.create "456 Oak Ave" (Some "Suite 200") "Osaka" "54321")
            [ UnvalidatedOrderLine.create (System.Guid.NewGuid().ToString()) "W1234" 50.0m
              UnvalidatedOrderLine.create (System.Guid.NewGuid().ToString()) "G5678" 10.5m
              UnvalidatedOrderLine.create (System.Guid.NewGuid().ToString()) "W5678" 100.0m ]

    // 実際のサービススタブを使用
    let checkProductCodeExists =
        ProductCodeService.checkProductCodeExists

    let checkAddressExists addr = Ok addr

    let getProductPrice =
        PriceService.getProductPrice

    let saveOrder (pricedOrder: PricedOrder) : Async<Result<OrderId, string>> =
        async { return Ok(pricedOrder.OrderId) }

    let sendAcknowledgment (acknowledgment: OrderAcknowledgment) : Result<unit, string> = Ok()

    // Act - ワークフロー全体を実行
    let result =
        PlaceOrderWorkflow.placeOrder
            checkProductCodeExists
            checkAddressExists
            getProductPrice
            saveOrder
            sendAcknowledgment
            unvalidatedOrder
        |> Async.RunSynchronously

    // Assert
    match result with
    | Ok events ->
        // 3 つのイベントが生成される
        events.Length |> should equal 3

        // 1. OrderPlaced イベント
        match events.[0] with
        | PlaceOrderEvent.OrderPlaced pricedOrder ->
            // 3 つの明細がある
            pricedOrder.Lines.Length |> should equal 3

            // 合計金額が正しい
            // W1234: 50 * 25.50 = 1275.00
            // G5678: 10.5 * 100.00 = 1050.00
            // W5678: 100 * 25.50 = 2550.00
            // Total: 4875.00
            BillingAmount.value pricedOrder.AmountToBill
            |> should equal 4875.00m
        | _ -> failwith "Expected OrderPlaced event"

        // 2. BillableOrderPlaced イベント
        match events.[1] with
        | PlaceOrderEvent.BillableOrderPlaced(orderId, amountToBill) ->
            BillingAmount.value amountToBill
            |> should equal 4875.00m
        | _ -> failwith "Expected BillableOrderPlaced event"

        // 3. AcknowledgmentSent イベント
        match events.[2] with
        | PlaceOrderEvent.AcknowledgmentSent acknowledgment ->
            EmailAddress.value acknowledgment.EmailAddress
            |> should equal "alice@example.com"
        | _ -> failwith "Expected AcknowledgmentSent event"
    | Error error -> failwith $"Expected Ok, got Error: {PlaceOrderError.toString error}"

[<Fact>]
let ``E2E: 境界値を含む有効な注文が正常に処理される`` () =
    // Arrange - 最小数量と最大数量
    let orderId =
        System.Guid.NewGuid().ToString()

    let unvalidatedOrder =
        UnvalidatedOrder.create
            orderId
            (UnvalidatedCustomerInfo.create "Bob" "Jones" "bob@example.com")
            (UnvalidatedAddress.create "789 Pine St" None "Kyoto" "98765")
            (UnvalidatedAddress.create "789 Pine St" None "Kyoto" "98765")
            [ UnvalidatedOrderLine.create (System.Guid.NewGuid().ToString()) "W1234" 1.0m // 最小
              UnvalidatedOrderLine.create (System.Guid.NewGuid().ToString()) "W5678" 100.0m // BillingAmount 上限に注意
              UnvalidatedOrderLine.create (System.Guid.NewGuid().ToString()) "G5678" 0.05m ] // 最小

    let checkProductCodeExists =
        ProductCodeService.checkProductCodeExists

    let checkAddressExists addr = Ok addr

    let getProductPrice =
        PriceService.getProductPrice

    let saveOrder (pricedOrder: PricedOrder) : Async<Result<OrderId, string>> =
        async { return Ok(pricedOrder.OrderId) }

    let sendAcknowledgment (acknowledgment: OrderAcknowledgment) : Result<unit, string> = Ok()

    // Act
    let result =
        PlaceOrderWorkflow.placeOrder
            checkProductCodeExists
            checkAddressExists
            getProductPrice
            saveOrder
            sendAcknowledgment
            unvalidatedOrder
        |> Async.RunSynchronously

    // Assert
    match result with
    | Ok events ->
        events.Length |> should equal 3

        match events.[0] with
        | PlaceOrderEvent.OrderPlaced pricedOrder ->
            pricedOrder.Lines.Length |> should equal 3

            // 各明細の数量を検証
            match pricedOrder.Lines.[0].Quantity with
            | OrderQuantity.Unit qty -> UnitQuantity.value qty |> should equal 1
            | _ -> failwith "Expected Unit quantity for first order line"

            match pricedOrder.Lines.[1].Quantity with
            | OrderQuantity.Unit qty -> UnitQuantity.value qty |> should equal 100
            | _ -> failwith "Expected Unit quantity for second order line"

            match pricedOrder.Lines.[2].Quantity with
            | OrderQuantity.Kilogram qty -> KilogramQuantity.value qty |> should equal 0.05m
            | _ -> failwith "Expected Kilogram quantity for third order line"
        | _ -> failwith "Expected OrderPlaced event in integration test"
    | Error error -> failwith $"Expected Ok, got Error: {PlaceOrderError.toString error}"

// ========================================
// T7.2: エラーケースの統合テスト
// ========================================

[<Fact>]
let ``E2E: 無効な商品コードでバリデーションエラーが返される`` () =
    // Arrange - 存在しない商品コード
    let orderId =
        System.Guid.NewGuid().ToString()

    let unvalidatedOrder =
        UnvalidatedOrder.create
            orderId
            (UnvalidatedCustomerInfo.create "Charlie" "Brown" "charlie@example.com")
            (UnvalidatedAddress.create "321 Elm St" None "Tokyo" "11111")
            (UnvalidatedAddress.create "321 Elm St" None "Tokyo" "11111")
            [ UnvalidatedOrderLine.create (System.Guid.NewGuid().ToString()) "INVALID" 10.0m ]

    let checkProductCodeExists =
        ProductCodeService.checkProductCodeExists

    let checkAddressExists addr = Ok addr

    let getProductPrice =
        PriceService.getProductPrice

    let saveOrder (pricedOrder: PricedOrder) : Async<Result<OrderId, string>> =
        async { return Ok(pricedOrder.OrderId) }

    let sendAcknowledgment (acknowledgment: OrderAcknowledgment) : Result<unit, string> = Ok()

    // Act
    let result =
        PlaceOrderWorkflow.placeOrder
            checkProductCodeExists
            checkAddressExists
            getProductPrice
            saveOrder
            sendAcknowledgment
            unvalidatedOrder
        |> Async.RunSynchronously

    // Assert
    match result with
    | Error(PlaceOrderError.ValidationError errors) ->
        errors.Length |> should be (greaterThan 0)

        errors
        |> List.exists (fun e -> (ValidationError.toString e).Contains("ProductCode"))
        |> should be True
    | Ok _ -> failwith "Expected ValidationError for invalid product code"
    | Error error -> failwith $"Expected ValidationError, got: {PlaceOrderError.toString error}"

[<Fact>]
let ``E2E: 範囲外の数量でバリデーションエラーが返される`` () =
    // Arrange - 数量が多すぎる
    let orderId =
        System.Guid.NewGuid().ToString()

    let unvalidatedOrder =
        UnvalidatedOrder.create
            orderId
            (UnvalidatedCustomerInfo.create "David" "Lee" "david@example.com")
            (UnvalidatedAddress.create "654 Maple Dr" None "Nagoya" "22222")
            (UnvalidatedAddress.create "654 Maple Dr" None "Nagoya" "22222")
            [ UnvalidatedOrderLine.create (System.Guid.NewGuid().ToString()) "W1234" 2000.0m ] // 1000 を超える

    let checkProductCodeExists =
        ProductCodeService.checkProductCodeExists

    let checkAddressExists addr = Ok addr

    let getProductPrice =
        PriceService.getProductPrice

    let saveOrder (pricedOrder: PricedOrder) : Async<Result<OrderId, string>> =
        async { return Ok(pricedOrder.OrderId) }

    let sendAcknowledgment (acknowledgment: OrderAcknowledgment) : Result<unit, string> = Ok()

    // Act
    let result =
        PlaceOrderWorkflow.placeOrder
            checkProductCodeExists
            checkAddressExists
            getProductPrice
            saveOrder
            sendAcknowledgment
            unvalidatedOrder
        |> Async.RunSynchronously

    // Assert
    match result with
    | Error(PlaceOrderError.ValidationError errors) ->
        errors.Length |> should be (greaterThan 0)

        errors
        |> List.exists (fun e -> (ValidationError.toString e).Contains("Quantity"))
        |> should be True
    | Ok _ -> failwith "Expected ValidationError for out of range quantity"
    | Error error -> failwith $"Expected ValidationError, got: {PlaceOrderError.toString error}"

[<Fact>]
let ``E2E: アドレス検証失敗でバリデーションエラーが返される`` () =
    // Arrange
    let orderId =
        System.Guid.NewGuid().ToString()

    let unvalidatedOrder =
        UnvalidatedOrder.create
            orderId
            (UnvalidatedCustomerInfo.create "Eve" "White" "eve@example.com")
            (UnvalidatedAddress.create "999 Invalid Addr" None "BadCity" "00000")
            (UnvalidatedAddress.create "123 Main St" None "Tokyo" "12345")
            [ UnvalidatedOrderLine.create (System.Guid.NewGuid().ToString()) "W1234" 10.0m ]

    let checkProductCodeExists =
        ProductCodeService.checkProductCodeExists

    // アドレス検証に失敗
    let checkAddressExists addr =
        if addr.City = "BadCity" then
            Error "Address not found"
        else
            Ok addr

    let getProductPrice =
        PriceService.getProductPrice

    let saveOrder (pricedOrder: PricedOrder) : Async<Result<OrderId, string>> =
        async { return Ok(pricedOrder.OrderId) }

    let sendAcknowledgment (acknowledgment: OrderAcknowledgment) : Result<unit, string> = Ok()

    // Act
    let result =
        PlaceOrderWorkflow.placeOrder
            checkProductCodeExists
            checkAddressExists
            getProductPrice
            saveOrder
            sendAcknowledgment
            unvalidatedOrder
        |> Async.RunSynchronously

    // Assert
    match result with
    | Error(PlaceOrderError.ValidationError errors) ->
        errors.Length |> should be (greaterThan 0)

        errors
        |> List.exists (fun e -> (ValidationError.toString e).Contains("ShippingAddress"))
        |> should be True
    | Ok _ -> failwith "Expected ValidationError for address validation failure"
    | Error error -> failwith $"Expected ValidationError, got: {PlaceOrderError.toString error}"

[<Fact>]
let ``E2E: 複数のバリデーションエラーが集約される`` () =
    // Arrange - 複数の問題を含む注文
    let orderId =
        System.Guid.NewGuid().ToString()

    let unvalidatedOrder =
        UnvalidatedOrder.create
            orderId
            (UnvalidatedCustomerInfo.create "" "" "invalid-email") // 3 つのエラー
            (UnvalidatedAddress.create "" None "" "ABCDE") // 複数のエラー
            (UnvalidatedAddress.create "123 Main St" None "Tokyo" "12345")
            [ UnvalidatedOrderLine.create (System.Guid.NewGuid().ToString()) "INVALID" 2000.0m ] // 2 つのエラー

    let checkProductCodeExists =
        ProductCodeService.checkProductCodeExists

    let checkAddressExists addr = Ok addr

    let getProductPrice =
        PriceService.getProductPrice

    let saveOrder (pricedOrder: PricedOrder) : Async<Result<OrderId, string>> =
        async { return Ok(pricedOrder.OrderId) }

    let sendAcknowledgment (acknowledgment: OrderAcknowledgment) : Result<unit, string> = Ok()

    // Act
    let result =
        PlaceOrderWorkflow.placeOrder
            checkProductCodeExists
            checkAddressExists
            getProductPrice
            saveOrder
            sendAcknowledgment
            unvalidatedOrder
        |> Async.RunSynchronously

    // Assert
    match result with
    | Error(PlaceOrderError.ValidationError errors) ->
        // 複数のエラーが返される
        errors.Length |> should be (greaterThan 1)
    | Ok _ -> failwith "Expected multiple validation errors"
    | Error error -> failwith $"Expected ValidationError, got: {PlaceOrderError.toString error}"

[<Fact>]
let ``E2E: 外部サービスエラーが適切に伝播される`` () =
    // Arrange
    let orderId =
        System.Guid.NewGuid().ToString()

    let unvalidatedOrder =
        UnvalidatedOrder.create
            orderId
            (UnvalidatedCustomerInfo.create "Frank" "Green" "frank@example.com")
            (UnvalidatedAddress.create "123 Main St" None "Tokyo" "12345")
            (UnvalidatedAddress.create "123 Main St" None "Tokyo" "12345")
            [ UnvalidatedOrderLine.create (System.Guid.NewGuid().ToString()) "W1234" 10.0m ]

    let checkProductCodeExists =
        ProductCodeService.checkProductCodeExists

    let checkAddressExists addr = Ok addr

    // 価格サービスがエラーを返す
    let getProductPrice productCode = Error "Price service unavailable"

    let saveOrder (pricedOrder: PricedOrder) : Async<Result<OrderId, string>> =
        async { return Ok(pricedOrder.OrderId) }

    let sendAcknowledgment (acknowledgment: OrderAcknowledgment) : Result<unit, string> = Ok()

    // Act
    let result =
        PlaceOrderWorkflow.placeOrder
            checkProductCodeExists
            checkAddressExists
            getProductPrice
            saveOrder
            sendAcknowledgment
            unvalidatedOrder
        |> Async.RunSynchronously

    // Assert
    match result with
    | Error(PlaceOrderError.PricingError msg) -> msg |> should equal "Price service unavailable"
    | Ok _ -> failwith "Expected PricingError"
    | Error error -> failwith $"Expected PricingError, got: {PlaceOrderError.toString error}"
