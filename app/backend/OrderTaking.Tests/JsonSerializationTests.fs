module OrderTaking.Tests.JsonSerialization

open Xunit
open FsUnit.Xunit
open OrderTaking.Domain.ConstrainedTypes
open OrderTaking.Domain.CompoundTypes
open OrderTaking.Domain.Entities
open OrderTaking.Domain.DomainServices
open OrderTaking.Infrastructure.JsonSerialization

// ========================================
// UnvalidatedOrder Deserialization Tests
// ========================================

[<Fact>]
let ``UnvalidatedOrder を JSON からデシリアライズできる`` () =
    // Arrange
    let json =
        """
        {
            "orderId": "12345678-1234-1234-1234-123456789012",
            "customerInfo": {
                "firstName": "John",
                "lastName": "Doe",
                "emailAddress": "john@example.com"
            },
            "shippingAddress": {
                "addressLine1": "123 Main St",
                "addressLine2": "Apt 1",
                "city": "Springfield",
                "zipCode": "12345"
            },
            "billingAddress": {
                "addressLine1": "123 Main St",
                "addressLine2": null,
                "city": "Springfield",
                "zipCode": "12345"
            },
            "lines": [
                {
                    "orderLineId": "87654321-4321-4321-4321-210987654321",
                    "productCode": "W1234",
                    "quantity": 2.0
                },
                {
                    "orderLineId": "11111111-1111-1111-1111-111111111111",
                    "productCode": "G123",
                    "quantity": 0.5
                }
            ]
        }
        """

    // Act
    let result =
        deserializeUnvalidatedOrder json

    // Assert
    match result with
    | Ok order ->
        order.OrderId
        |> should equal "12345678-1234-1234-1234-123456789012"

        order.CustomerInfo.FirstName
        |> should equal "John"

        order.CustomerInfo.LastName |> should equal "Doe"
        order.Lines.Length |> should equal 2
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``無効な JSON は Error を返す`` () =
    // Arrange
    let invalidJson = "{invalid json"

    // Act
    let result =
        deserializeUnvalidatedOrder invalidJson

    // Assert
    match result with
    | Error msg -> Assert.Contains("JSON", msg)
    | Ok _ -> failwith "Expected error: Invalid JSON should be rejected"

// ========================================
// PlaceOrderEvent Serialization Tests
// ========================================

[<Fact>]
let ``OrderPlaced イベントを JSON にシリアライズできる`` () =
    // Arrange
    let orderId =
        OrderId.create (System.Guid.NewGuid())

    let email =
        EmailAddress.unsafeCreate "test@example.com"

    let customerInfo =
        match CustomerInfo.create "John" "Doe" "test@example.com" with
        | Ok ci -> ci
        | Error msg -> failwith $"Failed to create CustomerInfo: {msg}"

    let address =
        match Address.create "123 Main St" None "Springfield" "12345" with
        | Ok addr -> addr
        | Error msg -> failwith $"Failed to create Address: {msg}"

    let orderLineId =
        OrderLineId.create (System.Guid.NewGuid())

    let productCode =
        ProductCode.Widget(WidgetCode.unsafeCreate "W1234")

    let quantity =
        OrderQuantity.Unit(UnitQuantity.unsafeCreate 1)

    let price = Price.unsafeCreate 25.50m
    let linePrice = Price.unsafeCreate 25.50m

    let pricedLine =
        PricedOrderLine.create orderLineId productCode quantity price linePrice

    let amountToBill =
        BillingAmount.unsafeCreate 25.50m

    let pricedOrder =
        PricedOrder.create orderId customerInfo address address [ pricedLine ] amountToBill

    let event =
        PlaceOrderEvent.OrderPlaced pricedOrder

    // Act
    let json = serializePlaceOrderEvent event

    // Assert
    json |> should not' (be NullOrEmptyString)
    Assert.Contains("OrderPlaced", json)

[<Fact>]
let ``BillableOrderPlaced イベントを JSON にシリアライズできる`` () =
    // Arrange
    let orderId =
        OrderId.create (System.Guid.NewGuid())

    let amount =
        BillingAmount.unsafeCreate 100.00m

    let event =
        PlaceOrderEvent.BillableOrderPlaced(orderId, amount)

    // Act
    let json = serializePlaceOrderEvent event

    // Assert
    json |> should not' (be NullOrEmptyString)
    Assert.Contains("BillableOrderPlaced", json)

[<Fact>]
let ``AcknowledgmentSent イベントを JSON にシリアライズできる`` () =
    // Arrange
    let orderId =
        OrderId.create (System.Guid.NewGuid())

    let email =
        EmailAddress.unsafeCreate "test@example.com"

    let acknowledgment =
        { OrderAcknowledgment.OrderId = orderId
          OrderAcknowledgment.EmailAddress = email }

    let event =
        PlaceOrderEvent.AcknowledgmentSent acknowledgment

    // Act
    let json = serializePlaceOrderEvent event

    // Assert
    json |> should not' (be NullOrEmptyString)
    Assert.Contains("AcknowledgmentSent", json)
