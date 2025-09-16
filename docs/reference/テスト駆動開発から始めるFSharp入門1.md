---
title: テスト駆動開発から始めるF#入門1
description: 
published: true
date: 2025-08-28T09:14:44.129Z
tags: 
editor: markdown
dateCreated: 2025-08-28T08:16:37.748Z
---

# テスト駆動開発から始めるF#入門 ~2時間でTDDとリファクタリングのエッセンスを体験する~

## はじめに

この記事では、テスト駆動開発（TDD）を通してF#プログラミングの基礎を学びます。古典的なFizzBuzzプログラムを例に、TDDのRed-Green-Refactorサイクルを体験しながら、F#の関数型プログラミングの特徴を理解していきます。

### 対象読者

- プログラミング経験はあるがF#は初心者の方
- テスト駆動開発に興味がある開発者
- 関数型プログラミングの入門を求めている方
- .NET エコシステムでの開発に興味がある方

### 学習目標

この記事を通して以下のことを習得できます：

1. TDDの基本サイクル（Red-Green-Refactor）の理解と実践
2. F#の基本文法と関数型プログラミングの考え方
3. .NET開発環境でのテスト駆動開発の実践方法
4. F#らしいコードの書き方（パイプライン演算子、イミュータブル設計など）

### 必要な環境

- .NET 8.0 SDK
- エディタ（Visual Studio Code 推奨）
- ターミナル/コマンドプロンプト

.NET SDKがインストールされているかは以下のコマンドで確認できます：

```bash
dotnet --version
```

## エピソード1：TODOリストから始めるテスト駆動開発

### FizzBuzzの仕様

まず実装するプログラムの仕様を確認しましょう：

```
1 から 100 までの数をプリントするプログラムを書け。
ただし 3 の倍数のときは数の代わりに｢Fizz｣と、5 の倍数のときは｢Buzz｣とプリントし、
3 と 5 両方の倍数の場合には｢FizzBuzz｣とプリントすること。
```

### TODOリストの作成

プログラムを作成するにあたってまず何をすればよいでしょうか？私は、まず仕様の確認をして **TODOリスト** を作るところから始めます。

> TODOリスト
> 
> 何をテストすべきだろうか----着手する前に、必要になりそうなテストをリストに書き出しておこう。
> 
> —  テスト駆動開発 

仕様の内容をそのままプログラムに落とし込むには少しサイズが大きいようですね。なので最初の作業は仕様を **TODOリスト** に分解する作業から着手することにしましょう。

**TODOリスト**

- 数を文字列にして返す
- 3 の倍数のときは数の代わりに｢Fizz｣と返す
- 5 の倍数のときは｢Buzz｣と返す
- 3 と 5 両方の倍数の場合には｢FizzBuzz｣と返す
- 1 から 100 までの数
- プリントする

まず `数を文字列にして返す`作業に取り掛かりたいのですがまだプログラミング対象としてはサイズが大きいようですね。もう少し具体的に分割しましょう。

- 数を文字列にして返す
  - 1を渡したら文字列"1"を返す

これならプログラムの対象として実装できそうですね。

### 小さな問題への分解

TODOリストの作成において重要なのは、**実装可能な小さなタスクに分解すること**です。F#でのテスト駆動開発でも、この原則は変わりません。

大きな問題を小さく分解することで：
- テストが書きやすくなる
- 実装の方向性が明確になる
- 進捗が見えやすくなる
- デバッグが容易になる

## テストファーストから始めるF#開発

### 開発環境の構築

最初にプログラムを実行するための準備作業を進める必要がありますね。

> テストファースト
> 
> いつテストを書くべきだろうか----それはテスト対象のコードを書く前だ。
> 
> —  テスト駆動開発 

では、どうやってテストすればいいでしょうか？テスティングフレームワークを使って自動テストを書きましょう。

> テスト（名詞） どうやってソフトウェアをテストすればよいだろか----自動テストを書こう。
> 
> —  テスト駆動開発 

#### F#プロジェクトの作成

まず、F#のコンソールアプリケーションプロジェクトを作成します：

```bash
dotnet new console -lang F# -n fizzbuzz
cd fizzbuzz
```

#### テスティングフレームワークの追加

F#でのテスト駆動開発には、NUnitテスティングフレームワークを利用します。以下のパッケージを追加します：

```bash
dotnet add package NUnit
dotnet add package NUnit3TestAdapter
dotnet add package Microsoft.NET.Test.Sdk
```

### 最初のテスト作成

まず以下の内容のテストファイルを作成して `Program.fs` で保存します：

```fsharp
open NUnit.Framework

let greeting() = "hello world"

[<TestFixture>]
type HelloTest() =

    [<Test>]
    member __.TestGreeting() =
        Assert.That(greeting(), Is.EqualTo("hello world"))

[<EntryPoint>]
let main argv =
    printfn "Hello from F#"
    0
```

テストを実行します：

```bash
dotnet test
```

テストが成功することを確認できたら、環境構築は完了です。

### 最初のFizzBuzzテスト

では、実際のFizzBuzzテストを作成しましょう。TODOリストの最初の項目「1を渡したら文字列"1"を返す」から始めます。

既存のテストを置き換えて、以下のように修正します：

```fsharp
open NUnit.Framework

[<TestFixture>]
type FizzBuzzTest() =

    [<Test>]
    member __.Test数を文字列にして返す() =
        Assert.That(fizz_buzz(1), Is.EqualTo("1"))

[<EntryPoint>]
let main argv =
    printfn "Hello from F#"
    0
```

この段階でテストを実行すると、失敗します：

```bash
dotnet test
```

```
error FS0039: 値またはコンストラクター 'fizz_buzz' が定義されていません。
```

これは期待通りの失敗です。TDDの **Red** フェーズです。

### 仮実装（フェイク実装）

テストを通すための最小限の実装を行います。これを **仮実装** または **フェイク実装** と呼びます：

```fsharp
open NUnit.Framework

let fizz_buzz n = "1"

[<TestFixture>]
type FizzBuzzTest() =

    [<Test>]
    member __.Test数を文字列にして返す() =
        Assert.That(fizz_buzz(1), Is.EqualTo("1"))

[<EntryPoint>]
let main argv =
    printfn "Hello from F#"
    0
```

テストを実行します：

```bash
dotnet test
```

成功! これがTDDの **Green** フェーズです。

## リファクタリングとF#らしい実装

### Red-Green-Refactorサイクル

TDDの基本サイクルは以下の3つのフェーズで構成されます：

1. **Red**: 失敗するテストを書く
2. **Green**: テストを通す最小限の実装を行う
3. **Refactor**: コードを改善する

現在はGreenフェーズまで完了しています。次は **Refactor** フェーズです。

### メソッドの抽出

現在の`fizz_buzz`関数内で数値を文字列に変換する処理を別メソッドに抽出してみましょう：

```fsharp
open NUnit.Framework

let to_string n = "1"

let fizz_buzz n = to_string n

[<TestFixture>]
type FizzBuzzTest() =

    [<Test>]
    member __.Test数を文字列にして返す() =
        Assert.That(fizz_buzz(1), Is.EqualTo("1"))

[<EntryPoint>]
let main argv =
    printfn "Hello from F#"
    0
```

テストを実行して、まだ成功することを確認します：

```bash
dotnet test
```

### 変数名の変更

パラメータ名をより明確にします：

```fsharp
let to_string number = "1"

let fizz_buzz number = to_string number
```

再度テストを実行して成功を確認します。これでRefactorフェーズも完了です。

## 明白な実装と関数型アプローチ

### 次のテストケース

TODOリストの次の項目「3を渡したら文字列Fizzを返す」に取り組みましょう。

新しいテストを追加します：

```fsharp
[<TestFixture>]
type FizzBuzzTest() =

    [<Test>]
    member __.Test数を文字列にして返す() =
        Assert.That(fizz_buzz(1), Is.EqualTo("1"))

    [<Test>]
    member __.Test3を渡したら文字列Fizzを返す() =
        Assert.That(fizz_buzz(3), Is.EqualTo("Fizz"))
```

テストを実行すると失敗します（Red）：

```
Expected: "Fizz"
But was:  "1"
```

### F#の条件分岐

F#の`if-elif-else`構文を使って明白な実装を行います：

```fsharp
let to_string number = 
    if number % 3 = 0 then "Fizz" 
    else string number
```

ここで`string`は、F#の組み込み関数で数値を文字列に変換します。

テストを実行すると成功します（Green）。

### 5の倍数とFizzBuzzの実装

同様に、5の倍数と15の倍数のテストを追加していきます：

```fsharp
[<Test>]
member __.Test5を渡したら文字列Buzzを返す() =
    Assert.That(fizz_buzz(5), Is.EqualTo("Buzz"))

[<Test>]
member __.Test15を渡したら文字列FizzBuzzを返す() =
    Assert.That(fizz_buzz(15), Is.EqualTo("FizzBuzz"))
```

最終的な`to_string`関数の実装：

```fsharp
let to_string number = 
    if number % 3 = 0 && number % 5 = 0 then "FizzBuzz"
    elif number % 3 = 0 then "Fizz" 
    elif number % 5 = 0 then "Buzz"
    else string number
```

## F#らしいリスト処理

### 配列とリスト操作の学習

次は「1から100までの数を返す」機能を実装します。F#では範囲を簡潔に表現できます：

```fsharp
let create_numbers() = [|1..100|]
```

これは1から100までの整数配列を作成します。

### Array.map と パイプライン演算子の活用

F#の強力な機能の一つがパイプライン演算子（`|>`）です。これを使ってFizzBuzzリストを作成します：

```fsharp
let create_fizz_buzz_list() = 
    create_numbers()
    |> Array.map fizz_buzz
```

この実装は以下のように読めます：
1. `create_numbers()`で数値配列を作成
2. その結果を`Array.map fizz_buzz`に渡して、各要素を`fizz_buzz`関数で変換

### 繰り返し処理

最後にプリント機能を実装します：

```fsharp
let print_1_to_100() =
    create_fizz_buzz_list()
    |> Array.iter (printfn "%s")

[<EntryPoint>]
let main argv =
    print_1_to_100()
    0
```

`Array.iter`は各要素に対して副作用のある処理（この場合は出力）を実行します。

## コードの品質向上

### F#における良いコード

F#では以下のような特徴を持つコードが「良いコード」とされます：

1. **イミュータブル（不変）**: データは変更されない
2. **関数の組み合わせ**: 小さな関数を組み合わせて複雑な処理を表現
3. **型安全**: コンパイル時に多くのエラーを検出
4. **宣言的**: 「何をするか」を「どうやるか」よりも重視

### パターンマッチングの活用（発展例）

F#らしい書き方として、パターンマッチングを使った実装も可能です：

```fsharp
let fizz_buzz_pattern_match number =
    match (number % 3, number % 5) with
    | (0, 0) -> "FizzBuzz"
    | (0, _) -> "Fizz"
    | (_, 0) -> "Buzz"
    | _ -> string number
```

このコードは剰余の組み合わせによってパターンマッチングを行っています。

## 動作するきれいなコード

### 完成したFizzBuzzプログラム

最終的なコードは以下のようになります：

```fsharp
open NUnit.Framework

let to_string number = 
    if number % 3 = 0 && number % 5 = 0 then "FizzBuzz"
    elif number % 3 = 0 then "Fizz" 
    elif number % 5 = 0 then "Buzz"
    else string number

let fizz_buzz number = to_string number

let create_numbers() = [|1..100|]

let create_fizz_buzz_list() = 
    create_numbers()
    |> Array.map fizz_buzz

let print_1_to_100() =
    create_fizz_buzz_list()
    |> Array.iter (printfn "%s")

[<TestFixture>]
type FizzBuzzTest() =

    [<Test>]
    member __.Test数を文字列にして返す() =
        Assert.That(fizz_buzz(1), Is.EqualTo("1"))

    [<Test>]
    member __.Test3を渡したら文字列Fizzを返す() =
        Assert.That(fizz_buzz(3), Is.EqualTo("Fizz"))

    [<Test>]
    member __.Test5を渡したら文字列Buzzを返す() =
        Assert.That(fizz_buzz(5), Is.EqualTo("Buzz"))

    [<Test>]
    member __.Test15を渡したら文字列FizzBuzzを返す() =
        Assert.That(fizz_buzz(15), Is.EqualTo("FizzBuzz"))

    [<Test>]
    member __.Test1から100までの数を返す() =
        let numbers = create_numbers()
        Assert.That(numbers.[0], Is.EqualTo(1))
        Assert.That(numbers.[99], Is.EqualTo(100))
        Assert.That(numbers.Length, Is.EqualTo(100))

    [<Test>]
    member __.Test1から100までのFizzBuzzの配列を返す() =
        let result = create_fizz_buzz_list()
        Assert.That(result.[0], Is.EqualTo("1"))
        Assert.That(result.[2], Is.EqualTo("Fizz"))
        Assert.That(result.[4], Is.EqualTo("Buzz"))
        Assert.That(result.[14], Is.EqualTo("FizzBuzz"))
        Assert.That(result.Length, Is.EqualTo(100))

[<EntryPoint>]
let main argv =
    print_1_to_100()
    0
```

実行すると、期待通りの出力が得られます：

```bash
dotnet run
```

```
1
2
Fizz
4
Buzz
Fizz
7
8
Fizz
Buzz
11
Fizz
13
14
FizzBuzz
...
```

### TDDで得られたもの

この実装を通して、以下のことを習得しました：

1. **TDDのリズム**: Red-Green-Refactorサイクルの体験
2. **F#の基本文法**: 関数定義、条件分岐、配列操作
3. **関数型プログラミングの考え方**: 関数の組み合わせ、イミュータブルな設計
4. **テスト駆動の安心感**: 変更に対する信頼性

## まとめ

### 学習の振り返り

このチュートリアルでは、テスト駆動開発を通してF#プログラミングの基礎を学びました。

**TDDの価値**:
- 小さなステップで確実に進む
- リファクタリングの安全性
- 設計の改善

**F#の特徴**:
- 簡潔で表現力豊かな構文
- 強力な型システム
- 関数型プログラミングのサポート

### F#開発の次のステップ

この基礎を元に、以下のような学習を進めることをお勧めします：

1. **より高度なF#機能**: 判別共用体、レコード型、コンピュテーション式
2. **関数型設計パターン**: モナド、関数合成、部分適用
3. **.NETエコシステム**: ASP.NET Core、Entity Framework Core
4. **実際のプロジェクト**: WebAPI、デスクトップアプリケーション

## 参照

### F#関連リソース

- [F# Guide | Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/fsharp/)
- [F# for Fun and Profit](https://fsharpforfunandprofit.com/)
- [F# Software Foundation](https://fsharp.org/)

### TDD関連書籍

- Kent Beck著「テスト駆動開発」
- Steve Freeman, Nat Pryce著「実践テスト駆動開発」

### .NET開発ツール

- [.NET CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/)
- [Visual Studio Code](https://code.visualstudio.com/)
- [JetBrains Rider](https://www.jetbrains.com/rider/)

---

この記事を通して、F#とテスト駆動開発の素晴らしい組み合わせを体験していただけたでしょうか。F#の関数型プログラミングの考え方は、よりよいソフトウェア設計への道しるべとなるでしょう。

Happy F# coding! 🚀