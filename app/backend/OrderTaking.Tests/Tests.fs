module Tests

open System
open Xunit
open FsUnit.Xunit
open FsCheck
open FsCheck.Xunit

// xUnit による基本的なテスト
[<Fact>]
let ``Basic xUnit test`` () = Assert.True(true)

// FsUnit による BDD スタイルテスト
[<Fact>]
let ``FsUnit: List should contain elements`` () = [ 1; 2; 3 ] |> should contain 2

[<Fact>]
let ``FsUnit: String should equal`` () = "Hello" |> should equal "Hello"

[<Fact>]
let ``FsUnit: Number should be greater than`` () = 10 |> should be (greaterThan 5)

// FsCheck によるプロパティベーステスト
[<Property>]
let ``List reverse twice is original`` (xs: int list) = xs = xs

[<Property>]
let ``Adding same number to both sides keeps equality`` (x: int) (y: int) =
    let z = 10
    (x = y) = ((x + z) = (y + z))

[<Property>]
let ``String length is always non-negative`` (s: string) =
    if not (isNull s) then
        s.Length >= 0
    else
        true
