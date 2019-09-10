$ErrorActionPreference = "Stop"

$executingScriptDirectory = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$rootDirectory = Resolve-Path "$executingScriptDirectory\.."

$artifacts = "$rootDirectory\artifacts"
$tools = "$rootDirectory\tools"
$src = "$rootDirectory\src"

Add-Type -assembly "system.io.compression.filesystem"

function CreateCleanDirectory ($directoryName)
{
    If (Test-Path $directoryName){
        Remove-Item $directoryName -recurse
    }
    mkdir $directoryName
}

function RemoveIfExists ($strFileName)
{
    If (Test-Path $strFileName){
        Remove-Item $strFileName
    }
}

function GetDescriptionFromReleaseNotes()
{
    $reader = [System.IO.File]::OpenText("$rootDirectory\ReleaseNotes.md")
    try {
        for(;;) {
            $line = $reader.ReadLine()
          If ($line.StartsWith("# ") -eq $True)
          {
            If($readFirstNote) { break }
            Else 
            { 
                $readFirstNote = $True 
            }
          }
          Else
          {
            $description += "$line`n"
          }
        }
    }
    finally {
        $reader.Close()
    }

    return $description;
}

# define version
function SetVersion
{
    $version = $env:GitVersion_NuGetVersionV2
    $tag = $version
    If ($env:APPVEYOR_REPO_TAG -eq $True)
    {
        $tag = $env:APPVEYOR_REPO_TAG_NAME
        
        Set-AppveyorBuildVariable 'releaseDescription' (GetDescriptionFromReleaseNotes)
    }
    
    return New-Object PsObject -Property @{version=$version ; tag=$tag}
}

function ReplaceChocoInstInfos($chocoInstPath, $version, $path, $zipPathForChecksum)
{
    $chochoInst = gc $chocoInstPath
    $chochoInst = $chochoInst.replace('{{version}}', $version)
    $chochoInst = $chochoInst.replace('{{tag}}', $tag)
    $chochoInst = $chochoInst.replace('{{checksum}}', (Get-FileHash $zipPathForChecksum -Algorithm SHA256).Hash)
    $chochoInst | sc $chocoInstPath
}

# Packaging
function CreatePackage($versionInfos)
{
    $version = $versionInfos.version
    $tag = $versionInfos.tag
    
    CreateCleanDirectory chocoTemp

    # create Pretzel zip
    RemoveIfExists Pretzel.$version.zip
    7z a $artifacts\Pretzel.$version.zip $src\Pretzel\bin\Release\*.dll
    7z a $artifacts\Pretzel.$version.zip $src\Pretzel\bin\Release\Pretzel.exe
    7z a $artifacts\Pretzel.$version.zip $src\Pretzel\bin\Release\Pretzel.exe.config
    7z a $artifacts\Pretzel.$version.zip ReleaseNotes.md

    # build Pretzel nupkg
    mkdir chocoTemp\Pretzel\tools
    
    Copy-Item $tools\chocolatey\Pretzel\pretzel.nuspec chocoTemp\Pretzel\pretzel.nuspec
    Copy-Item $tools\chocolatey\Pretzel\chocolateyInstall.ps1 chocoTemp\Pretzel\tools\chocolateyInstall.ps1

    ReplaceChocoInstInfos chocoTemp\Pretzel\tools\chocolateyInstall.ps1 $version $tag $artifacts\Pretzel.$version.zip

    nuget pack chocoTemp\Pretzel\pretzel.nuspec -OutputDirectory $artifacts -Version $version -NoPackageAnalysis

    # create Pretzel.ScriptCs zip
    RemoveIfExists Pretzel.ScriptCs.$version.zip
    7z a $artifacts\Pretzel.ScriptCs.$version.zip $src\Pretzel.ScriptCs\bin\Release\*.dll

    # build Pretzel.ScriptCs nupkg
    
    mkdir chocoTemp\Pretzel.ScriptCs\tools
    Copy-Item $tools\chocolatey\Pretzel.ScriptCs\pretzel.scriptcs.nuspec chocoTemp\Pretzel.ScriptCs\pretzel.scriptcs.nuspec
    Copy-Item $tools\chocolatey\Pretzel.ScriptCs\chocolateyInstall.ps1 chocoTemp\Pretzel.ScriptCs\tools\chocolateyInstall.ps1
    Copy-Item $tools\chocolatey\Pretzel.ScriptCs\chocolateyUninstall.ps1 chocoTemp\Pretzel.ScriptCs\tools\chocolateyUninstall.ps1

    ReplaceChocoInstInfos chocoTemp\Pretzel.ScriptCs\tools\chocolateyInstall.ps1 $version $tag $artifacts\Pretzel.ScriptCs.$version.zip
    ReplaceChocoInstInfos chocoTemp\Pretzel.ScriptCs\tools\chocolateyUninstall.ps1 $version $tag $artifacts\Pretzel.ScriptCs.$version.zip

    nuget pack chocoTemp\Pretzel.ScriptCs\pretzel.scriptcs.nuspec -OutputDirectory $artifacts -Version $version -NoPackageAnalysis

    # build Pretzel.Logic nupkg
    nuget pack $src\Pretzel.Logic\Pretzel.Logic.csproj -OutputDirectory $artifacts -Version $version -symbols
}

# Test
function ExecuteTests($cover)
{
    If($cover -eq $true)
    {
        cinst opencover.portable -y
        cinst coveralls.io -source https://nuget.org/api/v2/
        & C:\ProgramData\chocolatey\lib\opencover.portable\tools\OpenCover.Console.exe -register:user -filter:"+[Pretzel.Logic]*" -excludebyattribute:*.ExcludeFromCodeCoverage* -target:"%xunit20%\xunit.console.exe" -targetargs:"""src\Pretzel.Tests\bin\Release\net462\Pretzel.Tests.dll"" -noshadow -appveyor" -output:$artifacts\coverage.xml -returntargetcode
        & coveralls.net --opencover $artifacts\coverage.xml
    }
    Else
    {
        &$tools\xunit\xunit.console.exe "$src\Pretzel.Tests\bin\Release\net462\Pretzel.Tests.dll"
    }
    
    if ($LastExitCode -ne 0) { throw "Tests failed" }
}

# build
function Build()
{
    CreateCleanDirectory $artifacts
    
    # AppVeyor
    If ($env:APPVEYOR -eq $true)
    {
            # Scheduled build
        If ($env:APPVEYOR_SCHEDULED_BUILD -eq $true)
        {
            # Coverity
            cinst PublishCoverity -source https://nuget.org/api/v2/
            
            cov-configure --config cov-config.xml --cs
            
            $buildArgs = @(
                          "$src\Pretzel.sln"
                          "/l:C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll",
                          "/m",
                          "/p:Configuration=Release"
                          );
            
            & cov-build --config cov-config.xml --dir $artifacts\cov-int msbuild $src\Pretzel.sln /p:Configuration=Release
            Push-AppveyorArtifact $artifacts\cov-int\build-log.txt
            & PublishCoverity compress -o $artifacts\coverity.zip -i $artifacts\cov-int;

            & PublishCoverity publish -z $artifacts\coverity.zip -r Code52/Pretzel -t $env:CoverityProjectToken -e $env:CoverityEmailDistribution -d "AppVeyor scheduled build." --codeVersion $env:GitVersion_NuGetVersionV2;

        }
        Else
        {
            & msbuild "$src\Pretzel.sln" /p:Configuration="Release" /verbosity:minimal /logger:"C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll"

            if ($LastExitCode -ne 0) { throw "Building solution failed" }

            ExecuteTests $true

            $versionInfos = SetVersion

            CreatePackage $versionInfos
        }
    }
    #Local build
    Else
    {
        Write-Warning "Chocolatey must be installed, along with Nuget.CommandLine, SevenZip, Visual Studio 2017 or later and VSWhere."
        
        $msbuildExe = &vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe
        
        &$msbuildExe "$src\Pretzel.sln" /p:Configuration="Release" /verbosity:minimal

        if ($LastExitCode -ne 0) { throw "Building solution failed" }
        
        ExecuteTests $false

        $versionInfos = SetVersion

        CreatePackage $versionInfos
    }
}

Build
