@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\msbuild.exe" %~dp0/build.proj /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false

pause