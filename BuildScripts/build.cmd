@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)



FOR /F "tokens=* USEBACKQ" %%F IN (`vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) DO (
    SET msbuild=%%F
)

"%msbuild%" %~dp0/build.proj /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false

pause