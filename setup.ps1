# setup.ps1
#
# Prepares training-csharp-and-onbase for Visual Studio (README.md, "Setup and requirements"):
#   0. Checks Git, and clones the repository into C:\Development\training\training-csharp-and-onbase if this script
#      isn't inside it. Then runs the cloned copy of this script, so every later step uses the repository's files.
#   1. Checks Visual Studio, with the .NET desktop development workload (ManagedDesktop).
#   2. Checks the .NET Framework 4.8 targeting pack, which the course's projects target.
#   3. Checks the .NET SDK is version 10 or later, which the LessonRunner and the .NET 10 samples need.
#   4. Checks the DataBank NuGet source is configured. The script doesn't store credentials.
#   5. Restores and builds LessonRunner and LessonRunner.Wpf, so the first run works.
#
# Optional pieces are reported as warnings, not stops: SQL Server with ExternalData.bak (database samples and the
# EForms navigator), and a live OnBase system (the Unity and REST tracks).
#
# With -InstallMissing, the script installs what's missing with winget: Git, Visual Studio Community with the .NET
# desktop workload, and the .NET 10 SDK. Installing needs an elevated (Administrator) PowerShell. Safe to run again.
#
# Run from anywhere with:
#   powershell -ExecutionPolicy Bypass -File .\setup.ps1
#   powershell -ExecutionPolicy Bypass -File .\setup.ps1 -InstallMissing

param(
    [switch]$InstallMissing,
    [string]$RepoUrl = "https://github.com/databankimx/training-csharp-and-onbase.git",
    [string]$Branch = "main"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
Set-Location $root

$defaultRepoPath = "C:\Development\training\training-csharp-and-onbase"
$gitDefault = "C:\Program Files\Git\cmd\git.exe"
$dotnetDefault = "C:\Program Files\dotnet\dotnet.exe"
$vsWingetId = "Microsoft.VisualStudio.Community"
$vsWorkload = "Microsoft.VisualStudio.Workload.ManagedDesktop"
$vsWebWorkload = "Microsoft.VisualStudio.Workload.NetWeb"
$dotnetSdkWingetId = "Microsoft.DotNet.SDK.10"
$requiredSdkMajor = 10
$summary = [ordered]@{}
$warnings = @()

function Write-Step([string]$title) {
    Write-Host ""
    Write-Host "== $title ==" -ForegroundColor Cyan
}

function Write-Note([string]$message) {
    Write-Host "   $message"
}

function Add-Warning([string]$message) {
    $script:warnings += $message
    Write-Host "   WARNING: $message" -ForegroundColor Yellow
}

function Stop-Setup([string]$message) {
    Write-Host ""
    Write-Host "SETUP STOPPED: $message" -ForegroundColor Red
    Write-Host "Fix the problem above, then run this script again. Steps already done are skipped."
    exit 1
}

function Test-Administrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Invoke-Winget([string]$id, [string]$override = "") {
    if (-not (Get-Command winget -ErrorAction SilentlyContinue)) {
        Stop-Setup "winget is not available. Install 'App Installer' from the Microsoft Store, or install $id by hand."
    }
    Write-Note "winget install -e --id $id"
    if ($override) {
        & winget install -e --id $id --silent --accept-package-agreements --accept-source-agreements --override $override
    } else {
        & winget install -e --id $id --silent --accept-package-agreements --accept-source-agreements
    }
    # winget exit code -1978335189 means the package is already installed, which is fine here.
    # 3010 means the installer succeeded but asks for a restart, such as Visual Studio's, so it's a warning.
    if ($LASTEXITCODE -eq 3010) {
        Add-Warning "$id installed, but Windows needs a restart before it's fully ready. Restart, then run this script again."
        return
    }
    if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne -1978335189) {
        Stop-Setup "winget could not install $id (exit code $LASTEXITCODE)."
    }
}

# Returns the folder of a Visual Studio installation that has the .NET desktop workload, or $null.
function Find-VisualStudioWithWorkload {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (-not (Test-Path $vswhere)) { return $null }
    $found = & $vswhere -products * -requires $vsWorkload -property installationPath
    if ($found) { return ($found | Select-Object -First 1) }
    return $null
}

# Returns the path to Visual Studio, found with vswhere, that has the given workload, or $null.
function Find-VisualStudioWithWorkloadId([string]$workload) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (-not (Test-Path $vswhere)) { return $null }
    $found = & $vswhere -products * -requires $workload -property installationPath
    if ($found) { return ($found | Select-Object -First 1) }
    return $null
}

# Adds a workload to an existing Visual Studio installation, using the Visual Studio Installer.
function Add-VisualStudioWorkload([string]$installPath, [string]$workload) {
    $setup = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\setup.exe"
    if (-not (Test-Path $setup)) {
        Stop-Setup "The Visual Studio Installer wasn't found at $setup. Add the workload with the Visual Studio Installer by hand."
    }
    Write-Note "Adding $workload to Visual Studio. This can take a long time."
    & $setup modify --installPath $installPath --add $workload --includeRecommended --quiet --wait --norestart
    if ($LASTEXITCODE -eq 3010) {
        Add-Warning "Visual Studio added $workload, but Windows needs a restart. Restart, then run this script again."
    } elseif ($LASTEXITCODE -ne 0) {
        Stop-Setup "Visual Studio couldn't add $workload (exit code $LASTEXITCODE)."
    }
}

# Returns the path to dotnet.exe, found on PATH or in its default folder, or $null.
function Find-Dotnet {
    $command = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }
    if (Test-Path $dotnetDefault) { return $dotnetDefault }
    return $null
}

Write-Host ""
Write-Host "training-csharp-and-onbase setup" -ForegroundColor Yellow

if ($InstallMissing -and -not (Test-Administrator)) {
    Stop-Setup "-InstallMissing needs an elevated PowerShell. Open PowerShell as Administrator, and run the script again."
}

# 0. Git and the repository ------------------------------------------------------------------------------

Write-Step "0. Git and the repository"

$gitExe = $gitDefault
if (-not (Test-Path $gitExe)) {
    $gitCommand = Get-Command git -ErrorAction SilentlyContinue
    if ($gitCommand) { $gitExe = $gitCommand.Source }
}
if (-not (Test-Path $gitExe)) {
    if ($InstallMissing) {
        Write-Note "Git is missing: installing it"
        Invoke-Winget "Git.Git"
        $gitExe = $gitDefault
    } else {
        Stop-Setup "Git isn't installed. Run this script with -InstallMissing, or install Git from https://git-scm.com."
    }
}
Write-Note "Git: $gitExe"
$summary["Git"] = "ok"

if (-not (Test-Path (Join-Path $root "DataBank.DeveloperTraining.sln"))) {
    # The script is running outside the repository. Clone it, or use the existing clone, then run its copy.
    if (Test-Path (Join-Path $defaultRepoPath "DataBank.DeveloperTraining.sln")) {
        Write-Note "Using the repository at $defaultRepoPath"
        $repoRoot = $defaultRepoPath
    } else {
        Write-Note "Cloning $RepoUrl ($Branch) into $defaultRepoPath"
        New-Item -ItemType Directory -Force -Path (Split-Path $defaultRepoPath -Parent) | Out-Null
        & $gitExe clone --branch $Branch $RepoUrl $defaultRepoPath
        if ($LASTEXITCODE -ne 0) {
            Stop-Setup "git clone failed (exit code $LASTEXITCODE). Check the address, and your access to the repository."
        }
        $repoRoot = $defaultRepoPath
    }
    $childScript = Join-Path $repoRoot "setup.ps1"
    if (Test-Path $childScript) {
        # The repository has its own copy of this script, so run that copy.
        $childParams = @{}
        if ($InstallMissing) { $childParams["InstallMissing"] = $true }
        & $childScript @childParams
        exit $LASTEXITCODE
    }
    # The repository doesn't have this script yet, so carry on here, using the clone's files.
    Write-Note "The repository has no setup.ps1 yet, so this copy continues the setup."
    $root = $repoRoot
    Set-Location $root
}
$summary["Repository"] = $root

# 1. Visual Studio ---------------------------------------------------------------------------------------

Write-Step "1. Visual Studio with the .NET desktop development workload"

$vsPath = Find-VisualStudioWithWorkload
if (-not $vsPath -and $InstallMissing) {
    Write-Note "Visual Studio with the .NET desktop workload is missing: installing Visual Studio Community."
    Write-Note "This is a large download, and it can take a long time. Don't close the window until it finishes."
    $override = "--quiet --wait --norestart --add $vsWorkload --add $vsWebWorkload --includeRecommended"
    Invoke-Winget $vsWingetId $override
    $vsPath = Find-VisualStudioWithWorkload
}
if (-not $vsPath) {
    Stop-Setup ("Visual Studio with the .NET desktop workload isn't installed. Run this script with -InstallMissing, or " +
        "install Visual Studio and tick '.NET desktop development' in the Visual Studio Installer.")
}
$webPath = Find-VisualStudioWithWorkloadId $vsWebWorkload
if (-not $webPath) {
    if ($InstallMissing) {
        Write-Note "The ASP.NET and web development workload is missing."
        Add-VisualStudioWorkload $vsPath $vsWebWorkload
        $webPath = Find-VisualStudioWithWorkloadId $vsWebWorkload
    }
    if (-not $webPath) {
        Stop-Setup ("The ASP.NET and web development workload isn't installed. The legacy web projects need it, and restore fails without it. " +
            "Run this script with -InstallMissing, or tick 'ASP.NET and web development' in the Visual Studio Installer.")
    }
}
Write-Note "ASP.NET and web development workload: installed"
Write-Note "Visual Studio: $vsPath"
$summary["Visual Studio"] = "ok"

# 2. .NET Framework 4.8 targeting pack -------------------------------------------------------------------

Write-Step "2. .NET Framework 4.8 targeting pack"
$targetingPack = Join-Path ${env:ProgramFiles(x86)} "Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8"
if (Test-Path $targetingPack) {
    Write-Note ".NET Framework 4.8 targeting pack is installed."
    $summary[".NET Framework 4.8"] = "ok"
} else {
    Add-Warning "The .NET Framework 4.8 targeting pack wasn't found. In the Visual Studio Installer, open Modify, and tick '.NET Framework 4.8 development tools' under Individual components."
    $summary[".NET Framework 4.8"] = "missing"
}

# 3. .NET SDK -------------------------------------------------------------------------------------------

Write-Step ".NET SDK $requiredSdkMajor or later"

$dotnetExe = Find-Dotnet
$sdkMajor = $null
if ($dotnetExe) {
    $sdks = & $dotnetExe --list-sdks 2>$null
    foreach ($line in $sdks) {
        if ($line -match "^(\d+)\.") {
            $major = [int]$Matches[1]
            if ($null -eq $sdkMajor -or $major -gt $sdkMajor) { $sdkMajor = $major }
        }
    }
}
if (-not $sdkMajor -or $sdkMajor -lt $requiredSdkMajor) {
    if ($InstallMissing) {
        Write-Note "The .NET SDK $requiredSdkMajor or later is missing: installing it."
        Invoke-Winget $dotnetSdkWingetId
        $dotnetExe = Find-Dotnet
        $sdkMajor = $null
        if ($dotnetExe) {
            foreach ($line in (& $dotnetExe --list-sdks 2>$null)) {
                if ($line -match "^(\d+)\.") { $sdkMajor = [int]$Matches[1] }
            }
        }
    }
}
if (-not $sdkMajor -or $sdkMajor -lt $requiredSdkMajor) {
    Stop-Setup ("The .NET SDK $requiredSdkMajor or later isn't installed. Run this script with -InstallMissing, or install it " +
        "from https://dotnet.microsoft.com/download with: winget install -e --id $dotnetSdkWingetId")
}
Write-Note ".NET SDK $sdkMajor found at $dotnetExe"
$summary[".NET SDK"] = "$sdkMajor"

# 4. DataBank NuGet source -------------------------------------------------------------------------------

Write-Step "4. DataBank NuGet source"
$sources = & $dotnetExe nuget list source 2>$null | Out-String
if ($sources -match "DataBank") {
    Write-Note "A DataBank NuGet source is configured."
    $summary["DataBank NuGet source"] = "ok"
} else {
    Stop-Setup ("No DataBank NuGet source is configured. Hyland.Unity and the internal DataBank packages come from the " +
        "DataBank GitHub feed, so restore will fail. Add the feed to your user NuGet.config with your own credentials " +
        "(see the README, 'NuGet packages'). This script doesn't store credentials.")
}

# 5. Restore and build the lesson runner -----------------------------------------------------------------

Write-Step "5. Restore and build the lesson runner"

# The legacy web projects import Visual Studio's own web targets, which only Visual Studio's MSBuild can find.
# Restore the whole solution with MSBuild first. dotnet restore can't evaluate those projects.
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
$msbuildExe = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
if (-not $msbuildExe) {
    Stop-Setup "MSBuild wasn't found in the Visual Studio installation. Check that Visual Studio is complete, then run this script again."
}
Write-Note "Restoring the solution with $msbuildExe"
& $msbuildExe (Join-Path $root "DataBank.DeveloperTraining.sln") "/t:Restore" "/v:minimal" "/nologo"
if ($LASTEXITCODE -ne 0) {
    Stop-Setup "The solution restore failed. Check the messages above. Usual causes: the DataBank NuGet source needs credentials, or a package is missing from the feed."
}
$summary["Solution restore"] = "ok (MSBuild)"

foreach ($project in @("LessonRunner", "LessonRunner.Wpf")) {
    Write-Note "dotnet build $project"
    & $dotnetExe build (Join-Path $root $project)
    if ($LASTEXITCODE -ne 0) {
        Stop-Setup "The build of $project failed. Check the messages above."
    }
}
$summary["Lesson runner"] = "built (LessonRunner, LessonRunner.Wpf)"

# 6. Optional pieces -------------------------------------------------------------------------------------

Write-Step "6. Optional pieces"
$sqlServer = Get-Service -Name "MSSQL*" -ErrorAction SilentlyContinue | Where-Object { $_.Status -eq "Running" }
if ($sqlServer) {
    Write-Note "A SQL Server instance is running. Restore Resources\ExternalData.bak if the database samples need it."
    $summary["SQL Server"] = "running"
} else {
    Add-Warning "No running SQL Server was found. The database samples and the EForms navigator need one, with ExternalData.bak restored. The core lessons don't."
    $summary["SQL Server"] = "not running (optional)"
}
Add-Warning "The Unity and REST API tracks need a live OnBase system. Those projects are reference only without one."

# Summary ------------------------------------------------------------------------------------------------

Write-Step "Summary"
foreach ($item in $summary.GetEnumerator()) {
    Write-Host ("   {0,-28} {1}" -f $item.Key, $item.Value)
}
if ($warnings.Count -gt 0) {
    Write-Host ""
    Write-Host "Things to know:" -ForegroundColor Yellow
    foreach ($warning in $warnings) {
        Write-Host "   - $warning"
    }
}
Write-Host ""
Write-Host "Setup finished. To start the lessons:" -ForegroundColor Green
Write-Host "   1. Open DataBank.DeveloperTraining.sln in Visual Studio."
Write-Host "   2. Right-click LessonRunner.Wpf, choose Set as Startup Project, then press F5."
Write-Host ""
