$packageName = 'Pretzel'
$url = 'https://github.com/Code52/Pretzel/releases/download/{{tag}}/Pretzel.{{version}}.zip'

try {
  $binRoot = Get-BinRoot
  $pretzelPath = "$binRoot\pretzel"
    
  Install-ChocolateyZipPackage "$packageName" "$url" $pretzelPath
  Install-ChocolateyPath $pretzelPath
  
  write-host '$packageName has been installed.'
  Write-ChocolateySuccess $packageName
} catch {
  Write-ChocolateyFailure '$packageName' $($_.Exception.Message)
  throw
}