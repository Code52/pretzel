#!/bin/bash
config=${1-Debug}

function nugetit {
  olddir=`pwd`
  cd $1
  mono --runtime=v4.0 ../.nuget/NuGet.exe install packages.config
  cd $olddir
}

cd src
nugetit ./Pretzel
nugetit ./Pretzel.Logic
nugetit ./Pretzel.Tests
cd ..
xbuild build.proj /t:Test /p:Configuration=$config /v:M /flp:LogFile=msbuild.log;Verbosity=Normal

