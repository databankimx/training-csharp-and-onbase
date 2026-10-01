# Script to sync wwwroot folders between Training Navigator and AuthGateway projects
# Excludes index.html from sync

param(
	[ValidateSet('Training', 'AuthGateway', 'Interactive')]
	[string]$Authoritative = 'Interactive',

	[switch]$Preview,

	[bool]$SkipIndexHtml = $true
)

# Define paths
$trainingPath = "C:\Development\training\developer-training\EForms.TrainingNavigator\wwwroot"
$authgatewayPath = "C:\Development\eforms-navigator-authgateway\EFormsNavigator.AuthGateway\wwwroot"

# Files to exclude from sync
$excludePatterns = @()
if ($SkipIndexHtml) {
	$excludePatterns += "index.html"
}

function Test-Excluded {
	param([string]$FilePath)
	foreach ($pattern in $excludePatterns) {
		if ($FilePath -like "*$pattern") {
			return $true
		}
	}
	return $false
}

function Get-AllFiles {
	param([string]$BasePath)
	Get-ChildItem -Path $BasePath -Recurse -File | 
	Where-Object { -not (Test-Excluded $_.FullName) } |
	ForEach-Object {
		$relative = $_.FullName.Replace($BasePath + "\", "")
		[PSCustomObject]@{
			Name = $_.Name
			RelativePath = $relative
			FullPath = $_.FullName
			Size = $_.Length
			Modified = $_.LastWriteTime
		}
	}
}

Write-Host "=== wwwroot Sync Tool ===" -ForegroundColor Blue
Write-Host "Training Path:   $trainingPath"
Write-Host "AuthGateway Path: $authgatewayPath"
Write-Host ""

# Get all files
Write-Host "Scanning files..." -ForegroundColor Yellow
$trainingFiles = Get-AllFiles $trainingPath | Group-Object -Property RelativePath -AsHashTable
$authgatewayFiles = Get-AllFiles $authgatewayPath | Group-Object -Property RelativePath -AsHashTable

# Compare
$onlyInTraining = $trainingFiles.Keys | Where-Object { -not $authgatewayFiles.ContainsKey($_) }
$onlyInAuthGateway = $authgatewayFiles.Keys | Where-Object { -not $trainingFiles.ContainsKey($_) }
$inBoth = $trainingFiles.Keys | Where-Object { $authgatewayFiles.ContainsKey($_) }

# Check differences in common files
$different = @()
foreach ($file in $inBoth) {
	$trainingFile = $trainingFiles[$file][0]
	$authgatewayFile = $authgatewayFiles[$file][0]

	$trainingContent = Get-Content -Path $trainingFile.FullPath -Raw -ErrorAction SilentlyContinue
	$authgatewayContent = Get-Content -Path $authgatewayFile.FullPath -Raw -ErrorAction SilentlyContinue

	if ($trainingContent -ne $authgatewayContent) {
		$different += [PSCustomObject]@{
			File = $file
			TrainingSize = $trainingFile.Size
			AuthGatewaySize = $authgatewayFile.Size
			TrainingModified = $trainingFile.Modified
			AuthGatewayModified = $authgatewayFile.Modified
			TrainingNewer = $trainingFile.Modified -gt $authgatewayFile.Modified
		}
	}
}

# Display results
Write-Host "Results:" -ForegroundColor Cyan
Write-Host "  Total files (excluding index.html): $($inBoth.Count)" -ForegroundColor Gray
Write-Host "  Files only in Training: $($onlyInTraining.Count)" -ForegroundColor Yellow
Write-Host "  Files only in AuthGateway: $($onlyInAuthGateway.Count)" -ForegroundColor Yellow
Write-Host "  Files with different content: $($different.Count)" -ForegroundColor Cyan
Write-Host ""

if ($different.Count -gt 0) {
	Write-Host "Files with differences:" -ForegroundColor Yellow
	Write-Host ""
	$different | Format-Table -Property @(
		@{Label = "File"; Expression = {$_.File}; Width = 50},
		@{Label = "Training"; Expression = {"{0} bytes ({1:g}" -f $_.TrainingSize, $_.TrainingModified}; Width = 30},
		@{Label = "AuthGateway"; Expression = {"{0} bytes ({1:g}" -f $_.AuthGatewaySize, $_.AuthGatewayModified}; Width = 30},
		@{Label = "Newer"; Expression = {if ($_.TrainingNewer) { "Training" } else { "AuthGateway" }}; Width = 15}
	) | Out-String | Write-Host
}

if ($onlyInTraining.Count -gt 0) {
	Write-Host "Only in Training:" -ForegroundColor Yellow
	$onlyInTraining | ForEach-Object { Write-Host "  $_" }
	Write-Host ""
}

if ($onlyInAuthGateway.Count -gt 0) {
	Write-Host "Only in AuthGateway:" -ForegroundColor Yellow
	$onlyInAuthGateway | ForEach-Object { Write-Host "  $_" }
	Write-Host ""
}

# Sync logic
if ($different.Count -eq 0 -and $onlyInTraining.Count -eq 0 -and $onlyInAuthGateway.Count -eq 0) {
	Write-Host "All files are in sync! No action needed." -ForegroundColor Green
	exit 0
}

if ($Preview) {
	Write-Host "Running in preview mode. No files were changed." -ForegroundColor Green
	exit 0
}

# Determine authoritative source
if ($Authoritative -eq 'Interactive') {
	Write-Host ""
	Write-Host "Which solution is authoritative?" -ForegroundColor Cyan
	Write-Host "  1 = Training Navigator (default)"
	Write-Host "  2 = AuthGateway"
	Write-Host "  3 = Manual selection per file"
	Write-Host "  Q = Quit without syncing"

	$choice = Read-Host "Enter choice"

	switch ($choice.ToUpper()) {
		'1' { $Authoritative = 'Training' }
		'2' { $Authoritative = 'AuthGateway' }
		'3' { $Authoritative = 'Manual' }
		'Q' { 
			Write-Host "Cancelled." -ForegroundColor Yellow
			exit 0 
		}
		default { 
			Write-Host "Invalid choice. Using Training as default." -ForegroundColor Yellow
			$Authoritative = 'Training' 
		}
	}
}

Write-Host ""
Write-Host "Syncing from: $Authoritative" -ForegroundColor Green
Write-Host ""

# Sync different files
if ($different.Count -gt 0) {
	foreach ($file in $different) {
		if ($Authoritative -eq 'Manual') {
			Write-Host "Update $($file.File)?" -ForegroundColor Yellow
			Write-Host "  Training: $($file.TrainingSize) bytes, Modified: $($file.TrainingModified)"
			Write-Host "  AuthGateway: $($file.AuthGatewaySize) bytes, Modified: $($file.AuthGatewayModified)"
			Write-Host "  1 = Copy from Training, 2 = Copy from AuthGateway, S = Skip"

			$fileChoice = Read-Host "Choice"

			switch ($fileChoice.ToUpper()) {
				'1' {
					$source = Join-Path $trainingPath $file.File
					$dest = Join-Path $authgatewayPath $file.File
					Copy-Item -Path $source -Destination $dest -Force
					Write-Host "  Copied from Training to AuthGateway" -ForegroundColor Green
				}
				'2' {
					$source = Join-Path $authgatewayPath $file.File
					$dest = Join-Path $trainingPath $file.File
					Copy-Item -Path $source -Destination $dest -Force
					Write-Host "  Copied from AuthGateway to Training" -ForegroundColor Green
				}
				default {
					Write-Host "  Skipped" -ForegroundColor Gray
				}
			}
		} else {
			if ($Authoritative -eq 'Training') {
				$source = Join-Path $trainingPath $file.File
				$dest = Join-Path $authgatewayPath $file.File
			} else {
				$source = Join-Path $authgatewayPath $file.File
				$dest = Join-Path $trainingPath $file.File
			}

			Copy-Item -Path $source -Destination $dest -Force
			Write-Host "Synced: $($file.File)" -ForegroundColor Green
		}
	}
}

# Sync files only in one location
if ($onlyInTraining.Count -gt 0 -or $onlyInAuthGateway.Count -gt 0) {
	Write-Host ""
	Write-Host "Note: Files only in one location were not synced." -ForegroundColor Yellow
	Write-Host "To sync these, you can manually delete them from the target, or use:" -ForegroundColor Gray
	Write-Host "  Sync-wwwroot.ps1 -Authoritative Training  # to copy missing files from Training to AuthGateway" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Sync complete!" -ForegroundColor Green
