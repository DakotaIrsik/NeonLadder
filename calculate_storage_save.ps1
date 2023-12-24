$unusedAssetsPath = ".\unused_assets.txt"
$rootDirectory = ".\"

# Initialize total size
$totalSize = 0

# Read each line in the unused assets file
$unusedAssets = Get-Content $unusedAssetsPath

foreach ($asset in $unusedAssets) {
    $fullPath = Join-Path -Path $rootDirectory -ChildPath $asset

    if (Test-Path $fullPath) {
        $fileSize = (Get-Item $fullPath).Length
        $totalSize += $fileSize
    } else {
        Write-Warning "File not found: $fullPath"
    }
}

# Function to format the size to a readable format
function Format-FileSize {
    param([long]$size)
    if ($size -lt 1KB) {
        return "${size} B"
    } elseif ($size -lt 1MB) {
        return ("{0:N2} KB" -f ($size / 1KB))
    } elseif ($size -lt 1GB) {
        return ("{0:N2} MB" -f ($size / 1MB))
    } else {
        return ("{0:N2} GB" -f ($size / 1GB))
    }
}

# Output the total size
$humanReadableSize = Format-FileSize -size $totalSize
Write-Output "Total size of unused assets: $humanReadableSize"
