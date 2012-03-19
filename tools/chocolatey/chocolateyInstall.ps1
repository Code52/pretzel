try {
  $sysDrive = $env:SystemDrive
  $pretzelPath = "$sysDrive\tools\pretzel"
  
  Install-ChocolateyZipPackage 'Pretzel' 'http://deploy.code52.org/pretzel/release/Pretzel-v{{version}}.zip' $pretzelPath
  Install-ChocolateyPath $pretzelPath

  write-host 'pretzel has been installed.'
  Write-ChocolateySuccess 'pretzel'
} catch {
  Write-ChocolateyFailure 'pretzel' $($_.Exception.Message)
  throw 
}