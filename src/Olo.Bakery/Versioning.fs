module Olo.Bakery.Versioning

open System.Text.RegularExpressions
open Cake.Common.Build

[<Literal>]
let private defaultVersion = "0.0.0"

let getPackageVersion (ctx: BakeryContext) =
    if ctx.BuildSystem().IsRunningOnTeamCity then
        let build = ctx.TeamCity().Environment.Build

        ctx
        |> TeamCity.getConfigParameter "major.version"
        |> Option.map (fun majVer -> $"%s{majVer}.%s{build.Number}")
        |> Option.defaultValue defaultVersion
    else
        defaultVersion

let getOctopusReleaseVersion (ctx: BakeryContext) =
    let truncate i (str: string) = str[..i]

    let trimEnd (c: char) (str: string) = str.TrimEnd c

    if not <| ctx.BuildSystem().IsRunningOnTeamCity then
        None
    else
        let version = getPackageVersion ctx
        let build = ctx.TeamCity().Environment.Build

        let commitHash = TeamCity.getConfigParameter "build.vcs.number" ctx

        let branchName = Regex.Replace(build.BranchName, "[^0-9A-Za-z-]", "")

        let toOctoVersionString commitHash =
            $"%s{version}-C%s{commitHash}-%s{branchName}"

        commitHash
        |> Option.map (truncate 6 >> toOctoVersionString >> truncate 22 >> trimEnd '-')
