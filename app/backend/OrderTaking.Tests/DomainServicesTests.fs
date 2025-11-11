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
