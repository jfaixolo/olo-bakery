module Olo.Bakery.Publishing

open Cake.Common.IO
open Cake.Common.Tools.DotNet
open Cake.Common.Tools.DotNet.NuGet.Push
open Cake.Core.Diagnostics
open Cake.Core.IO
open System
open System.IO
open System.Linq
open System.Text.RegularExpressions
open System.Xml.Linq

type PushSettings = NuGet.Push.DotNetNuGetPushSettings

let publish (ctx: BakeryContext) =
    let publishAll _ =
        let publish apiKey url (path: Cake.Core.IO.Path) =
            ctx.Log.Information($"Publishing {path.Segments |> Seq.last}...")

            ctx.DotNetNuGetPush(
                FilePath.FromString path.FullPath,
                DotNetNuGetPushSettings(ApiKey = apiKey, Source = url)
            )

        let nugetApiKey = ctx |> TeamCity.getConfigParameter "OloNugetApiKey"

        let nugetUrl = ctx |> TeamCity.getConfigParameter "OloNugetUrl"

        match nugetApiKey, nugetUrl with
        | Some apiKey, Some url -> ctx.GetPaths "nupkg/*.nupkg" |> Seq.iter (publish apiKey url)
        | _, _ -> ctx.Log.Warning("Nuget API Key and/or Nuget URL not defined. Nothing was published.")

    TeamCity.guard ctx publishAll

let publishLocal (projects: (string * string) list) (ctx: BakeryContext) =
    
    // Hard-coded for now. Consider making a configuration value later.
    let repoPath = "C:\\nuget-repo" // backslash is required for nuget.config
    let nugetConfigPath = "../nuget.config"

    let ensureRepoExists () =
        if not <| Directory.Exists repoPath then
            Directory.CreateDirectory repoPath |> ignore

    let ensureRepoIsConfigured (ctx: BakeryContext) =
        let configPath = (ctx.File nugetConfigPath).Path.FullPath
        let doc = XDocument.Load configPath

        let packageSourcesElement =
            doc.Descendants("configuration").First().Descendants("packageSources").First()

        let repoIsAlreadyConfigured =
            packageSourcesElement.Descendants("add")
            |> Seq.exists (fun el -> el.Value.ToString() = repoPath)

        if not repoIsAlreadyConfigured then
            packageSourcesElement.Add(XElement("add", XAttribute("key", "Local"), XAttribute("value", repoPath)))
            File.WriteAllText(configPath, doc.ToString())

    let publishProjectLocally (ctx: BakeryContext) (settings: PushSettings) (proj: string, ext: string) =
        let path = (ctx.File $"src/{proj}/{proj}.{ext}").Path.FullPath

        // Set the project version
        let text = File.ReadAllText path
        // Using regex instead of Linq2Xml to avoid whitespace changes
        let text =
            Regex.Replace(text, "<Version>([^<]+)</Version>", $"<Version>{ctx.PackageVersion}</Version>")

        File.WriteAllText(path, text)

        // Pack
        ctx.DotNetPack path

        // Add to nuget
        ctx.DotNetNuGetPush($"src/{proj}/bin/Debug/{proj}.{ctx.PackageVersion}.nupkg", settings)

    if String.IsNullOrWhiteSpace(ctx.PackageVersion) then
        failwith "PackageVersion required for local NuGet package"

    ensureRepoExists ()
    ensureRepoIsConfigured ctx

    let settings = PushSettings(Source = repoPath)

    projects |> Seq.iter (publishProjectLocally ctx settings)