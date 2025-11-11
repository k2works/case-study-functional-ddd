namespace OrderTaking.Domain

// ========================================
// エンティティ
//
// 注文のライフサイクルを表現するエンティティ
// ========================================

module Entities =

    // ========================================
    // UnvalidatedCustomerInfo
    // ========================================

    /// 未検証の顧客情報（生データ）
    type UnvalidatedCustomerInfo =
        { FirstName: string
          LastName: string
          EmailAddress: string }

    module UnvalidatedCustomerInfo =
        /// UnvalidatedCustomerInfo を作成する
        let create firstName lastName emailAddress =
            { FirstName = firstName
              LastName = lastName
              EmailAddress = emailAddress }

    // ========================================
    // UnvalidatedAddress
    // ========================================

    /// 未検証の住所（生データ）
    type UnvalidatedAddress =
        { AddressLine1: string
          AddressLine2: string option
          City: string
          ZipCode: string }

    module UnvalidatedAddress =
        /// UnvalidatedAddress を作成する
        let create addressLine1 addressLine2 city zipCode =
            { AddressLine1 = addressLine1
              AddressLine2 = addressLine2
              City = city
              ZipCode = zipCode }

    // ========================================
    // UnvalidatedOrderLine
    // ========================================

    /// 未検証の注文明細（生データ）
    type UnvalidatedOrderLine =
        { OrderLineId: string
          ProductCode: string
          Quantity: decimal }

    module UnvalidatedOrderLine =
        /// UnvalidatedOrderLine を作成する
        let create orderLineId productCode quantity =
            { OrderLineId = orderLineId
              ProductCode = productCode
              Quantity = quantity }

    // ========================================
    // UnvalidatedOrder
    // ========================================

    /// 未検証の注文（生データ）
    type UnvalidatedOrder =
        { OrderId: string
          CustomerInfo: UnvalidatedCustomerInfo
          ShippingAddress: UnvalidatedAddress
          BillingAddress: UnvalidatedAddress
          Lines: UnvalidatedOrderLine list }

    module UnvalidatedOrder =
        /// UnvalidatedOrder を作成する
        let create orderId customerInfo shippingAddress billingAddress lines =
            { OrderId = orderId
              CustomerInfo = customerInfo
              ShippingAddress = shippingAddress
              BillingAddress = billingAddress
              Lines = lines }
