module OrderTaking.Tests.SimpleTypesTests

open NUnit.Framework
open OrderTaking.Common

[<TestFixture>]
type String50Tests() =

    [<Test>]
    member _.``空文字列の場合はエラーを返す``() =
        let result = String50.create ""
        match result with
        | Error msg ->
            Assert.That(msg, Is.EqualTo("String50は必須です"))
        | Ok _ ->
            Assert.Fail("空文字列が受け入れられました")

    [<Test>]
    member _.``50文字を超える文字列の場合はエラーを返す``() =
        let longString = String.replicate 51 "a"
        let result = String50.create longString
        match result with
        | Error msg ->
            Assert.That(msg, Is.EqualTo("String50は50文字以下である必要があります"))
        | Ok _ ->
            Assert.Fail("51文字の文字列が受け入れられました")

    [<Test>]
    member _.``有効な文字列の場合は成功する``() =
        let result = String50.create "有効な文字列"
        match result with
        | Ok value ->
            Assert.That(String50.value value, Is.EqualTo("有効な文字列"))
        | Error msg ->
            Assert.Fail($"有効な文字列が拒否されました: {msg}")

[<TestFixture>]
type EmailAddressTests() =

    [<Test>]
    member _.``空文字列の場合はエラーを返す``() =
        let result = EmailAddress.create ""
        match result with
        | Error msg ->
            Assert.That(msg, Is.EqualTo("メールアドレスは必須です"))
        | Ok _ ->
            Assert.Fail("空文字列が受け入れられました")

    [<Test>]
    member _.``@を含まない場合はエラーを返す``() =
        let result = EmailAddress.create "invalid.email"
        match result with
        | Error msg ->
            Assert.That(msg, Is.EqualTo("有効なメールアドレスを入力してください"))
        | Ok _ ->
            Assert.Fail("無効なメールアドレスが受け入れられました")

    [<Test>]
    member _.``有効なメールアドレスの場合は成功する``() =
        let result = EmailAddress.create "test@example.com"
        match result with
        | Ok value ->
            Assert.That(EmailAddress.value value, Is.EqualTo("test@example.com"))
        | Error msg ->
            Assert.Fail($"有効なメールアドレスが拒否されました: {msg}")

[<TestFixture>]
type OrderIdTests() =

    [<Test>]
    member _.``空文字列の場合はエラーを返す``() =
        let result = OrderId.create ""
        match result with
        | Error msg ->
            Assert.That(msg, Is.EqualTo("注文IDは必須です"))
        | Ok _ ->
            Assert.Fail("空文字列が受け入れられました")

    [<Test>]
    member _.``10文字を超える場合はエラーを返す``() =
        let result = OrderId.create "12345678901"
        match result with
        | Error msg ->
            Assert.That(msg, Is.EqualTo("注文IDは10文字以下である必要があります"))
        | Ok _ ->
            Assert.Fail("11文字のIDが受け入れられました")

    [<Test>]
    member _.``有効な注文IDの場合は成功する``() =
        let result = OrderId.create "ORDER001"
        match result with
        | Ok value ->
            Assert.That(OrderId.value value, Is.EqualTo("ORDER001"))
        | Error msg ->
            Assert.Fail($"有効な注文IDが拒否されました: {msg}")

[<TestFixture>]
type WidgetCodeTests() =

    [<Test>]
    member _.``空文字列の場合はエラーを返す``() =
        let result = WidgetCode.create ""
        match result with
        | Error msg ->
            Assert.That(msg, Is.EqualTo("WidgetCodeは必須です"))
        | Ok _ ->
            Assert.Fail("空文字列が受け入れられました")

    [<Test>]
    member _.``Wで始まらない場合はエラーを返す``() =
        let result = WidgetCode.create "A1234"
        match result with
        | Error msg ->
            Assert.That(msg, Is.EqualTo("WidgetCodeは'W'で始まる5文字である必要があります"))
        | Ok _ ->
            Assert.Fail("無効なコードが受け入れられました")

    [<Test>]
    member _.``5文字でない場合はエラーを返す``() =
        let result = WidgetCode.create "W123"
        match result with
        | Error msg ->
            Assert.That(msg, Is.EqualTo("WidgetCodeは'W'で始まる5文字である必要があります"))
        | Ok _ ->
            Assert.Fail("4文字のコードが受け入れられました")

    [<Test>]
    member _.``有効なWidgetCodeの場合は成功する``() =
        let result = WidgetCode.create "W1234"
        match result with
        | Ok value ->
            Assert.That(WidgetCode.value value, Is.EqualTo("W1234"))
        | Error msg ->
            Assert.Fail($"有効なWidgetCodeが拒否されました: {msg}")

[<TestFixture>]
type GizmoCodeTests() =

    [<Test>]
    member _.``Gで始まらない場合はエラーを返す``() =
        let result = GizmoCode.create "A123"
        match result with
        | Error msg ->
            Assert.That(msg, Is.EqualTo("GizmoCodeは'G'で始まる4文字である必要があります"))
        | Ok _ ->
            Assert.Fail("無効なコードが受け入れられました")

    [<Test>]
    member _.``4文字でない場合はエラーを返す``() =
        let result = GizmoCode.create "G12345"
        match result with
        | Error msg ->
            Assert.That(msg, Is.EqualTo("GizmoCodeは'G'で始まる4文字である必要があります"))
        | Ok _ ->
            Assert.Fail("5文字のコードが受け入れられました")

    [<Test>]
    member _.``有効なGizmoCodeの場合は成功する``() =
        let result = GizmoCode.create "G123"
        match result with
        | Ok value ->
            Assert.That(GizmoCode.value value, Is.EqualTo("G123"))
        | Error msg ->
            Assert.Fail($"有効なGizmoCodeが拒否されました: {msg}")

[<TestFixture>]
type UnitQuantityTests() =

    [<Test>]
    member _.``0の場合はエラーを返す``() =
        let result = UnitQuantity.create 0
        match result with
        | Error msg ->
            Assert.That(msg, Is.EqualTo("Unit数量は1-1000の範囲である必要があります"))
        | Ok _ ->
            Assert.Fail("0が受け入れられました")

    [<Test>]
    member _.``1001の場合はエラーを返す``() =
        let result = UnitQuantity.create 1001
        match result with
        | Error msg ->
            Assert.That(msg, Is.EqualTo("Unit数量は1-1000の範囲である必要があります"))
        | Ok _ ->
            Assert.Fail("1001が受け入れられました")

    [<Test>]
    member _.``有効な数量の場合は成功する``() =
        let result = UnitQuantity.create 100
        match result with
        | Ok value ->
            Assert.That(UnitQuantity.value value, Is.EqualTo(100))
        | Error msg ->
            Assert.Fail($"有効な数量が拒否されました: {msg}")

[<TestFixture>]
type KilogramQuantityTests() =

    [<Test>]
    member _.``0.04の場合はエラーを返す``() =
        let result = KilogramQuantity.create 0.04m
        match result with
        | Error msg ->
            Assert.That(msg, Is.EqualTo("Kilogram数量は0.05-100.00の範囲である必要があります"))
        | Ok _ ->
            Assert.Fail("0.04が受け入れられました")

    [<Test>]
    member _.``100.01の場合はエラーを返す``() =
        let result = KilogramQuantity.create 100.01m
        match result with
        | Error msg ->
            Assert.That(msg, Is.EqualTo("Kilogram数量は0.05-100.00の範囲である必要があります"))
        | Ok _ ->
            Assert.Fail("100.01が受け入れられました")

    [<Test>]
    member _.``有効な数量の場合は成功する``() =
        let result = KilogramQuantity.create 10.5m
        match result with
        | Ok value ->
            Assert.That(KilogramQuantity.value value, Is.EqualTo(10.5m))
        | Error msg ->
            Assert.Fail($"有効な数量が拒否されました: {msg}")