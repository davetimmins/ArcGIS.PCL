@echo off
cls

.nuget\NuGet.exe update -self

if not exist "packages\FAKE\tools\Fake.exe" (
    .nuget\NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion
)

if not exist "packages\FAKE\xunit.runners\tools\xunit.console.clr4.exe" (
    .nuget\NuGet.exe install xunit.runners -OutputDirectory packages/FAKE -ExcludeVersion
)

packages\FAKE\tools\FAKE.exe build.fsx RunTests

pause
