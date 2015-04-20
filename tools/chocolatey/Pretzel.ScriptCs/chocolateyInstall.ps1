$packageName = 'Pretzel.ScriptCs'
$url = 'https://github.com/Code52/Pretzel/releases/download/{{tag}}/Pretzel.ScriptCs.{{version}}.zip'

$binRoot = Get-BinRoot
$pretzelPath = "$binRoot\Pretzel"

Install-ChocolateyZipPackage "$packageName" "$url" $pretzelPath