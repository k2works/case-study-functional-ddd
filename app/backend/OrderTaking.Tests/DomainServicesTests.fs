module OrderTaking.Tests.DomainServices

open Xunit
open FsUnit.Xunit
open OrderTaking.Domain.ConstrainedTypes
open OrderTaking.Domain.CompoundTypes
open OrderTaking.Domain.Entities
open OrderTaking.Domain.DomainServices

// ========================================
// ValidationError Tests
// ========================================

[<Fact>]
let ``ValidationError.toString は適切なエラーメッセージを返す`` () =
    // Arrange
    let error =
        ValidationError.create "Field" "must not be empty"

    // Act
    let message = ValidationError.toString error

    // Assert
    message |> should equal "Field: must not be empty"

// ========================================
// validateOrder Tests - 成功ケース
// ========================================

[<Fact>]
let ``validateOrder は有効な注文を検証する`` () =
    // Arrange
    let guid = System.Guid.NewGuid().ToString()

    let unvalidatedOrder =
        UnvalidatedOrder.create
            guid
            (UnvalidatedCustomerInfo.create "John" "Doe" "john@example.com")
            (UnvalidatedAddress.create "123 Main St" None "Tokyo" "12345")
            (UnvalidatedAddress.create "123 Main St" None "Tokyo" "12345")
            [ UnvalidatedOrderLine.create (System.Guid.NewGuid().ToString()) "W1234" 10.0m ]

    // Mock dependencies
    let checkProductCodeExists code =
        Ok(ProductCode.Widget(WidgetCode.unsafeCreate code))

    let checkAddressExists addr = Ok addr

    // Act
    let result =
        Validation.validateOrder checkProductCodeExists checkAddressExists unvalidatedOrder

    // Assert
    match result with
    | Ok validatedOrder ->
        validatedOrder.Lines.Length |> should equal 1

        validatedOrder.Lines.[0].Quantity
        |> should equal (OrderQuantity.Unit(UnitQuantity.unsafeCreate 10))
    | Error errors -> failwith $"Expected Ok, got Error: {errors}"

// ========================================
// validateOrder Tests - エラーケース
// ========================================

[<Fact>]
let ``validateOrder は空の名前を拒否する`` () =
    // Arrange
    let guid = System.Guid.NewGuid().ToString()

    let unvalidatedOrder =
        UnvalidatedOrder.create
            guid
            (UnvalidatedCustomerInfo.create "" "Doe" "john@example.com")
            (UnvalidatedAddress.create "123 Main St" None "Tokyo" "12345")
            (UnvalidatedAddress.create "123 Main St" None "Tokyo" "12345")
            []

    let checkProductCodeExists code =
        Ok(ProductCode.Widget(WidgetCode.unsafeCreate code))

    let checkAddressExists addr = Ok addr

    // Act
    let result =
        Validation.validateOrder checkProductCodeExists checkAddressExists unvalidatedOrder

    // Assert
    match result with
    | Error errors ->
        errors.Length |> should be (greaterThan 0)

        errors
        |> List.exists (fun e -> (ValidationError.toString e).Contains("FirstName"))
        |> should be True
    | Ok _ -> failwith "Expected Error for empty first name"

[<Fact>]
let ``validateOrder は無効なメールアドレスを拒否する`` () =
    // Arrange
    let guid = System.Guid.NewGuid().ToString()

    let unvalidatedOrder =
        UnvalidatedOrder.create
            guid
            (UnvalidatedCustomerInfo.create "John" "Doe" "invalid-email")
            (UnvalidatedAddress.create "123 Main St" None "Tokyo" "12345")
            (UnvalidatedAddress.create "123 Main St" None "Tokyo" "12345")
            []

    let checkProductCodeExists code =
        Ok(ProductCode.Widget(WidgetCode.unsafeCreate code))

    let checkAddressExists addr = Ok addr

    // Act
    let result =
        Validation.validateOrder checkProductCodeExists checkAddressExists unvalidatedOrder

    // Assert
    match result with
    | Error errors ->
        errors
        |> List.exists (fun e -> (ValidationError.toString e).Contains("EmailAddress"))
        |> should be True
    | Ok _ -> failwith "Expected Error for invalid email"

[<Fact>]
let ``validateOrder は複数のバリデーションエラーを集約する`` () =
    // Arrange
    let guid = System.Guid.NewGuid().ToString()

    let unvalidatedOrder =
        UnvalidatedOrder.create
            guid
            (UnvalidatedCustomerInfo.create "" "" "invalid")
            (UnvalidatedAddress.create "" None "" "ABC")
            (UnvalidatedAddress.create "123 Main St" None "Tokyo" "12345")
            []

    let checkProductCodeExists code =
        Ok(ProductCode.Widget(WidgetCode.unsafeCreate code))

    let checkAddressExists addr = Ok addr

    // Act
    let result =
        Validation.validateOrder checkProductCodeExists checkAddressExists unvalidatedOrder

    // Assert
    match result with
    | Error errors ->
        // 複数のエラーが返される
        errors.Length |> should be (greaterThan 1)
    | Ok _ -> failwith "Expected multiple validation errors"

// ========================================
// ProductCodeService Tests
// ========================================

[<Fact>]
let ``ProductCodeService.checkProductCodeExists は Widget コードを受け入れる`` () =
    // Arrange
    let widgetCode = "W1234"

    // Act
    let result =
        ProductCodeService.checkProductCodeExists widgetCode

    // Assert
    match result with
    | Ok(ProductCode.Widget wc) -> WidgetCode.value wc |> should equal widgetCode
    | Ok(ProductCode.Gizmo _) -> failwith "Expected Widget, got Gizmo"
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``ProductCodeService.checkProductCodeExists は Gizmo コードを受け入れる`` () =
    // Arrange
    let gizmoCode = "G5678"

    // Act
    let result =
        ProductCodeService.checkProductCodeExists gizmoCode

    // Assert
    match result with
    | Ok(ProductCode.Gizmo gc) -> GizmoCode.value gc |> should equal gizmoCode
    | Ok(ProductCode.Widget _) -> failwith "Expected Gizmo, got Widget"
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``ProductCodeService.checkProductCodeExists は無効なコードを拒否する`` () =
    // Arrange
    let invalidCode = "INVALID"

    // Act
    let result =
        ProductCodeService.checkProductCodeExists invalidCode

    // Assert
    match result with
    | Error _ -> ()
    | Ok _ -> failwith "Expected Error for invalid product code"

// ========================================
// Pricing Tests
// ========================================

[<Fact>]
let ``priceOrder は有効な注文を価格計算する`` () =
    // Arrange
    let orderId = OrderId.generate ()

    let customerInfo =
        match CustomerInfo.create "John" "Doe" "john@example.com" with
        | Ok c -> c
        | Error e -> failwith e

    let shippingAddress =
        match Address.create "123 Main St" None "Tokyo" "12345" with
        | Ok a -> a
        | Error e -> failwith e

    let validatedOrder =
        ValidatedOrder.create
            orderId
            customerInfo
            shippingAddress
            shippingAddress
            [ ValidatedOrderLine.create
                  (OrderLineId.generate ())
                  (ProductCode.Widget(WidgetCode.unsafeCreate "W1234"))
                  (OrderQuantity.Unit(UnitQuantity.unsafeCreate 10)) ]

    // Mock dependency
    let getProductPrice productCode = Ok(Price.unsafeCreate 25.50m)

    // Act
    let result =
        Pricing.priceOrder getProductPrice validatedOrder

    // Assert
    match result with
    | Ok pricedOrder ->
        pricedOrder.Lines.Length |> should equal 1

        pricedOrder.Lines.[0].Price
        |> should equal (Price.unsafeCreate 25.50m)

        pricedOrder.Lines.[0].LinePrice
        |> should equal (Price.unsafeCreate 255.00m)

        pricedOrder.AmountToBill
        |> should equal (BillingAmount.unsafeCreate 255.00m)
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``priceOrder は複数明細の合計金額を計算する`` () =
    // Arrange
    let orderId = OrderId.generate ()

    let customerInfo =
        match CustomerInfo.create "John" "Doe" "john@example.com" with
        | Ok c -> c
        | Error e -> failwith e

    let shippingAddress =
        match Address.create "123 Main St" None "Tokyo" "12345" with
        | Ok a -> a
        | Error e -> failwith e

    let validatedOrder =
        ValidatedOrder.create
            orderId
            customerInfo
            shippingAddress
            shippingAddress
            [ ValidatedOrderLine.create
                  (OrderLineId.generate ())
                  (ProductCode.Widget(WidgetCode.unsafeCreate "W1234"))
                  (OrderQuantity.Unit(UnitQuantity.unsafeCreate 10))
              ValidatedOrderLine.create
                  (OrderLineId.generate ())
                  (ProductCode.Gizmo(GizmoCode.unsafeCreate "G5678"))
                  (OrderQuantity.Kilogram(KilogramQuantity.unsafeCreate 2.5m)) ]

    // Mock dependency - 商品コードに応じて価格を返す
    let getProductPrice productCode =
        match productCode with
        | ProductCode.Widget _ -> Ok(Price.unsafeCreate 25.50m)
        | ProductCode.Gizmo _ -> Ok(Price.unsafeCreate 100.00m)

    // Act
    let result =
        Pricing.priceOrder getProductPrice validatedOrder

    // Assert
    match result with
    | Ok pricedOrder ->
        pricedOrder.Lines.Length |> should equal 2
        // 255.00 + 250.00 = 505.00
        pricedOrder.AmountToBill
        |> should equal (BillingAmount.unsafeCreate 505.00m)
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

// ========================================
// PriceService Tests
// ========================================

[<Fact>]
let ``PriceService.getProductPrice は商品の価格を返す`` () =
    // Arrange
    let widgetCode =
        ProductCode.Widget(WidgetCode.unsafeCreate "W1234")

    // Act
    let result =
        PriceService.getProductPrice widgetCode

    // Assert
    match result with
    | Ok price -> Price.value price |> should be (greaterThan 0.0m)
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"
