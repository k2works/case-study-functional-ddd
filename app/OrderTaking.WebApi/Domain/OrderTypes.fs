namespace OrderTaking.Domain

open OrderTaking.Common

type CustomerInfo = {
    Name: String50
    Email: EmailAddress
}

type Address = {
    AddressLine1: String50
    AddressLine2: String50 option
    City: String50
    ZipCode: String50
}

type OrderLine = {
    OrderLineId: string
    ProductCode: ProductCode
    Quantity: OrderQuantity
}

type UnvalidatedOrder = {
    OrderId: string
    CustomerInfo: UnvalidatedCustomerInfo
    ShippingAddress: UnvalidatedAddress
    BillingAddress: UnvalidatedAddress
    Lines: UnvalidatedOrderLine list
}

and UnvalidatedCustomerInfo = {
    FirstName: string
    LastName: string
    EmailAddress: string
}

and UnvalidatedAddress = {
    AddressLine1: string
    AddressLine2: string
    City: string
    ZipCode: string
}

and UnvalidatedOrderLine = {
    OrderLineId: string
    ProductCode: string
    Quantity: decimal
}

type ValidatedOrder = {
    OrderId: OrderId
    CustomerInfo: CustomerInfo
    ShippingAddress: Address
    BillingAddress: Address
    Lines: ValidatedOrderLine list
}

and ValidatedOrderLine = {
    OrderLineId: string
    ProductCode: ProductCode
    Quantity: OrderQuantity
}

type PricedOrder = {
    OrderId: OrderId
    CustomerInfo: CustomerInfo
    ShippingAddress: Address
    BillingAddress: Address
    Lines: PricedOrderLine list
    AmountToBill: decimal
}

and PricedOrderLine = {
    OrderLineId: string
    ProductCode: ProductCode
    Quantity: OrderQuantity
    LinePrice: decimal
}

type OrderEvent =
    | OrderPlaced of PricedOrder
    | BillableOrderPlaced of BillableOrderPlaced
    | AcknowledgmentSent of AcknowledgmentSent

and BillableOrderPlaced = {
    OrderId: OrderId
    BillingAddress: Address
    AmountToBill: decimal
}

and AcknowledgmentSent = {
    OrderId: OrderId
    EmailAddress: EmailAddress
}

type ValidationError =
    | FieldIsMissing of string
    | FieldOutOfRange of string * decimal * decimal
    | FieldInvalidFormat of string

type PricingError =
    | ProductNotFound of ProductCode

type PlaceOrderError =
    | Validation of ValidationError
    | Pricing of PricingError
    | RemoteService of string