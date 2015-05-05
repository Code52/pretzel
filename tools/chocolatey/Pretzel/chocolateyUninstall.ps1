Write-Debug "Uninstall Pretzel"

$binRoot = Get-BinRoot
$installDir = Join-Path $binRoot "Pretzel"

# Remove folder
If (Test-Path $installDir){
    Remove-Item $installDir -Recurse
}

# Remove path

#get the PATH variable
$envPath = $env:PATH
$pathType = [System.EnvironmentVariableTarget]::User

if ($envPath.ToLower().Contains($installDir.ToLower()))
{
    $statementTerminator = ";"
    Write-Debug "PATH environment variable contains old pretzel path $installDir. Removing..."
    $actualPath = [System.Collections.ArrayList](Get-EnvironmentVariable -Name 'Path' -Scope $pathType).split($statementTerminator)

    $actualPath.Remove($installDir)
    $newPath = $actualPath -Join $statementTerminator

    Set-EnvironmentVariable -Name 'Path' -Value $newPath -Scope $pathType

} else {
    Write-Debug " The path to uninstall `'$installDir`' was not found in the `'$pathType`' PATH. Could not remove."
}