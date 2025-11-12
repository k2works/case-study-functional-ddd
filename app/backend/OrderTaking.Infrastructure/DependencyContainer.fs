namespace OrderTaking.Infrastructure

open OrderTaking.Domain.DomainServices

// ========================================
// Dependency Container
//
// アプリケーション全体で使用する依存性を提供
// ========================================

module DependencyContainer =

    /// PlaceOrder ワークフローに必要な全ての依存性
    type PlaceOrderDependencies =
        { CheckProductCodeExists: PlaceOrderWorkflow.CheckProductCodeExists
          CheckAddressExists: PlaceOrderWorkflow.CheckAddressExists
          GetProductPrice: PlaceOrderWorkflow.GetProductPrice
          SendOrderAcknowledgment: PlaceOrderWorkflow.SendOrderAcknowledgment }

    /// デフォルトの依存性（ダミーアダプター使用）
    let createDefaultDependencies () : PlaceOrderDependencies =
        { CheckProductCodeExists = Adapters.ProductCodeAdapter.checkProductCodeExists
          CheckAddressExists = Adapters.AddressAdapter.checkAddressExists
          GetProductPrice = Adapters.PriceAdapter.getProductPrice
          SendOrderAcknowledgment = Adapters.AcknowledgmentAdapter.sendOrderAcknowledgment }
