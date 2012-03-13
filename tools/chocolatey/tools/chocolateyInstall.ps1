try {
  $sysDrive = $env:SystemDrive
  $pretzelPath = "$sysDrive\tools\pretzel"
  
  Install-ChocolateyZipPackage 'Pretzel' 'https://github.com/downloads/Code52/pretzel/Pretzel-0.1.0.zip' $pretzelPath
  Install-ChocolateyPath $pretzelPath

  write-host 'pretzel has been installed.'
  Write-ChocolateySuccess 'pretzel'
} catch {
  Write-ChocolateyFailure 'pretzel' $($_.Exception.Message)
  throw 
}