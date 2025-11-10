module OrderTaking.Tests.ConstrainedTypes

open Xunit
open FsUnit.Xunit
open OrderTaking.Domain.ConstrainedTypes

// ========================================
// String50 Tests
// ========================================

[<Fact>]
let ``String50.create should accept valid string`` () =
    // Arrange
    let validString = "Valid String"

    // Act
    let result = String50.create "TestField" validString

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``String50.create should reject empty string`` () =
    // Arrange
    let emptyString = ""

    // Act
    let result = String50.create "TestField" emptyString

    // Assert
    match result with
    | Error msg -> msg |> should contain "must not be null or empty"
    | Ok _ -> failwith "Expected error"

[<Fact>]
let ``String50.create should reject too long string`` () =
    // Arrange
    let longString = System.String('a', 51)

    // Act
    let result = String50.create "TestField" longString

    // Assert
    match result with
    | Error msg -> msg |> should contain "must not be more than 50 chars"
    | Ok _ -> failwith "Expected error"

[<Fact>]
let ``String50.value should return internal value`` () =
    // Arrange
    let str = "Test"
    let string50 = String50.unsafeCreate str

    // Act
    let value = String50.value string50

    // Assert
    value |> should equal str

// ========================================
// EmailAddress Tests
// ========================================

[<Fact>]
let ``EmailAddress.create should accept valid email`` () =
    // Arrange
    let validEmail = "test@example.com"

    // Act
    let result = EmailAddress.create "Email" validEmail

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``EmailAddress.create should reject email without @`` () =
    // Arrange
    let invalidEmail = "invalid-email.com"

    // Act
    let result = EmailAddress.create "Email" invalidEmail

    // Assert
    match result with
    | Error msg -> msg |> should contain "must contain @"
    | Ok _ -> failwith "Expected error"

// ========================================
// ZipCode Tests
// ========================================

[<Fact>]
let ``ZipCode.create should accept valid zip code`` () =
    // Arrange
    let validZip = "12345"

    // Act
    let result = ZipCode.create "Zip" validZip

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``ZipCode.create should reject non-5-digit string`` () =
    // Arrange
    let invalidZip = "1234"

    // Act
    let result = ZipCode.create "Zip" invalidZip

    // Assert
    match result with
    | Error msg -> msg |> should contain "must be 5 chars"
    | Ok _ -> failwith "Expected error"

[<Fact>]
let ``ZipCode.create should reject non-numeric string`` () =
    // Arrange
    let invalidZip = "ABCDE"

    // Act
    let result = ZipCode.create "Zip" invalidZip

    // Assert
    match result with
    | Error msg -> msg |> should contain "must be all digits"
    | Ok _ -> failwith "Expected error"

// ========================================
// WidgetCode Tests
// ========================================

[<Fact>]
let ``WidgetCode.create should accept valid widget code`` () =
    // Arrange
    let validCode = "W1234"

    // Act
    let result = WidgetCode.create "ProductCode" validCode

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``WidgetCode.create should reject code without W prefix`` () =
    // Arrange
    let invalidCode = "A1234"

    // Act
    let result = WidgetCode.create "ProductCode" invalidCode

    // Assert
    match result with
    | Error msg -> msg |> should contain "must start with 'W'"
    | Ok _ -> failwith "Expected error"

// ========================================
// GizmoCode Tests
// ========================================

[<Fact>]
let ``GizmoCode.create should accept valid gizmo code`` () =
    // Arrange
    let validCode = "G123"

    // Act
    let result = GizmoCode.create "ProductCode" validCode

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``GizmoCode.create should reject code without G prefix`` () =
    // Arrange
    let invalidCode = "A123"

    // Act
    let result = GizmoCode.create "ProductCode" invalidCode

    // Assert
    match result with
    | Error msg -> msg |> should contain "must start with 'G'"
    | Ok _ -> failwith "Expected error"

// ========================================
// UnitQuantity Tests
// ========================================

[<Fact>]
let ``UnitQuantity.create should accept valid quantity`` () =
    // Arrange
    let validQty = 10

    // Act
    let result = UnitQuantity.create "Quantity" validQty

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``UnitQuantity.create should reject zero`` () =
    // Arrange
    let invalidQty = 0

    // Act
    let result = UnitQuantity.create "Quantity" invalidQty

    // Assert
    match result with
    | Error msg -> msg |> should contain "must be at least 1"
    | Ok _ -> failwith "Expected error"

[<Fact>]
let ``UnitQuantity.create should reject too large quantity`` () =
    // Arrange
    let invalidQty = 1001

    // Act
    let result = UnitQuantity.create "Quantity" invalidQty

    // Assert
    match result with
    | Error msg -> msg |> should contain "must not be more than 1000"
    | Ok _ -> failwith "Expected error"

// ========================================
// Price Tests
// ========================================

[<Fact>]
let ``Price.create should accept valid price`` () =
    // Arrange
    let validPrice = 10.50m

    // Act
    let result = Price.create "Price" validPrice

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``Price.create should reject negative price`` () =
    // Arrange
    let invalidPrice = -1.0m

    // Act
    let result = Price.create "Price" invalidPrice

    // Assert
    match result with
    | Error msg -> msg |> should contain "must not be negative"
    | Ok _ -> failwith "Expected error"

[<Fact>]
let ``Price.multiply should calculate correctly`` () =
    // Arrange
    let price = Price.unsafeCreate 10.0m
    let qty = 5

    // Act
    let result = Price.multiply qty price

    // Assert
    Price.value result |> should equal 50.0m

// ========================================
// BillingAmount Tests
// ========================================

[<Fact>]
let ``BillingAmount.sumPrices should calculate total`` () =
    // Arrange
    let prices = [
        Price.unsafeCreate 10.0m
        Price.unsafeCreate 20.0m
        Price.unsafeCreate 30.0m
    ]

    // Act
    let result = BillingAmount.sumPrices prices

    // Assert
    BillingAmount.value result |> should equal 60.0m

// ========================================
// OrderId Tests
// ========================================

[<Fact>]
let ``OrderId.create should accept valid string`` () =
    // Arrange
    let validId = "ORDER-123"

    // Act
    let result = OrderId.create "OrderId" validId

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``OrderId.create should reject too long string`` () =
    // Arrange
    let longId = System.String('a', 51)

    // Act
    let result = OrderId.create "OrderId" longId

    // Assert
    match result with
    | Error _ -> ()
    | Ok _ -> failwith "Expected error"
