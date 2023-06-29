module Olo.Bakery.TeamCity

open Cake.Common.Build
open Cake.Core.Diagnostics
open Olo.Bakery

let getConfigParameter key (ctx: BakeryContext) =
    if not <| ctx.BuildSystem().IsRunningOnTeamCity then
        None
    else
        match ctx.TeamCity().Environment.Build.ConfigProperties.TryGetValue key with
        | true, prop -> Some prop
        | _ -> None

let guard (ctx: BakeryContext) f =
    if not <| ctx.BuildSystem().IsRunningOnTeamCity then
        ctx.Log.Warning("Build is not running in TeamCity; this action will be skipped.")
    else
        f <| ctx.TeamCity()
