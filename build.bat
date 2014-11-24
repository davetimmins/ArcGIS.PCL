@echo off
cls

.nuget\NuGet.exe update -self

.nuget\NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion

.nuget\NuGet.exe install xunit.runners -pre -OutputDirectory packages/FAKE -ExcludeVersion

packages\FAKE\tools\FAKE.exe build.fsx RunTests

pause
