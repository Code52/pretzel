$executingScriptDirectory = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
# define version

$version = $env:GitVersion_ClassicVersion
$tag = $version
If ($env:appveyor_repo_tag -eq $True)
{
	$tag = $env:appveyor_repo_tag_name
	
	$reader = [System.IO.File]::OpenText("$executingScriptDirectory/ReleaseNotes.md")
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
		
	Set-AppveyorBuildVariable 'releaseDescription' $description
}

mkdir artifacts

# build Pretzel nupkg
mkdir chocoTemp\Pretzel\tools

move tools\chocolatey\Pretzel\pretzel.nuspec chocoTemp\Pretzel\pretzel.nuspec
move tools\chocolatey\Pretzel\chocolateyInstall.ps1 chocoTemp\Pretzel\tools\chocolateyInstall.ps1
move tools\chocolatey\Pretzel\chocolateyUninstall.ps1 chocoTemp\Pretzel\tools\chocolateyUninstall.ps1

(gc chocoTemp\Pretzel\tools\chocolateyInstall.ps1).replace('{{version}}',$version).replace('{{tag}}',$tag)|sc chocoTemp\Pretzel\tools\chocolateyInstall.ps1

nuget pack chocoTemp\Pretzel\pretzel.nuspec -OutputDirectory artifacts -Version $version -NoPackageAnalysis

# create Pretzel zip
7z a Pretzel.$version.zip $env:appveyor_build_folder\src\Pretzel\bin\Release\Pretzel.exe*
7z a Pretzel.$version.zip ReleaseNotes.md

# build Pretzel.ScriptCs nupkg
mkdir chocoTemp\Pretzel.ScriptCs\tools
move tools\chocolatey\Pretzel.ScriptCs\pretzel.scriptcs.nuspec chocoTemp\Pretzel.ScriptCs\pretzel.scriptcs.nuspec
move tools\chocolatey\Pretzel.ScriptCs\chocolateyInstall.ps1 chocoTemp\Pretzel.ScriptCs\tools\chocolateyInstall.ps1
move tools\chocolatey\Pretzel.ScriptCs\chocolateyUninstall.ps1 chocoTemp\Pretzel.ScriptCs\tools\chocolateyUninstall.ps1
(gc chocoTemp\Pretzel.ScriptCs\tools\chocolateyInstall.ps1).replace('{{version}}',$version).replace('{{tag}}',$tag)|sc chocoTemp\Pretzel.ScriptCs\tools\chocolateyInstall.ps1
nuget pack chocoTemp\Pretzel.ScriptCs\pretzel.scriptcs.nuspec -OutputDirectory artifacts -Version $version -NoPackageAnalysis

# create Pretzel.ScriptCs zip
get-childitem src\Pretzel.ScriptCs\bin\Release -filter *.dll | % { $_.Name } | out-file artifacts\Pretzel.ScriptCs.Files.txt

7z a Pretzel.ScriptCs.$version.zip $env:appveyor_build_folder\src\Pretzel.ScriptCs\bin\Release\*.dll
7z a Pretzel.ScriptCs.$version.zip $env:appveyor_build_folder\artifacts\Pretzel.ScriptCs.Files.txt