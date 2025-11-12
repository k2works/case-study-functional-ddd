module OrderTaking.Tests.CompoundTypes

open Xunit
open FsUnit.Xunit
open OrderTaking.Domain.ConstrainedTypes
open OrderTaking.Domain.CompoundTypes

// ========================================
// PersonalName Tests
// ========================================

[<Fact>]
let ``PersonalName.create は有効な名前を受け入れる`` () =
    // Arrange
    let firstName = "John"
    let lastName = "Doe"

    // Act
    let result =
        PersonalName.create firstName lastName

    // Assert
    match result with
    | Ok name ->
        let (fn, ln) = PersonalName.value name
        String50.value fn |> should equal firstName
        String50.value ln |> should equal lastName
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``PersonalName.create は空の名前を拒否する`` () =
    // Act
    let result1 = PersonalName.create "" "Doe"
    let result2 = PersonalName.create "John" ""

    // Assert
    match result1 with
    | Error _ -> ()
    | Ok _ -> failwith "Expected Error for empty firstName"

    match result2 with
    | Error _ -> ()
    | Ok _ -> failwith "Expected Error for empty lastName"

[<Fact>]
let ``PersonalName.create は長すぎる名前を拒否する`` () =
    // Arrange
    let longName = String.replicate 51 "a"

    // Act
    let result1 =
        PersonalName.create longName "Doe"

    let result2 =
        PersonalName.create "John" longName

    // Assert
    match result1 with
    | Error _ -> ()
    | Ok _ -> failwith "Expected Error for long firstName"

    match result2 with
    | Error _ -> ()
    | Ok _ -> failwith "Expected Error for long lastName"

// ========================================
// CustomerInfo Tests
// ========================================

[<Fact>]
let ``CustomerInfo.create は有効な顧客情報を受け入れる`` () =
    // Arrange
    let firstName = "John"
    let lastName = "Doe"
    let email = "john.doe@example.com"

    // Act
    let result =
        CustomerInfo.create firstName lastName email

    // Assert
    match result with
    | Ok _ -> ()
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``CustomerInfo.create は無効なメールアドレスを拒否する`` () =
    // Arrange
    let firstName = "John"
    let lastName = "Doe"
    let invalidEmail = "not-an-email"

    // Act
    let result =
        CustomerInfo.create firstName lastName invalidEmail

    // Assert
    match result with
    | Error _ -> ()
    | Ok _ -> failwith "Expected Error for invalid email"

[<Fact>]
let ``CustomerInfo.value は元の値を返す`` () =
    // Arrange
    let firstName = "John"
    let lastName = "Doe"
    let email = "john.doe@example.com"

    // Act
    match CustomerInfo.create firstName lastName email with
    | Ok customerInfo ->
        let (name, emailAddr) =
            CustomerInfo.value customerInfo

        let (fn, ln) = PersonalName.value name
        String50.value fn |> should equal firstName
        String50.value ln |> should equal lastName
        EmailAddress.value emailAddr |> should equal email
    | Error _ -> failwith "Expected Ok"

// ========================================
// Address Tests
// ========================================

[<Fact>]
let ``Address.create は有効な住所を受け入れる`` () =
    // Arrange
    let line1 = "123 Main St"
    let city = "Tokyo"
    let zipCode = "12345"

    // Act
    let result =
        Address.create line1 None city zipCode

    // Assert
    match result with
    | Ok address ->
        let (l1, l2, c, z) = Address.value address
        String50.value l1 |> should equal line1
        l2 |> should equal None
        String50.value c |> should equal city
        ZipCode.value z |> should equal zipCode
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``Address.create は AddressLine2 が Some の場合も受け入れる`` () =
    // Arrange
    let line1 = "123 Main St"
    let line2 = Some "Apt 456"
    let city = "Tokyo"
    let zipCode = "12345"

    // Act
    let result =
        Address.create line1 line2 city zipCode

    // Assert
    match result with
    | Ok address ->
        let (l1, l2opt, c, z) =
            Address.value address

        String50.value l1 |> should equal line1

        match l2opt with
        | Some l2 -> String50.value l2 |> should equal "Apt 456"
        | None -> failwith "Expected Some AddressLine2"

        String50.value c |> should equal city
        ZipCode.value z |> should equal zipCode
    | Error msg -> failwith $"Expected Ok, got Error: {msg}"

[<Fact>]
let ``Address.create は無効な郵便番号を拒否する`` () =
    // Arrange
    let line1 = "123 Main St"
    let city = "Tokyo"
    let invalidZip = "ABC"

    // Act
    let result =
        Address.create line1 None city invalidZip

    // Assert
    match result with
    | Error _ -> ()
    | Ok _ -> failwith "Expected Error for invalid zip code"

// ========================================
// OrderLineId Tests
// ========================================

[<Fact>]
let ``OrderLineId.create は有効な GUID を受け入れる`` () =
    // Arrange
    let guid = System.Guid.NewGuid()

    // Act
    let orderLineId = OrderLineId.create guid

    // Assert
    OrderLineId.value orderLineId |> should equal guid

[<Fact>]
let ``OrderLineId.generate は新しい GUID を生成する`` () =
    // Act
    let id1 = OrderLineId.generate ()
    let id2 = OrderLineId.generate ()

    // Assert
    OrderLineId.value id1
    |> should not' (equal (OrderLineId.value id2))

[<Fact>]
let ``OrderLineId: 等価性比較が正しく動作する`` () =
    // Arrange
    let guid = System.Guid.NewGuid()
    let id1 = OrderLineId.create guid
    let id2 = OrderLineId.create guid
    let id3 = OrderLineId.generate ()

    // Assert
    id1 |> should equal id2
    id1 |> should not' (equal id3)

// ========================================
// OrderId Tests
// ========================================

[<Fact>]
let ``OrderId.create は有効な GUID を受け入れる`` () =
    // Arrange
    let guid = System.Guid.NewGuid()

    // Act
    let orderId = OrderId.create guid

    // Assert
    OrderId.value orderId |> should equal guid

[<Fact>]
let ``OrderId.generate は新しい GUID を生成する`` () =
    // Act
    let id1 = OrderId.generate ()
    let id2 = OrderId.generate ()

    // Assert
    OrderId.value id1
    |> should not' (equal (OrderId.value id2))

[<Fact>]
let ``OrderId: 等価性比較が正しく動作する`` () =
    // Arrange
    let guid = System.Guid.NewGuid()
    let id1 = OrderId.create guid
    let id2 = OrderId.create guid
    let id3 = OrderId.generate ()

    // Assert
    id1 |> should equal id2
    id1 |> should not' (equal id3)

// ========================================
// Property-Based Tests
// ========================================

open FsCheck.Xunit

[<Property>]
let ``PersonalName: 有効な名前（1-50文字）のラウンドトリップテスト`` (firstName: string) (lastName: string) =
    if
        not (System.String.IsNullOrWhiteSpace(firstName))
        && not (System.String.IsNullOrWhiteSpace(lastName))
        && firstName.Length <= 50
        && lastName.Length <= 50
    then
        match PersonalName.create firstName lastName with
        | Ok name ->
            let (fn, ln) = PersonalName.value name

            String50.value fn = firstName
            && String50.value ln = lastName
        | Error _ -> false
    else
        true

[<Property>]
let ``CustomerInfo: 有効な顧客情報のラウンドトリップテスト`` (firstName: string) (lastName: string) (email: string) =
    if
        not (System.String.IsNullOrWhiteSpace(firstName))
        && not (System.String.IsNullOrWhiteSpace(lastName))
        && firstName.Length <= 50
        && lastName.Length <= 50
        && not (System.String.IsNullOrWhiteSpace(email))
        && email.Contains("@")
        && email.Length <= 100
    then
        match CustomerInfo.create firstName lastName email with
        | Ok customerInfo ->
            let (name, emailAddr) =
                CustomerInfo.value customerInfo

            let (fn, ln) = PersonalName.value name

            String50.value fn = firstName
            && String50.value ln = lastName
            && EmailAddress.value emailAddr = email
        | Error _ -> false
    else
        true
