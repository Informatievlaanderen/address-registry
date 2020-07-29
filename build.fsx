#r "paket:
version 5.247.4
framework: netstandard20
source https://api.nuget.org/v3/index.json
nuget Be.Vlaanderen.Basisregisters.Build.Pipeline 4.2.2 //"

#load "packages/Be.Vlaanderen.Basisregisters.Build.Pipeline/Content/build-generic.fsx"

open Fake
open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO
open Fake.IO.FileSystemOperators
open ``Build-generic``

let product = "Basisregisters Vlaanderen"
let copyright = "Copyright (c) Vlaamse overheid"
let company = "Vlaamse overheid"

let dockerRepository = "address-registry"
let assemblyVersionNumber = (sprintf "2.%s")
let nugetVersionNumber = (sprintf "%s")

let build = buildSolution assemblyVersionNumber
let setVersions = (setSolutionVersions assemblyVersionNumber product copyright company)
let test = testSolution
let publish = publish assemblyVersionNumber
let pack = pack nugetVersionNumber
let containerize = containerize dockerRepository
let push = push dockerRepository

supportedRuntimeIdentifiers <- [ "msil"; "linux-x64" ]

// Solution -----------------------------------------------------------------------

Target.create "Restore_Solution" (fun _ -> restore "AddressRegistry")

Target.create "Build_Solution" (fun _ ->
  setVersions "SolutionInfo.cs"
  build "AddressRegistry")

Target.create "Test_Solution" (fun _ -> test "AddressRegistry")

Target.create "Publish_Solution" (fun _ ->
  [
    "AddressRegistry.Projector"
    "AddressRegistry.Api.Legacy"
    "AddressRegistry.Api.Extract"
    "AddressRegistry.Api.Backoffice"
    "AddressRegistry.Api.CrabImport"
    "AddressRegistry.Projections.Legacy"
    "AddressRegistry.Projections.Extract"
    "AddressRegistry.Projections.LastChangedList"
    "AddressRegistry.Projections.Syndication"
  ] |> List.iter publish

  let dist = (buildDir @@ "AddressRegistry.CacheWarmer" @@ "linux")
  let source = "src" @@ "AddressRegistry.CacheWarmer"

  Directory.ensure dist
  Shell.copyFile dist (source @@ "Dockerfile")
 )

Target.create "Pack_Solution" (fun _ ->
  [
    "AddressRegistry.Projector"
    "AddressRegistry.Api.Legacy"
    "AddressRegistry.Api.Extract"
    "AddressRegistry.Api.Backoffice"
    "AddressRegistry.Api.CrabImport"
  ] |> List.iter pack)

Target.create "Containerize_Projector" (fun _ -> containerize "AddressRegistry.Projector" "projector")
Target.create "PushContainer_Projector" (fun _ -> push "projector")

Target.create "Containerize_ApiLegacy" (fun _ -> containerize "AddressRegistry.Api.Legacy" "api-legacy")
Target.create "PushContainer_ApiLegacy" (fun _ -> push "api-legacy")

Target.create "Containerize_ApiExtract" (fun _ -> containerize "AddressRegistry.Api.Extract" "api-extract")
Target.create "PushContainer_ApiExtract" (fun _ -> push "api-extract")

Target.create "Containerize_ApiBackoffice" (fun _ -> containerize "AddressRegistry.Api.Backoffice" "api-backoffice")
Target.create "PushContainer_ApiBackoffice" (fun _ -> push "api-backoffice")

Target.create "Containerize_ApiCrabImport" (fun _ -> containerize "AddressRegistry.Api.CrabImport" "api-crab-import")
Target.create "PushContainer_ApiCrabImport" (fun _ -> push "api-crab-import")

Target.create "Containerize_ProjectionsSyndication" (fun _ -> containerize "AddressRegistry.Projections.Syndication" "projections-syndication")
Target.create "PushContainer_ProjectionsSyndication" (fun _ -> push "projections-syndication")

Target.create "Containerize_CacheWarmer" (fun _ -> containerize "AddressRegistry.CacheWarmer" "cache-warmer")
Target.create "PushContainer_CacheWarmer" (fun _ -> push "cache-warmer")

// --------------------------------------------------------------------------------

Target.create "Build" ignore
Target.create "Test" ignore
Target.create "Publish" ignore
Target.create "Pack" ignore
Target.create "Containerize" ignore
Target.create "Push" ignore

"NpmInstall"
  ==> "DotNetCli"
  ==> "Clean"
  ==> "Restore_Solution"
  ==> "Build_Solution"
  ==> "Build"

"Build"
  ==> "Test_Solution"
  ==> "Test"

"Test"
  ==> "Publish_Solution"
  ==> "Publish"

"Publish"
  ==> "Pack_Solution"
  ==> "Pack"

"Pack"
  ==> "Containerize_Projector"
  ==> "Containerize_ApiLegacy"
  ==> "Containerize_ApiExtract"
  ==> "Containerize_ApiBackoffice"
  ==> "Containerize_ApiCrabImport"
  ==> "Containerize_ProjectionsSyndication"
  ==> "Containerize_CacheWarmer"
  ==> "Containerize"
// Possibly add more projects to containerize here

"Containerize"
  ==> "DockerLogin"
  ==> "PushContainer_Projector"
  ==> "PushContainer_ApiLegacy"
  ==> "PushContainer_ApiExtract"
  ==> "PushContainer_ApiBackoffice"
  ==> "PushContainer_ApiCrabImport"
  ==> "PushContainer_ProjectionsSyndication"
  ==> "PushContainer_CacheWarmer"
  ==> "Push"
// Possibly add more projects to push here

// By default we build & test
Target.runOrDefault "Test"
