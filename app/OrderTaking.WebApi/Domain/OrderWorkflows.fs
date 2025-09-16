namespace OrderTaking.Domain

open OrderTaking.Common

// 型エイリアス
type AsyncResult<'a, 'b> = Async<Result<'a, 'b>>

// 外部依存の抽象化
type CheckProductCodeExists = ProductCode -> bool
type GetProductPrice = ProductCode -> decimal option
type CheckAddressExists = UnvalidatedAddress -> AsyncResult<Address, ValidationError>
type SendOrderAcknowledgment = PricedOrder -> AsyncResult<AcknowledgmentSent, string>

// ワークフローの関数型定義
type ValidateOrder = CheckProductCodeExists -> CheckAddressExists -> UnvalidatedOrder -> AsyncResult<ValidatedOrder, ValidationError>
type PriceOrder = GetProductPrice -> ValidatedOrder -> Result<PricedOrder, PricingError>
type AcknowledgeOrder = SendOrderAcknowledgment -> PricedOrder -> AsyncResult<AcknowledgmentSent option, string>
type CreateEvents = PricedOrder -> AcknowledgmentSent option -> OrderEvent list

// メインワークフロー
type PlaceOrderWorkflow = UnvalidatedOrder -> AsyncResult<OrderEvent list, PlaceOrderError>

// 便利なヘルパーモジュール
module AsyncResult =
    let ofResult result =
        async { return result }

    let ofError error =
        async { return Error error }

    let map f asyncResult =
        async {
            let! result = asyncResult
            return Result.map f result
        }

    let mapError f asyncResult =
        async {
            let! result = asyncResult
            return Result.mapError f result
        }

    let bind f asyncResult =
        async {
            let! result = asyncResult
            match result with
            | Ok value -> return! f value
            | Error error -> return Error error
        }

    let sequence asyncResults =
        let rec loop acc remaining =
            async {
                match remaining with
                | [] -> return Ok (List.rev acc)
                | head :: tail ->
                    let! headResult = head
                    match headResult with
                    | Ok value ->
                        return! loop (value :: acc) tail
                    | Error error ->
                        return Error error
            }
        loop [] asyncResults

    let catch handler asyncResult =
        async {
            try
                return! asyncResult
            with
            | ex -> return! handler ex
        }

type AsyncResultBuilder() =
    member _.Return(value) = AsyncResult.ofResult (Ok value)
    member _.ReturnFrom(asyncResult) = asyncResult
    member _.Bind(asyncResult, f) = AsyncResult.bind f asyncResult
    member _.Zero() = AsyncResult.ofResult (Ok ())
    member _.Combine(m, f) = AsyncResult.bind (fun _ -> f()) m
    member _.Delay(f) = f
    member _.Run(f) = f()
    member _.TryFinally(m, finalizer) = try m() finally finalizer()
    member _.TryWith(m, handler) = try m() with | e -> handler e

module AsyncResultBuilder =
    let asyncResult = AsyncResultBuilder()

module Result =
    let sequence results =
        let rec loop acc remaining =
            match remaining with
            | [] -> Ok (List.rev acc)
            | (Ok value) :: tail -> loop (value :: acc) tail
            | (Error error) :: _ -> Error error
        loop [] results

    let toOption = function
        | Ok value -> Some value
        | Error _ -> None

module OrderWorkflows =

    // 商品コードパース
    let parseProductCode (codeStr: string) =
        if codeStr.StartsWith("W") then
            WidgetCode.create codeStr
            |> Result.map Widget
            |> Result.mapError (fun msg -> FieldInvalidFormat msg)
            |> AsyncResult.ofResult
        elif codeStr.StartsWith("G") then
            GizmoCode.create codeStr
            |> Result.map Gizmo
            |> Result.mapError (fun msg -> FieldInvalidFormat msg)
            |> AsyncResult.ofResult
        else
            AsyncResult.ofError (FieldInvalidFormat "無効な商品コード形式")

    // 数量パース
    let parseQuantity productCode qty =
        match productCode with
        | Widget _ ->
            UnitQuantity.create (int qty)
            |> Result.map Unit
            |> Result.mapError (fun msg -> FieldOutOfRange("Unit", qty, qty))
            |> AsyncResult.ofResult
        | Gizmo _ ->
            KilogramQuantity.create qty
            |> Result.map Kilogram
            |> Result.mapError (fun msg -> FieldOutOfRange("Kilogram", qty, qty))
            |> AsyncResult.ofResult

    // 注文明細検証
    let validateOrderLine checkProductExists (line: UnvalidatedOrderLine) =
        AsyncResultBuilder.asyncResult {
            // 商品コードの検証とパース
            let! productCode = parseProductCode line.ProductCode

            // 商品の存在確認
            let productExists = checkProductExists productCode
            if not productExists then
                return! AsyncResult.ofError (FieldIsMissing "ProductCode not found")
            else
                ()

            // 数量の検証
            let! quantity = parseQuantity productCode line.Quantity

            return {
                OrderLineId = line.OrderLineId
                ProductCode = productCode
                Quantity = quantity
            }
        }

    // 注文検証の実装
    let validateOrder: ValidateOrder =
        fun checkProductExists checkAddressExists unvalidatedOrder ->
            AsyncResultBuilder.asyncResult {
                // 顧客情報の検証
                let! customerName =
                    String50.create (unvalidatedOrder.CustomerInfo.FirstName + " " + unvalidatedOrder.CustomerInfo.LastName)
                    |> Result.mapError (fun msg -> FieldInvalidFormat msg)
                    |> AsyncResult.ofResult

                let! customerEmail =
                    EmailAddress.create unvalidatedOrder.CustomerInfo.EmailAddress
                    |> Result.mapError (fun msg -> FieldInvalidFormat msg)
                    |> AsyncResult.ofResult

                // 住所の検証
                let! shippingAddress = checkAddressExists unvalidatedOrder.ShippingAddress
                let! billingAddress = checkAddressExists unvalidatedOrder.BillingAddress

                // 注文明細の検証
                let! validatedLines =
                    unvalidatedOrder.Lines
                    |> List.map (validateOrderLine checkProductExists)
                    |> AsyncResult.sequence

                let! orderId =
                    OrderId.create unvalidatedOrder.OrderId
                    |> Result.mapError (fun msg -> FieldInvalidFormat msg)
                    |> AsyncResult.ofResult

                return {
                    OrderId = orderId
                    CustomerInfo = {
                        Name = customerName
                        Email = customerEmail
                    }
                    ShippingAddress = shippingAddress
                    BillingAddress = billingAddress
                    Lines = validatedLines
                }
            }

    // 価格計算の実装
    let priceOrder: PriceOrder =
        fun getProductPrice validatedOrder ->
            let pricedLines =
                validatedOrder.Lines
                |> List.map (fun line ->
                    match getProductPrice line.ProductCode with
                    | Some price ->
                        let qty =
                            match line.Quantity with
                            | Unit unitQty -> decimal (UnitQuantity.value unitQty)
                            | Kilogram kgQty -> KilogramQuantity.value kgQty
                        Ok {
                            OrderLineId = line.OrderLineId
                            ProductCode = line.ProductCode
                            Quantity = line.Quantity
                            LinePrice = price * qty
                        }
                    | None ->
                        Error (ProductNotFound line.ProductCode)
                )
                |> Result.sequence

            pricedLines
            |> Result.map (fun lines ->
                let totalAmount = lines |> List.sumBy (fun line -> line.LinePrice)
                {
                    OrderId = validatedOrder.OrderId
                    CustomerInfo = validatedOrder.CustomerInfo
                    ShippingAddress = validatedOrder.ShippingAddress
                    BillingAddress = validatedOrder.BillingAddress
                    Lines = lines
                    AmountToBill = totalAmount
                }
            )

    // 確認送信の実装
    let acknowledgeOrder: AcknowledgeOrder =
        fun sendAcknowledgment pricedOrder ->
            AsyncResultBuilder.asyncResult {
                let! acknowledgment = sendAcknowledgment pricedOrder
                return Some acknowledgment
            }
            |> AsyncResult.catch (fun _ -> AsyncResult.ofResult (Ok None))

    // イベント生成の実装
    let createEvents: CreateEvents =
        fun pricedOrder acknowledgmentOpt ->
            [
                Some (OrderPlaced pricedOrder)

                if pricedOrder.AmountToBill > 0m then
                    Some (BillableOrderPlaced {
                        OrderId = pricedOrder.OrderId
                        BillingAddress = pricedOrder.BillingAddress
                        AmountToBill = pricedOrder.AmountToBill
                    })
                else
                    None

                match acknowledgmentOpt with
                | Some ack -> Some (AcknowledgmentSent ack)
                | None -> None
            ]
            |> List.choose id

    // メインワークフロー実装
    let placeOrder
        (checkProductExists: CheckProductCodeExists)
        (getProductPrice: GetProductPrice)
        (checkAddressExists: CheckAddressExists)
        (sendAcknowledgment: SendOrderAcknowledgment)
        : PlaceOrderWorkflow =
        fun unvalidatedOrder ->
            AsyncResultBuilder.asyncResult {
                // 検証
                let! validatedOrder =
                    validateOrder checkProductExists checkAddressExists unvalidatedOrder
                    |> AsyncResult.mapError Validation

                // 価格計算
                let! pricedOrder =
                    priceOrder getProductPrice validatedOrder
                    |> Result.mapError Pricing
                    |> AsyncResult.ofResult

                // 確認送信
                let! acknowledgment =
                    acknowledgeOrder sendAcknowledgment pricedOrder
                    |> AsyncResult.mapError RemoteService

                // イベント生成
                let events = createEvents pricedOrder acknowledgment

                return events
            }