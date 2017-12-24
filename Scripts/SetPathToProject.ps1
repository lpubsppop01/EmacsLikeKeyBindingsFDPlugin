# SetFDPathToProject.ps1

# Check environment
$outputDirPath = ""
$pluginCoreDllPath = ""
$pluginCoreDllAbsPath = ""
$fdExePath = ""
if ($env:FLASH_DEVELOP_APP_ROOT -ne $null) {
    $outputDirPath = Join-Path $env:FLASH_DEVELOP_APP_ROOT "Plugins"
    $pluginCoreDllPath = $pluginCoreDllAbsPath = Join-Path $env:FLASH_DEVELOP_APP_ROOT "PluginCore.dll"
    $fdExePath = Join-Path $env:FLASH_DEVELOP_APP_ROOT "FlashDevelop.exe"
} else {
    $outputDirPath = '$(SolutionDir)bin'
    $pluginCoreDllPath = "..\SDK\FlashDevelop-5.2.0\PluginCore.dll"
    $pluginCoreDllAbsPath = Join-Path $PSScriptRoot "..\SDK\FlashDevelop-5.2.0\PluginCore.dll"
}
$ildasm = ""
if (Test-Path "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\ildasm.exe") {
    $ildasm = "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\ildasm.exe"
} elseif (Test-Path "C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\ildasm.exe") {
    $ildasm = "C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\ildasm.exe"
} elseif (Test-Path "C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\ildasm.exe") {
    $ildasm = "C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\ildasm.exe"
} else {
    Write-Output "Command not found: ildasm"
    return
}
$metadataVersion = (&$ildasm /text /noil /metadata=MDHEADER $pluginCoreDllPath `
    | Select-String "Metadata section: ").Line.Split(" ")[-1]
$targetFrameworkVersion = ""
if ($metadataVersion.StartsWith("v2.0.50727")) {
    $targetFrameworkVersion = "v3.5"
} elseif ($metadataVersion.StartsWith("v4.0")) {
    $targetFrameworkVersion = "v4.0"
} else {
    Write-Output ("Unexpected Metadata CLR version: " + $metadataVersion)
    return
}

# Edit csproj
$csprojPath = Join-Path $PSScriptRoot "..\EmacsLikeKeyBindingsFDPlugin\EmacsLikeKeyBindingsFDPlugin.csproj"
if (Test-Path $csprojPath) {
    $xml = [xml](Get-Content $csprojPath)
    $ns = new-object Xml.XmlNamespaceManager $xml.NameTable
    $ns.AddNamespace("msb", "http://schemas.microsoft.com/developer/msbuild/2003")
    $outputNodes = $xml.SelectNodes("//msb:OutputPath", $ns)
    foreach ($node in $outputNodes) {
        $node.InnerText = $outputDirPath
        Write-Output ("Output: " + $node.InnerText)
    }
    $hintPathNode = $xml.SelectSingleNode("//msb:Reference[@Include='PluginCore']/msb:HintPath", $ns)
    if ($hintPathNode -ne $null) {
        $hintPathNode.InnerText = $pluginCoreDllPath
        Write-Output ("HintPath: " + $hintPathNode.InnerText)
    }
    $targetFrameworkVersionNode = $xml.SelectSingleNode("/msb:Project/msb:PropertyGroup/msb:TargetFrameworkVersion", $ns)
    if ($targetFrameworkVersionNode -ne $null) {
        $targetFrameworkVersionNode.InnerText = $targetFrameworkVersion
        Write-Output ("TargetFrameworkVersion: " + $targetFrameworkVersionNode.InnerText)
    }
    $xml.Save($csprojPath)
}

# Edit csproj.user
$userPath = Join-Path $PSScriptRoot "..\EmacsLikeKeyBindingsFDPlugin\EmacsLikeKeyBindingsFDPlugin.csproj.user"
if (Test-Path $userPath) {
    $xml = [xml](Get-Content $userPath)
    $ns = new-object Xml.XmlNamespaceManager $xml.NameTable
    $ns.AddNamespace("msb", "http://schemas.microsoft.com/developer/msbuild/2003")
    $startProgramNode = $xml.SelectSingleNode("//msb:StartProgram", $ns)
    if ($startProgramNode -ne $null) {
        $startProgramNode.InnerText = $fdExePath
        Write-Output ("StartProgram: " + $startProgramNode.InnerText)
    }
    $xml.Save($userPath)
}