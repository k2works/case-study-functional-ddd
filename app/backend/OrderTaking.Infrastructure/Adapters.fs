namespace OrderTaking.Infrastructure

open OrderTaking.Domain.ConstrainedTypes
open OrderTaking.Domain.CompoundTypes
open OrderTaking.Domain.Entities
open OrderTaking.Domain.DomainServices

// ========================================
// Infrastructure Adapters
//
// ドメインサービスの依存性を実装するアダプター
// ========================================

module Adapters =

    // ========================================
    // ProductCodeAdapter
    // ========================================

    module ProductCodeAdapter =

        /// 有効な商品コードのリスト（ダミーデータ）
        let private validProductCodes =
            [ "W1234"
              "W5678"
              "W9012" // Widget codes
              "G5678"
              "G1234"
              "G123"
              "G9012" ] // Gizmo codes

        /// 商品コードが存在するかチェックする（ダミー実装）
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
    // AddressAdapter
    // ========================================

    module AddressAdapter =

        /// 住所が存在するかチェックする（ダミー実装）
        let checkAddressExists (address: UnvalidatedAddress) : Result<UnvalidatedAddress, string> =
            // ダミー実装として全ての住所を受け入れる
            // 実際の実装では外部の住所検証 API を呼び出す
            Ok address

    // ========================================
    // PriceAdapter
    // ========================================

    module PriceAdapter =

        /// 商品の単価を取得する（ダミー実装）
        let getProductPrice (productCode: ProductCode) : Result<Price, string> =
            // ダミー実装として固定価格を返す
            // 実際の実装では価格マスタから価格を取得
            match productCode with
            | ProductCode.Widget _ -> Ok(Price.unsafeCreate 25.50m)
            | ProductCode.Gizmo _ -> Ok(Price.unsafeCreate 100.00m)

    // ========================================
    // AcknowledgmentAdapter
    // ========================================

    module AcknowledgmentAdapter =

        /// メール送信サービス（ダミー実装）
        let sendOrderAcknowledgment (acknowledgment: OrderAcknowledgment) : Result<unit, string> =
            // ダミー実装として常に成功を返す
            // 実際の実装では SMTP や外部 API を使用してメールを送信
            Ok()

    // ========================================
    // OrderRepositoryAdapter
    // ========================================

    module OrderRepositoryAdapter =

        /// 注文を保存する（ダミー実装）
        let saveOrder (pricedOrder: PricedOrder) : Async<Result<OrderId, string>> =
            async {
                // ダミー実装として常に成功を返す
                // 実際の実装では OrderRepository.SaveAsync を呼び出す
                return Ok(pricedOrder.OrderId)
            }
