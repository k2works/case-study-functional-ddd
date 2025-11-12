namespace OrderTaking.Infrastructure

open System.Text.Json
open System.Text.Json.Serialization
open OrderTaking.Domain.Entities
open OrderTaking.Domain.DomainServices

// ========================================
// JSON Serialization
//
// UnvalidatedOrder と PlaceOrderEvent の JSON シリアライゼーション
// ========================================

module JsonSerialization =

    /// JSON シリアライゼーションオプション
    let private jsonOptions =
        let options = JsonSerializerOptions()
        options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
        options.WriteIndented <- true
        options.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull

        // F# のオプション型と判別共用体のサポート
        options.Converters.Add(JsonFSharpConverter())
        options

    /// UnvalidatedOrder を JSON からデシリアライズする
    let deserializeUnvalidatedOrder (json: string) : Result<UnvalidatedOrder, string> =
        try
            let order =
                JsonSerializer.Deserialize<UnvalidatedOrder>(json, jsonOptions)

            Ok order
        with ex ->
            Error $"JSON deserialization failed: {ex.Message}"

    /// PlaceOrderEvent を JSON にシリアライズする
    let serializePlaceOrderEvent (event: PlaceOrderEvent) : string =
        JsonSerializer.Serialize(event, jsonOptions)
