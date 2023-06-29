namespace Olo.Bakery

open Cake.Core
open Cake.Frosting
open Cake.Common
open Cake.Common.Build

[<Sealed>]
type BakeryContext(ctx: ICakeContext) =
    inherit FrostingContext(ctx)

    member this.MsBuildConfiguration =
        let defaultConfiguration =
            if this.BuildSystem().IsLocalBuild then
                "Debug"
            else
                "Release"

        ctx.Argument("configuration", defaultConfiguration)

    member _.TestFilter = ctx.Argument("test-filter", "")

    member _.LogAllTests = ctx.Argument("log-all-tests", false)
    
    member _.PackageVersion = ctx.Argument("package-version", "")