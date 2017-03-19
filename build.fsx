// include Fake lib
#I @"packages/FAKE/tools"
#r @"FakeLib.dll"
open Fake
open Fake.AssemblyInfoFile
open Fake.FileUtils
open Fake.FileHelper
open Fake.Testing.XUnit2

// Properties
let tempDirectory = "./"
let buildDir = tempDirectory @@ "artefacts"
let buildLibsDir = buildDir @@ "build" @@ "lib"
let buildPortableDir = "netstandard1.3"
let testDir = buildDir @@ "test"
let nupacksPath = buildDir @@ "packs"
let testRunnerDir = currentDirectory @@ "packages" @@ "FAKE" @@ "xunit.runner.console" @@ "tools"
let assemblyVersion = getBuildParamOrDefault "assemblyVersion" "6.0.0"
let assemblyInformationalVersion = getBuildParamOrDefault "assemblyInformationalVersion" "6.0.0-beta-1"

CleanDirs [buildDir]

try
    RestorePackages()
with
    | :? System.IO.PathTooLongException as ex -> printfn "Skipping package restore";

//--------------------------------------------------------------------------------
// Build
//--------------------------------------------------------------------------------

Target "Build" (fun _ ->

    CreateCSharpAssemblyInfo "./src/CommonAssemblyInfo.cs"
        [Attribute.Company ""
         Attribute.Copyright "Copyright Dave Timmins (c) 2013"
         Attribute.Product "ArcGIS REST API ServiceModel PCL netstandard"
         Attribute.Version assemblyVersion
         Attribute.FileVersion assemblyVersion
         Attribute.InformationalVersion assemblyInformationalVersion]

    !! "src/ArcGIS.ServiceModel/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ buildPortableDir) "Build"
      |> Log "AppBuild-Output: "

    !! "tests/**/*.csproj"
      |> MSBuildDebug testDir "Build"
      |> Log "TestBuild-Output: "
)

//--------------------------------------------------------------------------------
// Test
//--------------------------------------------------------------------------------

Target "Test" (fun _ ->
    !! (testDir + @"\ArcGIS.Test*.dll")
      |> xUnit2 (fun p ->
        {p with
            WorkingDir = Some testDir
            ToolPath = (testRunnerDir @@ "xunit.console.exe")
            TimeOut = System.TimeSpan.FromMinutes 5.0   })
)

//--------------------------------------------------------------------------------
// NuGet
//--------------------------------------------------------------------------------

Target "Pack" (fun _ ->

    mkdir nupacksPath

    NuGet (fun p ->
     {p with
        Version = assemblyInformationalVersion
        OutputPath = nupacksPath
        WorkingDir = currentDirectory
        Publish = false })
        "ArcGIS.PCL.nuspec"
)

//--------------------------------------------------------------------------------
//  Target dependencies
//--------------------------------------------------------------------------------

Target "NuGet" DoNothing
"Build" ==> "Test" ==> "Pack" ==> "NuGet"

// start build
RunTargetOrDefault "NuGet"
