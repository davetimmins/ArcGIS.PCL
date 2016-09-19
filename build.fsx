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
let buildDir = tempDirectory @@ "artifacts"
let buildLibsDir = buildDir @@ "build" @@ "lib"
let buildSerializerLibsDir = buildDir @@ "build" @@ "serializers"
let buildPortableDir = "portable-net45+win81+wpa81+MonoAndroid10+MonoTouch10+Xamarin.iOS10"
let testDir = buildDir @@ "test"
let packagesDir = buildDir @@ "packages"
let nupacksPath = buildDir @@ "packs"
let testRunnerDir = currentDirectory @@ "packages" @@ "FAKE" @@ "xunit.runner.console" @@ "tools"
let assemblyVersion = getBuildParamOrDefault "assemblyVersion" "5.8.0"
let assemblyInformationalVersion = getBuildParamOrDefault "assemblyInformationalVersion" "5.8.0"

CleanDirs [buildDir]

try
    RestorePackages()
with
    | :? System.IO.PathTooLongException as ex -> printfn "Skipping package restore";

//--------------------------------------------------------------------------------
// Build
//--------------------------------------------------------------------------------

Target "BuildWindows" (fun _ ->

    CreateCSharpAssemblyInfo "./src/CommonAssemblyInfo.cs"
        [Attribute.Company ""
         Attribute.Copyright "Copyright Dave Timmins (c) 2013"
         Attribute.Product "ArcGIS REST API ServiceModel PCL"
         Attribute.Version assemblyVersion
         Attribute.FileVersion assemblyVersion
         Attribute.InformationalVersion assemblyInformationalVersion]

    !! "src/ArcGIS.ServiceModel/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ buildPortableDir) "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.NET/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "net45") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.WinStore/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "win81") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.WP/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "wpa81") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.Serializers.*/*.csproj"
      |> MSBuildRelease buildSerializerLibsDir "Build"
      |> Log "AppBuild-Output: "

    !! "tests/**/*.csproj"
      |> MSBuildDebug testDir "Build"
      |> Log "TestBuild-Output: "
)

Target "BuildMono" (fun _ ->

    CreateCSharpAssemblyInfo "./src/CommonAssemblyInfo.cs"
        [Attribute.Company ""
         Attribute.Copyright "Copyright Dave Timmins (c) 2013"
         Attribute.Product "ArcGIS REST API ServiceModel PCL"
         Attribute.Version assemblyVersion
         Attribute.FileVersion assemblyVersion
         Attribute.InformationalVersion assemblyInformationalVersion]

    !! "src/ArcGIS.ServiceModel/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ buildPortableDir) "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.NET/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "net45") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.Serializers.*/*.csproj"
      |> MSBuildRelease buildSerializerLibsDir "Build"
      |> Log "AppBuild-Output: "

    !! "tests/**/*.csproj"
      |> MSBuildDebug testDir "Build"
      |> Log "TestBuild-Output: "
)

Target "BuildAll" (fun _ ->

    CreateCSharpAssemblyInfo "./src/CommonAssemblyInfo.cs"
        [Attribute.Company ""
         Attribute.Copyright "Copyright Dave Timmins (c) 2013"
         Attribute.Product "ArcGIS REST API ServiceModel PCL"
         Attribute.Version assemblyVersion
         Attribute.FileVersion assemblyVersion
         Attribute.InformationalVersion assemblyInformationalVersion]

    !! "src/ArcGIS.ServiceModel/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ buildPortableDir) "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.Android/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "MonoAndroid10") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.iOS/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "MonoTouch10") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.iOSUnified/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "Xamarin.iOS10") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.NET/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "net45") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.WinStore/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "win81") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.WP/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "wpa81") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.Serializers.*/*.csproj"
      |> MSBuildRelease buildSerializerLibsDir "Build"
      |> Log "AppBuild-Output: "

    !! "tests/**/*.csproj"
      |> MSBuildDebug testDir "Build"
      |> Log "TestBuild-Output: "
)

//--------------------------------------------------------------------------------
// Test
//--------------------------------------------------------------------------------

Target "TestWindows" (fun _ ->
    !! (testDir + @"\ArcGIS.Test*.dll")
      |> xUnit2 (fun p ->
        {p with
            WorkingDir = Some testDir
            ToolPath = (testRunnerDir @@ "xunit.console.exe")
            TimeOut = System.TimeSpan.FromMinutes 5.0   })
)

Target "TestMono" (fun _ ->
    !! (testDir + @"\ArcGIS.Test*.dll")
      |> xUnit2 (fun p ->
        {p with
            WorkingDir = Some testDir
            ToolPath = (testRunnerDir @@ "xunit.console.exe")
            TimeOut = System.TimeSpan.FromMinutes 5.0   })
)

Target "TestAll" (fun _ ->
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

    cp "src/ArcGIS.ServiceModel.Serializers.ServiceStackV3/ServiceStackSerializer.cs" (buildSerializerLibsDir @@ "ServiceStackSerializer.cs.pp")
    cp "src/ArcGIS.ServiceModel.Serializers.JsonDotNet/JsonDotNetSerializer.cs" (buildSerializerLibsDir @@ "JsonDotNetSerializer.cs.pp")

    let replacements = seq {yield ("namespace ArcGIS.ServiceModel.Serializers", "namespace $rootnamespace$"); yield ("public class ", "internal class ")}
    let files = seq {yield (buildSerializerLibsDir @@ "JsonDotNetSerializer.cs.pp"); yield (buildSerializerLibsDir @@ "ServiceStackSerializer.cs.pp")}

    ReplaceInFiles replacements files

    NuGet (fun p ->
     {p with
        Version = assemblyInformationalVersion
        OutputPath = nupacksPath
        WorkingDir = currentDirectory
        Publish = false })
        "ArcGIS.PCL.nuspec"

    NuGet (fun p ->
     {p with
        Version = "2.0.3"
        OutputPath = nupacksPath
        WorkingDir = currentDirectory
        Publish = false })
        "ArcGIS.PCL.JsonDotNetSerializer.Source.nuspec"

    NuGet (fun p ->
     {p with
        Version = "2.0.2"
        OutputPath = nupacksPath
        WorkingDir = currentDirectory
        Publish = false })
        "ArcGIS.PCL.JsonDotNetSerializer.nuspec"

    NuGet (fun p ->
     {p with
        Version = "1.0.2"
        OutputPath = nupacksPath
        WorkingDir = currentDirectory
        Publish = false })
        "ArcGIS.PCL.ServiceStackSerializer.Source.nuspec"

    NuGet (fun p ->
     {p with
        Version = "1.0.1"
        OutputPath = nupacksPath
        WorkingDir = currentDirectory
        Publish = false })
        "ArcGIS.PCL.ServiceStackV3Serializer.nuspec"
)

//--------------------------------------------------------------------------------
//  Target dependencies
//--------------------------------------------------------------------------------

Target "RunTestsMono" DoNothing
"BuildMono" ==> "TestMono" ==> "RunTestsMono"

Target "RunTests" DoNothing
"BuildWindows" ==> "TestWindows" ==> "RunTests"

Target "NuGet" DoNothing
"BuildAll" ==> "Pack" ==> "NuGet"

// start build
RunTargetOrDefault "RunTests"
