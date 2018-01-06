# ArchiveBuildResult.ps1

$solutionDirPath = Split-Path $PSScriptRoot -Parent
$srcBinDirPath = Join-Path $solutionDirPath "bin"

if (!(Test-Path -LiteralPath $srcBinDirPath)) {
    Write-Output "Directory not exists: $srcBinDirPath"
    return
}

$workDirPath = Join-Path $solutionDirPath '$(AppDir)'
$workPluginsDirPath = Join-Path $workDirPath "Plugins"

if (!(Test-Path -LiteralPath $workPluginsDirPath)) {
    New-Item -ItemType Directory $workPluginsDirPath
}

Copy-Item (Join-Path $srcBinDirPath "lpubsppop01.EmacsLikeKeyBindingsFDPlugin.dll") $workPluginsDirPath

$archiveFilename = ""
if ($env:FLASH_DEVELOP_SDK_VERSION -ne $null) {
    $archiveFilename = "lpubsppop01.EmacsLikeKeyBindingsFDPlugin_FD-${env:FLASH_DEVELOP_SDK_VERSION}.fdz"
} else {
    $archiveFilename = "lpubsppop01.EmacsLikeKeyBindingsFDPlugin.fdz"
}

$archiveFilePath = Join-Path $solutionDirPath $archiveFilename
if (Test-Path -LiteralPath $archiveFilePath) {
    Remove-Item $archiveFilePath
}
7z -tzip a $archiveFilePath $workDirPath

Remove-Item -Recurse $workDirPath
