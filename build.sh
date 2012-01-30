#!/bin/bash
config=${1-Debug}
xbuild build.proj /t:Test /p:Configuration=$config /v:M /flp:LogFile=msbuild.log;Verbosity=Normal
