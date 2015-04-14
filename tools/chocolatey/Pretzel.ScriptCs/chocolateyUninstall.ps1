$packageName = 'Pretzel.ScriptCs'

try {
  $binRoot = Get-BinRoot
  $pretzelPath = "$binRoot\pretzel"
   
  # Remove folder
  If (Test-Path $pretzelPath){
    gc $pretzelPath\Pretzel.ScriptCs.Files.txt | foreach ($_) { If (($_) -And (Test-Path $pretzelPath\$_)) { Remove-Item $pretzelPath\$_ } }
    Remove-Item $pretzelPath\Pretzel.ScriptCs.Files.txt
  }
   
} catch {
  Write-ChocolateyFailure '$packageName uninstallation' $($_.Exception.Message)
  throw 
}