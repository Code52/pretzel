@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

"%ProgramFiles(x86)%\MSBuild\14.0\Bin\msbuild" %~dp0/build.proj /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false

pause