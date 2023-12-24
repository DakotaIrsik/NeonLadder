$unusedAssetsPath = ".\unused_assets.txt"
$rootDirectory = ".\"

# Read each line in the unused assets file
$unusedAssets = Get-Content $unusedAssetsPath

foreach ($asset in $unusedAssets) {
    $fullPath = Join-Path -Path $rootDirectory -ChildPath $asset

    if (Test-Path $fullPath) {
        Remove-Item $fullPath -Force
        Write-Output "Deleted: $fullPath"
    } else {
        Write-Warning "File not found: $fullPath"
    }
}

Write-Output "Deletion of unused assets completed."
