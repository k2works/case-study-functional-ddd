module OrderTaking.Tests.ConstrainedTypes

open Xunit
open FsUnit.Xunit
open OrderTaking.Domain.ConstrainedTypes

// ========================================
// String50 Tests
// ========================================

[<Fact>]
let ``String50.create は有効な文字列を受け入れる`` () =
    // Arrange
    let validString = "Valid String"

    // Act
    let result =
        String50.create "TestField" validString

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``String50.create は空文字列を拒否する`` () =
    // Arrange
    let emptyString = ""

    // Act
    let result =
        String50.create "TestField" emptyString

    // Assert
    match result with
    | Error msg -> Assert.Contains("must not be null or empty", msg)
    | Ok _ -> failwith "Expected error"

[<Fact>]
let ``String50.create は長すぎる文字列を拒否する`` () =
    // Arrange
    let longString = System.String('a', 51)

    // Act
    let result =
        String50.create "TestField" longString

    // Assert
    match result with
    | Error msg -> Assert.Contains("must not be more than 50 chars", msg)
    | Ok _ -> failwith "Expected error"

[<Fact>]
let ``String50.value は内部値を返す`` () =
    // Arrange
    let str = "Test"
    let string50 = String50.unsafeCreate str

    // Act
    let value = String50.value string50

    // Assert
    value |> should equal str

// ========================================
// String100 Tests
// ========================================

[<Fact>]
let ``String100.create は有効な文字列を受け入れる`` () =
    // Arrange
    let validString =
        "Valid String for String100"

    // Act
    let result =
        String100.create "TestField" validString

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``String100.create は空文字列を拒否する`` () =
    // Arrange
    let emptyString = ""

    // Act
    let result =
        String100.create "TestField" emptyString

    // Assert
    match result with
    | Error msg -> Assert.Contains("must not be null or empty", msg)
    | Ok _ -> failwith "Expected error"

[<Fact>]
let ``String100.create は長すぎる文字列を拒否する`` () =
    // Arrange
    let longString = System.String('a', 101)

    // Act
    let result =
        String100.create "TestField" longString

    // Assert
    match result with
    | Error msg -> Assert.Contains("must not be more than 100 chars", msg)
    | Ok _ -> failwith "Expected error"

[<Fact>]
let ``String100.value は元の文字列を返す`` () =
    // Arrange
    let str = "Test String"
    let string100 = String100.unsafeCreate str

    // Act
    let value = String100.value string100

    // Assert
    value |> should equal str

// ========================================
// String255 Tests
// ========================================

[<Fact>]
let ``String255.create は有効な文字列を受け入れる`` () =
    // Arrange
    let validString =
        "Valid String for String255"

    // Act
    let result =
        String255.create "TestField" validString

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``String255.create は空文字列を拒否する`` () =
    // Arrange
    let emptyString = ""

    // Act
    let result =
        String255.create "TestField" emptyString

    // Assert
    match result with
    | Error msg -> Assert.Contains("must not be null or empty", msg)
    | Ok _ -> failwith "Expected error"

[<Fact>]
let ``String255.create は長すぎる文字列を拒否する`` () =
    // Arrange
    let longString = System.String('a', 256)

    // Act
    let result =
        String255.create "TestField" longString

    // Assert
    match result with
    | Error msg -> Assert.Contains("must not be more than 255 chars", msg)
    | Ok _ -> failwith "Expected error"

[<Fact>]
let ``String255.value は元の文字列を返す`` () =
    // Arrange
    let str = "Test String for String255"
    let string255 = String255.unsafeCreate str

    // Act
    let value = String255.value string255

    // Assert
    value |> should equal str

// ========================================
// EmailAddress Tests
// ========================================

[<Fact>]
let ``EmailAddress.create は有効なメールアドレスを受け入れる`` () =
    // Arrange
    let validEmail = "test@example.com"

    // Act
    let result =
        EmailAddress.create "Email" validEmail

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``EmailAddress.create はアットマークを含まないメールアドレスを拒否する`` () =
    // Arrange
    let invalidEmail = "invalid-email.com"

    // Act
    let result =
        EmailAddress.create "Email" invalidEmail

    // Assert
    match result with
    | Error msg -> Assert.Contains("must contain @", msg)
    | Ok _ -> failwith "Expected error"

// ========================================
// ZipCode Tests
// ========================================

[<Fact>]
let ``ZipCode.create は有効な郵便番号を受け入れる`` () =
    // Arrange
    let validZip = "12345"

    // Act
    let result = ZipCode.create "Zip" validZip

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``ZipCode.create は5桁でない文字列を拒否する`` () =
    // Arrange
    let invalidZip = "1234"

    // Act
    let result = ZipCode.create "Zip" invalidZip

    // Assert
    match result with
    | Error msg -> Assert.Contains("must be 5 chars", msg)
    | Ok _ -> failwith "Expected error"

[<Fact>]
let ``ZipCode.create は数字でない文字列を拒否する`` () =
    // Arrange
    let invalidZip = "ABCDE"

    // Act
    let result = ZipCode.create "Zip" invalidZip

    // Assert
    match result with
    | Error msg -> Assert.Contains("must be all digits", msg)
    | Ok _ -> failwith "Expected error"

// ========================================
// WidgetCode Tests
// ========================================

[<Fact>]
let ``WidgetCode.create は有効な Widget コードを受け入れる`` () =
    // Arrange
    let validCode = "W1234"

    // Act
    let result =
        WidgetCode.create "ProductCode" validCode

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``WidgetCode.create は W で始まらないコードを拒否する`` () =
    // Arrange
    let invalidCode = "A1234"

    // Act
    let result =
        WidgetCode.create "ProductCode" invalidCode

    // Assert
    match result with
    | Error msg -> Assert.Contains("must start with 'W'", msg)
    | Ok _ -> failwith "Expected error"

// ========================================
// GizmoCode Tests
// ========================================

[<Fact>]
let ``GizmoCode.create は有効な Gizmo コードを受け入れる`` () =
    // Arrange
    let validCode = "G123"

    // Act
    let result =
        GizmoCode.create "ProductCode" validCode

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``GizmoCode.create は G で始まらないコードを拒否する`` () =
    // Arrange
    let invalidCode = "A123"

    // Act
    let result =
        GizmoCode.create "ProductCode" invalidCode

    // Assert
    match result with
    | Error msg -> Assert.Contains("must start with 'G'", msg)
    | Ok _ -> failwith "Expected error"

// ========================================
// UnitQuantity Tests
// ========================================

[<Fact>]
let ``UnitQuantity.create は有効な数量を受け入れる`` () =
    // Arrange
    let validQty = 10

    // Act
    let result =
        UnitQuantity.create "Quantity" validQty

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``UnitQuantity.create はゼロを拒否する`` () =
    // Arrange
    let invalidQty = 0

    // Act
    let result =
        UnitQuantity.create "Quantity" invalidQty

    // Assert
    match result with
    | Error msg -> Assert.Contains("must be at least 1", msg)
    | Ok _ -> failwith "Expected error"

[<Fact>]
let ``UnitQuantity.create は大きすぎる数量を拒否する`` () =
    // Arrange
    let invalidQty = 1001

    // Act
    let result =
        UnitQuantity.create "Quantity" invalidQty

    // Assert
    match result with
    | Error msg -> Assert.Contains("must not be more than 1000", msg)
    | Ok _ -> failwith "Expected error"

// ========================================
// Price Tests
// ========================================

[<Fact>]
let ``Price.create は有効な価格を受け入れる`` () =
    // Arrange
    let validPrice = 10.50m

    // Act
    let result = Price.create "Price" validPrice

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``Price.create は負の価格を拒否する`` () =
    // Arrange
    let invalidPrice = -1.0m

    // Act
    let result =
        Price.create "Price" invalidPrice

    // Assert
    match result with
    | Error msg -> Assert.Contains("must not be negative", msg)
    | Ok _ -> failwith "Expected error"

[<Fact>]
let ``Price.multiply は正しく計算する`` () =
    // Arrange
    let price = Price.unsafeCreate 10.0m
    let qty = 5.0m

    // Act
    let result = Price.multiply qty price

    // Assert
    Price.value result |> should equal 50.0m

// ========================================
// BillingAmount Tests
// ========================================

[<Fact>]
let ``BillingAmount.sumPrices は合計を計算する`` () =
    // Arrange
    let prices =
        [ Price.unsafeCreate 10.0m
          Price.unsafeCreate 20.0m
          Price.unsafeCreate 30.0m ]

    // Act
    let result = BillingAmount.sumPrices prices

    // Assert
    BillingAmount.value result |> should equal 60.0m

// ========================================
// OrderId Tests
// ========================================

[<Fact>]
let ``OrderId.create は有効な文字列を受け入れる`` () =
    // Arrange
    let validId = "ORDER-123"

    // Act
    let result =
        OrderId.create "OrderId" validId

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``OrderId.create は長すぎる文字列を拒否する`` () =
    // Arrange
    let longId = System.String('a', 51)

    // Act
    let result = OrderId.create "OrderId" longId

    // Assert
    match result with
    | Error _ -> ()
    | Ok _ -> failwith "Expected error"
