#r "paket:
version 7.0.2
framework: net6.0
source https://api.nuget.org/v3/index.json

nuget Microsoft.Build 17.3.2
nuget Microsoft.Build.Framework 17.3.2
nuget Microsoft.Build.Tasks.Core 17.3.2
nuget Microsoft.Build.Utilities.Core 17.3.2

nuget Be.Vlaanderen.Basisregisters.Build.Pipeline 6.0.6 //"

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

let buildSolution = buildSolution assemblyVersionNumber
let buildSource = build assemblyVersionNumber
let buildTest = buildTest assemblyVersionNumber
let setVersions = (setSolutionVersions assemblyVersionNumber product copyright company)
let test = testSolution
let publishSource = publish assemblyVersionNumber
let pack = pack nugetVersionNumber
let containerize = containerize dockerRepository
let push = push dockerRepository

supportedRuntimeIdentifiers <- [ "msil"; "linux-x64" ]

// Solution -----------------------------------------------------------------------

Target.create "Restore_Solution" (fun _ -> restore "AddressRegistry")

Target.create "Build_Solution" (fun _ ->
  setVersions "SolutionInfo.cs"
  buildSolution "AddressRegistry"
)

Target.create "Test_Solution" (fun _ ->
    [
        "test" @@ "AddressRegistry.Tests"
        "test" @@ "AddressRegistry.Api.Legacy.Tests"
    ] |> List.iter testWithDotNet
)

Target.create "Publish_Solution" (fun _ ->
  [
    "AddressRegistry.Projector"
    "AddressRegistry.Api.Legacy"
    "AddressRegistry.Api.Oslo"
    "AddressRegistry.Api.Extract"
    "AddressRegistry.Api.CrabImport"
    "AddressRegistry.Api.BackOffice"
    "AddressRegistry.Api.BackOffice.Abstractions"
    "AddressRegistry.Api.BackOffice.Handlers.Lambda"
    "AddressRegistry.Consumer"
    "AddressRegistry.Consumer.Read.Municipality"
    "AddressRegistry.Consumer.Read.StreetName"
    "AddressRegistry.Migrator.Address"
    "AddressRegistry.Producer"
    "AddressRegistry.Producer.Snapshot.Oslo"
    "AddressRegistry.Projections.BackOffice"
    "AddressRegistry.Projections.Legacy"
    "AddressRegistry.Projections.Extract"
    "AddressRegistry.Projections.LastChangedList"
    "AddressRegistry.Projections.Syndication"
    "AddressRegistry.Projections.Wfs"
    "AddressRegistry.Projections.Wms"
  ] |> List.iter publishSource

  let dist = (buildDir @@ "AddressRegistry.CacheWarmer" @@ "linux")
  let source = "src" @@ "AddressRegistry.CacheWarmer"

  Directory.ensure dist
  Shell.copyFile dist (source @@ "Dockerfile")
 )

Target.create "Pack_Solution" (fun _ ->
  [
    "AddressRegistry.Projector"
    "AddressRegistry.Api.Legacy"
    "AddressRegistry.Api.Oslo"
    "AddressRegistry.Api.Extract"
    "AddressRegistry.Api.CrabImport"
    "AddressRegistry.Api.BackOffice"
    "AddressRegistry.Api.BackOffice.Abstractions"
    "AddressRegistry.Consumer"
    "AddressRegistry.Consumer.Read.Municipality"
    "AddressRegistry.Consumer.Read.StreetName"
    "AddressRegistry.Migrator.Address"
    "AddressRegistry.Producer"
  ] |> List.iter pack)

Target.create "Containerize_Projector" (fun _ -> containerize "AddressRegistry.Projector" "projector")
Target.create "PushContainer_Projector" (fun _ -> push "projector")

Target.create "Containerize_ApiLegacy" (fun _ -> containerize "AddressRegistry.Api.Legacy" "api-legacy")
Target.create "PushContainer_ApiLegacy" (fun _ -> push "api-legacy")

Target.create "Containerize_ApiOslo" (fun _ -> containerize "AddressRegistry.Api.Oslo" "api-oslo")
Target.create "PushContainer_ApiOslo" (fun _ -> push "api-oslo")

Target.create "Containerize_ApiExtract" (fun _ -> containerize "AddressRegistry.Api.Extract" "api-extract")
Target.create "PushContainer_ApiExtract" (fun _ -> push "api-extract")

Target.create "Containerize_ApiBackOffice" (fun _ -> containerize "AddressRegistry.Api.BackOffice" "api-backoffice")
Target.create "PushContainer_ApiBackOffice" (fun _ -> push "api-backoffice")

Target.create "Containerize_ApiCrabImport" (fun _ -> containerize "AddressRegistry.Api.CrabImport" "api-crab-import")
Target.create "PushContainer_ApiCrabImport" (fun _ -> push "api-crab-import")

Target.create "Containerize_Consumer" (fun _ -> containerize "AddressRegistry.Consumer" "consumer")
Target.create "PushContainer_Consumer" (fun _ -> push "consumer")

Target.create "Containerize_ConsumerMunicipality" (fun _ -> containerize "AddressRegistry.Consumer.Read.Municipality" "consumer-read-municipality")
Target.create "PushContainer_ConsumerMunicipality" (fun _ -> push "consumer-read-municipality")

Target.create "Containerize_ConsumerStreetName" (fun _ -> containerize "AddressRegistry.Consumer.Read.StreetName" "consumer-read-streetname")
Target.create "PushContainer_ConsumerStreetName" (fun _ -> push "consumer-read-streetname")

Target.create "Containerize_Migrator_Address" (fun _ -> containerize "AddressRegistry.Migrator.Address" "migrator-address")
Target.create "PushContainer_Migrator_Address" (fun _ -> push "migrator-address")

Target.create "Containerize_Producer" (fun _ -> containerize "AddressRegistry.Producer" "producer")
Target.create "PushContainer_Producer" (fun _ -> push "producer")

Target.create "Containerize_Producer_Snapshot_Oslo" (fun _ -> containerize "AddressRegistry.Producer.Snapshot.Oslo" "producer-snapshot-oslo")
Target.create "PushContainer_Producer_Snapshot_Oslo" (fun _ -> push "producer-snapshot-oslo")

Target.create "Containerize_ProjectionsSyndication" (fun _ -> containerize "AddressRegistry.Projections.Syndication" "projections-syndication")
Target.create "PushContainer_ProjectionsSyndication" (fun _ -> push "projections-syndication")

Target.create "Containerize_ProjectionsBackOffice" (fun _ -> containerize "AddressRegistry.Projections.BackOffice" "projections-backoffice")
Target.create "PushContainer_ProjectionsBackOffice" (fun _ -> push "projections-backoffice")

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
  ==> "Containerize_ApiOslo"
  ==> "Containerize_ApiExtract"
  ==> "Containerize_ApiBackOffice"
  ==> "Containerize_ApiCrabImport"
  ==> "Containerize_Consumer"
  ==> "Containerize_ConsumerMunicipality"
  ==> "Containerize_ConsumerStreetName"
  ==> "Containerize_Migrator_Address"
  ==> "Containerize_Producer"
  ==> "Containerize_Producer_Snapshot_Oslo"
  ==> "Containerize_ProjectionsSyndication"
  ==> "Containerize_ProjectionsBackOffice"
  ==> "Containerize_CacheWarmer"
  ==> "Containerize"
// Possibly add more projects to containerize here

"Containerize"
  ==> "DockerLogin"
  ==> "PushContainer_Projector"
  ==> "PushContainer_ApiLegacy"
  ==> "PushContainer_ApiOslo"
  ==> "PushContainer_ApiExtract"
  ==> "PushContainer_ApiBackOffice"
  ==> "PushContainer_ApiCrabImport"
  ==> "PushContainer_Consumer"
  ==> "PushContainer_ConsumerMunicipality"
  ==> "PushContainer_ConsumerStreetName"
  ==> "PushContainer_Migrator_Address"
  ==> "PushContainer_Producer"
  ==> "PushContainer_Producer_Snapshot_Oslo"
  ==> "PushContainer_ProjectionsSyndication"
  ==> "PushContainer_ProjectionsBackOffice"
  ==> "PushContainer_CacheWarmer"
  ==> "Push"
// Possibly add more projects to push here

// By default we build & test
Target.runOrDefault "Test"
