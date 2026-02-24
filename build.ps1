[CmdletBinding()]
param(
    [ValidateSet("Build", "CleanBuild")]
    [string]$Action = "Build",

    [string]$Configuration = "Debug",

    [string]$Platform = "Mixed Platforms",

    [string]$Solution = "CrazyStorm2.0.sln",

    [int]$MaxCpuCount = 0
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-MsBuild {
    $cmd = Get-Command msbuild -ErrorAction SilentlyContinue
    if ($cmd) {
        return $cmd.Source
    }

    throw "msbuild was not found in PATH. Open a Developer PowerShell / VS Developer Command Prompt, or install Visual Studio Build Tools."
}

Push-Location $PSScriptRoot
try {
    if (-not (Test-Path -LiteralPath $Solution)) {
        throw "Solution file not found: $Solution"
    }

    $msbuild = Resolve-MsBuild
    $targets = if ($Action -eq "CleanBuild") { "Clean;Build" } else { "Build" }

    $args = @(
        $Solution
        "/t:$targets"
        "/p:Configuration=$Configuration"
        "/p:Platform=$Platform"
        "/nologo"
    )

    if ($MaxCpuCount -gt 0) {
        $args += "/m:$MaxCpuCount"
    }
    else {
        $args += "/m"
    }

    Write-Host "msbuild: $msbuild"
    Write-Host "solution: $Solution"
    Write-Host "action: $Action"
    Write-Host "configuration: $Configuration"
    Write-Host "platform: $Platform"
    Write-Host ""

    & $msbuild @args
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}
finally {
    Pop-Location
}
