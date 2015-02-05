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
			$description += "$line`r`n"
		  }
		}
	}
	finally {
		$reader.Close()
	}
		
	$env:releaseDescription = $description
}

# build nupkg

mkdir artifacts
mkdir chocoTemp\tools

move tools\chocolatey\pretzel.nuspec chocoTemp\pretzel.nuspec
move tools\chocolatey\chocolateyInstall.ps1 chocoTemp\tools\chocolateyInstall.ps1

(gc chocoTemp\tools\chocolateyInstall.ps1).replace('{{version}}',$version).replace('{{tag}}',$tag)|sc chocoTemp\tools\chocolateyInstall.ps1

nuget pack chocoTemp\pretzel.nuspec -OutputDirectory artifacts -Version $version -NoPackageAnalysis

# create zip

7z a Pretzel.$version.zip $env:appveyor_build_folder\src\Pretzel\bin\Release\Pretzel.exe*
7z a Pretzel.$version.zip ReleaseNotes.md