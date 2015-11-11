$ErrorActionPreference = "Stop"

$executingScriptDirectory = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$rootDirectory = Resolve-Path "$executingScriptDirectory\.."

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
$version = $env:GitVersion_NuGetVersionV2
$tag = $version
If ($env:appveyor_repo_tag -eq $True)
{
    $tag = $env:appveyor_repo_tag_name
    
    Set-AppveyorBuildVariable 'releaseDescription' (GetDescriptionFromReleaseNotes)
}

$artifacts = "$rootDirectory\artifacts"
$tools = "$rootDirectory\tools"
$src = "$rootDirectory\src"
CreateCleanDirectory $artifacts
CreateCleanDirectory chocoTemp

# build Pretzel nupkg
mkdir chocoTemp\Pretzel\tools

Copy-Item $tools\chocolatey\Pretzel\pretzel.nuspec chocoTemp\Pretzel\pretzel.nuspec
Copy-Item $tools\chocolatey\Pretzel\chocolateyInstall.ps1 chocoTemp\Pretzel\tools\chocolateyInstall.ps1
Copy-Item $tools\chocolatey\Pretzel\chocolateyUninstall.ps1 chocoTemp\Pretzel\tools\chocolateyUninstall.ps1

(gc chocoTemp\Pretzel\tools\chocolateyInstall.ps1).replace('{{version}}', $version).replace('{{tag}}', $tag)|sc chocoTemp\Pretzel\tools\chocolateyInstall.ps1

nuget pack chocoTemp\Pretzel\pretzel.nuspec -OutputDirectory $artifacts -Version $version -NoPackageAnalysis

# create Pretzel zip
RemoveIfExists Pretzel.$version.zip
7z a $artifacts\Pretzel.$version.zip $src\Pretzel\bin\Release\*.dll
7z a $artifacts\Pretzel.$version.zip $src\Pretzel\bin\Release\Pretzel.exe
7z a $artifacts\Pretzel.$version.zip $src\Pretzel\bin\Release\Pretzel.exe.config
7z a $artifacts\Pretzel.$version.zip ReleaseNotes.md

# build Pretzel.ScriptCs nupkg
mkdir chocoTemp\Pretzel.ScriptCs\tools
Copy-Item $tools\chocolatey\Pretzel.ScriptCs\pretzel.scriptcs.nuspec chocoTemp\Pretzel.ScriptCs\pretzel.scriptcs.nuspec
Copy-Item $tools\chocolatey\Pretzel.ScriptCs\chocolateyInstall.ps1 chocoTemp\Pretzel.ScriptCs\tools\chocolateyInstall.ps1
Copy-Item $tools\chocolatey\Pretzel.ScriptCs\chocolateyUninstall.ps1 chocoTemp\Pretzel.ScriptCs\tools\chocolateyUninstall.ps1
(gc chocoTemp\Pretzel.ScriptCs\tools\chocolateyInstall.ps1).replace('{{version}}',$version).replace('{{tag}}',$tag)|sc chocoTemp\Pretzel.ScriptCs\tools\chocolateyInstall.ps1
nuget pack chocoTemp\Pretzel.ScriptCs\pretzel.scriptcs.nuspec -OutputDirectory $artifacts -Version $version -NoPackageAnalysis

# create Pretzel.ScriptCs zip
get-childitem $src\Pretzel.ScriptCs\bin\Release -filter *.dll | % { $_.Name } | out-file $artifacts\Pretzel.ScriptCs.Files.txt

RemoveIfExists Pretzel.ScriptCs.$version.zip
7z a $artifacts\Pretzel.ScriptCs.$version.zip $src\Pretzel.ScriptCs\bin\Release\*.dll
7z a $artifacts\Pretzel.ScriptCs.$version.zip $artifacts\Pretzel.ScriptCs.Files.txt

# build Pretzel.Logic nupkg
nuget pack $src\Pretzel.Logic\Pretzel.Logic.csproj -OutputDirectory $artifacts -Version $version -symbols