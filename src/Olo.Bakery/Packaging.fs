module Olo.Bakery.Packaging

open System.Linq
open System.Text.RegularExpressions
open Cake.Common.IO
open Cake.Common.Tools.DotNet
open Cake.Common.Tools.DotNet.MSBuild
open Cake.Common.Tools.DotNet.Pack
open Cake.Common.Tools.DotNet.Publish
open Cake.Core.Diagnostics
open Cake.Core.IO

let private packageService
    (selfContained: bool)
    (ctx: BakeryContext) 
    (path: Path)
    (runtime: Runtime option)
    packageVersion 
    packageSuffix =

    let packageName =
        let baseName = Regex.Replace(path.Segments.Last(), @"\..sproj", "")

        packageSuffix |> Option.map ((+) baseName) |> Option.defaultValue baseName

    ctx.DotNetPublish(
        path.FullPath,
        DotNetPublishSettings(
            Configuration = ctx.MsBuildConfiguration,
            Runtime = (runtime |> Option.map Runtime.toString |> Option.defaultValue null),
            SelfContained = selfContained,
            OutputDirectory = DirectoryPath.FromString $"publish/{packageName}",
            MSBuildSettings = DotNetMSBuildSettings(Version = packageVersion, PackageVersion = packageVersion)
        )
    )

    ctx.DotNetTool(
        "octo pack "
        + $"--id={packageName} "
        + $"""--basePath={DirectoryPath.FromString("publish").FullPath}/{packageName} """
        + $"""--version={Versioning.getOctopusReleaseVersion ctx |> Option.defaultValue "0.0.0.0"} """
        + $"""--outFolder={DirectoryPath.FromString("nupkg").FullPath} """
        + "--overwrite "
    )

let packageServices
    (selfContained: bool) 
    (services: (string * Runtime option * string option) seq) 
    (ctx: BakeryContext) =

    ctx.Log.Information("Packaging Services...")
    let version = Versioning.getPackageVersion ctx

    let package (glob: string, runtime: Runtime option, suffix: string option) =
        packageService selfContained ctx (ctx.GetPaths(glob).First()) runtime version suffix

    services |> Seq.iter package


let private packageClient (glob: string) version isPrerelease (ctx: BakeryContext) =
    let packageVersion = if isPrerelease then $"{version}-prerelease" else version

    ctx.DotNetPack(
        (ctx.GetPaths(glob) |> Seq.head).FullPath,
        DotNetPackSettings(
            Configuration = ctx.MsBuildConfiguration,
            OutputDirectory = DirectoryPath.FromString "nupkg",
            MSBuildSettings = DotNetMSBuildSettings(AssemblyVersion = version, PackageVersion = packageVersion)
        )
    )

let packageClients (libraries: string seq) (ctx: BakeryContext) =
    let version = Versioning.getPackageVersion ctx

    let pack (glob, isPrerelease) =
        ctx |> packageClient glob version isPrerelease

    // Package two identical versions of each package, one labeled "prerelease"
    // to use as a poor mans' package promotion system in TeamCity
    libraries
    |> Seq.collect (fun lib -> [(lib, true); (lib, false)])
    |> Seq.iter pack