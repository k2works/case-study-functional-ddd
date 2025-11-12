module OrderTaking.Tests.Api

open System.Net
open System.Net.Http
open System.Text
open Xunit
open FsUnit.Xunit
open Microsoft.AspNetCore.Mvc.Testing

// ========================================
// API Integration Tests
// ========================================

[<Fact>]
let ``POST /api/orders with valid order returns 200 OK`` () =
    task {
        use factory =
            new WebApplicationFactory<OrderTaking.WebApi.Program>()

        use client = factory.CreateClient()

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
                    "addressLine2": null,
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
                    }
                ]
            }
            """

        use content =
            new StringContent(json, Encoding.UTF8, "application/json")

        let! response = client.PostAsync("/api/orders", content)

        response.StatusCode
        |> should equal HttpStatusCode.OK

        let! responseBody = response.Content.ReadAsStringAsync()
        Assert.Contains("events", responseBody)
        Assert.Contains("OrderPlaced", responseBody)
    }

[<Fact>]
let ``POST /api/orders with invalid order returns 400 BadRequest`` () =
    task {
        use factory =
            new WebApplicationFactory<OrderTaking.WebApi.Program>()

        use client = factory.CreateClient()

        let json =
            """
            {
                "orderId": "invalid-guid",
                "customerInfo": {
                    "firstName": "",
                    "lastName": "",
                    "emailAddress": "invalid-email"
                },
                "shippingAddress": {
                    "addressLine1": "",
                    "addressLine2": null,
                    "city": "",
                    "zipCode": ""
                },
                "billingAddress": {
                    "addressLine1": "",
                    "addressLine2": null,
                    "city": "",
                    "zipCode": ""
                },
                "lines": []
            }
            """

        use content =
            new StringContent(json, Encoding.UTF8, "application/json")

        let! response = client.PostAsync("/api/orders", content)

        response.StatusCode
        |> should equal HttpStatusCode.BadRequest

        let! responseBody = response.Content.ReadAsStringAsync()
        Assert.Contains("error", responseBody)
    }

[<Fact>]
let ``POST /api/orders with empty body returns 400 BadRequest`` () =
    task {
        use factory =
            new WebApplicationFactory<OrderTaking.WebApi.Program>()

        use client = factory.CreateClient()

        use content =
            new StringContent("", Encoding.UTF8, "application/json")

        let! response = client.PostAsync("/api/orders", content)

        response.StatusCode
        |> should equal HttpStatusCode.BadRequest

        let! responseBody = response.Content.ReadAsStringAsync()
        Assert.Contains("error", responseBody)
    }

[<Fact>]
let ``POST /api/orders with invalid JSON returns 400 BadRequest`` () =
    task {
        use factory =
            new WebApplicationFactory<OrderTaking.WebApi.Program>()

        use client = factory.CreateClient()

        let invalidJson = "{invalid json"

        use content =
            new StringContent(invalidJson, Encoding.UTF8, "application/json")

        let! response = client.PostAsync("/api/orders", content)

        response.StatusCode
        |> should equal HttpStatusCode.BadRequest

        let! responseBody = response.Content.ReadAsStringAsync()
        Assert.Contains("error", responseBody)
    }

[<Fact>]
let ``POST /api/orders with product code not found returns 400 BadRequest`` () =
    task {
        use factory =
            new WebApplicationFactory<OrderTaking.WebApi.Program>()

        use client = factory.CreateClient()

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
                    "addressLine2": null,
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
                        "productCode": "INVALID",
                        "quantity": 2.0
                    }
                ]
            }
            """

        use content =
            new StringContent(json, Encoding.UTF8, "application/json")

        let! response = client.PostAsync("/api/orders", content)

        response.StatusCode
        |> should equal HttpStatusCode.BadRequest

        let! responseBody = response.Content.ReadAsStringAsync()
        Assert.Contains("error", responseBody)
        Assert.Contains("Product code not found", responseBody)
    }
