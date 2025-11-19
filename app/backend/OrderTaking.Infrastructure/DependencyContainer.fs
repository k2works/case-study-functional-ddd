namespace OrderTaking.Infrastructure

open OrderTaking.Domain.DomainServices
open OrderTaking.Domain.Entities
open OrderTaking.Domain.CompoundTypes

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
          SaveOrder: PlaceOrderWorkflow.SaveOrder
          SendOrderAcknowledgment: PlaceOrderWorkflow.SendOrderAcknowledgment }

    /// デフォルトの依存性（ダミーアダプター使用）
    let createDefaultDependencies () : PlaceOrderDependencies =
        { CheckProductCodeExists = Adapters.ProductCodeAdapter.checkProductCodeExists
          CheckAddressExists = Adapters.AddressAdapter.checkAddressExists
          GetProductPrice = Adapters.PriceAdapter.getProductPrice
          SaveOrder = Adapters.OrderRepositoryAdapter.saveOrder
          SendOrderAcknowledgment = Adapters.AcknowledgmentAdapter.sendOrderAcknowledgment }

    /// OrderRepository を使用する依存性
    let createDependenciesWithRepository (repository: IOrderRepository) : PlaceOrderDependencies =
        let saveOrder (pricedOrder: PricedOrder) : Async<Result<OrderId, string>> =
            async {
                try
                    let! savedOrderId = repository.SaveAsync pricedOrder
                    return Ok savedOrderId
                with ex ->
                    return Error $"Database error: {ex.Message}"
            }

        { CheckProductCodeExists = Adapters.ProductCodeAdapter.checkProductCodeExists
          CheckAddressExists = Adapters.AddressAdapter.checkAddressExists
          GetProductPrice = Adapters.PriceAdapter.getProductPrice
          SaveOrder = saveOrder
          SendOrderAcknowledgment = Adapters.AcknowledgmentAdapter.sendOrderAcknowledgment }
