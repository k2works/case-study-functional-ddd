namespace OrderTaking.WebApi

// ========================================
// API レスポンス型定義
// ========================================

/// 成功レスポンス - 注文受付成功
type PlaceOrderSuccessResponse = { events: obj list }

/// エラーレスポンス - 単純なエラーメッセージ
type ErrorResponse = { error: string }

/// エラーレスポンス - 詳細なバリデーションエラー
type ValidationErrorDetail =
    { field: string
      message: string
      errorCode: string }

/// エラーレスポンス - 構造化されたエラー
type StructuredErrorResponse =
    { errorType: string
      message: string
      details: ValidationErrorDetail list option }
