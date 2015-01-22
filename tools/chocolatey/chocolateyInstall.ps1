try {
  $sysDrive = $env:SystemDrive
  $pretzelPath = "$sysDrive\tools\pretzel"
  
  Install-ChocolateyZipPackage 'Pretzel' 'https://github.com/Code52/Pretzel/releases/download/{{version}}/Pretzel.{{version}}.zip' $pretzelPath
  Install-ChocolateyPath $pretzelPath

  write-host 'pretzel has been installed.'
  Write-ChocolateySuccess 'pretzel'
} catch {
  Write-ChocolateyFailure 'pretzel' $($_.Exception.Message)
  throw 
}