try {
  $sysDrive = $env:SystemDrive
  $pretzelPath = "$sysDrive\tools\pretzel"
  
  # Remove old folder
  If (Test-Path $pretzelPath){
    Remove-Item $pretzelPath
  }
  
  # Remove old path
  
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
  Write-ChocolateyFailure 'Old Pretzel uninstallation' $($_.Exception.Message)
  throw 
}

$packageName = 'Pretzel'
$url = 'https://github.com/Code52/Pretzel/releases/download/{{tag}}/Pretzel.{{version}}.zip'

Install-ChocolateyZipPackage "$packageName" "$url" "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"