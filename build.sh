#!/bin/bash

mono .nuget/NuGet.exe update -self

if [ ! -f packages/FAKE/tools/FAKE.exe ]; then
    mono .nuget/NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion
fi

if [ ! -f packages/FAKE/xunit.runners/tools/xunit.console.clr4.exe ]; then
    mono .nuget/NuGet.exe install xunit.runners -OutputDirectory packages/FAKE -ExcludeVersion
fi

mono packages/FAKE/tools/FAKE.exe build.fsx RunTestsMono $@
