<#
.SYNOPSIS
    Sets up a DataBank IMX training repository. The same script is used in every training repository.

.DESCRIPTION
    Reads setup.json next to this script, then:

      1. Installs the programs listed under "installs". A program is installed when it's missing, upgraded when
         it's older than its "pinned" version, or installed alongside the old one when "sideBySide" is true. A failed
         upgrade is a warning and the script carries on. A failed install of a missing program is a failure, and
         any step that "requires" it is skipped. Git is always installed when it's missing, because the repository
         can't be cloned without it. -SkipInstall doesn't skip Git.
      2. Runs the "steps" in order. A step that "requires" a program that failed is skipped.
      3. Runs the steps marked "runTests": true, unless -SkipTests is given or "runTests" is false. Only the failing
         lines are shown. The full output goes to the log file.
      4. Starts the lesson runner, unless -NoLaunch is given or "launch" is missing from setup.json.

    Everything is written to a log file in <base>\logs. Failures and warnings are listed at the end. The exit code
    is 1 when anything failed.

    When setup.json is missing, the script was downloaded on its own. It clones the repository from -RepoUrl, and
    then runs the clone's setup.ps1 with the same base directory. load-tutorial.ps1 does the same thing.

    Step types: npm (command: ci or install), npm-run (script, for example test), python-venv (path, requirements),
    command (folder, exe, args, optional check, optional pathPrepend), copy (source, dest, backup),
    msbuild-restore (solution), write-file (path, content or contentFrom, optional ifMissing), dotnet-build (projects),
    download (url, path, and optional extract "zip" with folder), verify-hashes (manifest: checks the files in the
    manifest's folder against its SHA-256 list, and fails on a missing, changed, or unlisted file).
    The launch section takes kind "command" (a long-running program, such as cmd.exe running npm run tauri dev, started
    in the background with "exe", "args" and "folder"), kind "electron" (the runner's Electron app in folder, optionally opening "main", a script
    path such as lesson-runner/main.js, instead of the folder itself) or "exe" (a built program at file).

    Every step may set: name, type, folder, enabled, requires (install names), runTests, elevated.
    A step or install that needs an elevated PowerShell makes the script stop before anything is installed.

.PARAMETER BaseDirectory
    Folder for the logs and any repository this script clones. Overrides "baseDirectory" in setup.json.
    Default: C:\Development\training

.PARAMETER RepoUrl
    Repository to clone. Only used when setup.json is missing.

.PARAMETER SkipInstall
    Skips every install except Git.

.PARAMETER SkipTests
    Skips the steps marked "runTests".

.PARAMETER NoLaunch
    Doesn't start the lesson runner at the end.

.PARAMETER RelaunchedInPowerShell7
    Internal. Set when this script restarts itself in PowerShell 7 after PowerShell 7 is installed.
#>
[CmdletBinding()]
param(
    [string]$BaseDirectory,
    [string]$RepoUrl,
    [switch]$SkipInstall,
    [switch]$SkipTests,
    [switch]$NoLaunch,
    [switch]$RelaunchedInPowerShell7
)

# Continue, not Stop: native programs write to stderr, and every failure is checked explicitly.
$ErrorActionPreference = "Continue"
# Node and npm print UTF-8 (check marks, arrows). Reading them as the console's default code page garbles the log.
try {
    [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
    $OutputEncoding = [System.Text.Encoding]::UTF8
} catch {
    # Keep the default encoding when the console doesn't allow a change.
}

$DefaultBaseDirectory = "C:\Development\training"
$WingetSuccessCodes = @(0, 3010, -1978335189)   # 3010 = restart required, -1978335189 = already installed
$WingetFallback = "Install 'App Installer' from the Microsoft Store, then run this script again. QUICKSTART.md lists manual setup options."
# Used when setup.json doesn't give a "winget" id for a program.
$DefaultWinget = @{
    "git"         = "Git.Git"
    "powershell7" = "Microsoft.PowerShell"
    "node"        = "OpenJS.NodeJS.LTS"
    "python"      = "Python.Python.3.12"
    "dotnet"      = "Microsoft.DotNet.SDK.10"
    "jdk"         = "EclipseAdoptium.Temurin.25.JDK"
    "vscode"      = "Microsoft.VisualStudioCode"
    "notepadPlusPlus" = "Notepad++.Notepad++"
}

$script:Config = $null
$script:BaseDir = $null
$script:Root = $PSScriptRoot
$script:LogFile = $null
$script:LogBuffer = New-Object System.Collections.ArrayList
$script:Status = @{}
$script:Problems = New-Object System.Collections.ArrayList
$script:Outcomes = New-Object System.Collections.ArrayList
$script:RunTestsEffective = $false
$script:InstallPaths = @{}

# ---------------------------------------------------------------------------------------------------------------------
# Output and logging
# ---------------------------------------------------------------------------------------------------------------------

function Write-LogOnly([string]$Message) {
    $line = "{0}  {1}" -f (Get-Date -Format "HH:mm:ss"), $Message
    if ($script:LogFile) {
        Add-Content -LiteralPath $script:LogFile -Value $line -Encoding UTF8
    } else {
        [void]$script:LogBuffer.Add($line)
    }
}

function Write-Log([string]$Message, [string]$Color = "Gray") {
    Write-Host $Message -ForegroundColor $Color
    Write-LogOnly $Message
}

function Set-LogFile([string]$Path) {
    $dir = Split-Path -Parent $Path
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    $script:LogFile = $Path
    foreach ($line in $script:LogBuffer) { Add-Content -LiteralPath $Path -Value $line -Encoding UTF8 }
    $script:LogBuffer.Clear()
}

function Write-Stage([string]$Title) {
    Write-Log ""
    Write-Log $Title "Yellow"
}

# Records the result of an install or step. Only failures and warnings are printed. Everything goes to the log.
function Set-Outcome([string]$Name, [string]$Status, [string]$Detail = "") {
    $script:Status[$Name] = $Status
    [void]$script:Outcomes.Add([pscustomobject]@{ Name = $Name; Status = $Status; Detail = $Detail })
    Write-LogOnly ("{0}: {1} {2}" -f $Name, $Status, $Detail)
    if ($Status -eq "failed" -or $Status -eq "warning") {
        [void]$script:Problems.Add([pscustomobject]@{ Name = $Name; Status = $Status; Detail = $Detail })
        # Printed once, in the summary at the end, so nothing appears twice.
    } else {
        $color = "Green"
        if ($Status -eq "skipped") { $color = "DarkGray" }
        Write-Host ("   {0}: {1} {2}" -f $Name, $Status, $Detail).TrimEnd() -ForegroundColor $color
    }
}

# ---------------------------------------------------------------------------------------------------------------------
# Small helpers
# ---------------------------------------------------------------------------------------------------------------------

# Reads a property from a parsed JSON object, with a default when it's missing or null.
# Returns a property, or $Default. Lists come back as plain values, so callers wrap them in @(...) to get an array.
# Don't add a leading comma here: it makes @(Get-Prop ...) hold the whole list as one item.
function Get-Prop($Object, [string]$Name, $Default = $null) {
    if ($null -eq $Object) { return $Default }
    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property -or $null -eq $property.Value) { return $Default }
    return $property.Value
}

function Test-Administrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Turns "v24.1.0", "Python 3.12.7", "git version 2.47.0.windows.1" or "24" into a [version] with three parts.
function ConvertTo-Version3([string]$Text) {
    if (-not $Text) { return $null }
    $match = [regex]::Match($Text, '\d+(\.\d+){1,3}')
    if (-not $match.Success) { return $null }
    $parts = @($match.Value.Split('.'))
    while ($parts.Count -lt 3) { $parts += "0" }
    return [version]("{0}.{1}.{2}" -f $parts[0], $parts[1], $parts[2])
}

# Compares an installed version with a pin. A pin names a major version ("24"), a major and minor ("3.12"), or all
# three ("3.10.0"). The parts the pin names must match: 3.12.7 matches "3.12", and 3.15.0 doesn't. For a pin with a
# patch, the installed patch must be the same or higher. With $OrNewer (the "orNewer" setting), a higher major
# version also matches, so a "24" pin accepts 26. Returns "match", "below", or "above".
function Test-Pinned([string]$Have, [string]$Pin, [bool]$OrNewer = $false) {
    if (-not $Pin) { return "match" }
    $haveVersion = ConvertTo-Version3 $Have
    if ($null -eq $haveVersion) { return "below" }
    $haveParts = @($haveVersion.Major, $haveVersion.Minor, $haveVersion.Build)
    $pinParts = @($Pin.Split('.') | ForEach-Object { [int]$_ })
    for ($i = 0; $i -lt [Math]::Min($pinParts.Count, 2); $i++) {
        if ($haveParts[$i] -lt $pinParts[$i]) { return "below" }
        if ($haveParts[$i] -gt $pinParts[$i]) {
            if ($OrNewer -and $i -eq 0) { return "match" }
            return "above"
        }
    }
    if ($pinParts.Count -ge 3 -and $haveParts[2] -lt $pinParts[2]) { return "below" }
    return "match"
}

# Reloads PATH from the machine and user settings, so programs installed in this session are found.
function Update-SessionPath {
    $machine = [Environment]::GetEnvironmentVariable("Path", "Machine")
    $user = [Environment]::GetEnvironmentVariable("Path", "User")
    $env:Path = "$machine;$user"
}

# Adds a folder to the user PATH, so programs started later (such as VS Code) can find it. Also adds it to this
# session. Does nothing when the folder is already listed.
function Add-UserPathEntry([string]$Folder) {
    $wanted = $Folder.TrimEnd("\")
    $user = [Environment]::GetEnvironmentVariable("Path", "User")
    $entries = @($user -split ";" | Where-Object { $_ })
    $listed = @($entries | Where-Object { $_.TrimEnd("\") -ieq $wanted })
    if ($listed.Count -eq 0) {
        $entries += $Folder
        [Environment]::SetEnvironmentVariable("Path", ($entries -join ";"), "User")
        Write-Log "Added $Folder to the user PATH. Restart VS Code to pick it up."
    }
    $sessionListed = @(($env:Path -split ";") | Where-Object { $_.TrimEnd("\") -ieq $wanted })
    if ($sessionListed.Count -eq 0) { $env:Path = "$Folder;$env:Path" }
}

# Returns the output of a command as one string, or $null when the command isn't found or fails to run.
function Get-CommandOutput([string]$Exe, [string[]]$CommandArguments = @()) {
    $command = Get-Command $Exe -ErrorAction SilentlyContinue
    if (-not $command) { return $null }
    try {
        $output = & $command.Source @CommandArguments 2>&1 | ForEach-Object { "$_" }
        return ($output -join "`n")
    } catch {
        return $null
    }
}

# Finds a working PowerShell 7. A winget install can land in the user profile (a WindowsApps alias) or in
# Program Files, so every PATH entry is tried, and then Program Files. The version property isn't used: the
# WindowsApps alias reports 0.0.0.0. Each candidate has to run and print a version before it's accepted.
function Find-Pwsh {
    $candidates = @()
    foreach ($command in @(Get-Command pwsh -All -ErrorAction SilentlyContinue)) {
        if ($command.Source) { $candidates += $command.Source }
    }
    $candidates += "$env:ProgramFiles\PowerShell\7\pwsh.exe"
    foreach ($candidate in $candidates) {
        if (-not (Test-Path -LiteralPath $candidate)) { continue }
        try {
            $version = & $candidate -NoProfile -Command '$PSVersionTable.PSVersion.ToString()' 2>$null
            if ($LASTEXITCODE -eq 0 -and (($version -join "") -match '^\d+\.\d+')) { return $candidate }
        } catch {
            # Try the next candidate.
        }
    }
    return $null
}

# Returns the first path that exists, or the command found on PATH.
function Find-Exe([string]$Name, [string[]]$Candidates = @()) {
    $command = Get-Command $Name -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }
    foreach ($candidate in $Candidates) {
        if ($candidate -and (Test-Path -LiteralPath $candidate)) { return $candidate }
    }
    return $null
}

# Runs a program, sends its output to the log, and returns the exit code and the output lines.
# The console only shows output when the caller decides something failed.
function Invoke-Quiet([string]$Exe, [string[]]$CommandArguments, [string]$WorkingDirectory = $null) {
    $output = @()
    if ($WorkingDirectory) { Push-Location -LiteralPath $WorkingDirectory }
    try {
        $output = @(& $Exe @CommandArguments 2>&1 | ForEach-Object { "$_" })
    } finally {
        if ($WorkingDirectory) { Pop-Location }
    }
    $code = $LASTEXITCODE
    foreach ($line in $output) { Write-LogOnly ("   " + $line) }
    return [pscustomobject]@{ Code = $code; Output = $output }
}

# Like Invoke-Quiet, but prints each line as it arrives, so slow steps show progress. Lines also go to the log.
function Invoke-Streamed([string]$Exe, [string[]]$CommandArguments, [string]$WorkingDirectory = $null) {
    $output = @()
    if ($WorkingDirectory) { Push-Location -LiteralPath $WorkingDirectory }
    try {
        $output = @(& $Exe @CommandArguments 2>&1 | ForEach-Object {
            $line = "$_"
            Write-Host ("      " + $line) -ForegroundColor DarkGray
            Write-LogOnly ("   " + $line)
            $line
        })
    } finally {
        if ($WorkingDirectory) { Pop-Location }
    }
    $code = $LASTEXITCODE
    return [pscustomobject]@{ Code = $code; Output = $output; Streamed = $true }
}

# Prints the last lines of a failed command, so the cause is visible without opening the log.
# Streamed output is already on screen, so it isn't printed a second time.
function Show-FailureTail($Result, [int]$Lines = 25) {
    if ($Result.PSObject.Properties['Streamed']) { return }
    $tail = @($Result.Output | Select-Object -Last $Lines)
    foreach ($line in $tail) { Write-Host ("      " + $line) -ForegroundColor DarkGray }
}

function Get-RepoPath([string]$Relative) {
    if (-not $Relative -or $Relative -eq ".") { return $script:Root }
    return (Join-Path $script:Root $Relative)
}

# Resolves the base directory: -BaseDirectory, then setup.json, then the default. Creates it when missing.
function Resolve-BaseDirectory([string]$ParamValue, $Config) {
    $value = $ParamValue
    if (-not $value) { $value = Get-Prop $Config "baseDirectory" $null }
    if (-not $value) { $value = $DefaultBaseDirectory }
    $full = [System.IO.Path]::GetFullPath($value)
    New-Item -ItemType Directory -Force -Path $full | Out-Null
    return $full
}

# ---------------------------------------------------------------------------------------------------------------------
# winget and Visual Studio
# ---------------------------------------------------------------------------------------------------------------------

function Invoke-Winget([string[]]$WingetArguments) {
    if (-not (Get-Command winget -ErrorAction SilentlyContinue)) {
        throw "winget isn't available. $WingetFallback"
    }
    & winget @WingetArguments
    $code = $LASTEXITCODE
    Update-SessionPath
    if ($WingetSuccessCodes -notcontains $code) {
        throw "winget exited with code $code."
    }
}

function Get-VsWhere {
    $path = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path -LiteralPath $path) { return $path }
    return $null
}

function Get-VsInstances {
    $vswhere = Get-VsWhere
    if (-not $vswhere) { return @() }
    $json = (& $vswhere -products * -format json 2>$null | Out-String)
    if (-not $json.Trim()) { return @() }
    return @($json | ConvertFrom-Json)
}

function Test-VsWorkload([string]$WorkloadId) {
    $vswhere = Get-VsWhere
    if (-not $vswhere) { return $false }
    $found = (& $vswhere -products * -requires $WorkloadId -property installationPath 2>$null | Out-String)
    return [bool]$found.Trim()
}

# Adds workloads to an existing Visual Studio instance with the Visual Studio Installer.
function Add-VsWorkloads($Instance, [string[]]$WorkloadIds) {
    $setup = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\setup.exe"
    $cmdArgs = @("modify", "--installPath", $Instance.installationPath)
    foreach ($id in $WorkloadIds) { $cmdArgs += @("--add", $id) }
    $cmdArgs += @("--includeRecommended", "--quiet", "--wait", "--norestart")
    & $setup @cmdArgs
    if ($WingetSuccessCodes -notcontains $LASTEXITCODE) {
        throw "Visual Studio Installer exited with code $LASTEXITCODE while adding workloads."
    }
}

# Visual Studio: uses Professional or Enterprise when either is installed, and installs Community only when neither is.
function Confirm-VisualStudio($Spec) {
    $minimum = Get-Prop $Spec "pinned" "17"
    $workloads = @(Get-Prop $Spec "workloads" @())
    $instances = @(Get-VsInstances)
    $full = @($instances | Where-Object { $_.productId -match 'Product\.(Professional|Enterprise)$' })
    $community = @($instances | Where-Object { $_.productId -match 'Product\.Community$' })

    $chosen = $null
    if ($full.Count -gt 0) { $chosen = $full[0] }
    elseif ($community.Count -gt 0) { $chosen = $community[0] }

    if (-not $chosen) {
        if (-not (Test-Administrator)) { throw "Visual Studio needs an elevated PowerShell to install." }
        $override = "--quiet --wait --norestart --includeRecommended"
        foreach ($id in $workloads) { $override += " --add $id" }
        Invoke-Winget @("install", "-e", "--id", "Microsoft.VisualStudio.Community", "--silent",
            "--accept-package-agreements", "--accept-source-agreements", "--override", $override)
        $instances = @(Get-VsInstances)
        $chosen = $instances | Where-Object { $_.productId -match 'Product\.Community$' } | Select-Object -First 1
        if (-not $chosen) { throw "Visual Studio Community was installed, but no instance was found." }
        Set-Outcome "visualStudio" "installed" "Visual Studio Community"
        return
    }

    $edition = $chosen.productId -replace '^.*Product\.', ''
    $pinState = Test-Pinned $chosen.installationVersion $minimum
    if ($pinState -eq "above") {
        Set-Outcome "visualStudio" "warning" "Visual Studio $edition $($chosen.installationVersion) is newer than the pinned $minimum. It was not changed."
    }
    if ($pinState -eq "below") {
        $setup = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\setup.exe"
        if (-not (Test-Administrator)) {
            Set-Outcome "visualStudio" "warning" "Visual Studio $edition is older than the pinned $minimum. Update it in the Visual Studio Installer."
        } else {
            & $setup update --installPath $chosen.installationPath --quiet --wait --norestart | Out-Null
            if ($WingetSuccessCodes -notcontains $LASTEXITCODE) {
                Set-Outcome "visualStudio" "warning" "Visual Studio $edition update exited with code $LASTEXITCODE."
            }
        }
    }

    $missing = @($workloads | Where-Object { -not (Test-VsWorkload $_) })
    if ($missing.Count -gt 0) {
        if (-not (Test-Administrator)) {
            Set-Outcome "visualStudio" "warning" "Missing workloads ($($missing -join ', ')). Run this script from an elevated PowerShell."
        } else {
            try {
                Add-VsWorkloads $chosen $missing
                Set-Outcome "visualStudio" "updated" "Visual Studio $edition, workloads added"
            } catch {
                Set-Outcome "visualStudio" "warning" $_.Exception.Message
            }
        }
    } else {
        Set-Outcome "visualStudio" "ok" "Visual Studio $edition $($chosen.installationVersion)"
    }
}

# C++ build tools (the VC++ toolset). Adds them to an existing Visual Studio instance, or installs Build Tools.
function Confirm-BuildTools($Spec) {
    if (Test-VsWorkload "Microsoft.VisualStudio.Component.VC.Tools.x86.x64") {
        Set-Outcome "buildTools" "ok" "C++ toolset present"
        return
    }
    if (-not (Test-Administrator)) { throw "The C++ build tools need an elevated PowerShell to install." }
    $instances = @(Get-VsInstances)
    if ($instances.Count -gt 0) {
        Add-VsWorkloads $instances[0] @("Microsoft.VisualStudio.Component.VC.Tools.x86.x64")
        Set-Outcome "buildTools" "installed" "C++ toolset added to Visual Studio"
    } else {
        $override = "--quiet --wait --norestart --add Microsoft.VisualStudio.Workload.VCTools --includeRecommended"
        Invoke-Winget @("install", "-e", "--id", "Microsoft.VisualStudio.BuildTools", "--silent",
            "--accept-package-agreements", "--accept-source-agreements", "--override", $override)
        Set-Outcome "buildTools" "installed" "Visual Studio Build Tools"
    }
}

# Loads the C++ compiler environment (vcvars64.bat) into this session, so native modules can be compiled.
function Import-BuildToolsEnvironment {
    $vswhere = Get-VsWhere
    if (-not $vswhere) { return $false }
    $install = (& $vswhere -products * -requires "Microsoft.VisualStudio.Component.VC.Tools.x86.x64" -latest -property installationPath 2>$null | Select-Object -First 1)
    if (-not $install) { return $false }
    $vcvars = Join-Path $install "VC\Auxiliary\Build\vcvars64.bat"
    if (-not (Test-Path -LiteralPath $vcvars)) { return $false }
    $lines = cmd /c "`"$vcvars`" >nul 2>&1 && set"
    foreach ($line in $lines) {
        if ($line -match "^([^=]+)=(.*)$") { Set-Item -Path "env:$($Matches[1])" -Value $Matches[2] }
    }
    return $true
}

# Electron's fallback. When Electron's binary won't download, the C++ Build Tools are installed if they're missing,
# the compiler environment is loaded, native modules are rebuilt, and Electron's installer runs again.
# Returns $true when the binary is in place afterwards.
function Invoke-ElectronBuildToolsFallback([string]$Folder, [string]$Npm, [string]$Electron) {
    Write-Host "      Falling back to the C++ Build Tools for Electron." -ForegroundColor DarkGray
    try {
        if (-not (Test-VsWorkload "Microsoft.VisualStudio.Component.VC.Tools.x86.x64")) {
            if (-not (Test-Administrator)) {
                Write-Host "      The C++ Build Tools are needed for this fallback, and installing them needs an elevated PowerShell. Run this script as Administrator to try again." -ForegroundColor Yellow
                return $false
            }
            Write-Host "      Installing the C++ Build Tools. This is a large download and can take a long time." -ForegroundColor DarkGray
            Confirm-BuildTools $null
        }
        if (-not (Import-BuildToolsEnvironment)) {
            Write-LogOnly "The C++ compiler environment couldn't be loaded, so the Electron rebuild was skipped."
            return $false
        }
        $null = Invoke-Streamed $Npm @("rebuild") $Folder
        $node = Find-Exe "node" @("$env:ProgramFiles\nodejs\node.exe")
        if ($node) { $null = Invoke-Streamed $node @("install.js") $Electron }
    } catch {
        Write-LogOnly ("Electron fallback failed: " + $_.Exception.Message)
        return $false
    }
    return (Test-Path -LiteralPath (Join-Path $Electron "dist\electron.exe"))
}

function Confirm-VcRuntime($Spec) {
    $system = [Environment]::GetFolderPath("System")
    $present = (Test-Path -LiteralPath (Join-Path $system "VCRUNTIME140.dll")) -and (Test-Path -LiteralPath (Join-Path $system "MSVCP140.dll"))
    if ($present) {
        Set-Outcome "vcRuntime" "ok" "VC++ runtime present"
        return
    }
    if (-not (Test-Administrator)) { throw "The VC++ runtime needs an elevated PowerShell to install." }
    Invoke-Winget @("install", "-e", "--id", "Microsoft.VCRedist.2015+.x64", "--silent",
        "--accept-package-agreements", "--accept-source-agreements")
    Set-Outcome "vcRuntime" "installed" "VC++ runtime"
}

# ---------------------------------------------------------------------------------------------------------------------
# Program versions and installs
# ---------------------------------------------------------------------------------------------------------------------

# Returns the installed version text for a program, or $null when it's missing.
function Get-ProgramVersion([string]$Name, $Spec) {
    switch ($Name) {
        "git" {
            return (Get-CommandOutput "git" @("--version"))
        }
        "powershell7" {
            $pwsh = Find-Pwsh
            if ($pwsh) { return (Get-CommandOutput $pwsh @("-NoProfile", "-Command", '$PSVersionTable.PSVersion.ToString()')) }
            return $null
        }
        "node" {
            return (Get-CommandOutput "node" @("--version"))
        }
        "python" {
            $text = Get-CommandOutput "py" @("-3.12", "--version")
            if (-not $text) {
                $path = Join-Path $env:LOCALAPPDATA "Programs\Python\Python312\python.exe"
                if (Test-Path -LiteralPath $path) { $text = Get-CommandOutput $path @("--version") }
            }
            return $text
        }
        "dotnet" {
            $list = Get-CommandOutput "dotnet" @("--list-sdks")
            if (-not $list) { return $null }
            $versions = @($list -split "`n" | ForEach-Object { ConvertTo-Version3 $_ } | Where-Object { $_ } | Sort-Object)
            if ($versions.Count -eq 0) { return $null }
            return $versions[-1].ToString()
        }
        "jdk" {
            $base = Join-Path $env:ProgramFiles "Eclipse Adoptium"
            $java = Get-ChildItem -LiteralPath $base -Directory -Filter "jdk-*" -ErrorAction SilentlyContinue |
                ForEach-Object { Join-Path $_.FullName "bin\java.exe" } |
                Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
            if ($java) {
                $script:InstallPaths["jdk"] = Split-Path -Parent (Split-Path -Parent $java)
                # Maven (mvn.cmd) and other Java tools refuse to run without JAVA_HOME. Point it at the JDK found here,
                # so every later step in this run, and the runner it starts, uses the same JDK.
                $env:JAVA_HOME = $script:InstallPaths["jdk"]
                return (Get-CommandOutput $java @("-version"))
            }
            return $null
        }
        "maven" {
            $root = Get-Prop $Spec "root" "C:\Tools"
            $mvn = Get-ChildItem -LiteralPath $root -Directory -Filter "apache-maven-*" -ErrorAction SilentlyContinue |
                ForEach-Object { Join-Path $_.FullName "bin\mvn.cmd" } |
                Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
            if ($mvn) { return (Get-CommandOutput $mvn @("-v")) }
            return $null
        }
        "msys2" {
            $root = Get-Prop $Spec "root" "C:\msys64"
            $gcc = Join-Path $root "ucrt64\bin\gcc.exe"
            if (Test-Path -LiteralPath $gcc) { return (Get-CommandOutput $gcc @("--version")) }
            return $null
        }
        "rust" {
            $text = Get-CommandOutput "rustc" @("--version")
            if (-not $text) {
                $path = Join-Path $env:USERPROFILE ".cargo\bin\rustc.exe"
                if (Test-Path -LiteralPath $path) { $text = Get-CommandOutput $path @("--version") }
            }
            return $text
        }
        "notepadPlusPlus" {
            $paths = @(
                (Join-Path $env:ProgramFiles "Notepad++\notepad++.exe"),
                (Join-Path ${env:ProgramFiles(x86)} "Notepad++\notepad++.exe"))
            $exe = $paths | Where-Object { $_ -and (Test-Path -LiteralPath $_) } | Select-Object -First 1
            if (-not $exe) { return $null }
            return (Get-Item -LiteralPath $exe).VersionInfo.ProductVersion
        }
        "vscode" {
            $text = Get-CommandOutput "code" @("--version")
            if (-not $text) {
                $path = Find-Exe "code" @(
                    (Join-Path $env:LOCALAPPDATA "Programs\Microsoft VS Code\bin\code.cmd"),
                    (Join-Path $env:ProgramFiles "Microsoft VS Code\bin\code.cmd"))
                if ($path) { $text = Get-CommandOutput $path @("--version") }
            }
            return $text
        }
        "vcRuntime" {
            $system = [Environment]::GetFolderPath("System")
            if (Test-Path -LiteralPath (Join-Path $system "VCRUNTIME140.dll")) { return "14.0" }
            return $null
        }
        default {
            return $null
        }
    }
}

# Installs a program with winget, or with its own method (Maven, MSYS2, Rust). $Upgrade selects upgrade over install.
function Install-Program([string]$Name, $Spec, [bool]$Upgrade) {
    switch ($Name) {
        "maven" { Install-Maven $Spec; return }
        "msys2" { Install-Msys2 $Spec $Upgrade; return }
        "rust" {
            if ($Upgrade) { Invoke-Quiet "rustup" @("update") | Out-Null }
            else { Invoke-Winget @("install", "-e", "--id", "Rustlang.Rustup", "--silent", "--accept-package-agreements", "--accept-source-agreements") }
            Update-SessionPath
            $default = Invoke-Quiet "rustup" @("default", "stable")
            if ($default.Code -ne 0) { throw "rustup couldn't set the stable toolchain (exit $($default.Code))." }
            return
        }
    }

    $id = Get-Prop $Spec "winget" $DefaultWinget[$Name]
    if (-not $id) { throw "setup.json has no winget id for $Name." }
    $wingetArgs = @()
    if ($Upgrade) { $wingetArgs += "upgrade" } else { $wingetArgs += "install" }
    $wingetArgs += @("-e", "--id", $id, "--silent", "--accept-package-agreements", "--accept-source-agreements")
    $override = Get-Prop $Spec "override" $null
    if ($override) { $wingetArgs += @("--override", $override) }
    Invoke-Winget $wingetArgs
}

function Install-Maven($Spec) {
    $root = Get-Prop $Spec "root" "C:\Tools"
    $url = Get-Prop $Spec "url" $null
    if (-not $url) { throw "setup.json has no url for maven." }
    New-Item -ItemType Directory -Force -Path $root | Out-Null
    $zip = Join-Path $env:TEMP ("maven-" + [guid]::NewGuid().ToString() + ".zip")
    try {
        Invoke-WebRequest -Uri $url -OutFile $zip -UseBasicParsing
        $expected = (Invoke-WebRequest -Uri ($url + ".sha1") -UseBasicParsing).Content.Trim()
        $actual = (Get-FileHash -LiteralPath $zip -Algorithm SHA1).Hash
        if ($expected.ToUpper() -notlike ($actual.ToUpper() + "*")) {
            throw "The Maven download failed its SHA-1 check. Nothing was installed."
        }
        Expand-Archive -LiteralPath $zip -DestinationPath $root -Force
    } catch {
        throw "Maven couldn't be downloaded or extracted to ${root}: $($_.Exception.Message)"
    } finally {
        Remove-Item -LiteralPath $zip -Force -ErrorAction SilentlyContinue
    }
}

function Install-Msys2($Spec, [bool]$Upgrade) {
    $root = Get-Prop $Spec "root" "C:\msys64"
    $bash = Join-Path $root "usr\bin\bash.exe"
    $freshInstall = -not (Test-Path -LiteralPath $bash)
    if ($freshInstall) {
        Invoke-Winget @("install", "-e", "--id", "MSYS2.MSYS2", "--silent", "--accept-package-agreements", "--accept-source-agreements")
    }
    if (-not (Test-Path -LiteralPath $bash)) { throw "MSYS2 was installed, but it isn't in $root." }

    # A brand-new MSYS2 needs its package signing keys before pacman can install anything. This first run is slow
    # and prints nothing, so say so first. Later runs skip it because the keys already exist.
    $gnupg = Join-Path $root "etc\pacman.d\gnupg"
    $keysReady = (Test-Path -LiteralPath (Join-Path $gnupg "pubring.kbx")) -or (Test-Path -LiteralPath (Join-Path $gnupg "pubring.gpg"))
    if (-not $keysReady) {
        Write-Host "      MSYS2 is creating its package keys (first run). This can take several minutes." -ForegroundColor DarkGray
        $init = Invoke-Quiet $bash @("-lc", "pacman-key --init && pacman-key --populate msys2")
        if ($init.Code -ne 0) {
            Show-FailureTail $init
            throw "MSYS2 couldn't create its package keys (exit $($init.Code))."
        }
    }

    # MSYS2 says to run "pacman -Syu" on a new install and after an upgrade. Core updates can ask for a rerun,
    # so try up to three passes and stop at the first success.
    if ($freshInstall -or $Upgrade) {
        for ($attempt = 1; $attempt -le 3; $attempt++) {
            Write-Host "      Updating MSYS2 packages (pass $attempt of 3)." -ForegroundColor DarkGray
            $sync = Invoke-Streamed $bash @("-lc", "pacman -Syu --noconfirm")
            if ($sync.Code -eq 0) { break }
            if ($attempt -eq 3) {
                Show-FailureTail $sync
                throw "MSYS2 package update failed (exit $($sync.Code))."
            }
        }
    }
    $packages = @(Get-Prop $Spec "packages" @())
    if ($packages.Count -gt 0) {
        Write-Host "      Installing the MSYS2 packages." -ForegroundColor DarkGray
        $result = Invoke-Streamed $bash @("-lc", ("pacman -S --needed --noconfirm " + ($packages -join " ")))
        if ($result.Code -ne 0) {
            Show-FailureTail $result
            throw "pacman couldn't install the MSYS2 packages (exit $($result.Code))."
        }
    }

    # VS Code and other programs read the user PATH, so the compiler folder goes there too. A step's pathPrepend
    # only lasts for that step, so without this the Debug button can't find gcc or gdb.
    Add-UserPathEntry (Join-Path $root "ucrt64\bin")
}

# Adds "Open with Code" to the Explorer right-click menu for files, folders, and the background of a folder.
# Written under HKEY_CURRENT_USER, so it needs no administrator rights and applies to the current user only.
function Install-VsCodeContextMenu {
    try {
        $exe = @(
            (Join-Path $env:LOCALAPPDATA "Programs\Microsoft VS Code\Code.exe"),
            (Join-Path $env:ProgramFiles "Microsoft VS Code\Code.exe")) |
            Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
        if (-not $exe) {
            Set-Outcome "vscode-context-menu" "warning" "Code.exe wasn't found, so the Open with Code menu entries weren't added."
            return
        }
        # The keys are written with .NET. The PowerShell registry provider treats * in a key name as a wildcard, and
        # New-Item and New-ItemProperty don't take -LiteralPath.
        $entries = @(
            @{ Path = "Software\Classes\*\shell\VSCode";                    Argument = "%1" },
            @{ Path = "Software\Classes\Directory\shell\VSCode";            Argument = "%1" },
            @{ Path = "Software\Classes\Directory\Background\shell\VSCode"; Argument = "%V" }
        )
        $hkcu = [Microsoft.Win32.Registry]::CurrentUser
        foreach ($entry in $entries) {
            $shellKey = $hkcu.CreateSubKey($entry.Path)
            $shellKey.SetValue("", "Open w&ith VS Code")
            $shellKey.SetValue("Icon", "`"$exe`",0")
            $shellKey.Close()
            $commandKey = $hkcu.CreateSubKey($entry.Path + "\command")
            $commandKey.SetValue("", "`"$exe`" `"$($entry.Argument)`"")
            $commandKey.Close()
        }
        Set-Outcome "vscode-context-menu" "ok" "Open with Code added to the Explorer right-click menu for this user"
    } catch {
        Set-Outcome "vscode-context-menu" "warning" ("Couldn't add the Open with Code menu entries: " + $_.Exception.Message)
    }
}

function Install-VsCodeExtensions($Spec) {
    $extensions = @(Get-Prop $Spec "extensions" @())
    if ($extensions.Count -eq 0) { return }
    $code = Find-Exe "code" @(
        (Join-Path $env:LOCALAPPDATA "Programs\Microsoft VS Code\bin\code.cmd"),
        (Join-Path $env:ProgramFiles "Microsoft VS Code\bin\code.cmd"))
    if (-not $code) {
        Set-Outcome "vscode-extensions" "warning" "VS Code isn't on PATH, so the extensions weren't installed. Open a new PowerShell window and run this script again."
        return
    }
    $failed = @()
    foreach ($extension in $extensions) {
        $result = Invoke-Quiet $code @("--install-extension", $extension, "--force")
        if ($result.Code -ne 0) { $failed += $extension }
    }
    if ($failed.Count -gt 0) {
        Set-Outcome "vscode-extensions" "warning" ("Couldn't install: " + ($failed -join ", "))
    } else {
        Set-Outcome "vscode-extensions" "ok" ("Installed: " + ($extensions -join ", "))
    }
}

# Installs or upgrades one program from the "installs" section of setup.json.
function Invoke-InstallItem([string]$Name) {
    $installs = Get-Prop $script:Config "installs" $null
    $spec = Get-Prop $installs $Name $null

    $enabled = [bool](Get-Prop $spec "install" $false)
    if ($Name -eq "git") { $enabled = $true }   # Git can't be skipped: the repository needs it to clone
    if (-not $spec -and $Name -ne "git") { Set-Outcome $Name "skipped" "not listed in setup.json"; return }
    if (-not $enabled) { Set-Outcome $Name "skipped" "install is false in setup.json"; return }
    if ($SkipInstall -and $Name -ne "git") { Set-Outcome $Name "skipped" "-SkipInstall"; return }

    Write-Host ("   Checking " + $Name + " ...") -ForegroundColor Gray
    try {
        switch ($Name) {
            "visualStudio" { Confirm-VisualStudio $spec; return }
            "buildTools"   { Confirm-BuildTools $spec; return }
            "vcRuntime"    { Confirm-VcRuntime $spec; return }
        }

        $minimum = Get-Prop $spec "pinned" $null
        $sideBySide = [bool](Get-Prop $spec "sideBySide" $false)
        $orNewer = [bool](Get-Prop $spec "orNewer" $false)
        $have = Get-ProgramVersion $Name $spec

        $pinState = "below"
        if ($have) { $pinState = Test-Pinned $have $minimum $orNewer }
        if ($pinState -eq "match") {
            Set-Outcome $Name "ok" $have.Trim()
        } elseif ($pinState -eq "above") {
            Set-Outcome $Name "warning" "The installed version $($have.Trim()) is newer than the pinned $minimum. It was not changed."
        } else {
            $upgrade = [bool]$have -and -not $sideBySide
            if ($upgrade) { $action = "upgrade" } else { $action = "install" }
            try {
                Install-Program $Name $spec $upgrade
            } catch {
                $firstError = $_.Exception.Message
                if ($upgrade) {
                    # Some programs can't be upgraded in place (no winget package is installed to upgrade). Install instead.
                    $action = "install"
                    try {
                        Install-Program $Name $spec $false
                    } catch {
                        if ($have) {
                            Set-Outcome $Name "warning" "upgrade failed ($firstError), and install failed too. Keeping $($have.Trim()): $($_.Exception.Message)"
                            return
                        }
                        throw
                    }
                } elseif ($have) {
                    Set-Outcome $Name "warning" "install failed, keeping $($have.Trim()): $firstError"
                    return
                } else {
                    throw
                }
            }

            $now = Get-ProgramVersion $Name $spec
            if ($now -and ((Test-Pinned $now $minimum $orNewer) -eq "match")) {
                if ($action -eq "upgrade") { $word = "upgraded" } else { $word = "installed" }
                Set-Outcome $Name $word $now.Trim()
            } elseif ($have) {
                Set-Outcome $Name "warning" "$action ran, but the version still isn't the pinned $minimum."
                return
            } else {
                Set-Outcome $Name "failed" "installed, but the version check didn't find the pinned $minimum."
                return
            }
        }

        if ($Name -eq "vscode") {
            Install-VsCodeExtensions $spec
            Install-VsCodeContextMenu
        }
    } catch {
        Set-Outcome $Name "failed" $_.Exception.Message
    }
}

# ---------------------------------------------------------------------------------------------------------------------
# Steps
# ---------------------------------------------------------------------------------------------------------------------

# Loads a package in a small Node script and returns the first line of its error cause chain, or "" when it loads.
# The script is written to the temp folder, so quoting never reaches the command line.
function Get-LoadCause([string]$Node, [string]$PackageIndex) {
    $checkFile = Join-Path $env:TEMP "setup-load-check.mjs"
    $js = @'
import { pathToFileURL } from "node:url"
try {
  await import(pathToFileURL(process.argv[2]).href)
  console.log("loaded")
} catch (e) {
  let c = e
  while (c) {
    console.log("cause: " + (c.code || "") + " " + c.message)
    c = c.cause
  }
}
'@
    Set-Content -LiteralPath $checkFile -Value $js -Encoding UTF8
    $check = Invoke-Quiet $Node @($checkFile, $PackageIndex)
    foreach ($line in $check.Output) { Write-Host ("      " + $line) -ForegroundColor DarkGray }
    $cause = @($check.Output | Where-Object { $_ -like "cause:*" })
    if ($cause.Count -gt 0) { return ($cause[0] -replace '^cause:\s*', '').Trim() }
    return ""
}

function Invoke-NpmStep($Step, [string]$Name) {
    $folder = Get-RepoPath (Get-Prop $Step "folder" ".")
    $npm = Find-Exe "npm.cmd" @()
    if (-not $npm) { $npm = Find-Exe "npm" @() }
    if (-not $npm) { throw "npm wasn't found. Install Node.js, then run this script again." }

    $command = Get-Prop $Step "command" "ci"
    if ($command -eq "ci" -and -not (Test-Path -LiteralPath (Join-Path $folder "package-lock.json"))) {
        $command = "install"
        Write-LogOnly "$Name has no package-lock.json, so npm install is used."
    }
    $result = Invoke-Quiet $npm @($command) $folder
    if ($result.Code -ne 0) {
        Show-FailureTail $result
        throw "npm $command failed (exit $($result.Code))."
    }

    $electron = Join-Path $folder "node_modules\electron"
    $electronExe = Join-Path $folder "node_modules\electron\dist\electron.exe"
    if ((Test-Path -LiteralPath $electron) -and -not (Test-Path -LiteralPath $electronExe)) {
        # npm can leave Electron's binary missing (npm bug 4828). A rebuild usually fixes it.
        Invoke-Quiet $npm @("rebuild") $folder | Out-Null
        if (-not (Test-Path -LiteralPath $electronExe)) {
            # Electron's own install script downloads the binary. Run it directly, so a blocked download shows its reason.
            $node = Find-Exe "node" @("$env:ProgramFiles\nodejs\node.exe")
            $cause = ""
            if ($node) {
                $install = Invoke-Quiet $node @("install.js") $electron
                if ($install.Code -ne 0) {
                    Show-FailureTail $install
                    $extractIndex = Join-Path $folder "node_modules\@electron-internal\extract-zip\index.js"
                    if (Test-Path -LiteralPath $extractIndex) {
                        Write-Host "   Checking why Electron's zip extractor won't load:" -ForegroundColor Gray
                        $cause = Get-LoadCause $node $extractIndex
                    }
                }
            }
        }
        if (-not (Test-Path -LiteralPath $electronExe)) {
            $null = Invoke-ElectronBuildToolsFallback $folder $npm $electron
        }
        if (-not (Test-Path -LiteralPath $electronExe)) {
            if ($cause) {
                Set-Outcome $Name "warning" "Electron's zip extractor didn't load: $cause. The full cause chain is above and in the log."
            } else {
                Set-Outcome $Name "warning" "Electron's binary didn't download. Check that github.com is reachable through your network or proxy, then run this script again. Electron's own installer output is in the log."
            }
            return
        }
    }
    if (Test-Path -LiteralPath $electronExe) {
        Set-Outcome $Name "ok" "$folder (Electron binary present)"
    } else {
        Set-Outcome $Name "ok" $folder
    }
}

function Invoke-NpmRunStep($Step, [string]$Name) {
    $folder = Get-RepoPath (Get-Prop $Step "folder" ".")
    $script = Get-Prop $Step "script" "test"
    $npm = Find-Exe "npm.cmd" @()
    if (-not $npm) { throw "npm wasn't found." }
    if ($script -eq "test") { $cmdArgs = @("test") } else { $cmdArgs = @("run", $script) }
    $result = Invoke-Quiet $npm $cmdArgs $folder
    if ($result.Code -ne 0) {
        # Only the failing lines are shown. The full output is in the log.
        $failing = @($result.Output | Where-Object { $_ -match '(?i)\bnot ok\b|\bfail|\berror\b' })
        if ($failing.Count -eq 0) { $failing = @($result.Output | Select-Object -Last 15) }
        foreach ($line in $failing) { Write-Host ("      " + $line) -ForegroundColor DarkGray }
        throw "npm $($cmdArgs -join ' ') failed (exit $($result.Code))."
    }
    Set-Outcome $Name "ok" "npm $($cmdArgs -join ' ')"
}

function Find-Python {
    $text = Get-CommandOutput "py" @("-3.12", "-c", "import sys; print(sys.executable)")
    if ($text) { return ($text -split "`n")[-1].Trim() }
    $path = Join-Path $env:LOCALAPPDATA "Programs\Python\Python312\python.exe"
    if (Test-Path -LiteralPath $path) { return $path }
    return $null
}

function Invoke-PythonVenvStep($Step, [string]$Name) {
    $folder = Get-RepoPath (Get-Prop $Step "folder" ".")
    $venv = Join-Path $folder (Get-Prop $Step "path" ".venv")
    $venvPython = Join-Path $venv "Scripts\python.exe"

    if (-not (Test-Path -LiteralPath $venvPython)) {
        $python = Find-Python
        if (-not $python) { throw "Python 3.12 wasn't found, so the virtual environment can't be created." }
        $made = Invoke-Quiet $python @("-m", "venv", $venv)
        if ($made.Code -ne 0) { Show-FailureTail $made; throw "Couldn't create $venv (exit $($made.Code))." }
    }

    $upgrade = Invoke-Quiet $venvPython @("-m", "pip", "install", "--upgrade", "pip")
    if ($upgrade.Code -ne 0) { Show-FailureTail $upgrade; throw "pip couldn't be upgraded (exit $($upgrade.Code))." }

    $requirements = Get-Prop $Step "requirements" $null
    if ($requirements) {
        $requirementsPath = Join-Path $folder $requirements
        $install = Invoke-Quiet $venvPython @("-m", "pip", "install", "-r", $requirementsPath)
        if ($install.Code -ne 0) { Show-FailureTail $install; throw "pip install -r $requirements failed (exit $($install.Code))." }
    }
    Set-Outcome $Name "ok" $venv
}

# Looks for an environment variable (in the user's settings, or in this session). A missing variable is reported as a
# warning, and the setup carries on. This step never fails the setup.
function Invoke-CheckVariableStep($Step, [string]$Name) {
    $variable = Get-Prop $Step "variable" $null
    $message = Get-Prop $Step "message" "The environment variable $variable isn't set."
    $value = [Environment]::GetEnvironmentVariable($variable, "User")
    if (-not $value) { $value = [Environment]::GetEnvironmentVariable($variable, "Process") }
    if ($value) {
        Set-Outcome $Name "ok" "$variable is set"
    } else {
        Set-Outcome $Name "warning" $message
    }
}

function Invoke-CommandStep($Step, [string]$Name) {
    $folder = Get-RepoPath (Get-Prop $Step "folder" ".")
    $check = Get-Prop $Step "check" $null
    if ($check -and (Test-Path -LiteralPath (Join-Path $folder $check))) {
        Set-Outcome $Name "ok" "already present: $check"
        return
    }
    $exe = Get-Prop $Step "exe" $null
    if (-not $exe) { throw "command step has no exe." }
    $cmdArgs = @(Get-Prop $Step "args" @())
    # pathPrepend adds folders to PATH for this step only, such as MSYS2's ucrt64\bin for the compiler tools.
    $prepend = @(Get-Prop $Step "pathPrepend" @())
    $savedPath = $env:Path
    try {
        if ($prepend.Count -gt 0) { $env:Path = (($prepend -join ";") + ";" + $env:Path) }
        $result = Invoke-Streamed $exe $cmdArgs $folder
    } finally {
        $env:Path = $savedPath
    }
    if ($result.Code -ne 0) {
        Show-FailureTail $result
        throw "$exe exited with code $($result.Code)."
    }
    Set-Outcome $Name "ok" $exe
}

# Downloads a file. With extract "zip", the zip is unpacked into folder and the download is removed. Skipped when the
# file (or folder, for extract) is already there.
function Invoke-DownloadStep($Step, [string]$Name) {
    $url = Get-Prop $Step "url" $null
    $dest = Get-RepoPath (Get-Prop $Step "path" $null)
    $extract = Get-Prop $Step "extract" $null
    $target = $dest
    if ($extract) { $target = Get-RepoPath (Get-Prop $Step "folder" $null) }
    if (Test-Path -LiteralPath $target) {
        Set-Outcome $Name "ok" "already present: $target"
        return
    }
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $dest) | Out-Null
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    Write-Host ("   Downloading " + $url)
    try {
        Invoke-WebRequest -Uri $url -OutFile $dest -UseBasicParsing
    } catch {
        throw "Download failed for $url. $($_.Exception.Message)"
    }
    if ($extract -eq "zip") {
        Expand-Archive -LiteralPath $dest -DestinationPath $target -Force
        Remove-Item -LiteralPath $dest -Force
    }
    Set-Outcome $Name "ok" $target
}

function Invoke-CopyStep($Step, [string]$Name) {
    $source = Get-RepoPath (Get-Prop $Step "source" $null)
    $dest = Get-RepoPath (Get-Prop $Step "dest" $null)
    if (-not (Test-Path -LiteralPath $source)) { throw "$source is missing." }
    $backup = [bool](Get-Prop $Step "backup" $false)
    if ($backup -and (Test-Path -LiteralPath $dest)) {
        $same = (Get-FileHash -LiteralPath $source).Hash -eq (Get-FileHash -LiteralPath $dest).Hash
        if (-not $same) {
            $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
            Copy-Item -LiteralPath $dest -Destination ($dest + ".backup-" + $stamp)
        }
    }
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $dest) | Out-Null
    Copy-Item -LiteralPath $source -Destination $dest -Force
    Set-Outcome $Name "ok" $dest
}

function Invoke-MsBuildRestoreStep($Step, [string]$Name) {
    $solution = Get-RepoPath (Get-Prop $Step "solution" $null)
    $vswhere = Get-VsWhere
    if (-not $vswhere) { throw "vswhere wasn't found, so MSBuild can't be located." }
    $msbuild = (& $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" 2>$null | Select-Object -First 1)
    if (-not $msbuild) { throw "MSBuild wasn't found in the Visual Studio installation." }
    $result = Invoke-Streamed $msbuild @($solution, "/t:Restore", "/v:minimal", "/nologo")
    if ($result.Code -ne 0) {
        # NuGet names each package it couldn't find. Those usually come from a private feed (such as the DataBank feed)
        # that isn't configured, or that this account can't read. List them, so the summary says what to fix.
        $text = ($result.Output -join "`n")
        $missing = @([regex]::Matches($text, 'Unable to find package (\S+?)\. ') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique)
        if ($missing.Count -gt 0) {
            throw ("restore couldn't find these NuGet packages: " + ($missing -join ", ") + ". They usually come from a private feed that isn't configured or can't be read. Check the feed's NuGet source and your access, then run this script again.")
        }
        Show-FailureTail $result
        throw "MSBuild restore failed (exit $($result.Code))."
    }
    Set-Outcome $Name "ok" "restored $solution"
}

function Invoke-WriteFileStep($Step, [string]$Name) {
    $path = Get-RepoPath (Get-Prop $Step "path" $null)
    # ifMissing keeps a file the learner may have edited: it is written only when it doesn't exist yet.
    if ([bool](Get-Prop $Step "ifMissing" $false) -and (Test-Path -LiteralPath $path)) {
        Set-Outcome $Name "ok" "already present: $path"
        return
    }
    $content = Get-Prop $Step "content" $null
    $from = Get-Prop $Step "contentFrom" $null
    if ($from) {
        if (-not $script:InstallPaths.ContainsKey($from)) { throw "No installed location is known for '$from'. Install it in this run first." }
        $content = $script:InstallPaths[$from]
    }
    if ($content -is [array]) { $content = ($content -join "`r`n") }
    elseif ($null -ne $content -and $content -isnot [string]) { $content = ConvertTo-Json -InputObject $content -Depth 10 }
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $path) | Out-Null
    Set-Content -LiteralPath $path -Value $content -Encoding UTF8
    Set-Outcome $Name "ok" $path
}

function Invoke-DotnetBuildStep($Step, [string]$Name) {
    $folder = Get-RepoPath (Get-Prop $Step "folder" ".")
    $dotnet = Find-Exe "dotnet" @("$env:ProgramFiles\dotnet\dotnet.exe")
    if (-not $dotnet) { throw "dotnet wasn't found." }
    $failed = @()
    $projects = @(Get-Prop $Step "projects" @())
    foreach ($project in $projects) {
        $result = Invoke-Streamed $dotnet @("build", (Join-Path $folder $project), "--nologo", "-v", "minimal")
        if ($result.Code -ne 0) {
            Show-FailureTail $result
            $failed += $project
        }
    }
    if ($failed.Count -gt 0) { throw "dotnet build failed for: $($failed -join ', ')" }
    Set-Outcome $Name "ok" "$($projects.Count) project(s)"
}

# Checks files against an approved hash list. Each line of the manifest is a SHA-256 hash, two spaces, and a path
# relative to the manifest's folder. Fails if an approved file is missing or changed, or if the folder has a file that
# isn't in the list. The manifest itself is not checked.
function Invoke-VerifyHashesStep($Step, [string]$Name) {
    $manifest = Get-RepoPath (Get-Prop $Step "manifest" $null)
    if (-not (Test-Path -LiteralPath $manifest -PathType Leaf)) { throw "No approved hash list at $manifest." }
    $folder = Split-Path -Parent $manifest
    $manifestName = Split-Path -Leaf $manifest

    $approved = @{}
    foreach ($line in Get-Content -LiteralPath $manifest) {
        if ($line.Trim() -eq "") { continue }
        $parts = $line -split "  ", 2
        if ($parts.Count -ne 2) { throw "Malformed line in ${manifest}: $line" }
        $approved[$parts[1].Replace('\', '/')] = $parts[0].ToLowerInvariant()
    }

    $problems = New-Object System.Collections.ArrayList
    foreach ($relative in $approved.Keys) {
        $path = Join-Path $folder ($relative -replace '/', '\')
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { [void]$problems.Add("missing: $relative"); continue }
        $actual = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actual -ne $approved[$relative]) { [void]$problems.Add("changed: $relative") }
    }

    $folderFull = (Resolve-Path -LiteralPath $folder).Path.TrimEnd('\')
    Get-ChildItem -LiteralPath $folder -Recurse -File | ForEach-Object {
        $relative = $_.FullName.Substring($folderFull.Length).TrimStart('\').Replace('\', '/')
        if ($relative -ne $manifestName -and -not $approved.ContainsKey($relative)) {
            [void]$problems.Add("not approved: $relative")
        }
    }

    if ($problems.Count -eq 0) {
        Set-Outcome $Name "ok" "$($approved.Count) file(s) match the approved hashes"
        return
    }
    $shown = @($problems | Select-Object -First 5) -join "; "
    $more = ""
    if ($problems.Count -gt 5) { $more = " and $($problems.Count - 5) more" }
    throw "$($problems.Count) problem(s): $shown$more."
}

function Invoke-Step($Step) {
    $name = Get-Prop $Step "name" "(unnamed step)"
    $type = Get-Prop $Step "type" ""

    if (-not (Get-Prop $Step "enabled" $true)) { Set-Outcome $name "skipped" "enabled is false"; return }
    if ((Get-Prop $Step "runTests" $false) -and -not $script:RunTestsEffective) {
        Set-Outcome $name "skipped" "tests are off (-SkipTests or runTests is false)"
        return
    }
    Write-Host ("   Running: " + $name + " (" + $type + ") ...") -ForegroundColor Gray
    foreach ($required in @(Get-Prop $Step "requires" @())) {
        if ($script:Status[$required] -eq "failed") {
            Set-Outcome $name "skipped" "requires $required, which failed"
            return
        }
    }

    try {
        switch ($type) {
            "npm"             { Invoke-NpmStep $Step $name }
            "npm-run"         { Invoke-NpmRunStep $Step $name }
            "python-venv"     { Invoke-PythonVenvStep $Step $name }
            "command"         { Invoke-CommandStep $Step $name }
            "copy"            { Invoke-CopyStep $Step $name }
            "download"        { Invoke-DownloadStep $Step $name }
            "msbuild-restore" { Invoke-MsBuildRestoreStep $Step $name }
            "write-file"      { Invoke-WriteFileStep $Step $name }
            "check-variable"  { Invoke-CheckVariableStep $Step $name }
            "verify-hashes"   { Invoke-VerifyHashesStep $Step $name }
            "dotnet-build"    { Invoke-DotnetBuildStep $Step $name }
            default           { throw "Unknown step type '$type' in setup.json." }
        }
    } catch {
        Set-Outcome $name "failed" $_.Exception.Message
    }
}

# ---------------------------------------------------------------------------------------------------------------------
# Launch
# ---------------------------------------------------------------------------------------------------------------------

function Start-Runner($Launch) {
    if (-not $Launch) { return }
    $folder = Get-RepoPath (Get-Prop $Launch "folder" ".")
    $ide = Get-Prop $Launch "ide" "none"
    if ($ide -eq "vscode") {
        $code = Find-Exe "code" @(
            (Join-Path $env:LOCALAPPDATA "Programs\Microsoft VS Code\bin\code.cmd"),
            (Join-Path $env:ProgramFiles "Microsoft VS Code\bin\code.cmd"))
        if ($code) { & $code $script:Root | Out-Null }
    }
    if ($ide -eq "visualstudio") {
        # Opens the solution in Visual Studio, with the file association that Windows uses for .sln files.
        $solution = Get-RepoPath (Get-Prop $Launch "solution" $null)
        if (Test-Path -LiteralPath $solution) {
            Start-Process -FilePath $solution
            Write-LogOnly "Opened $solution in Visual Studio."
        } else {
            Set-Outcome "visual-studio" "warning" "$solution wasn't found, so Visual Studio wasn't opened."
        }
    }
    if ((Get-Prop $Launch "kind" "") -eq "exe") {
        $file = Get-RepoPath (Get-Prop $Launch "file" $null)
        $logDir = Split-Path -Parent $script:LogFile
        if (Test-Path -LiteralPath $file) {
            Start-Process -FilePath $file -WorkingDirectory $folder `
                -RedirectStandardOutput (Join-Path $logDir "lesson-runner-out.log") `
                -RedirectStandardError (Join-Path $logDir "lesson-runner-err.log")
            Write-LogOnly "Started $file."
        } else {
            Set-Outcome "lesson-runner" "warning" "$file isn't built yet, so the runner wasn't started."
        }
    }
    if ((Get-Prop $Launch "kind" "") -eq "command") {
        # A long-running command, such as npm run tauri dev. It runs in the background, and its output goes to the log
        # folder, so the setup doesn't wait for it.
        $exe = Get-Prop $Launch "exe" $null
        $logDir = Split-Path -Parent $script:LogFile
        Start-Process -FilePath $exe -ArgumentList @(Get-Prop $Launch "args" @()) -WorkingDirectory $folder -WindowStyle Hidden `
            -RedirectStandardOutput (Join-Path $logDir "lesson-runner-out.log") `
            -RedirectStandardError (Join-Path $logDir "lesson-runner-err.log")
        Write-LogOnly "Started $exe in $folder."
    }
    if ((Get-Prop $Launch "kind" "") -eq "electron") {
        $exe = Join-Path $folder "node_modules\electron\dist\electron.exe"
        # npm hoists node_modules to the workspace root when the runner is a workspace, so check the repository root too.
        if (-not (Test-Path -LiteralPath $exe)) { $exe = Join-Path $script:Root "node_modules\electron\dist\electron.exe" }
        if (Test-Path -LiteralPath $exe) {
            # The runner's output goes to files. Without this, Electron writes its messages into this console and holds it.
            $logDir = Split-Path -Parent $script:LogFile
            Start-Process -FilePath $exe -ArgumentList (Get-Prop $Launch "main" ".") -WorkingDirectory $folder `
                -RedirectStandardOutput (Join-Path $logDir "lesson-runner-out.log") `
                -RedirectStandardError (Join-Path $logDir "lesson-runner-err.log")
            Write-LogOnly "Started the lesson runner from $folder."
        } else {
            Set-Outcome "lesson-runner" "warning" "Electron isn't installed in $folder, so the runner wasn't started."
        }
    }
}

# ---------------------------------------------------------------------------------------------------------------------
# Bootstrap: when setup.json is missing, clone the repository and run its setup.ps1
# ---------------------------------------------------------------------------------------------------------------------

function Get-RepoNameFromUrl([string]$Url) {
    $last = ($Url.TrimEnd('/') -split '/')[-1]
    return ($last -replace '\.git$', '')
}

function Invoke-Bootstrap {
    if (-not $RepoUrl) {
        Write-Host "setup.json isn't next to this script, so the repository has to be cloned first. Run load-tutorial.ps1, or pass -RepoUrl." -ForegroundColor Red
        exit 1
    }
    $base = Resolve-BaseDirectory $BaseDirectory $null
    Write-Host "Setting up the tutorial bootstrap in $base" -ForegroundColor Yellow

    if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
        Update-SessionPath
        if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
            try {
                Invoke-Winget @("install", "-e", "--id", "Git.Git", "--silent", "--accept-package-agreements", "--accept-source-agreements")
            } catch {
                Write-Host ("Git couldn't be installed: " + $_.Exception.Message) -ForegroundColor Red
                exit 1
            }
        }
    }
    if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
        Write-Host "Git was installed, but it isn't found yet. Open a new PowerShell window and run this script again." -ForegroundColor Red
        exit 1
    }

    $target = Join-Path $base (Get-RepoNameFromUrl $RepoUrl)
    if (-not (Test-Path -LiteralPath (Join-Path $target ".git"))) {
        & git clone $RepoUrl $target
        if ($LASTEXITCODE -ne 0) {
            Write-Host "git clone failed (exit $LASTEXITCODE). Check the address, and your access to the repository." -ForegroundColor Red
            exit 1
        }
    }

    $child = Join-Path $target "setup.ps1"
    if (-not (Test-Path -LiteralPath $child)) {
        Write-Host "The clone has no setup.ps1 at $child." -ForegroundColor Red
        exit 1
    }
    $shell = Find-Pwsh
    if (-not $shell) { $shell = "powershell.exe" }
    & $shell -NoProfile -ExecutionPolicy Bypass -File $child -BaseDirectory $base
    exit $LASTEXITCODE
}

# ---------------------------------------------------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------------------------------------------------

# ---------------------------------------------------------------------------------------------------------------------
# setup.json checks
# ---------------------------------------------------------------------------------------------------------------------

$KnownInstalls = @("git", "powershell7", "node", "python", "dotnet", "jdk", "maven", "msys2", "rust", "visualStudio", "buildTools", "vcRuntime", "vscode", "notepadPlusPlus")
$KnownStepTypes = @("npm", "npm-run", "python-venv", "command", "copy", "msbuild-restore", "write-file", "dotnet-build", "download", "verify-hashes", "check-variable")
$KnownRootSettings = @("schemaVersion", "name", "folder", "repoUrl", "branch", "baseDirectory", "runTests", "installs", "steps", "launch")
$KnownInstallSettings = @("install", "pinned", "sideBySide", "orNewer", "winget", "override", "root", "url", "workloads", "packages", "extensions")
$KnownStepSettings = @("name", "type", "folder", "enabled", "requires", "runTests", "command", "script", "path", "requirements", "exe", "args", "check", "source", "dest", "backup", "solution", "content", "contentFrom", "projects", "elevated", "url", "extract", "pathPrepend", "ifMissing", "manifest", "variable", "message")

function Test-IsText($Value) { return ($Value -is [string]) }
function Test-IsFlag($Value) { return ($Value -is [bool]) }

function Test-IsTextList($Value) {
    if ($Value -isnot [array]) { return $false }
    foreach ($item in $Value) {
        if ($item -isnot [string]) { return $false }
    }
    return $true
}

function Get-RequiredStepSettings([string]$Type) {
    switch ($Type) {
        "npm-run"         { return @("script") }
        "command"         { return @("exe") }
        "copy"            { return @("source", "dest") }
        "msbuild-restore" { return @("solution") }
        "write-file"      { return @("path") }
        "check-variable"  { return @("variable") }
        "download"        { return @("url", "path") }
        "verify-hashes"   { return @("manifest") }
        default           { return @() }
    }
}

# The JSON parser doesn't always give a line number. These checks find the most common mistakes and give line numbers.
function Get-JsonLayoutProblems([string]$Text) {
    $problems = New-Object System.Collections.ArrayList
    $lines = @($Text -split "`r?`n")
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $trimmed = $lines[$i].Trim()
        if ($trimmed -match '^(//|#)') {
            [void]$problems.Add("Line $($i + 1): comments aren't allowed in JSON. Delete the line.")
        }
        if ($trimmed -match ',$') {
            $next = ""
            for ($j = $i + 1; $j -lt $lines.Count; $j++) {
                $candidate = $lines[$j].Trim()
                if ($candidate -ne "") { $next = $candidate; break }
            }
            if ($next -match '^[\]\}]') {
                [void]$problems.Add("Line $($i + 1): a comma before '$next' isn't allowed. Remove the comma from the end of this line.")
            }
        }
    }
    return @($problems)
}

# Checks the parsed setup.json against what this script supports. Returns a list of problems, one per line.
function Get-SchemaProblems($Config) {
    $problems = New-Object System.Collections.ArrayList
    if ($Config -isnot [pscustomobject]) {
        [void]$problems.Add("setup.json must hold one JSON object, starting with { and ending with }.")
        return @($problems)
    }

    foreach ($property in $Config.PSObject.Properties) {
        if ($KnownRootSettings -notcontains $property.Name) {
            [void]$problems.Add("'$($property.Name)' isn't a setting that setup.json supports. Check the spelling.")
        }
    }

    if ((Get-Prop $Config "schemaVersion" $null) -ne 2) {
        [void]$problems.Add("'schemaVersion' must be 2.")
    }
    foreach ($key in @("name", "folder", "repoUrl")) {
        $value = Get-Prop $Config $key $null
        if (-not (Test-IsText $value) -or -not $value.Trim()) {
            [void]$problems.Add("'$key' is required, and must be text.")
        } elseif ($value -match '<|>') {
            [void]$problems.Add("'$key' still has a placeholder ($value). Replace it with the real value.")
        }
    }
    $url = Get-Prop $Config "repoUrl" ""
    if ((Test-IsText $url) -and $url -notmatch '^https://') {
        [void]$problems.Add("'repoUrl' must start with https://.")
    }
    foreach ($key in @("branch", "baseDirectory")) {
        $value = Get-Prop $Config $key $null
        if ($null -ne $value -and -not (Test-IsText $value)) { [void]$problems.Add("'$key' must be text.") }
    }
    $runTests = Get-Prop $Config "runTests" $null
    if ($null -ne $runTests -and -not (Test-IsFlag $runTests)) { [void]$problems.Add("'runTests' must be true or false.") }

    $installs = Get-Prop $Config "installs" $null
    if ($null -eq $installs -or $installs -isnot [pscustomobject]) {
        [void]$problems.Add("'installs' is required, and must be an object.")
    } else {
        foreach ($program in $installs.PSObject.Properties) {
            $programName = $program.Name
            $spec = $program.Value
            if ($KnownInstalls -notcontains $programName) {
                [void]$problems.Add("installs.$programName isn't a known program. Known programs: $($KnownInstalls -join ', ').")
                continue
            }
            if ($spec -isnot [pscustomobject]) {
                [void]$problems.Add("installs.$programName must be an object.")
                continue
            }
            foreach ($setting in $spec.PSObject.Properties) {
                $settingName = $setting.Name
                $value = $setting.Value
                $where = "installs.$programName.$settingName"
                if ($settingName -eq "minimum") {
                    [void]$problems.Add("$where was renamed to 'pinned'. Change the name in setup.json.")
                    continue
                }
                if ($KnownInstallSettings -notcontains $settingName) {
                    [void]$problems.Add("$where isn't a setting that installs supports.")
                    continue
                }
                if ($settingName -eq "install" -or $settingName -eq "sideBySide" -or $settingName -eq "orNewer") {
                    if (-not (Test-IsFlag $value)) { [void]$problems.Add("$where must be true or false.") }
                } elseif ($settingName -eq "pinned") {
                    if (-not (Test-IsText $value) -or $value -notmatch '^\d+(\.\d+){0,2}$') {
                        [void]$problems.Add("$where must be a version in quotes, such as ""24"", ""3.12"" or ""3.10.0"" (found: $value).")
                    }
                } elseif ($settingName -eq "workloads" -or $settingName -eq "packages" -or $settingName -eq "extensions") {
                    if (-not (Test-IsTextList $value)) { [void]$problems.Add("$where must be a list of text values.") }
                } elseif (-not (Test-IsText $value)) {
                    [void]$problems.Add("$where must be text.")
                }
            }
        }
    }

    $steps = Get-Prop $Config "steps" $null
    if ($null -ne $steps -and $steps -isnot [array]) {
        [void]$problems.Add("'steps' must be a list, in [ ... ].")
    }
    $number = 0
    # A step can require a program from "installs" or the name of a step that comes before it.
    $earlierStepNames = New-Object System.Collections.Generic.List[string]
    foreach ($step in @($steps)) {
        $label = "steps[$number]"
        $number++
        if ($step -isnot [pscustomobject]) {
            [void]$problems.Add("$label must be an object.")
            continue
        }
        $stepName = Get-Prop $step "name" $null
        if (Test-IsText $stepName) {
            $label = "steps[$($number - 1)] '$stepName'"
        } else {
            [void]$problems.Add("$label needs a 'name', in text.")
        }
        if (Test-IsText $stepName) { $earlierStepNames.Add($stepName) }
        foreach ($setting in $step.PSObject.Properties) {
            if ($KnownStepSettings -notcontains $setting.Name) {
                [void]$problems.Add("$label has '$($setting.Name)', which isn't a step setting.")
            }
        }
        $type = Get-Prop $step "type" $null
        if ($KnownStepTypes -notcontains $type) {
            [void]$problems.Add("$label has type '$type'. Known types: $($KnownStepTypes -join ', ').")
            continue
        }
        foreach ($required in @(Get-RequiredStepSettings $type)) {
            if (-not (Test-IsText (Get-Prop $step $required $null))) {
                [void]$problems.Add("$label ($type) needs '$required', in text.")
            }
        }
        if ($type -eq "write-file" -and $null -eq (Get-Prop $step "content" $null) -and $null -eq (Get-Prop $step "contentFrom" $null)) {
            [void]$problems.Add("$label (write-file) needs 'content', or 'contentFrom' naming an installed program.")
        }
        $projects = Get-Prop $step "projects" $null
        if ($type -eq "dotnet-build" -and ($null -eq $projects -or -not (Test-IsTextList @($projects)))) {
            [void]$problems.Add("$label (dotnet-build) needs 'projects', a list of project paths.")
        }
        foreach ($flag in @("enabled", "runTests", "backup", "elevated")) {
            $value = Get-Prop $step $flag $null
            if ($null -ne $value -and -not (Test-IsFlag $value)) { [void]$problems.Add("$label '$flag' must be true or false.") }
        }
        $requires = Get-Prop $step "requires" $null
        if ($null -ne $requires) {
            if (-not (Test-IsTextList @($requires))) {
                [void]$problems.Add("$label 'requires' must be a list of program names.")
            } else {
                foreach ($need in @($requires)) {
                    if (($KnownInstalls -notcontains $need) -and ($earlierStepNames -notcontains $need)) {
                        [void]$problems.Add("$label requires '$need', which isn't a program in installs or an earlier step.")
                    }
                }
            }
        }
    }

    $launch = Get-Prop $Config "launch" $null
    if ($null -ne $launch) {
        if ($launch -isnot [pscustomobject]) {
            [void]$problems.Add("'launch' must be an object.")
        } else {
            $ide = Get-Prop $launch "ide" "none"
            if (@("none", "vscode", "visualstudio") -notcontains $ide) { [void]$problems.Add("launch.ide must be 'none', 'vscode', or 'visualstudio'.") }
            if ($ide -eq "visualstudio" -and -not (Test-IsText (Get-Prop $launch "solution" $null))) {
                [void]$problems.Add("launch.ide 'visualstudio' needs 'solution', the .sln path in the repository.")
            }
            $kind = Get-Prop $launch "kind" "none"
            if (@("none", "electron", "exe", "command") -notcontains $kind) { [void]$problems.Add("launch.kind must be 'none', 'electron', 'exe', or 'command'.") }
            if ($kind -eq "command" -and -not (Get-Prop $launch "exe" $null)) { [void]$problems.Add("launch.kind 'command' needs 'exe', the program to run.") }
            if ($kind -eq "exe" -and -not (Test-IsText (Get-Prop $launch "file" $null))) {
                [void]$problems.Add("launch.kind 'exe' needs 'file', the built program's path in the repository.")
            }
        }
    }
    return @($problems)
}

# Reads and checks setup.json. Returns Config (or $null) and Problems (empty when the file is good).
function Read-Config([string]$Path) {
    $problems = New-Object System.Collections.ArrayList
    if (-not (Test-Path -LiteralPath $Path)) {
        return [pscustomobject]@{ Config = $null; Problems = @("setup.json wasn't found at $Path.") }
    }
    try {
        $text = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    } catch {
        return [pscustomobject]@{ Config = $null; Problems = @("setup.json couldn't be read at $Path. $($_.Exception.Message)") }
    }
    if (-not $text -or -not $text.Trim()) {
        return [pscustomobject]@{ Config = $null; Problems = @("setup.json at $Path is empty.") }
    }

    foreach ($item in (Get-JsonLayoutProblems $text)) { [void]$problems.Add($item) }

    $config = $null
    try {
        $config = ConvertFrom-Json -InputObject $text
    } catch {
        [void]$problems.Add("setup.json isn't valid JSON. The parser said: $($_.Exception.Message)")
        [void]$problems.Add("The parser message doesn't always give a line number. Look for a missing quote, comma, or bracket. Opening the file in VS Code shows JSON errors with line numbers.")
        return [pscustomobject]@{ Config = $null; Problems = @($problems) }
    }
    foreach ($item in (Get-SchemaProblems $config)) { [void]$problems.Add($item) }
    return [pscustomobject]@{ Config = $config; Problems = @($problems) }
}

# ---------------------------------------------------------------------------------------------------------------------
# Repository checks: the folder name, the base directory, the origin URL and the branch from setup.json
# ---------------------------------------------------------------------------------------------------------------------

function Get-NormalizedUrl([string]$Url) {
    $normalized = $Url.Trim().ToLowerInvariant().TrimEnd('/')
    if ($normalized.EndsWith(".git")) { $normalized = $normalized.Substring(0, $normalized.Length - 4) }
    return $normalized
}

function Confirm-Repository {
    $folderName = Split-Path -Leaf $script:Root
    $expectedFolder = Get-Prop $script:Config "folder" $null
    if ($expectedFolder -and $expectedFolder -ne $folderName) {
        Set-Outcome "repository" "warning" "setup.json expects the folder '$expectedFolder', but this copy is in '$folderName'. Paths and logs may not match the other repos."
    }
    if (-not $script:Root.StartsWith($script:BaseDir, [System.StringComparison]::OrdinalIgnoreCase)) {
        Set-Outcome "repository" "warning" "This repository isn't under the base directory $($script:BaseDir)."
    }

    if (-not (Test-Path -LiteralPath (Join-Path $script:Root ".git"))) {
        Set-Outcome "repository" "warning" "This folder isn't a git clone, so its URL and branch can't be checked."
        return
    }
    if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
        Set-Outcome "repository" "warning" "git isn't available, so the URL and branch can't be checked."
        return
    }

    Push-Location -LiteralPath $script:Root
    try {
        $origin = (& git remote get-url origin 2>$null | Out-String).Trim()
        $expectedUrl = Get-Prop $script:Config "repoUrl" $null
        if ($origin -and $expectedUrl -and ((Get-NormalizedUrl $origin) -ne (Get-NormalizedUrl $expectedUrl))) {
            Write-Log "" "Yellow"
            Write-Log "The clone's origin is $origin." "Yellow"
            Write-Log "setup.json expects $expectedUrl." "Yellow"
            Write-Log "   1. Keep the clone's origin" "Yellow"
            Write-Log "   2. Use setup.json's repoUrl, and point the clone's origin at it" "Yellow"
            Write-Log "   3. Stop" "Yellow"
            $answer = (Read-Host "Choose 1, 2, or 3").Trim()
            switch ($answer) {
                "1" {
                    Set-Outcome "repository" "warning" "Kept origin $origin, which differs from repoUrl in setup.json."
                }
                "2" {
                    & git remote set-url origin $expectedUrl
                    if ($LASTEXITCODE -ne 0) {
                        Write-Log "Couldn't change the origin to $expectedUrl. Stopped." "Red"
                        exit 1
                    }
                    Set-Outcome "repository" "ok" "Origin changed to $expectedUrl, as setup.json says."
                }
                default {
                    Write-Log "Stopped. Point the clone at the right repository, or correct repoUrl in setup.json, then run this script again." "Red"
                    exit 1
                }
            }
        }

        $expectedBranch = Get-Prop $script:Config "branch" $null
        if ($expectedBranch) {
            $current = (& git rev-parse --abbrev-ref HEAD 2>$null | Out-String).Trim()
            if ($current -ne $expectedBranch) {
                $changes = (& git status --porcelain 2>$null | Out-String).Trim()
                if ($changes) {
                    Set-Outcome "repository" "warning" "The clone is on '$current', and setup.json expects '$expectedBranch'. It has uncommitted changes, so it wasn't switched."
                } else {
                    & git checkout $expectedBranch 2>&1 | Out-Null
                    if ($LASTEXITCODE -ne 0) {
                        & git checkout -t ("origin/" + $expectedBranch) 2>&1 | Out-Null
                    }
                    if ($LASTEXITCODE -eq 0) {
                        Write-Log "   Switched the clone from '$current' to '$expectedBranch', as setup.json expects."
                    } else {
                        Set-Outcome "repository" "warning" "The clone is on '$current', and setup.json expects '$expectedBranch'. Switching failed."
                    }
                }
            }
        }
    } finally {
        Pop-Location
    }
}

# Items and steps that need an elevated PowerShell. An install that is already satisfied doesn't count.
function Test-ElevatedItemSatisfied([string]$Name, $Spec) {
    switch ($Name) {
        "visualStudio" {
            $instances = @(Get-VsInstances | Where-Object { $_.productId -match 'Product\.(Professional|Enterprise|Community)$' })
            if ($instances.Count -eq 0) { return $false }
            foreach ($workload in @(Get-Prop $Spec "workloads" @())) {
                if (-not (Test-VsWorkload $workload)) { return $false }
            }
            return $true
        }
        "buildTools" {
            return (Test-VsWorkload "Microsoft.VisualStudio.Component.VC.Tools.x86.x64")
        }
        "vcRuntime" {
            $system = [Environment]::GetFolderPath("System")
            return ((Test-Path -LiteralPath (Join-Path $system "VCRUNTIME140.dll")) -and (Test-Path -LiteralPath (Join-Path $system "MSVCP140.dll")))
        }
        default { return $true }
    }
}

function Get-ElevationNeeds {
    $needs = New-Object System.Collections.ArrayList
    $installs = Get-Prop $script:Config "installs" $null
    if (-not $SkipInstall) {
        foreach ($name in @("visualStudio", "buildTools", "vcRuntime")) {
            $spec = Get-Prop $installs $name $null
            if (-not [bool](Get-Prop $spec "install" $false)) { continue }
            if (-not (Test-ElevatedItemSatisfied $name $spec)) { [void]$needs.Add("install: $name") }
        }
    }
    foreach ($step in @(Get-Prop $script:Config "steps" @())) {
        if (-not (Get-Prop $step "elevated" $false)) { continue }
        if (-not (Get-Prop $step "enabled" $true)) { continue }
        if ((Get-Prop $step "runTests" $false) -and -not $script:RunTestsEffective) { continue }
        [void]$needs.Add("step: " + (Get-Prop $step "name" "(unnamed)"))
    }
    return @($needs)
}

if (-not (Test-Path -LiteralPath (Join-Path $PSScriptRoot "setup.json"))) {
    Invoke-Bootstrap
}

$configResult = Read-Config (Join-Path $PSScriptRoot "setup.json")
if ($configResult.Problems.Count -gt 0) {
    Write-Host "setup.json has problems. Fix these, then run setup.ps1 again:" -ForegroundColor Red
    foreach ($problem in $configResult.Problems) {
        Write-Host ("   - " + $problem) -ForegroundColor Red
    }
    exit 1
}
$script:Config = $configResult.Config

$script:BaseDir = Resolve-BaseDirectory $BaseDirectory $script:Config
$folderName = Split-Path -Leaf $script:Root
$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
Set-LogFile (Join-Path $script:BaseDir ("logs\setup-" + $folderName + "-" + $stamp + ".log"))
$script:RunTestsEffective = ([bool](Get-Prop $script:Config "runTests" $true)) -and -not $SkipTests

$title = Get-Prop $script:Config "name" $folderName
Write-Log "$title setup" "Yellow"
Write-Log "Repository: $script:Root"
Write-Log "Base directory: $script:BaseDir"
Write-Log "Log: $script:LogFile"
$elevatedNeeds = @(Get-ElevationNeeds)
if ($elevatedNeeds.Count -gt 0 -and -not (Test-Administrator)) {
    Write-Log "This setup needs an elevated PowerShell (Run as administrator) for:" "Red"
    foreach ($need in $elevatedNeeds) { Write-Log ("   - " + $need) "Red" }
    Write-Log "Nothing was installed. Open PowerShell as Administrator, and run this script again." "Red"
    exit 1
}

Write-Stage "Installing programs"
Invoke-InstallItem "git"
if (-not $RelaunchedInPowerShell7) { Confirm-Repository }
Invoke-InstallItem "powershell7"

# Windows PowerShell 5.1 can run this script, but the tutorial uses PowerShell 7. Restart in PowerShell 7 once it's installed.
$pwsh = Find-Pwsh
if ($PSVersionTable.PSVersion.Major -lt 7 -and -not $RelaunchedInPowerShell7 -and $pwsh) {
    Write-Log "Restarting the setup in PowerShell 7."
    $forward = @("-NoProfile", "-ExecutionPolicy", "Bypass", "-File", $PSCommandPath, "-RelaunchedInPowerShell7")
    if ($BaseDirectory) { $forward += @("-BaseDirectory", $BaseDirectory) }
    if ($RepoUrl) { $forward += @("-RepoUrl", $RepoUrl) }
    if ($SkipInstall) { $forward += "-SkipInstall" }
    if ($SkipTests) { $forward += "-SkipTests" }
    if ($NoLaunch) { $forward += "-NoLaunch" }
    & $pwsh @forward
    exit $LASTEXITCODE
}

foreach ($name in @("node", "python", "dotnet", "jdk", "maven", "msys2", "rust", "visualStudio", "buildTools", "vcRuntime", "vscode", "notepadPlusPlus")) {
    Invoke-InstallItem $name
}

Write-Stage "Setting up the repository"
foreach ($step in @(Get-Prop $script:Config "steps" @())) {
    if (Get-Prop $step "runTests" $false) { continue }   # tests run after the setup steps
    Invoke-Step $step
}

if ($script:RunTestsEffective) {
    Write-Stage "Running tests"
    foreach ($step in @(Get-Prop $script:Config "steps" @())) {
        if (-not (Get-Prop $step "runTests" $false)) { continue }
        Invoke-Step $step
    }
}

if (-not $NoLaunch) {
    # A runner started after a failed step would open a broken window, so it's held back until the setup passes.
    if (@($script:Problems | Where-Object { $_.Status -eq "failed" }).Count -gt 0) {
        Write-Stage "Not starting the lesson runner, because a step failed. Fix it, then run the script again."
    } else {
        Write-Stage "Starting the lesson runner"
        Start-Runner (Get-Prop $script:Config "launch" $null)
    }
}

Write-Log ""
Write-Log "Summary" "Yellow"
foreach ($item in $script:Outcomes) {
    switch ($item.Status) {
        "failed"  { $color = "Red" }
        "warning" { $color = "Yellow" }
        "skipped" { $color = "DarkGray" }
        default   { $color = "Green" }
    }
    Write-Log ("   {0,-28} {1,-10} {2}" -f $item.Name, $item.Status, $item.Detail).TrimEnd() $color
}
Write-Log ""
$failed = @($script:Problems | Where-Object { $_.Status -eq "failed" })
$warnings = @($script:Problems | Where-Object { $_.Status -eq "warning" })
if ($failed.Count -eq 0 -and $warnings.Count -eq 0) {
    Write-Log "Setup finished with no problems." "Green"
} else {
    # The problems are already listed in the summary above, so this line only counts them.
    Write-Log ("Setup finished with {0} failure(s) and {1} warning(s), listed in the summary above." -f $failed.Count, $warnings.Count) "Yellow"
}
Write-Log "Full log: $script:LogFile"

if ($failed.Count -gt 0) { exit 1 }
exit 0
