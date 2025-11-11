namespace OrderTaking.Domain

open OrderTaking.Domain.ConstrainedTypes
open OrderTaking.Domain.CompoundTypes
open OrderTaking.Domain.Entities

// ========================================
// ドメインサービス
//
// ビジネスロジックを実装するサービス
// ========================================

module DomainServices =

    // ========================================
    // ValidationError
    // ========================================

    /// バリデーションエラー
    type ValidationError = private ValidationError of FieldName: string * Message: string

    module ValidationError =
        /// ValidationError を作成する
        let create fieldName message = ValidationError(fieldName, message)

        /// ValidationError を文字列に変換する
        let toString (ValidationError(fieldName, message)) = $"{fieldName}: {message}"

    // ========================================
    // Validation Service
    // ========================================

    module Validation =

        /// 商品コード検証の依存性
        type CheckProductCodeExists = string -> Result<ProductCode, string>

        /// 住所検証の依存性
        type CheckAddressExists = UnvalidatedAddress -> Result<UnvalidatedAddress, string>

        /// 顧客情報を検証する
        let private validateCustomerInfo (unvalidatedCustomer: UnvalidatedCustomerInfo) =
            match
                CustomerInfo.create
                    unvalidatedCustomer.FirstName
                    unvalidatedCustomer.LastName
                    unvalidatedCustomer.EmailAddress
            with
            | Ok customerInfo -> Ok customerInfo
            | Error msg -> Error [ ValidationError.create "CustomerInfo" msg ]

        /// 住所を検証する
        let private validateAddress
            fieldName
            (checkAddressExists: CheckAddressExists)
            (unvalidatedAddress: UnvalidatedAddress)
            =
            // まず外部サービスで住所をチェック
            match checkAddressExists unvalidatedAddress with
            | Error msg -> Error [ ValidationError.create fieldName msg ]
            | Ok _ ->
                // 次に制約付き型で検証
                match
                    Address.create
                        unvalidatedAddress.AddressLine1
                        unvalidatedAddress.AddressLine2
                        unvalidatedAddress.City
                        unvalidatedAddress.ZipCode
                with
                | Ok address -> Ok address
                | Error msg -> Error [ ValidationError.create fieldName msg ]

        /// 商品コードを検証する
        let private validateProductCode (checkProductCodeExists: CheckProductCodeExists) (productCodeStr: string) =
            match checkProductCodeExists productCodeStr with
            | Ok productCode -> Ok productCode
            | Error msg -> Error [ ValidationError.create "ProductCode" msg ]

        /// 数量を検証する
        let private validateQuantity (productCode: ProductCode) (quantityDecimal: decimal) =
            match productCode with
            | ProductCode.Widget _ ->
                // Widget は単位数量
                let quantityInt = int quantityDecimal

                match UnitQuantity.create "Quantity" quantityInt with
                | Ok qty -> Ok(OrderQuantity.Unit qty)
                | Error msg -> Error [ ValidationError.create "Quantity" msg ]
            | ProductCode.Gizmo _ ->
                // Gizmo は重量数量
                match KilogramQuantity.create "Quantity" quantityDecimal with
                | Ok qty -> Ok(OrderQuantity.Kilogram qty)
                | Error msg -> Error [ ValidationError.create "Quantity" msg ]

        /// 注文明細を検証する
        let private validateOrderLine
            (checkProductCodeExists: CheckProductCodeExists)
            (unvalidatedLine: UnvalidatedOrderLine)
            =
            // OrderLineId の検証（GUID に変換）
            let orderLineIdResult =
                try
                    Ok(OrderLineId.create (System.Guid.Parse(unvalidatedLine.OrderLineId)))
                with _ ->
                    Error [ ValidationError.create "OrderLineId" "Invalid GUID format" ]

            // ProductCode の検証
            let productCodeResult =
                validateProductCode checkProductCodeExists unvalidatedLine.ProductCode

            // 両方の結果を組み合わせる
            match orderLineIdResult, productCodeResult with
            | Ok orderLineId, Ok productCode ->
                // Quantity の検証
                match validateQuantity productCode unvalidatedLine.Quantity with
                | Ok quantity -> Ok(ValidatedOrderLine.create orderLineId productCode quantity)
                | Error errors -> Error errors
            | Error errors1, Error errors2 -> Error(errors1 @ errors2)
            | Error errors, Ok _ -> Error errors
            | Ok _, Error errors -> Error errors

        /// 注文を検証する
        let validateOrder
            (checkProductCodeExists: CheckProductCodeExists)
            (checkAddressExists: CheckAddressExists)
            (unvalidatedOrder: UnvalidatedOrder)
            : Result<ValidatedOrder, ValidationError list> =

            // OrderId の検証（GUID に変換）
            let orderIdResult =
                try
                    Ok(OrderId.create (System.Guid.Parse(unvalidatedOrder.OrderId)))
                with _ ->
                    Error [ ValidationError.create "OrderId" "Invalid GUID format" ]

            // CustomerInfo の検証
            let customerInfoResult =
                validateCustomerInfo unvalidatedOrder.CustomerInfo

            // ShippingAddress の検証
            let shippingAddressResult =
                validateAddress "ShippingAddress" checkAddressExists unvalidatedOrder.ShippingAddress

            // BillingAddress の検証
            let billingAddressResult =
                validateAddress "BillingAddress" checkAddressExists unvalidatedOrder.BillingAddress

            // Lines の検証
            let linesResult =
                let validateLine =
                    validateOrderLine checkProductCodeExists

                unvalidatedOrder.Lines
                |> List.map validateLine
                |> List.fold
                    (fun acc result ->
                        match acc, result with
                        | Ok lines, Ok line -> Ok(lines @ [ line ])
                        | Error errors, Ok _ -> Error errors
                        | Ok _, Error errors -> Error errors
                        | Error errors1, Error errors2 -> Error(errors1 @ errors2))
                    (Ok [])

            // すべての結果を組み合わせる
            match orderIdResult, customerInfoResult, shippingAddressResult, billingAddressResult, linesResult with
            | Ok orderId, Ok customerInfo, Ok shippingAddress, Ok billingAddress, Ok lines ->
                Ok(ValidatedOrder.create orderId customerInfo shippingAddress billingAddress lines)
            | _ ->
                // すべてのエラーを集約
                let allErrors =
                    [ match orderIdResult with
                      | Error errors -> yield! errors
                      | Ok _ -> ()
                      match customerInfoResult with
                      | Error errors -> yield! errors
                      | Ok _ -> ()
                      match shippingAddressResult with
                      | Error errors -> yield! errors
                      | Ok _ -> ()
                      match billingAddressResult with
                      | Error errors -> yield! errors
                      | Ok _ -> ()
                      match linesResult with
                      | Error errors -> yield! errors
                      | Ok _ -> () ]

                Error allErrors

    // ========================================
    // ProductCodeService (Stub)
    // ========================================

    module ProductCodeService =

        /// 有効な商品コードのリスト（テスト用スタブ）
        let private validProductCodes =
            [ "W1234"
              "W5678"
              "W9012" // Widget codes
              "G5678"
              "G1234"
              "G9012" ] // Gizmo codes

        /// 商品コードが存在するかチェックする（スタブ実装）
        let checkProductCodeExists (code: string) : Result<ProductCode, string> =
            if validProductCodes |> List.contains code then
                // コードの最初の文字で Widget か Gizmo か判定
                if code.StartsWith("W") then
                    Ok(ProductCode.Widget(WidgetCode.unsafeCreate code))
                elif code.StartsWith("G") then
                    Ok(ProductCode.Gizmo(GizmoCode.unsafeCreate code))
                else
                    Error $"Invalid product code format: {code}"
            else
                Error $"Product code not found: {code}"

    // ========================================
    // PriceService (Stub)
    // ========================================

    module PriceService =

        /// 商品の単価を取得する（スタブ実装）
        let getProductPrice (productCode: ProductCode) : Result<Price, string> =
            // スタブとして固定価格を返す
            match productCode with
            | ProductCode.Widget _ -> Ok(Price.unsafeCreate 25.50m)
            | ProductCode.Gizmo _ -> Ok(Price.unsafeCreate 100.00m)

    // ========================================
    // Pricing Service
    // ========================================

    module Pricing =

        /// 単価取得の依存性
        type GetProductPrice = ProductCode -> Result<Price, string>

        /// 数量から int または decimal を取得する
        let private getQuantityValue (quantity: OrderQuantity) : decimal =
            match quantity with
            | OrderQuantity.Unit uq -> decimal (UnitQuantity.value uq)
            | OrderQuantity.Kilogram kq -> KilogramQuantity.value kq

        /// 注文明細の価格を計算する
        let private priceOrderLine (getProductPrice: GetProductPrice) (line: ValidatedOrderLine) =
            match getProductPrice line.ProductCode with
            | Error msg -> Error msg
            | Ok price ->
                let quantity =
                    getQuantityValue line.Quantity

                let linePrice =
                    Price.multiply quantity price

                Ok(PricedOrderLine.create line.OrderLineId line.ProductCode line.Quantity price linePrice)

        /// 注文全体の価格を計算する
        let priceOrder
            (getProductPrice: GetProductPrice)
            (validatedOrder: ValidatedOrder)
            : Result<PricedOrder, string> =
            // すべての明細を価格計算
            let pricedLinesResult =
                validatedOrder.Lines
                |> List.map (priceOrderLine getProductPrice)
                |> List.fold
                    (fun acc result ->
                        match acc, result with
                        | Ok lines, Ok line -> Ok(lines @ [ line ])
                        | Error msg, _ -> Error msg
                        | _, Error msg -> Error msg)
                    (Ok [])

            match pricedLinesResult with
            | Error msg -> Error msg
            | Ok pricedLines ->
                // 合計金額を計算
                let totalAmount =
                    pricedLines
                    |> List.map (fun line -> Price.value line.LinePrice)
                    |> List.sum

                // BillingAmount を作成
                match BillingAmount.create "AmountToBill" totalAmount with
                | Error msg -> Error msg
                | Ok amountToBill ->
                    Ok(
                        PricedOrder.create
                            validatedOrder.OrderId
                            validatedOrder.CustomerInfo
                            validatedOrder.ShippingAddress
                            validatedOrder.BillingAddress
                            pricedLines
                            amountToBill
                    )

    // ========================================
    // Events
    // ========================================

    /// 注文確認情報
    type OrderAcknowledgment =
        { OrderId: OrderId
          EmailAddress: EmailAddress }

    /// 注文配置イベント
    type PlaceOrderEvent =
        | OrderPlaced of PricedOrder
        | BillableOrderPlaced of OrderId * BillingAmount
        | AcknowledgmentSent of OrderAcknowledgment

    // ========================================
    // Acknowledgment Service
    // ========================================

    module Acknowledgment =

        /// メール送信の依存性
        type SendOrderAcknowledgment = OrderAcknowledgment -> Result<unit, string>

        /// 注文確認を行い、イベントを生成する
        let acknowledgeOrder
            (sendAcknowledgment: SendOrderAcknowledgment)
            (pricedOrder: PricedOrder)
            : Result<PlaceOrderEvent list, string> =

            // 1. OrderPlaced イベントを生成
            let orderPlacedEvent =
                OrderPlaced pricedOrder

            // 2. BillableOrderPlaced イベントを生成（AmountToBill > 0 の場合）
            let billableEvents =
                if BillingAmount.value pricedOrder.AmountToBill > 0.0m then
                    [ BillableOrderPlaced(pricedOrder.OrderId, pricedOrder.AmountToBill) ]
                else
                    []

            // 3. メール送信を試行
            let (_, email) =
                CustomerInfo.value pricedOrder.CustomerInfo

            let acknowledgment =
                { OrderId = pricedOrder.OrderId
                  EmailAddress = email }

            match sendAcknowledgment acknowledgment with
            | Error msg -> Error msg
            | Ok() ->
                // 4. AcknowledgmentSent イベントを生成
                let acknowledgmentEvent =
                    AcknowledgmentSent acknowledgment

                // すべてのイベントを結合
                Ok(
                    [ orderPlacedEvent ]
                    @ billableEvents
                    @ [ acknowledgmentEvent ]
                )

    // ========================================
    // SendOrderAcknowledgment Service (Stub)
    // ========================================

    module SendOrderAcknowledgmentService =

        /// メール送信サービス（スタブ実装）
        let sendOrderAcknowledgment (acknowledgment: OrderAcknowledgment) : Result<unit, string> =
            // スタブとして常に成功を返す
            // 実際の実装では SMTP や外部 API を使用してメールを送信
            Ok()
