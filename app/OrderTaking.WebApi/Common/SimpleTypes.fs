namespace OrderTaking.Common

open System

type 文字列50 = private 文字列50 of string

module 文字列50 =
    let 作成 str =
        if String.IsNullOrWhiteSpace(str) then
            Error "文字列50は必須です"
        elif str.Length > 50 then
            Error "文字列50は50文字以下である必要があります"
        else
            Ok (文字列50 str)

    let 値 (文字列50 str) = str

type メールアドレス = private メールアドレス of string

module メールアドレス =
    let 作成 str =
        if String.IsNullOrWhiteSpace(str) then
            Error "メールアドレスは必須です"
        elif not (str.Contains("@")) then
            Error "有効なメールアドレスを入力してください"
        else
            Ok (メールアドレス str)

    let 値 (メールアドレス email) = email

type 注文ID = private 注文ID of string

module 注文ID =
    let 作成 str =
        if String.IsNullOrWhiteSpace(str) then
            Error "注文IDは必須です"
        elif str.Length > 10 then
            Error "注文IDは10文字以下である必要があります"
        else
            Ok (注文ID str)

    let 値 (注文ID id) = id

type 商品コード =
    | ウィジェット of ウィジェットコード
    | ギズモ of ギズモコード

and ウィジェットコード = private ウィジェットコード of string
and ギズモコード = private ギズモコード of string

module ウィジェットコード =
    let 作成 str =
        if String.IsNullOrWhiteSpace(str) then
            Error "ウィジェットコードは必須です"
        elif not (str.StartsWith("W") && str.Length = 5) then
            Error "ウィジェットコードは'W'で始まる5文字である必要があります"
        else
            Ok (ウィジェットコード str)

    let 値 (ウィジェットコード code) = code

module ギズモコード =
    let 作成 str =
        if String.IsNullOrWhiteSpace(str) then
            Error "ギズモコードは必須です"
        elif not (str.StartsWith("G") && str.Length = 4) then
            Error "ギズモコードは'G'で始まる4文字である必要があります"
        else
            Ok (ギズモコード str)

    let 値 (ギズモコード code) = code

type 注文数量 =
    | 単位 of 単位数量
    | キログラム of キログラム数量

and 単位数量 = private 単位数量 of int
and キログラム数量 = private キログラム数量 of decimal

module 単位数量 =
    let 作成 qty =
        if qty < 1 || qty > 1000 then
            Error "単位数量は1-1000の範囲である必要があります"
        else
            Ok (単位数量 qty)

    let 値 (単位数量 qty) = qty

module キログラム数量 =
    let 作成 qty =
        if qty < 0.05m || qty > 100.00m then
            Error "キログラム数量は0.05-100.00の範囲である必要があります"
        else
            Ok (キログラム数量 qty)

    let 値 (キログラム数量 qty) = qty