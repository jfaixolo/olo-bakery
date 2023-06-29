namespace Olo.Bakery

module DiscriminatedUnion =
    open Microsoft.FSharp.Reflection

    // for parsing discriminated union from a string
    // https://stackoverflow.com/a/36828630/167920
    let fromString<'a> (s:string) =
        match FSharpType.GetUnionCases typeof<'a> |> Array.filter (fun case -> case.Name = s) with
        |[|case|] -> Some(FSharpValue.MakeUnion(case,[||]) :?> 'a)
        |_ -> None
