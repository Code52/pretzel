$packageName = 'Pretzel.ScriptCs'
$url = 'https://github.com/Code52/Pretzel/releases/download/{{tag}}/Pretzel.ScriptCs.{{version}}.zip'

try {
  $binRoot = Get-BinRoot
  $pretzelPath = "$binRoot\pretzel"
    
  Install-ChocolateyZipPackage "$packageName" "$url" $pretzelPath
  
  write-host '$packageName has been installed.'
  Write-ChocolateySuccess $packageName
} catch {
  Write-ChocolateyFailure '$packageName' $($_.Exception.Message)
  throw
}