namespace OrderTaking.Domain

/// 制約付き型のサンプル実装
/// イテレーション 1: 基本的な型のみ実装
/// イテレーション 2: 完全な実装と追加の型
module ConstrainedTypes =

    // ========================================
    // 基本的な文字列制約型
    // ========================================

    /// 50 文字以内の非空文字列
    type String50 = private String50 of string

    module String50 =
        /// String50 を作成する（検証付き）
        let create fieldName str =
            if System.String.IsNullOrWhiteSpace(str) then
                Error $"{fieldName} must not be null or empty"
            elif str.Length > 50 then
                Error $"{fieldName} must not be more than 50 chars"
            else
                Ok(String50 str)

        /// String50 の内部値を取得
        let value (String50 str) = str

        /// 安全でない作成（テスト用）
        let unsafeCreate str = String50 str

    // ========================================
    // メールアドレス
    // ========================================

    /// メールアドレス（@ を含む文字列）
    type EmailAddress = private EmailAddress of string

    module EmailAddress =
        /// EmailAddress を作成する（簡易検証）
        let create fieldName str =
            if System.String.IsNullOrWhiteSpace(str) then
                Error $"{fieldName} must not be null or empty"
            elif not (str.Contains("@")) then
                Error $"{fieldName} must contain @"
            elif str.Length > 100 then
                Error $"{fieldName} must not be more than 100 chars"
            else
                Ok(EmailAddress str)

        /// EmailAddress の内部値を取得
        let value (EmailAddress str) = str

        /// 安全でない作成（テスト用）
        let unsafeCreate str = EmailAddress str

    // ========================================
    // 郵便番号
    // ========================================

    /// 郵便番号（5 桁の数字）
    type ZipCode = private ZipCode of string

    module ZipCode =
        /// ZipCode を作成する（簡易検証）
        let create fieldName str =
            if System.String.IsNullOrWhiteSpace(str) then
                Error $"{fieldName} must not be null or empty"
            elif str.Length <> 5 then
                Error $"{fieldName} must be 5 chars"
            elif not (str |> Seq.forall System.Char.IsDigit) then
                Error $"{fieldName} must be all digits"
            else
                Ok(ZipCode str)

        /// ZipCode の内部値を取得
        let value (ZipCode str) = str

        /// 安全でない作成（テスト用）
        let unsafeCreate str = ZipCode str

    // ========================================
    // 商品コード
    // ========================================

    /// Widget 商品コード（"W" + 4桁数字）
    type WidgetCode = private WidgetCode of string

    module WidgetCode =
        /// WidgetCode を作成する
        let create fieldName str =
            if System.String.IsNullOrWhiteSpace(str) then
                Error $"{fieldName} must not be null or empty"
            elif str.Length <> 5 then
                Error $"{fieldName} must be 5 chars"
            elif not (str.StartsWith("W")) then
                Error $"{fieldName} must start with 'W'"
            elif not (str.Substring(1) |> Seq.forall System.Char.IsDigit) then
                Error $"{fieldName} must be 'W' followed by 4 digits"
            else
                Ok(WidgetCode str)

        /// WidgetCode の内部値を取得
        let value (WidgetCode str) = str

        /// 安全でない作成（テスト用）
        let unsafeCreate str = WidgetCode str

    /// Gizmo 商品コード（"G" + 3桁数字）
    type GizmoCode = private GizmoCode of string

    module GizmoCode =
        /// GizmoCode を作成する
        let create fieldName str =
            if System.String.IsNullOrWhiteSpace(str) then
                Error $"{fieldName} must not be null or empty"
            elif str.Length <> 4 then
                Error $"{fieldName} must be 4 chars"
            elif not (str.StartsWith("G")) then
                Error $"{fieldName} must start with 'G'"
            elif not (str.Substring(1) |> Seq.forall System.Char.IsDigit) then
                Error $"{fieldName} must be 'G' followed by 3 digits"
            else
                Ok(GizmoCode str)

        /// GizmoCode の内部値を取得
        let value (GizmoCode str) = str

        /// 安全でない作成（テスト用）
        let unsafeCreate str = GizmoCode str

    /// 商品コード（Widget または Gizmo）
    type ProductCode =
        | Widget of WidgetCode
        | Gizmo of GizmoCode

    module ProductCode =
        /// ProductCode の内部値を取得
        let value =
            function
            | Widget wc -> WidgetCode.value wc
            | Gizmo gc -> GizmoCode.value gc

    // ========================================
    // 数量
    // ========================================

    /// 単位数量（1-1000 の整数）
    type UnitQuantity = private UnitQuantity of int

    module UnitQuantity =
        /// UnitQuantity を作成する
        let create fieldName qty =
            if qty < 1 then
                Error $"{fieldName} must be at least 1"
            elif qty > 1000 then
                Error $"{fieldName} must not be more than 1000"
            else
                Ok(UnitQuantity qty)

        /// UnitQuantity の内部値を取得
        let value (UnitQuantity qty) = qty

        /// 安全でない作成（テスト用）
        let unsafeCreate qty = UnitQuantity qty

    /// 重量数量（0.05-100.00 の小数）
    type KilogramQuantity = private KilogramQuantity of decimal

    module KilogramQuantity =
        /// KilogramQuantity を作成する
        let create fieldName qty =
            if qty < 0.05m then
                Error $"{fieldName} must be at least 0.05"
            elif qty > 100.0m then
                Error $"{fieldName} must not be more than 100.00"
            else
                Ok(KilogramQuantity qty)

        /// KilogramQuantity の内部値を取得
        let value (KilogramQuantity qty) = qty

        /// 安全でない作成（テスト用）
        let unsafeCreate qty = KilogramQuantity qty

    /// 注文数量（単位 または 重量）
    type OrderQuantity =
        | Unit of UnitQuantity
        | Kilogram of KilogramQuantity

    // ========================================
    // 価格と請求金額
    // ========================================

    /// 価格（0.0-1000.00 の小数）
    type Price = private Price of decimal

    module Price =
        /// Price を作成する
        let create fieldName price =
            if price < 0.0m then
                Error $"{fieldName} must not be negative"
            elif price > 1000.0m then
                Error $"{fieldName} must not be more than 1000.00"
            else
                Ok(Price price)

        /// Price の内部値を取得
        let value (Price price) = price

        /// 安全でない作成（テスト用）
        let unsafeCreate price = Price price

        /// Price を乗算する
        let multiply qty (Price price) =
            Price(decimal qty * price)

    /// 請求金額（0.0-10000.00 の小数）
    type BillingAmount = private BillingAmount of decimal

    module BillingAmount =
        /// BillingAmount を作成する
        let create fieldName amount =
            if amount < 0.0m then
                Error $"{fieldName} must not be negative"
            elif amount > 10000.0m then
                Error $"{fieldName} must not be more than 10000.00"
            else
                Ok(BillingAmount amount)

        /// BillingAmount の内部値を取得
        let value (BillingAmount amount) = amount

        /// 安全でない作成（テスト用）
        let unsafeCreate amount = BillingAmount amount

        /// Price のリストから BillingAmount を作成
        let sumPrices prices =
            prices
            |> List.map Price.value
            |> List.sum
            |> unsafeCreate

    // ========================================
    // 識別子
    // ========================================

    /// 注文 ID（50 文字以内の非空文字列）
    type OrderId = private OrderId of string

    module OrderId =
        /// OrderId を作成する
        let create fieldName str =
            String50.create fieldName str
            |> Result.map (String50.value >> OrderId)

        /// OrderId の内部値を取得
        let value (OrderId str) = str

        /// 安全でない作成（テスト用）
        let unsafeCreate str = OrderId str

    /// 注文明細 ID（50 文字以内の非空文字列）
    type OrderLineId = private OrderLineId of string

    module OrderLineId =
        /// OrderLineId を作成する
        let create fieldName str =
            String50.create fieldName str
            |> Result.map (String50.value >> OrderLineId)

        /// OrderLineId の内部値を取得
        let value (OrderLineId str) = str

        /// 安全でない作成（テスト用）
        let unsafeCreate str = OrderLineId str
