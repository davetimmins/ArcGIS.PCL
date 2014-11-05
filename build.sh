#!/bin/bash

SCRIPT_PATH="${BASH_SOURCE[0]}";
if ([ -h "${SCRIPT_PATH}" ]) then
  while([ -h "${SCRIPT_PATH}" ]) do SCRIPT_PATH=`readlink "${SCRIPT_PATH}"`; done
fi
pushd . > /dev/null
cd `dirname ${SCRIPT_PATH}` > /dev/null
SCRIPT_PATH=`pwd`;
popd  > /dev/null

mono $SCRIPT_PATH/.nuget/NuGet.exe update -self

mono $SCRIPT_PATH/.nuget/NuGet.exe install FAKE -OutputDirectory $SCRIPT_PATH/src/packages -ExcludeVersion

mono $SCRIPT_PATH/.nuget/NuGet.exe install xunit.runners -OutputDirectory $SCRIPT_PATH/src/packages/FAKE -ExcludeVersion

export encoding=utf-8

mono $SCRIPT_PATH/packages/FAKE/tools/FAKE.exe build.fsx RunTestsNix "$@"