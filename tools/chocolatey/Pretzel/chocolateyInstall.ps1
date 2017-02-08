$packageName = 'Pretzel'
$url = 'https://github.com/Code52/Pretzel/releases/download/{{tag}}/Pretzel.{{version}}.zip'
$installDir = $(Split-Path -parent $MyInvocation.MyCommand.Definition)

Install-ChocolateyZipPackage $packageName $url $installDir -checksum {{checksum}} -checksumType sha256