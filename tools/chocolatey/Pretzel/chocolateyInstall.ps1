$packageName = 'Pretzel'
$url = 'https://github.com/Code52/Pretzel/releases/download/{{tag}}/Pretzel.{{version}}.zip'

$binRoot = Get-BinRoot
$pretzelPath = "$binRoot\$packageName"

Install-ChocolateyZipPackage "$packageName" "$url" $pretzelPath -checksum {{checksum}} -checksumType sha256
Install-ChocolateyPath $pretzelPath