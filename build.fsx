// include Fake lib
#I @"packages/FAKE/tools"
#r @"FakeLib.dll"
open Fake
open Fake.AssemblyInfoFile
open Fake.FileUtils

// Properties
let tempDirectory = currentDirectory
let buildDir = tempDirectory @@ "artifacts"
let buildLibsDir = buildDir @@ "build" @@ "lib"
let buildSerializerLibsDir = buildDir @@ "build" @@ "serializers"
let testDir = buildDir @@ "test"
let packagesDir = buildDir @@ "packages"
let nupacksPath = buildDir @@ "packs"
let testRunnerDir = currentDirectory @@ "packages" @@ "FAKE" @@ "xunit.runners" @@ "tools"

CleanDirs [tempDirectory]

for packageConfig in !! (currentDirectory @@ "src" @@ "**" @@ "packages.config") do
     RestorePackage (fun p ->
         { p with
             OutputPath = (tempDirectory @@ "packages")})

//--------------------------------------------------------------------------------
// Build
//--------------------------------------------------------------------------------

Target "BuildWindows" (fun _ ->
    !! "src/ArcGIS.ServiceModel/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "portable-net4+sl5+netcore45+wp8+MonoAndroid1+MonoTouch1") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.NET/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "net45") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.NETv4/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "net40") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.WinStore/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "win8") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.WP/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "wp8") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.Serializers.*/*.csproj"
      |> MSBuildRelease buildSerializerLibsDir "Build"
      |> Log "AppBuild-Output: "

    !! "tests/**/*.csproj"
      |> MSBuildDebug testDir "Build"
      |> Log "TestBuild-Output: "
)

Target "BuildMono" (fun _ ->
    !! "src/ArcGIS.ServiceModel/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "portable-net4+sl5+netcore45+wp8+MonoAndroid1+MonoTouch1") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.Android/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "MonoAndroid1") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.iOS/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "MonoTouch1") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.NET/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "net45") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.NETv4/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "net40") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.Serializers.*/*.csproj"
      |> MSBuildRelease buildSerializerLibsDir "Build"
      |> Log "AppBuild-Output: "

    !! "tests/**/*.csproj"
      |> MSBuildDebug testDir "Build"
      |> Log "TestBuild-Output: "
)

Target "BuildAll" (fun _ ->
    !! "src/ArcGIS.ServiceModel/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "portable-net4+sl5+netcore45+wp8+MonoAndroid1+MonoTouch1") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.Android/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "MonoAndroid1") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.iOS/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "MonoTouch1") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.NET/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "net45") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.NETv4/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "net40") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.WinStore/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "win8") "Build"
      |> Log "AppBuild-Output: "

    !! "src/ArcGIS.ServiceModel.WP/*.csproj"
      |> MSBuildRelease (buildLibsDir @@ "wp8") "Build"
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
    !! (testDir + @"\*.Test.dll")
      |> xUnit (fun p -> 
        {p with 
            OutputDir = testDir 
            ToolPath = (testRunnerDir @@ "xunit.console.clr4.exe")
            TimeOut = System.TimeSpan.FromMinutes 5.0   })
)

Target "TestMono" (fun _ ->
    !! (testDir + @"\*.Test.dll")
      |> xUnit (fun p -> 
        {p with 
            OutputDir = testDir 
            ToolPath = (testRunnerDir @@ "xunit.console.clr4.exe")
            TimeOut = System.TimeSpan.FromMinutes 5.0   })
)

Target "TestAll" (fun _ ->
    !! (testDir + @"\*.Test.dll")
      |> xUnit (fun p -> 
        {p with 
            OutputDir = testDir 
            ToolPath = (testRunnerDir @@ "xunit.console.clr4.exe")
            TimeOut = System.TimeSpan.FromMinutes 5.0   })
)

//--------------------------------------------------------------------------------
// NuGet
//--------------------------------------------------------------------------------

Target "Pack" (fun _ ->

    mkdir nupacksPath

    NuGet (fun p ->
     {p with
        Version = "5.0.0-beta3"
        OutputPath = nupacksPath
        WorkingDir = currentDirectory
        Publish = false })
        "ArcGIS.PCL.nuspec"

    NuGet (fun p ->
     {p with
        Version = "1.0.0"
        OutputPath = nupacksPath
        WorkingDir = currentDirectory
        Publish = false })
        "ArcGIS.PCL.JsonDotNetSerializer.nuspec"

    NuGet (fun p ->
     {p with
        Version = "1.0.0"
        OutputPath = nupacksPath
        WorkingDir = currentDirectory
        Publish = false })
        "ArcGIS.PCL.ServiceStackSerializer.nuspec"

    NuGet (fun p ->
     {p with
        Version = "1.0.0"
        OutputPath = nupacksPath
        WorkingDir = currentDirectory
        Publish = false })
        "ArcGIS.PCL.ServiceStackPCLSerializer.nuspec"

    NuGet (fun p ->
     {p with
        Version = "1.0.0"
        OutputPath = nupacksPath
        WorkingDir = currentDirectory
        Publish = false })
        "ArcGIS.PCL.ServiceStackV3Serializer.nuspec"

    NuGet (fun p ->
     {p with
        Version = "0.1.0-alpha1"
        OutputPath = nupacksPath
        WorkingDir = currentDirectory
        Publish = false })
        "ArcGIS.PCL.JilSerializer.nuspec"

    NuGet (fun p ->
     {p with
        Version = "0.1.0-alpha1"
        OutputPath = nupacksPath
        WorkingDir = currentDirectory
        Publish = false })
        "ArcGIS.PCL.SimpleJsonSerializer.nuspec"
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
