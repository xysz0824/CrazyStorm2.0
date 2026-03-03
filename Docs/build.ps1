param(
    [ValidateSet("zh-cn", "en", "all")]
    [string]$Language = "all",
    [switch]$Clean,
    [switch]$CleanOnly,
    [switch]$InstallSphinx
)

$ErrorActionPreference = "Stop"

$docsRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$sourceRoot = Join-Path $docsRoot "source"

$targets = @($Language)
if ($Language -eq "all") {
    $targets = @("zh-cn", "en")
}

if ($CleanOnly -and $InstallSphinx) {
    throw "Do not use -CleanOnly with -InstallSphinx."
}

if ($CleanOnly) {
    foreach ($lang in $targets) {
        $langOutput = Join-Path $docsRoot $lang
        if (Test-Path $langOutput) {
            Remove-Item $langOutput -Recurse -Force
            Write-Host "Removed output folder: $langOutput"
        }
        else {
            Write-Host "Skip ${lang}: output folder not found: $langOutput"
        }
    }

    $rootIndex = Join-Path $docsRoot "index.html"
    if (Test-Path $rootIndex) {
        Remove-Item $rootIndex -Force
        Write-Host "Removed entry page: $rootIndex"
    }
    else {
        Write-Host "Skip entry page: not found: $rootIndex"
    }

    Write-Host "Clean only finished."
    return
}

function Resolve-Python {
    foreach ($name in @("py", "python")) {
        $cmd = Get-Command $name -ErrorAction SilentlyContinue
        if ($cmd) {
            return $cmd.Source
        }
    }
    return $null
}

function Resolve-SphinxRunner {
    $sphinxBuild = Get-Command sphinx-build -ErrorAction SilentlyContinue
    if ($sphinxBuild) {
        return @{
            Command = $sphinxBuild.Source
            Prefix  = @()
        }
    }

    $python = Resolve-Python
    if ($python) {
        & $python -m sphinx --version *> $null
        if ($LASTEXITCODE -eq 0) {
            return @{
                Command = $python
                Prefix  = @("-m", "sphinx")
            }
        }
    }

    if (-not $InstallSphinx) {
        throw "Sphinx was not found. Re-run with -InstallSphinx, or install it manually via 'python -m pip install sphinx'."
    }

    if (-not $python) {
        throw "Python was not found. Install Python first, then re-run with -InstallSphinx."
    }

    Write-Host "Installing Sphinx..."
    & $python -m pip install --user sphinx
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to install Sphinx."
    }

    & $python -m sphinx --version *> $null
    if ($LASTEXITCODE -ne 0) {
        throw "Sphinx still unavailable after install."
    }

    return @{
        Command = $python
        Prefix  = @("-m", "sphinx")
    }
}

function Ensure-PythonPackage {
    param(
        [string]$PythonCommand,
        [string]$ModuleName,
        [string]$PackageName
    )

    if (-not $PythonCommand) {
        throw "Python was not found. Install Python first."
    }

    & $PythonCommand -c "import importlib.util,sys;sys.exit(0 if importlib.util.find_spec('$ModuleName') else 1)" *> $null
    if ($LASTEXITCODE -eq 0) {
        return
    }

    Write-Host "Installing package: $PackageName..."
    & $PythonCommand -m pip install --user $PackageName
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to install required package: $PackageName"
    }

    & $PythonCommand -c "import importlib.util,sys;sys.exit(0 if importlib.util.find_spec('$ModuleName') else 1)" *> $null
    if ($LASTEXITCODE -ne 0) {
        throw "Package '$PackageName' is still unavailable after installation."
    }
}

$sphinxRunner = Resolve-SphinxRunner
$pythonCmd = Resolve-Python
$builtLanguageCount = 0

foreach ($lang in $targets) {
    $langSource = Join-Path $sourceRoot $lang
    if (-not (Test-Path $langSource)) {
        Write-Host "Skip ${lang}: source folder not found: $langSource"
        continue
    }

    $buildLanguage = $lang
    $searchLanguage = "en"
    switch ($lang) {
        "zh-cn" {
            $buildLanguage = "zh_CN"
            $searchLanguage = "zh"
            Ensure-PythonPackage -PythonCommand $pythonCmd -ModuleName "jieba" -PackageName "jieba"
        }
        "en" {
            $buildLanguage = "en"
            $searchLanguage = "en"
        }
    }

    $langOutput = Join-Path $docsRoot $lang
    if ($Clean -and (Test-Path $langOutput)) {
        Remove-Item $langOutput -Recurse -Force
    }

    Write-Host "Building docs for $lang..."
    $args = @()
    $args += $sphinxRunner.Prefix
    $args += @("-b", "html", "-D", "language=$buildLanguage", "-D", "html_search_language=$searchLanguage", "-c", $sourceRoot, $langSource, $langOutput)
    & $sphinxRunner.Command @args
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed for language: $lang"
    }

    $builtLanguageCount++
}

if ($builtLanguageCount -eq 0) {
    $rootIndex = Join-Path $docsRoot "index.html"
    if (Test-Path $rootIndex) {
        Remove-Item $rootIndex -Force
        Write-Host "Removed entry page: $rootIndex"
    }

    Write-Host "Docs build finished. No languages were built."
    return
}

Write-Host "Docs build finished."
