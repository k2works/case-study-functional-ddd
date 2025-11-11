namespace OrderTaking.Domain

open OrderTaking.Domain.ConstrainedTypes

// ========================================
// 複合値オブジェクト
//
// ビジネスルールを保持する複合型
// ========================================

module CompoundTypes =

    // ========================================
    // PersonalName
    // ========================================

    /// 個人名（名と姓）
    type PersonalName = private PersonalName of FirstName: String50 * LastName: String50

    module PersonalName =
        /// PersonalName を作成する
        let create firstName lastName =
            match String50.create "FirstName" firstName with
            | Error e -> Error e
            | Ok fn ->
                match String50.create "LastName" lastName with
                | Error e -> Error e
                | Ok ln -> Ok(PersonalName(fn, ln))

        /// PersonalName の値を取得する
        let value (PersonalName(firstName, lastName)) = (firstName, lastName)

    // ========================================
    // CustomerInfo
    // ========================================

    /// 顧客情報（名前とメールアドレス）
    type CustomerInfo = private CustomerInfo of Name: PersonalName * EmailAddress: EmailAddress

    module CustomerInfo =
        /// CustomerInfo を作成する
        let create firstName lastName email =
            match PersonalName.create firstName lastName with
            | Error e -> Error e
            | Ok name ->
                match EmailAddress.create "EmailAddress" email with
                | Error e -> Error e
                | Ok emailAddr -> Ok(CustomerInfo(name, emailAddr))

        /// CustomerInfo の値を取得する
        let value (CustomerInfo(name, email)) = (name, email)
