module Olo.Bakery.Tests

open Cake.Common.Tools.DotNet.Test
open Cake.Core.Diagnostics
open Cake.Common.IO
open Cake.Common.Tools.DotNet

let runTests projectQualifier (ctx: BakeryContext) =
    let run () =
        ctx.GetPaths($"tests/**/*.{projectQualifier}/*.?sproj")
        |> Seq.map (fun path ->
            async {
                ctx.DotNetTest(
                    path.FullPath,
                    DotNetTestSettings(
                        Configuration = ctx.MsBuildConfiguration,
                        Filter = ctx.TestFilter,
                        NoRestore = true,
                        NoBuild = true,
                        Loggers =
                            if ctx.LogAllTests then
                                [| "console;verbosity=normal" |]
                            else
                                null
                    )
                )
            })
        |> fun tasks -> Async.Parallel(tasks, 3)
        |> Async.Ignore

    do
        (if not <| System.String.IsNullOrWhiteSpace(ctx.TestFilter) then
             ctx.Log.Information($"Test filter applied: {ctx.TestFilter}"))

    run () |> Async.RunSynchronously
