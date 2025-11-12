module OrderTaking.Tests.Adapters

open Xunit
open FsUnit.Xunit
open OrderTaking.Domain.ConstrainedTypes
open OrderTaking.Domain.CompoundTypes
open OrderTaking.Domain.Entities

// ========================================
// CheckProductCodeExists Adapter Tests
// ========================================

[<Fact>]
let ``CheckProductCodeExists は有効な Widget コードを受け入れる`` () =
    // Arrange
    let checkProductCodeExists =
        OrderTaking.Infrastructure.Adapters.ProductCodeAdapter.checkProductCodeExists

    // Act
    let result = checkProductCodeExists "W1234"

    // Assert
    match result with
    | Ok productCode ->
        match productCode with
        | ProductCode.Widget widgetCode ->
            WidgetCode.value widgetCode
            |> should equal "W1234"
        | _ -> failwith "Expected Widget product code"
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``CheckProductCodeExists は有効な Gizmo コードを受け入れる`` () =
    // Arrange
    let checkProductCodeExists =
        OrderTaking.Infrastructure.Adapters.ProductCodeAdapter.checkProductCodeExists

    // Act
    let result = checkProductCodeExists "G123"

    // Assert
    match result with
    | Ok productCode ->
        match productCode with
        | ProductCode.Gizmo gizmoCode -> GizmoCode.value gizmoCode |> should equal "G123"
        | _ -> failwith "Expected Gizmo product code"
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``CheckProductCodeExists は無効なコードを拒否する`` () =
    // Arrange
    let checkProductCodeExists =
        OrderTaking.Infrastructure.Adapters.ProductCodeAdapter.checkProductCodeExists

    // Act
    let result =
        checkProductCodeExists "INVALID"

    // Assert
    match result with
    | Error msg -> Assert.Contains("not found", msg)
    | Ok _ -> failwith "Expected error: Product code should be rejected"

// ========================================
// CheckAddressExists Adapter Tests
// ========================================

[<Fact>]
let ``CheckAddressExists は有効な住所を受け入れる`` () =
    // Arrange
    let checkAddressExists =
        OrderTaking.Infrastructure.Adapters.AddressAdapter.checkAddressExists

    let unvalidatedAddress =
        { AddressLine1 = "123 Main St"
          AddressLine2 = None
          City = "Springfield"
          ZipCode = "12345" }

    // Act
    let result =
        checkAddressExists unvalidatedAddress

    // Assert
    match result with
    | Ok address -> address |> should equal unvalidatedAddress
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

// ========================================
// GetProductPrice Adapter Tests
// ========================================

[<Fact>]
let ``GetProductPrice は Widget の価格を返す`` () =
    // Arrange
    let getProductPrice =
        OrderTaking.Infrastructure.Adapters.PriceAdapter.getProductPrice

    let widgetCode =
        WidgetCode.unsafeCreate "W1234"

    let productCode =
        ProductCode.Widget widgetCode

    // Act
    let result = getProductPrice productCode

    // Assert
    match result with
    | Ok price -> Price.value price |> should equal 25.50m
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``GetProductPrice は Gizmo の価格を返す`` () =
    // Arrange
    let getProductPrice =
        OrderTaking.Infrastructure.Adapters.PriceAdapter.getProductPrice

    let gizmoCode =
        GizmoCode.unsafeCreate "G123"

    let productCode =
        ProductCode.Gizmo gizmoCode

    // Act
    let result = getProductPrice productCode

    // Assert
    match result with
    | Ok price -> Price.value price |> should equal 100.00m
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

// ========================================
// SendOrderAcknowledgment Adapter Tests
// ========================================

[<Fact>]
let ``SendOrderAcknowledgment は成功を返す`` () =
    // Arrange
    let sendAcknowledgment =
        OrderTaking.Infrastructure.Adapters.AcknowledgmentAdapter.sendOrderAcknowledgment

    let orderId =
        OrderId.create (System.Guid.NewGuid())

    let email =
        EmailAddress.unsafeCreate "test@example.com"

    let acknowledgment =
        { OrderTaking.Domain.DomainServices.OrderAcknowledgment.OrderId = orderId
          OrderTaking.Domain.DomainServices.OrderAcknowledgment.EmailAddress = email }

    // Act
    let result =
        sendAcknowledgment acknowledgment

    // Assert
    match result with
    | Ok() -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

// ========================================
// DependencyContainer Tests
// ========================================

[<Fact>]
let ``DependencyContainer は全ての依存性を提供する`` () =
    // Act
    let dependencies =
        OrderTaking.Infrastructure.DependencyContainer.createDefaultDependencies ()

    // Assert
    dependencies.CheckProductCodeExists
    |> should not' (be Null)

    dependencies.CheckAddressExists
    |> should not' (be Null)

    dependencies.GetProductPrice
    |> should not' (be Null)

    dependencies.SendOrderAcknowledgment
    |> should not' (be Null)
