$packageName = 'Pretzel'

try {
  $binRoot = Get-BinRoot
  $pretzelPath = "$binRoot\pretzel"
   
  # Remove folder
  If (Test-Path $pretzelPath){
    Remove-Item $pretzelPath
  }
   
  # Remove path
   
  #get the PATH variable
  $envPath = $env:PATH
  $pathType = [System.EnvironmentVariableTarget]::User
   
  if ($envPath.ToLower().Contains($pretzelPath.ToLower()))
  {
    $statementTerminator = ";"
    Write-Host "PATH environment variable contains old pretzel path $pretzelPath. Removing..."
    $actualPath = [System.Collections.ArrayList](Get-EnvironmentVariable -Name 'Path' -Scope $pathType).split($statementTerminator)
 
    $actualPath.Remove($pretzelPath)
    $newPath = $actualPath -Join $statementTerminator
 
    Set-EnvironmentVariable -Name 'Path' -Value $newPath -Scope $pathType
     
  } else {
    Write-Debug " The path to uninstall `'$pretzelPath`' was not found in the `'$pathType`' PATH. Could not remove."
  }
   
} catch {
  Write-ChocolateyFailure '$packageName uninstallation' $($_.Exception.Message)
  throw 
}