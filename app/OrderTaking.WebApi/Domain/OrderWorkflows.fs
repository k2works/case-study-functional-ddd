namespace OrderTaking.Domain

open OrderTaking.Common

// 型エイリアス
type 非同期結果<'a, 'b> = Async<Result<'a, 'b>>

// 外部依存の抽象化
type 商品コード存在確認 = 商品コード -> bool
type 商品価格取得 = 商品コード -> decimal option
type 住所存在確認 = 未検証住所 -> 非同期結果<住所, 検証エラー>
type 注文確認送信 = 価格計算済注文 -> 非同期結果<確認送信完了, string>

// ワークフローの関数型定義
type 注文検証 = 商品コード存在確認 -> 住所存在確認 -> 未検証注文 -> 非同期結果<検証済注文, 検証エラー>
type 価格計算 = 商品価格取得 -> 検証済注文 -> Result<価格計算済注文, 価格計算エラー>
type 注文確認 = 注文確認送信 -> 価格計算済注文 -> 非同期結果<確認送信完了 option, string>
type イベント作成 = 価格計算済注文 -> 確認送信完了 option -> 注文イベント list

// メインワークフロー
type 注文受付ワークフロー = 未検証注文 -> 非同期結果<注文イベント list, 注文受付エラー>

// 便利なヘルパーモジュール
module 非同期結果 =
    let 結果から 結果値 =
        async { return 結果値 }

    let エラーから エラー値 =
        async { return Error エラー値 }

    let マップ 関数 非同期結果 =
        async {
            let! 結果 = 非同期結果
            return Result.map 関数 結果
        }

    let エラーマップ 関数 非同期結果 =
        async {
            let! 結果 = 非同期結果
            return Result.mapError 関数 結果
        }

    let バインド 関数 非同期結果 =
        async {
            let! 結果 = 非同期結果
            match 結果 with
            | Ok 値 -> return! 関数 値
            | Error エラー -> return Error エラー
        }

    let シーケンス 非同期結果リスト =
        let rec ループ 累積値 残りリスト =
            async {
                match 残りリスト with
                | [] -> return Ok (List.rev 累積値)
                | 頭要素 :: 尾部 ->
                    let! 頭要素の結果 = 頭要素
                    match 頭要素の結果 with
                    | Ok 値 ->
                        return! ループ (値 :: 累積値) 尾部
                    | Error エラー ->
                        return Error エラー
            }
        ループ [] 非同期結果リスト

    let キャッチ ハンドラー 非同期結果 =
        async {
            try
                return! 非同期結果
            with
            | 例外 -> return! ハンドラー 例外
        }

type 非同期結果ビルダー() =
    member _.Return(値) = 非同期結果.結果から (Ok 値)
    member _.ReturnFrom(非同期結果) = 非同期結果
    member this.Bind(非同期結果: Async<Result<'T, 'Error>>, 関数: 'T -> Async<Result<'U, 'Error>>) : Async<Result<'U, 'Error>> =
        async {
            let! 結果 = 非同期結果
            match 結果 with
            | Ok 値 -> return! 関数 値
            | Error エラー -> return Error エラー
        }
    member _.Zero() = 非同期結果.結果から (Ok ())
    member _.Combine(前の値, 次の関数) = 非同期結果.バインド (fun _ -> 次の関数()) 前の値
    member _.Delay(関数) = 関数
    member _.Run(関数) = 関数()
    member _.TryFinally(メイン, ファイナライザー) = try メイン() finally ファイナライザー()
    member _.TryWith(メイン, ハンドラー) = try メイン() with | 例外 -> ハンドラー 例外

module 非同期結果ビルダー =
    let 非同期結果 = 非同期結果ビルダー()

module 結果 =
    let シーケンス 結果リスト =
        let rec ループ 累積値 残りリスト =
            match 残りリスト with
            | [] -> Ok (List.rev 累積値)
            | (Ok 値) :: 尾部 -> ループ (値 :: 累積値) 尾部
            | (Error エラー) :: _ -> Error エラー
        ループ [] 結果リスト

    let オプションへ = function
        | Ok 値 -> Some 値
        | Error _ -> None

module 注文ワークフロー =

    // 商品コードパース
    let 商品コードを解析 (コード文字列: string) =
        if コード文字列.StartsWith("W") then
            ウィジェットコード.作成 コード文字列
            |> Result.map ウィジェット
            |> Result.mapError (fun メッセージ -> フィールド形式不正 メッセージ)
            |> 非同期結果.結果から
        elif コード文字列.StartsWith("G") then
            ギズモコード.作成 コード文字列
            |> Result.map ギズモ
            |> Result.mapError (fun メッセージ -> フィールド形式不正 メッセージ)
            |> 非同期結果.結果から
        else
            非同期結果.エラーから (フィールド形式不正 "無効な商品コード形式")

    // 数量パース
    let 数量を解析 商品コード 数量 =
        match 商品コード with
        | ウィジェット _ ->
            単位数量.作成 (int 数量)
            |> Result.map 単位
            |> Result.mapError (fun メッセージ -> フィールド範囲外("Unit", 数量, 数量))
            |> 非同期結果.結果から
        | ギズモ _ ->
            キログラム数量.作成 数量
            |> Result.map キログラム
            |> Result.mapError (fun メッセージ -> フィールド範囲外("Kilogram", 数量, 数量))
            |> 非同期結果.結果から

    // 注文明細検証
    let 注文明細を検証 商品コード存在確認 (明細: 未検証注文明細) =
        非同期結果ビルダー.非同期結果 {
            // 商品コードの検証とパース
            let! 商品コード = 商品コードを解析 明細.商品コード

            // 商品の存在確認
            let 商品存在状態 = 商品コード存在確認 商品コード
            if not 商品存在状態 then
                return! 非同期結果.エラーから (フィールド欠如 "ProductCode not found")
            else
                ()

            // 数量の検証
            let! 数量 = 数量を解析 商品コード 明細.数量

            return {
                注文明細ID = 明細.注文明細ID
                商品コード = 商品コード
                数量 = 数量
            }
        }

    // 注文検証の実装
    let 注文を検証: 注文検証 =
        fun 商品コード存在確認 住所存在確認 未検証注文 ->
            非同期結果ビルダー.非同期結果 {
                // 顧客情報の検証
                let! 顧客名 =
                    文字列50.作成 (未検証注文.顧客情報.名 + " " + 未検証注文.顧客情報.姓)
                    |> Result.mapError (fun メッセージ -> フィールド形式不正 メッセージ)
                    |> 非同期結果.結果から

                let! 顧客メール =
                    メールアドレス.作成 未検証注文.顧客情報.メールアドレス
                    |> Result.mapError (fun メッセージ -> フィールド形式不正 メッセージ)
                    |> 非同期結果.結果から

                // 住所の検証
                let! 配送先住所 = 住所存在確認 未検証注文.配送先住所
                let! 請求先住所 = 住所存在確認 未検証注文.請求先住所

                // 注文明細の検証
                let! 検証済明細 =
                    未検証注文.明細
                    |> List.map (注文明細を検証 商品コード存在確認)
                    |> 非同期結果.シーケンス

                let! 注文ID値 =
                    注文ID.作成 未検証注文.注文ID
                    |> Result.mapError (fun メッセージ -> フィールド形式不正 メッセージ)
                    |> 非同期結果.結果から

                return {
                    注文ID = 注文ID値
                    顧客情報 = {
                        名前 = 顧客名
                        メール = 顧客メール
                    }
                    配送先住所 = 配送先住所
                    請求先住所 = 請求先住所
                    明細 = 検証済明細
                }
            }

    // 価格計算の実装
    let 注文価格を計算: 価格計算 =
        fun 商品価格取得 検証済注文 ->
            let 価格計算済明細 =
                検証済注文.明細
                |> List.map (fun 明細 ->
                    match 商品価格取得 明細.商品コード with
                    | Some 価格 ->
                        let 数量値 =
                            match 明細.数量 with
                            | 単位 単位数量値 -> decimal (単位数量.値 単位数量値)
                            | キログラム キログラム数量値 -> キログラム数量.値 キログラム数量値
                        Ok {
                            注文明細ID = 明細.注文明細ID
                            商品コード = 明細.商品コード
                            数量 = 明細.数量
                            明細価格 = 価格 * 数量値
                        }
                    | None ->
                        Error (商品が見つからない 明細.商品コード)
                )
                |> 結果.シーケンス

            価格計算済明細
            |> Result.map (fun 明細リスト ->
                let 合計金額 = 明細リスト |> List.sumBy (fun 明細 -> 明細.明細価格)
                {
                    注文ID = 検証済注文.注文ID
                    顧客情報 = 検証済注文.顧客情報
                    配送先住所 = 検証済注文.配送先住所
                    請求先住所 = 検証済注文.請求先住所
                    明細 = 明細リスト
                    請求金額 = 合計金額
                }
            )

    // 確認送信の実装
    let 注文確認を送信: 注文確認 =
        fun 注文確認送信 価格計算済注文 ->
            非同期結果ビルダー.非同期結果 {
                let! 確認結果 = 注文確認送信 価格計算済注文
                return Some 確認結果
            }
            |> 非同期結果.キャッチ (fun _ -> 非同期結果.結果から (Ok None))

    // イベント生成の実装
    let イベントを作成: イベント作成 =
        fun 価格計算済注文 確認オプション ->
            [
                Some (注文受付 価格計算済注文)

                if 価格計算済注文.請求金額 > 0m then
                    Some (請求対象注文受付 {
                        注文ID = 価格計算済注文.注文ID
                        請求先住所 = 価格計算済注文.請求先住所
                        請求金額 = 価格計算済注文.請求金額
                    })
                else
                    None

                match 確認オプション with
                | Some 確認結果 -> Some (確認送信完了 確認結果)
                | None -> None
            ]
            |> List.choose id

    // メインワークフロー実装
    let 注文を受け付け
        (商品コード存在確認: 商品コード存在確認)
        (商品価格取得: 商品価格取得)
        (住所存在確認: 住所存在確認)
        (注文確認送信: 注文確認送信)
        : 注文受付ワークフロー =
        fun 未検証注文 ->
            非同期結果ビルダー.非同期結果 {
                // 検証
                let! 検証済注文 =
                    注文を検証 商品コード存在確認 住所存在確認 未検証注文
                    |> 非同期結果.エラーマップ 検証エラー

                // 価格計算
                let! 価格計算済注文 =
                    注文価格を計算 商品価格取得 検証済注文
                    |> Result.mapError 価格計算エラー
                    |> 非同期結果.結果から

                // 確認送信
                let! 確認結果 =
                    注文確認を送信 注文確認送信 価格計算済注文
                    |> 非同期結果.エラーマップ 外部サービスエラー

                // イベント生成
                let イベントリスト = イベントを作成 価格計算済注文 確認結果

                return イベントリスト
            }