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

        // Swagger UI の設定（開発環境のみ）
        if app.Environment.IsDevelopment() then
            app.UseSwagger() |> ignore
            app.UseSwaggerUI() |> ignore

        // POST /api/orders エンドポイント
        app.MapPost(
            "/api/orders",
            Func<HttpRequest, PlaceOrderDependencies, Task<IResult>>
                (fun (request: HttpRequest) (deps: PlaceOrderDependencies) ->
                    task {
                        use reader = new StreamReader(request.Body)
                        let! json = reader.ReadToEndAsync()

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
        |> ignore

        app.Run()

        0 // Exit code
