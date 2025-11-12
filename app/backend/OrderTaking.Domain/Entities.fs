namespace OrderTaking.Domain

open OrderTaking.Domain.ConstrainedTypes
open OrderTaking.Domain.CompoundTypes

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

    // ========================================
    // ValidatedOrderLine
    // ========================================

    /// 検証済みの注文明細
    type ValidatedOrderLine =
        { OrderLineId: OrderLineId
          ProductCode: ProductCode
          Quantity: OrderQuantity }

    module ValidatedOrderLine =
        /// ValidatedOrderLine を作成する
        let create orderLineId productCode quantity =
            { OrderLineId = orderLineId
              ProductCode = productCode
              Quantity = quantity }

    // ========================================
    // ValidatedOrder
    // ========================================

    /// 検証済みの注文
    type ValidatedOrder =
        { OrderId: OrderId
          CustomerInfo: CustomerInfo
          ShippingAddress: Address
          BillingAddress: Address
          Lines: ValidatedOrderLine list }

    module ValidatedOrder =
        /// ValidatedOrder を作成する
        let create orderId customerInfo shippingAddress billingAddress lines =
            { OrderId = orderId
              CustomerInfo = customerInfo
              ShippingAddress = shippingAddress
              BillingAddress = billingAddress
              Lines = lines }

    // ========================================
    // PricedOrderLine
    // ========================================

    /// 価格計算済みの注文明細
    type PricedOrderLine =
        { OrderLineId: OrderLineId
          ProductCode: ProductCode
          Quantity: OrderQuantity
          Price: Price
          LinePrice: Price }

    module PricedOrderLine =
        /// PricedOrderLine を作成する
        let create orderLineId productCode quantity price linePrice =
            { OrderLineId = orderLineId
              ProductCode = productCode
              Quantity = quantity
              Price = price
              LinePrice = linePrice }

    // ========================================
    // PricedOrder
    // ========================================

    /// 価格計算済みの注文
    type PricedOrder =
        { OrderId: OrderId
          CustomerInfo: CustomerInfo
          ShippingAddress: Address
          BillingAddress: Address
          Lines: PricedOrderLine list
          AmountToBill: BillingAmount }

    module PricedOrder =
        /// PricedOrder を作成する
        let create orderId customerInfo shippingAddress billingAddress lines amountToBill =
            { OrderId = orderId
              CustomerInfo = customerInfo
              ShippingAddress = shippingAddress
              BillingAddress = billingAddress
              Lines = lines
              AmountToBill = amountToBill }
