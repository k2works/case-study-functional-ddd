namespace OrderTaking.WebApi

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open OrderTaking.Domain.Entities
open OrderTaking.Domain.DomainServices
open OrderTaking.Infrastructure.DependencyContainer
open OrderTaking.Infrastructure.JsonSerialization

// WebApplicationFactory から参照可能にするためのダミークラス
type Program() = class end

module Main =
    [<EntryPoint>]
    let main args =
        let builder =
            WebApplication.CreateBuilder(args)

        // Swagger/OpenAPI の設定
        builder.Services.AddEndpointsApiExplorer()
        |> ignore

        builder.Services.AddSwaggerGen() |> ignore

        // JSON シリアライゼーションの設定
        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>
            (fun (options: Microsoft.AspNetCore.Http.Json.JsonOptions) ->
                options.SerializerOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
                options.SerializerOptions.WriteIndented <- true
                options.SerializerOptions.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull
                options.SerializerOptions.Converters.Add(JsonFSharpConverter()))
        |> ignore

        // DI: PlaceOrderDependencies の登録
        let dependencies =
            createDefaultDependencies ()

        builder.Services.AddSingleton<PlaceOrderDependencies>(dependencies)
        |> ignore

        let app = builder.Build()

        // Swagger UI の設定
        app.UseSwagger() |> ignore
        app.UseSwaggerUI() |> ignore

        // ルートから Swagger UI へリダイレクト
        app
            .MapGet("/", Func<IResult>(fun () -> Results.Redirect("/swagger")))
            .ExcludeFromDescription()
        |> ignore

        // POST /api/orders エンドポイント
        app
            .MapPost(
                "/api/orders",
                Func<HttpRequest, PlaceOrderDependencies, Task<IResult>>
                    (fun (request: HttpRequest) (deps: PlaceOrderDependencies) ->
                        task {
                            use reader = new StreamReader(request.Body)
                            let! json = reader.ReadToEndAsync()

                            // 空のボディをチェック
                            if System.String.IsNullOrWhiteSpace(json) then
                                return Results.BadRequest({| error = "Request body is empty" |})
                            else
                                match deserializeUnvalidatedOrder json with
                                | Error deserializeError -> return Results.BadRequest({| error = deserializeError |})
                                | Ok unvalidatedOrder ->
                                    match
                                        PlaceOrderWorkflow.placeOrder
                                            deps.CheckProductCodeExists
                                            deps.CheckAddressExists
                                            deps.GetProductPrice
                                            deps.SendOrderAcknowledgment
                                            unvalidatedOrder
                                    with
                                    | Error error ->
                                        let errorMessage =
                                            PlaceOrderError.toString error

                                        return Results.BadRequest({| error = errorMessage |})
                                    | Ok events ->
                                        let eventJsons =
                                            events |> List.map serializePlaceOrderEvent

                                        return Results.Ok({| events = eventJsons |})
                        })
            )
            .Accepts<UnvalidatedOrder>("application/json")
            .WithName("PlaceOrder")
            .WithTags("Orders")
            .WithSummary("Place a new order")
            .WithDescription(
                """Submit a new order with customer information, shipping address, and order lines.

**Example Request:**
```json
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
```

**Valid Product Codes:**
- Widget codes: W1234, W5678, W9012
- Gizmo codes: G123, G1234, G5678, G9012"""
            )
        |> ignore

        app.Run()

        0 // Exit code
