#load "packages/Be.Vlaanderen.Basisregisters.Build.Pipeline/Content/build-generic.fsx"

open Fake
open ``Build-generic``

// The buildserver passes in `BITBUCKET_BUILD_NUMBER` as an integer to version the results
// and `BUILD_DOCKER_REGISTRY` to point to a Docker registry to push the resulting Docker images.

// NpmInstall
// Run an `npm install` to setup Commitizen and Semantic Release.

// DotNetCli
// Checks if the requested .NET Core SDK and runtime version defined in global.json are available.
// We are pedantic about these being the exact versions to have identical builds everywhere.

// Clean
// Make sure we have a clean build directory to start with.

// Restore
// Restore dependencies for debian.8-x64 and win10-x64 using dotnet restore and Paket.

// Build
// Builds the solution in Release mode with the .NET Core SDK and runtime specified in global.json
// It builds it platform-neutral, debian.8-x64 and win10-x64 version.

// Test
// Runs `dotnet test` against the test projects.

// Publish
// Runs a `dotnet publish` for the debian.8-x64 and win10-x64 version as a self-contained application.
// It does this using the Release configuration.

// Pack
// Packs the solution using Paket in Release mode and places the result in the dist folder.
// This is usually used to build documentation NuGet packages.

// Containerize
// Executes a `docker build` to package the application as a docker image. It does not use a Docker cache.
// The result is tagged as latest and with the current version number.

// DockerLogin
// Executes `ci-docker-login.sh`, which does an aws ecr login to login to Amazon Elastic Container Registry.
// This uses the local aws settings, make sure they are working!

// Push
// Executes `docker push` to push the built images to the registry.

let product = "Basisregisters Vlaanderen"
let copyright = "Copyright (c) Vlaamse overheid"
let company = "Vlaamse overheid"

let dockerRepository = "addressregistry"
let assemblyVersionNumber = (sprintf "2.%s")
let nugetVersionNumber = (sprintf "%s")

let build = buildSolution assemblyVersionNumber
let setVersions = (setSolutionVersions assemblyVersionNumber product copyright company)
let test = testSolution
let publish = publish assemblyVersionNumber
let pack = pack nugetVersionNumber
let containerize = containerize dockerRepository
let push = push dockerRepository

// Solution -----------------------------------------------------------------------

Target "Restore_Solution" (fun _ -> restore "AddressRegistry")

Target "Build_Solution" (fun _ ->
  setVersions "SolutionInfo.cs"
  build "AddressRegistry")

Target "Test_Solution" (fun _ -> test "AddressRegistry")

Target "Publish_Solution" (fun _ ->
  [
    "AddressRegistry.Projector"
    "AddressRegistry.Api.Legacy"
    "AddressRegistry.Api.Extract"
    "AddressRegistry.Api.CrabImport"
    "AddressRegistry.Projections.Legacy"
    "AddressRegistry.Projections.Extract"
    "AddressRegistry.Projections.LastChangedList"
    "AddressRegistry.Projections.Syndication"
  ] |> List.iter publish)

Target "Pack_Solution" (fun _ ->
  [
    "AddressRegistry.Projector"
    "AddressRegistry.Api.Legacy"
    "AddressRegistry.Api.Extract"
    "AddressRegistry.Api.CrabImport"
  ] |> List.iter pack)

Target "Containerize_Projector" (fun _ -> containerize "AddressRegistry.Projector" "projections")
Target "PushContainer_Projector" (fun _ -> push "projections")

Target "Containerize_ApiLegacy" (fun _ -> containerize "AddressRegistry.Api.Legacy" "api-legacy")
Target "PushContainer_ApiLegacy" (fun _ -> push "api-legacy")

Target "Containerize_ApiExtract" (fun _ -> containerize "AddressRegistry.Api.Extract" "api-extract")
Target "PushContainer_ApiExtract" (fun _ -> push "api-extract")

Target "Containerize_ApiCrabImport" (fun _ -> containerize "AddressRegistry.Api.CrabImport" "api-crab-import")
Target "PushContainer_ApiCrabImport" (fun _ -> push "api-crab-import")

Target "Containerize_ProjectionsLegacy" (fun _ -> containerize "AddressRegistry.Projections.Legacy" "projections-legacy")
Target "PushContainer_ProjectionsLegacy" (fun _ -> push "projections-legacy")

Target "Containerize_ProjectionsExtract" (fun _ -> containerize "AddressRegistry.Projections.Extract" "projections-extract")
Target "PushContainer_ProjectionsExtract" (fun _ -> push "projections-extract")

Target "Containerize_ProjectionsRedis" (fun _ -> containerize "AddressRegistry.Projections.LastChangedList" "projections-redis")
Target "PushContainer_ProjectionsRedis" (fun _ -> push "projections-redis")

Target "Containerize_ProjectionsSyndication" (fun _ -> containerize "AddressRegistry.Projections.Syndication" "projections-syndication")
Target "PushContainer_ProjectionsSyndication" (fun _ -> push "projections-syndication")

// --------------------------------------------------------------------------------

Target "Build" DoNothing
Target "Test" DoNothing
Target "Publish" DoNothing
Target "Pack" DoNothing
Target "Containerize" DoNothing
Target "Push" DoNothing

"NpmInstall"         ==> "Build"
"DotNetCli"          ==> "Build"
"Clean"              ==> "Build"
"Restore_Solution"   ==> "Build"
"Build_Solution"     ==> "Build"

"Build"              ==> "Test"
"Test_Solution"      ==> "Test"

"Test"               ==> "Publish"
"Publish_Solution"   ==> "Publish"

"Publish"            ==> "Pack"
"Pack_Solution"      ==> "Pack"

"Pack"                                    ==> "Containerize"
"Containerize_Projector"                  ==> "Containerize"
"Containerize_ApiLegacy"                  ==> "Containerize"
"Containerize_ApiExtract"                 ==> "Containerize"
"Containerize_ApiCrabImport"              ==> "Containerize"
"Containerize_ProjectionsSyndication"     ==> "Containerize"
// Possibly add more projects to containerize here

"Containerize"                            ==> "Push"
"DockerLogin"                             ==> "Push"
"PushContainer_Projector"                 ==> "Push"
"PushContainer_ApiLegacy"                 ==> "Push"
"PushContainer_ApiExtract"                ==> "Push"
"PushContainer_ApiCrabImport"             ==> "Push"
"PushContainer_ProjectionsSyndication"    ==> "Push"
// Possibly add more projects to push here

// By default we build & test
RunTargetOrDefault "Test"
