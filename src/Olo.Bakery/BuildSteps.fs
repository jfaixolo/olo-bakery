module Olo.Bakery.BuildSteps

open Cake.Common.IO
open Cake.Common.Tools.DotNet
open Cake.Common.Tools.DotNet.Build
open Cake.Common.Tools.DotNet.MSBuild
open Cake.Common.Tools.DotNet.NuGet.Push
open Cake.Core.Diagnostics
open Cake.Core.IO

let clean (ctx: BakeryContext) =
    ctx.Log.Information("Cleaning all `/bin` and `/obj` directories, as well as `/publish` and `/nupkg`.")
    ctx.CleanDirectories("src/**/bin/")
    ctx.CleanDirectories("src/**/obj/")
    ctx.CleanDirectories("tests/**/bin/")
    ctx.CleanDirectories("tests/**/obj/")
    ctx.CleanDirectories("publish/")
    ctx.CleanDirectories("nupkg/")

let restore (solutionName: string) (ctx: BakeryContext) =
    ctx.DotNetRestore(solutionName)

let compile (solutionName: string) (ctx: BakeryContext) =
    let version = Versioning.getPackageVersion ctx
    let verbosity =
        ctx
        |> TeamCity.getConfigParameter "MsBuildVerbosity"
        |> Option.bind DiscriminatedUnion.fromString<DotNetVerbosity>
        |> Option.toNullable

    ctx.DotNetBuild(
        solutionName,
        DotNetBuildSettings(NoRestore = true, Configuration = ctx.MsBuildConfiguration, MSBuildSettings = DotNetMSBuildSettings(Version = version, Verbosity = verbosity))
    )

let unitTests = Tests.runTests "UnitTests"

let integrationTests = Tests.runTests "IntegrationTests"

let package = Packaging.packageServices

let packageClient = Packaging.packageClients

let publish = Publishing.publish

let publishLocal = Publishing.publishLocal