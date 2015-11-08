$env:appveyor_build_folder = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$env:GitVersion_NuGetVersionV2 = "0.42.0"
# if release test
$env:appveyor_repo_tag = $true
$env:appveyor_repo_tag_name = "v0.42.0"

function Set-AppveyorBuildVariable ($variableName, $variablevalue)
{
    Write-Host $variableName
    Write-Host $variablevalue
}

# Packaging
.\AppVeyor-Build.ps1