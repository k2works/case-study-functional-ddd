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

    // ========================================
    // Address
    // ========================================

    /// 住所（住所行1、住所行2（オプション）、都市、郵便番号）
    type Address =
        private | Address of AddressLine1: String50 * AddressLine2: String50 option * City: String50 * ZipCode: ZipCode

    module Address =
        /// Address を作成する
        let create addressLine1 addressLine2 city zipCode =
            match String50.create "AddressLine1" addressLine1 with
            | Error e -> Error e
            | Ok line1 ->
                // AddressLine2 は option なので、Some の場合のみバリデーション
                let line2Result =
                    match addressLine2 with
                    | None -> Ok None
                    | Some line2Str ->
                        match String50.create "AddressLine2" line2Str with
                        | Error e -> Error e
                        | Ok line2 -> Ok(Some line2)

                match line2Result with
                | Error e -> Error e
                | Ok line2Opt ->
                    match String50.create "City" city with
                    | Error e -> Error e
                    | Ok cityValue ->
                        match ZipCode.create "ZipCode" zipCode with
                        | Error e -> Error e
                        | Ok zipCodeValue -> Ok(Address(line1, line2Opt, cityValue, zipCodeValue))

        /// Address の値を取得する
        let value (Address(line1, line2, city, zipCode)) = (line1, line2, city, zipCode)
