namespace OrderTaking.Common

open System

type String50 = private String50 of string

module String50 =
    let create str =
        if String.IsNullOrWhiteSpace(str) then
            Error "String50は必須です"
        elif str.Length > 50 then
            Error "String50は50文字以下である必要があります"
        else
            Ok (String50 str)

    let value (String50 str) = str

type EmailAddress = private EmailAddress of string

module EmailAddress =
    let create str =
        if String.IsNullOrWhiteSpace(str) then
            Error "メールアドレスは必須です"
        elif not (str.Contains("@")) then
            Error "有効なメールアドレスを入力してください"
        else
            Ok (EmailAddress str)

    let value (EmailAddress email) = email

type OrderId = private OrderId of string

module OrderId =
    let create str =
        if String.IsNullOrWhiteSpace(str) then
            Error "注文IDは必須です"
        elif str.Length > 10 then
            Error "注文IDは10文字以下である必要があります"
        else
            Ok (OrderId str)

    let value (OrderId id) = id

type ProductCode =
    | Widget of WidgetCode
    | Gizmo of GizmoCode

and WidgetCode = private WidgetCode of string
and GizmoCode = private GizmoCode of string

module WidgetCode =
    let create str =
        if String.IsNullOrWhiteSpace(str) then
            Error "WidgetCodeは必須です"
        elif not (str.StartsWith("W") && str.Length = 5) then
            Error "WidgetCodeは'W'で始まる5文字である必要があります"
        else
            Ok (WidgetCode str)

    let value (WidgetCode code) = code

module GizmoCode =
    let create str =
        if String.IsNullOrWhiteSpace(str) then
            Error "GizmoCodeは必須です"
        elif not (str.StartsWith("G") && str.Length = 4) then
            Error "GizmoCodeは'G'で始まる4文字である必要があります"
        else
            Ok (GizmoCode str)

    let value (GizmoCode code) = code

type OrderQuantity =
    | Unit of UnitQuantity
    | Kilogram of KilogramQuantity

and UnitQuantity = private UnitQuantity of int
and KilogramQuantity = private KilogramQuantity of decimal

module UnitQuantity =
    let create qty =
        if qty < 1 || qty > 1000 then
            Error "Unit数量は1-1000の範囲である必要があります"
        else
            Ok (UnitQuantity qty)

    let value (UnitQuantity qty) = qty

module KilogramQuantity =
    let create qty =
        if qty < 0.05m || qty > 100.00m then
            Error "Kilogram数量は0.05-100.00の範囲である必要があります"
        else
            Ok (KilogramQuantity qty)

    let value (KilogramQuantity qty) = qty