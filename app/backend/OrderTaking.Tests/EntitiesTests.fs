module OrderTaking.Tests.Entities

open Xunit
open FsUnit.Xunit
open OrderTaking.Domain.Entities

// ========================================
// UnvalidatedCustomerInfo Tests
// ========================================

[<Fact>]
let ``UnvalidatedCustomerInfo.create は生データを受け入れる`` () =
    // Arrange
    let firstName = "John"
    let lastName = "Doe"
    let email = "john.doe@example.com"

    // Act
    let customerInfo =
        UnvalidatedCustomerInfo.create firstName lastName email

    // Assert
    customerInfo.FirstName |> should equal firstName
    customerInfo.LastName |> should equal lastName
    customerInfo.EmailAddress |> should equal email

// ========================================
// UnvalidatedAddress Tests
// ========================================

[<Fact>]
let ``UnvalidatedAddress.create は生データを受け入れる`` () =
    // Arrange
    let line1 = "123 Main St"
    let line2 = Some "Apt 456"
    let city = "Tokyo"
    let zipCode = "12345"

    // Act
    let address =
        UnvalidatedAddress.create line1 line2 city zipCode

    // Assert
    address.AddressLine1 |> should equal line1
    address.AddressLine2 |> should equal line2
    address.City |> should equal city
    address.ZipCode |> should equal zipCode

[<Fact>]
let ``UnvalidatedAddress.create は AddressLine2 が None でも受け入れる`` () =
    // Arrange
    let line1 = "123 Main St"
    let city = "Tokyo"
    let zipCode = "12345"

    // Act
    let address =
        UnvalidatedAddress.create line1 None city zipCode

    // Assert
    address.AddressLine1 |> should equal line1
    address.AddressLine2 |> should equal None
    address.City |> should equal city
    address.ZipCode |> should equal zipCode

// ========================================
// UnvalidatedOrderLine Tests
// ========================================

[<Fact>]
let ``UnvalidatedOrderLine.create は生データを受け入れる`` () =
    // Arrange
    let orderLineId = "line-001"
    let productCode = "WIDGET-A"
    let quantity = 10.0m

    // Act
    let orderLine =
        UnvalidatedOrderLine.create orderLineId productCode quantity

    // Assert
    orderLine.OrderLineId |> should equal orderLineId
    orderLine.ProductCode |> should equal productCode
    orderLine.Quantity |> should equal quantity

// ========================================
// UnvalidatedOrder Tests
// ========================================

[<Fact>]
let ``UnvalidatedOrder.create は生データを受け入れる`` () =
    // Arrange
    let orderId = "order-001"

    let customerInfo =
        UnvalidatedCustomerInfo.create "John" "Doe" "john@example.com"

    let shippingAddress =
        UnvalidatedAddress.create "123 Main St" None "Tokyo" "12345"

    let billingAddress =
        UnvalidatedAddress.create "456 Elm St" (Some "Suite 100") "Osaka" "67890"

    let lines =
        [ UnvalidatedOrderLine.create "line-001" "WIDGET-A" 10.0m
          UnvalidatedOrderLine.create "line-002" "GIZMO-B" 5.0m ]

    // Act
    let order =
        UnvalidatedOrder.create orderId customerInfo shippingAddress billingAddress lines

    // Assert
    order.OrderId |> should equal orderId
    order.CustomerInfo |> should equal customerInfo

    order.ShippingAddress
    |> should equal shippingAddress

    order.BillingAddress
    |> should equal billingAddress

    order.Lines |> should equal lines

[<Fact>]
let ``UnvalidatedOrder.create は空の明細リストを受け入れる`` () =
    // Arrange
    let orderId = "order-002"

    let customerInfo =
        UnvalidatedCustomerInfo.create "Jane" "Smith" "jane@example.com"

    let shippingAddress =
        UnvalidatedAddress.create "789 Oak Ave" None "Kyoto" "11111"

    let billingAddress = shippingAddress

    // Act
    let order =
        UnvalidatedOrder.create orderId customerInfo shippingAddress billingAddress []

    // Assert
    order.OrderId |> should equal orderId
    order.Lines |> should be Empty
