$packageName = 'Pretzel.ScriptCs'
$url = 'https://github.com/Code52/Pretzel/releases/download/{{tag}}/Pretzel.ScriptCs.{{version}}.zip'

# It is a plugin to Pretzel and as such need to be installed in the same directory
$installDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)\..\..\pretzel\tools"

Install-ChocolateyZipPackage $packageName $url $installDir -checksum {{checksum}} -checksumType sha256