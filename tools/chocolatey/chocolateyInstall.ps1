$packageName = 'Pretzel'
$url = 'https://github.com/Code52/Pretzel/releases/download/{{tag}}/Pretzel.{{version}}.zip'

Install-ChocolateyZipPackage "$packageName" "$url" "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"