Write-Debug "Uninstall Pretzel.ScriptCs"

$installDir = $(Split-Path -parent $MyInvocation.MyCommand.Definition)
$pretzelPath = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)\..\..\pretzel\tools"

# Remove folder
If (Test-Path $pretzelPath){
    gc $installDir\..\Pretzel.ScriptCs.{{version}}.zip.txt | foreach ($_) { If (($_) -And (Test-Path $_)) { Remove-Item $_ } }
}