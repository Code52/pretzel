Write-Debug "Uninstall Pretzel.ScriptCs"

$binRoot = Get-BinRoot
$pretzelPath = "$binRoot\Pretzel"

# Remove folder
If (Test-Path $pretzelPath){
    gc $pretzelPath\Pretzel.ScriptCs.Files.txt | foreach ($_) { If (($_) -And (Test-Path $pretzelPath\$_)) { Remove-Item $pretzelPath\$_ } }
    Remove-Item $pretzelPath\Pretzel.ScriptCs.Files.txt
}