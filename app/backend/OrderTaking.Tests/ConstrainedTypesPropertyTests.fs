module OrderTaking.Tests.ConstrainedTypesPropertyTests

open FsCheck.Xunit
open OrderTaking.Domain.ConstrainedTypes

// ========================================
// String50 Property Tests
// ========================================

[<Property>]
let ``String50: 有効な文字列（1-50文字）のラウンドトリップテスト`` (str: string) =
    if
        not (System.String.IsNullOrWhiteSpace(str))
        && str.Length <= 50
    then
        match String50.create "Field" str with
        | Ok s50 ->
            let value = String50.value s50
            value = str
        | Error _ -> false
    else
        true

[<Property>]
let ``String50: 値の長さは常に 50 文字以下`` (str: string) =
    if
        not (System.String.IsNullOrWhiteSpace(str))
        && str.Length <= 50
    then
        match String50.create "Field" str with
        | Ok s50 ->
            let value = String50.value s50
            value.Length > 0 && value.Length <= 50
        | Error _ -> false
    else
        true

[<Property>]
let ``String50: 空文字列は作成失敗`` () =
    match String50.create "Field" "" with
    | Error _ -> true
    | Ok _ -> false

[<Property>]
let ``String50: 51文字以上は作成失敗`` (str: string) =
    if str <> null && str.Length > 50 then
        match String50.create "Field" str with
        | Error _ -> true
        | Ok _ -> false
    else
        true

// ========================================
// String100 Property Tests
// ========================================

[<Property>]
let ``String100: 有効な文字列（1-100文字）のラウンドトリップテスト`` (str: string) =
    if
        not (System.String.IsNullOrWhiteSpace(str))
        && str.Length <= 100
    then
        match String100.create "Field" str with
        | Ok s100 ->
            let value = String100.value s100
            value = str
        | Error _ -> false
    else
        true

[<Property>]
let ``String100: 値の長さは常に 100 文字以下`` (str: string) =
    if
        not (System.String.IsNullOrWhiteSpace(str))
        && str.Length <= 100
    then
        match String100.create "Field" str with
        | Ok s100 ->
            let value = String100.value s100
            value.Length > 0 && value.Length <= 100
        | Error _ -> false
    else
        true

// ========================================
// String255 Property Tests
// ========================================

[<Property>]
let ``String255: 有効な文字列（1-255文字）のラウンドトリップテスト`` (str: string) =
    if
        not (System.String.IsNullOrWhiteSpace(str))
        && str.Length <= 255
    then
        match String255.create "Field" str with
        | Ok s255 ->
            let value = String255.value s255
            value = str
        | Error _ -> false
    else
        true

[<Property>]
let ``String255: 値の長さは常に 255 文字以下`` (str: string) =
    if
        not (System.String.IsNullOrWhiteSpace(str))
        && str.Length <= 255
    then
        match String255.create "Field" str with
        | Ok s255 ->
            let value = String255.value s255
            value.Length > 0 && value.Length <= 255
        | Error _ -> false
    else
        true

// ========================================
// UnitQuantity Property Tests
// ========================================

[<Property>]
let ``UnitQuantity: 有効な数量（1-1000）のラウンドトリップテスト`` (qty: int) =
    if qty >= 1 && qty <= 1000 then
        match UnitQuantity.create "Quantity" qty with
        | Ok uq ->
            let value = UnitQuantity.value uq
            value = qty
        | Error _ -> false
    else
        true

[<Property>]
let ``UnitQuantity: 値は常に 1-1000 の範囲内`` (qty: int) =
    if qty >= 1 && qty <= 1000 then
        match UnitQuantity.create "Quantity" qty with
        | Ok uq ->
            let value = UnitQuantity.value uq
            value >= 1 && value <= 1000
        | Error _ -> false
    else
        true

[<Property>]
let ``UnitQuantity: 0 以下は作成失敗`` (qty: int) =
    if qty <= 0 then
        match UnitQuantity.create "Quantity" qty with
        | Error _ -> true
        | Ok _ -> false
    else
        true

[<Property>]
let ``UnitQuantity: 1001 以上は作成失敗`` (qty: int) =
    if qty > 1000 then
        match UnitQuantity.create "Quantity" qty with
        | Error _ -> true
        | Ok _ -> false
    else
        true

// ========================================
// Price Property Tests
// ========================================

[<Property>]
let ``Price: 有効な価格（0-1000）のラウンドトリップテスト`` (price: decimal) =
    if price >= 0.0m && price <= 1000.0m then
        match Price.create "Price" price with
        | Ok p ->
            let value = Price.value p
            value = price
        | Error _ -> false
    else
        true

[<Property>]
let ``Price: 値は常に 0-1000 の範囲内`` (price: decimal) =
    if price >= 0.0m && price <= 1000.0m then
        match Price.create "Price" price with
        | Ok p ->
            let value = Price.value p
            value >= 0.0m && value <= 1000.0m
        | Error _ -> false
    else
        true

[<Property>]
let ``Price: 負の値は作成失敗`` (price: decimal) =
    if price < 0.0m then
        match Price.create "Price" price with
        | Error _ -> true
        | Ok _ -> false
    else
        true

// ========================================
// EmailAddress Property Tests
// ========================================

[<Property>]
let ``EmailAddress: アットマークを含む文字列は作成可能`` (str: string) =
    if
        not (System.String.IsNullOrWhiteSpace(str))
        && str.Length <= 50
    then
        let email = $"{str}@example.com"

        if email.Length <= 100 then
            match EmailAddress.create "Email" email with
            | Ok ea ->
                let value = EmailAddress.value ea
                value.Contains("@")
            | Error _ -> false
        else
            true
    else
        true

[<Property>]
let ``EmailAddress: アットマークを含まない文字列は作成失敗`` (str: string) =
    if
        not (System.String.IsNullOrWhiteSpace(str))
        && str.Length <= 100
        && not (str.Contains("@"))
    then
        match EmailAddress.create "Email" str with
        | Error _ -> true
        | Ok _ -> false
    else
        true
