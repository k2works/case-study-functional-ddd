namespace OrderTaking.Domain

open OrderTaking.Common

type 顧客情報 = {
    名前: 文字列50
    メール: メールアドレス
}

type 住所 = {
    住所行1: 文字列50
    住所行2: 文字列50 option
    都市: 文字列50
    郵便番号: 文字列50
}

type 注文明細 = {
    注文明細ID: string
    商品コード: 商品コード
    数量: 注文数量
}

type 未検証注文 = {
    注文ID: string
    顧客情報: 未検証顧客情報
    配送先住所: 未検証住所
    請求先住所: 未検証住所
    明細: 未検証注文明細 list
}

and 未検証顧客情報 = {
    名: string
    姓: string
    メールアドレス: string
}

and 未検証住所 = {
    住所行1: string
    住所行2: string
    都市: string
    郵便番号: string
}

and 未検証注文明細 = {
    注文明細ID: string
    商品コード: string
    数量: decimal
}

type 検証済注文 = {
    注文ID: 注文ID
    顧客情報: 顧客情報
    配送先住所: 住所
    請求先住所: 住所
    明細: 検証済注文明細 list
}

and 検証済注文明細 = {
    注文明細ID: string
    商品コード: 商品コード
    数量: 注文数量
}

type 価格計算済注文 = {
    注文ID: 注文ID
    顧客情報: 顧客情報
    配送先住所: 住所
    請求先住所: 住所
    明細: 価格計算済注文明細 list
    請求金額: decimal
}

and 価格計算済注文明細 = {
    注文明細ID: string
    商品コード: 商品コード
    数量: 注文数量
    明細価格: decimal
}

type 注文イベント =
    | 注文受付 of 価格計算済注文
    | 請求対象注文受付 of 請求対象注文受付
    | 確認送信完了 of 確認送信完了

and 請求対象注文受付 = {
    注文ID: 注文ID
    請求先住所: 住所
    請求金額: decimal
}

and 確認送信完了 = {
    注文ID: 注文ID
    メールアドレス: メールアドレス
}

type 検証エラー =
    | フィールド欠如 of string
    | フィールド範囲外 of string * decimal * decimal
    | フィールド形式不正 of string

type 価格計算エラー =
    | 商品が見つからない of 商品コード

type 注文受付エラー =
    | 検証エラー of 検証エラー
    | 価格計算エラー of 価格計算エラー
    | 外部サービスエラー of string