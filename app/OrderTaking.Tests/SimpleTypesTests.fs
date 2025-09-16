module OrderTaking.Tests.SimpleTypesTests

open NUnit.Framework
open OrderTaking.Common

[<TestFixture>]
type String50Tests() =

    [<Test>]
    member _.``空文字列の場合はエラーを返す``() =
        let 結果 = 文字列50.作成 ""
        match 結果 with
        | Error メッセージ ->
            Assert.That(メッセージ, Is.EqualTo("文字列50は必須です"))
        | Ok _ ->
            Assert.Fail("空文字列が受け入れられました")

    [<Test>]
    member _.``50文字を超える文字列の場合はエラーを返す``() =
        let 長い文字列 = String.replicate 51 "a"
        let 結果 = 文字列50.作成 長い文字列
        match 結果 with
        | Error メッセージ ->
            Assert.That(メッセージ, Is.EqualTo("文字列50は50文字以下である必要があります"))
        | Ok _ ->
            Assert.Fail("51文字の文字列が受け入れられました")

    [<Test>]
    member _.``有効な文字列の場合は成功する``() =
        let 結果 = 文字列50.作成 "有効な文字列"
        match 結果 with
        | Ok 値 ->
            Assert.That(文字列50.値 値, Is.EqualTo("有効な文字列"))
        | Error メッセージ ->
            Assert.Fail($"有効な文字列が拒否されました: {メッセージ}")

[<TestFixture>]
type EmailAddressTests() =

    [<Test>]
    member _.``空文字列の場合はエラーを返す``() =
        let 結果 = メールアドレス.作成 ""
        match 結果 with
        | Error メッセージ ->
            Assert.That(メッセージ, Is.EqualTo("メールアドレスは必須です"))
        | Ok _ ->
            Assert.Fail("空文字列が受け入れられました")

    [<Test>]
    member _.``@を含まない場合はエラーを返す``() =
        let 結果 = メールアドレス.作成 "invalid.email"
        match 結果 with
        | Error メッセージ ->
            Assert.That(メッセージ, Is.EqualTo("有効なメールアドレスを入力してください"))
        | Ok _ ->
            Assert.Fail("無効なメールアドレスが受け入れられました")

    [<Test>]
    member _.``有効なメールアドレスの場合は成功する``() =
        let 結果 = メールアドレス.作成 "test@example.com"
        match 結果 with
        | Ok 値 ->
            Assert.That(メールアドレス.値 値, Is.EqualTo("test@example.com"))
        | Error メッセージ ->
            Assert.Fail($"有効なメールアドレスが拒否されました: {メッセージ}")

[<TestFixture>]
type OrderIdTests() =

    [<Test>]
    member _.``空文字列の場合はエラーを返す``() =
        let 結果 = 注文ID.作成 ""
        match 結果 with
        | Error メッセージ ->
            Assert.That(メッセージ, Is.EqualTo("注文IDは必須です"))
        | Ok _ ->
            Assert.Fail("空文字列が受け入れられました")

    [<Test>]
    member _.``10文字を超える場合はエラーを返す``() =
        let 結果 = 注文ID.作成 "12345678901"
        match 結果 with
        | Error メッセージ ->
            Assert.That(メッセージ, Is.EqualTo("注文IDは10文字以下である必要があります"))
        | Ok _ ->
            Assert.Fail("11文字のIDが受け入れられました")

    [<Test>]
    member _.``有効な注文IDの場合は成功する``() =
        let 結果 = 注文ID.作成 "ORDER001"
        match 結果 with
        | Ok 値 ->
            Assert.That(注文ID.値 値, Is.EqualTo("ORDER001"))
        | Error メッセージ ->
            Assert.Fail($"有効な注文IDが拒否されました: {メッセージ}")

[<TestFixture>]
type WidgetCodeTests() =

    [<Test>]
    member _.``空文字列の場合はエラーを返す``() =
        let 結果 = ウィジェットコード.作成 ""
        match 結果 with
        | Error メッセージ ->
            Assert.That(メッセージ, Is.EqualTo("ウィジェットコードは必須です"))
        | Ok _ ->
            Assert.Fail("空文字列が受け入れられました")

    [<Test>]
    member _.``Wで始まらない場合はエラーを返す``() =
        let 結果 = ウィジェットコード.作成 "A1234"
        match 結果 with
        | Error メッセージ ->
            Assert.That(メッセージ, Is.EqualTo("ウィジェットコードは'W'で始まる5文字である必要があります"))
        | Ok _ ->
            Assert.Fail("無効なコードが受け入れられました")

    [<Test>]
    member _.``5文字でない場合はエラーを返す``() =
        let 結果 = ウィジェットコード.作成 "W123"
        match 結果 with
        | Error メッセージ ->
            Assert.That(メッセージ, Is.EqualTo("ウィジェットコードは'W'で始まる5文字である必要があります"))
        | Ok _ ->
            Assert.Fail("4文字のコードが受け入れられました")

    [<Test>]
    member _.``有効なウィジェットコードの場合は成功する``() =
        let 結果 = ウィジェットコード.作成 "W1234"
        match 結果 with
        | Ok 値 ->
            Assert.That(ウィジェットコード.値 値, Is.EqualTo("W1234"))
        | Error メッセージ ->
            Assert.Fail($"有効なウィジェットコードが拒否されました: {メッセージ}")

[<TestFixture>]
type GizmoCodeTests() =

    [<Test>]
    member _.``Gで始まらない場合はエラーを返す``() =
        let 結果 = ギズモコード.作成 "A123"
        match 結果 with
        | Error メッセージ ->
            Assert.That(メッセージ, Is.EqualTo("ギズモコードは'G'で始まる4文字である必要があります"))
        | Ok _ ->
            Assert.Fail("無効なコードが受け入れられました")

    [<Test>]
    member _.``4文字でない場合はエラーを返す``() =
        let 結果 = ギズモコード.作成 "G12345"
        match 結果 with
        | Error メッセージ ->
            Assert.That(メッセージ, Is.EqualTo("ギズモコードは'G'で始まる4文字である必要があります"))
        | Ok _ ->
            Assert.Fail("5文字のコードが受け入れられました")

    [<Test>]
    member _.``有効なギズモコードの場合は成功する``() =
        let 結果 = ギズモコード.作成 "G123"
        match 結果 with
        | Ok 値 ->
            Assert.That(ギズモコード.値 値, Is.EqualTo("G123"))
        | Error メッセージ ->
            Assert.Fail($"有効なギズモコードが拒否されました: {メッセージ}")

[<TestFixture>]
type UnitQuantityTests() =

    [<Test>]
    member _.``0の場合はエラーを返す``() =
        let 結果 = 単位数量.作成 0
        match 結果 with
        | Error メッセージ ->
            Assert.That(メッセージ, Is.EqualTo("単位数量は1-1000の範囲である必要があります"))
        | Ok _ ->
            Assert.Fail("0が受け入れられました")

    [<Test>]
    member _.``1001の場合はエラーを返す``() =
        let 結果 = 単位数量.作成 1001
        match 結果 with
        | Error メッセージ ->
            Assert.That(メッセージ, Is.EqualTo("単位数量は1-1000の範囲である必要があります"))
        | Ok _ ->
            Assert.Fail("1001が受け入れられました")

    [<Test>]
    member _.``有効な数量の場合は成功する``() =
        let 結果 = 単位数量.作成 100
        match 結果 with
        | Ok 値 ->
            Assert.That(単位数量.値 値, Is.EqualTo(100))
        | Error メッセージ ->
            Assert.Fail($"有効な数量が拒否されました: {メッセージ}")

[<TestFixture>]
type KilogramQuantityTests() =

    [<Test>]
    member _.``0.04の場合はエラーを返す``() =
        let 結果 = キログラム数量.作成 0.04m
        match 結果 with
        | Error メッセージ ->
            Assert.That(メッセージ, Is.EqualTo("キログラム数量は0.05-100.00の範囲である必要があります"))
        | Ok _ ->
            Assert.Fail("0.04が受け入れられました")

    [<Test>]
    member _.``100.01の場合はエラーを返す``() =
        let 結果 = キログラム数量.作成 100.01m
        match 結果 with
        | Error メッセージ ->
            Assert.That(メッセージ, Is.EqualTo("キログラム数量は0.05-100.00の範囲である必要があります"))
        | Ok _ ->
            Assert.Fail("100.01が受け入れられました")

    [<Test>]
    member _.``有効な数量の場合は成功する``() =
        let 結果 = キログラム数量.作成 10.5m
        match 結果 with
        | Ok 値 ->
            Assert.That(キログラム数量.値 値, Is.EqualTo(10.5m))
        | Error メッセージ ->
            Assert.Fail($"有効な数量が拒否されました: {メッセージ}")