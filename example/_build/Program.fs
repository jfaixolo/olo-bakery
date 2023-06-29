open Cake.Frosting
open Olo.Bakery

[<TaskName(nameof Clean)>]
[<TaskDescription("Clears out all `/bin` and `/obj` directories, along with the top-level `/publish` and `/nupkg` directories")>]
type Clean() =
    inherit FrostingTask<BakeryContext>()
    override _.Run ctx = ctx |> BuildSteps.clean

[<TaskName(nameof Restore)>]
[<TaskDescription("Runs dotnet restore against the solution to restore nuget packages")>]
type Restore() =
    inherit FrostingTask<BakeryContext>()
    override _.Run ctx = ctx |> BuildSteps.restore "Olo.Bakery.sln"

[<TaskName(nameof Compile)>]
[<TaskDescription("Builds the entire solution using the specified configuration")>]
[<IsDependentOn(typeof<Clean>); IsDependentOn(typeof<Restore>)>]
type Compile() =
    inherit FrostingTask<BakeryContext>()
    override _.Run ctx = ctx |> BuildSteps.compile "Olo.Bakery.sln"

[<TaskName(nameof UnitTest)>]
[<TaskDescription("Runs all unit tests projects using local file system DLLs")>]
type UnitTest() =
    inherit FrostingTask<BakeryContext>()
    override _.Run ctx = ctx |> BuildSteps.unitTests

[<TaskName(nameof IntegrationTest)>]
[<TaskDescription("Runs all integration test projects using local file system DLLs")>]
type IntegrationTest() =
    inherit FrostingTask<BakeryContext>()
    override _.Run ctx = ctx |> BuildSteps.integrationTests

[<TaskName(nameof Package)>]
[<TaskDescription("Produces nuget and octopus release packages for the API and Database projects")>]
[<IsDependentOn(typeof<Compile>); IsDependentOn(typeof<UnitTest>)>]
type Package() =
    inherit FrostingTask<BakeryContext>()
    override _.Run ctx = ctx |> BuildSteps.package false [ "Example.App/*.fsproj", Some Windows64Bit, None ]

[<TaskName(nameof PackageClient)>]
[<TaskDescription("Creates a nuget package for the api client project")>]
[<IsDependentOn(typeof<Compile>)>]
type PackageClient() =
    inherit FrostingTask<BakeryContext>()
    override _.Run ctx = ctx |> BuildSteps.packageClient [ "Example.App/*.fsproj" ]

[<TaskName(nameof Publish)>]
[<TaskDescription("Pushes all `.nupkg`s from nupkg/ to our internal nuget repo")>]
type Publish() =
    inherit FrostingTask<BakeryContext>()
    override _.Run ctx = ctx |> BuildSteps.publish

[<TaskName(nameof PublishLocal)>]
[<TaskDescription("Publishes a new version of the client library package to a local NuGet repo")>]
type PublishLocal() =
    inherit FrostingTask<BakeryContext>()
    override _.Run ctx = ctx |> BuildSteps.publishLocal [ "Example.App", "fsproj" ]

[<EntryPoint>]
let main args =
    CakeHost().UseContext<BakeryContext>().Run(args)
