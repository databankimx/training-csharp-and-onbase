# Script to sync wwwroot folders between Training Navigator and AuthGateway projects
# Excludes index.html from sync

param(
	[ValidateSet('Training', 'AuthGateway', 'Interactive')]
	[string]$Authoritative = 'Interactive',

	[switch]$Preview,

	[bool]$SkipIndexHtml = $true,

	[switch]$Verbose
)

# Define paths
$trainingPath = "C:\Development\training\developer-training\EForms.TrainingNavigator\wwwroot"
$authgatewayPath = "C:\Development\eforms-navigator-authgateway\EFormsNavigator.AuthGateway\wwwroot"

# Files to exclude from sync
$excludePatterns = @()
if ($SkipIndexHtml) {
	$excludePatterns += "index.html"
}
# Exclude old images folder
$excludePatterns += "old-Images"

function Test-Excluded {
	param([string]$FilePath)
	foreach ($pattern in $script:excludePatterns) {
		if ($FilePath -like "*$pattern*") {
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

# Check differences in common files using hash verification
$different = @()
Write-Host "Comparing files..." -ForegroundColor DarkGray
$fileCount = 0
$totalFiles = @($inBoth).Count

foreach ($file in $inBoth) {
	$fileCount++
	if ($fileCount % 50 -eq 0) {
		Write-Host "  Processed $fileCount of $totalFiles files..." -ForegroundColor DarkGray
	}

	$trainingFile = $trainingFiles[$file][0]
	$authgatewayFile = $authgatewayFiles[$file][0]

	# Use PowerShell's built-in Get-FileHash (SHA256 by default)
	$trainingHash = (Get-FileHash -Path $trainingFile.FullPath -Algorithm SHA256 -ErrorAction SilentlyContinue).Hash
	$authgatewayHash = (Get-FileHash -Path $authgatewayFile.FullPath -Algorithm SHA256 -ErrorAction SilentlyContinue).Hash

	if ($trainingHash -ne $authgatewayHash) {
		$different += [PSCustomObject]@{
			File = $file
			TrainingSize = $trainingFile.Size
			AuthGatewaySize = $authgatewayFile.Size
			TrainingModified = $trainingFile.Modified
			AuthGatewayModified = $authgatewayFile.Modified
			TrainingNewer = $trainingFile.Modified -gt $authgatewayFile.Modified
			TrainingHash = $trainingHash
			AuthGatewayHash = $authgatewayHash
		}
	}
}

# Display results
if ($Verbose) {
	Write-Host "Results:" -ForegroundColor Cyan
	Write-Host "  Total files scanned (excluding index.html): $($inBoth.Count)" -ForegroundColor Gray
	Write-Host "  Files only in Training: $($onlyInTraining.Count)" -ForegroundColor Yellow
	Write-Host "  Files only in AuthGateway: $($onlyInAuthGateway.Count)" -ForegroundColor Yellow
	Write-Host "  Files with DIFFERENT content: $($different.Count)" -ForegroundColor Cyan
	if ($different.Count -eq 0) {
		Write-Host "  Identical files: $($inBoth.Count)" -ForegroundColor Green
	} else {
		Write-Host "  Identical files: $($inBoth.Count - $different.Count)" -ForegroundColor Green
	}
	Write-Host ""

	if ($different.Count -gt 0) {
		Write-Host "Files with differences that will be synced:" -ForegroundColor Yellow
		Write-Host ""
		$different | Format-Table -Property @(
			@{Label = "File"; Expression = {$_.File}; Width = 50},
			@{Label = "Training"; Expression = {"{0} bytes ({1:g}" -f $_.TrainingSize, $_.TrainingModified}; Width = 30},
			@{Label = "AuthGateway"; Expression = {"{0} bytes ({1:g}" -f $_.AuthGatewaySize, $_.AuthGatewayModified}; Width = 30},
			@{Label = "Newer"; Expression = {if ($_.TrainingNewer) { "Training" } else { "AuthGateway" }}; Width = 15}
		) | Out-String | Write-Host
	}
} else {
	# Summary mode - show one-line status
	Write-Host "Status: Files scanned=$($inBoth.Count), Differences=$($different.Count), Only in Training=$($onlyInTraining.Count), Only in AuthGateway=$($onlyInAuthGateway.Count)" -ForegroundColor Gray
}

if ($Verbose) {
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
	if ($Verbose) {
		Write-Host ""
	}
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
if ($different.Count -eq 0) {
	Write-Host "Syncing from: $Authoritative" -ForegroundColor Green
	Write-Host "(No files need updating - all files are identical)" -ForegroundColor Green
} else {
	Write-Host "Syncing from: $Authoritative (updating $($different.Count) file(s))" -ForegroundColor Green
}
Write-Host ""

# Sync only the different files
if ($different.Count -gt 0) {
	$syncedCount = 0
	foreach ($file in $different) {
		if ($Authoritative -eq 'Manual') {
			Write-Host "Update $($file.File)?" -ForegroundColor Yellow
			Write-Host "  Training: $($file.TrainingSize) bytes, Modified: $($file.TrainingModified)"
			Write-Host "  AuthGateway: $($file.AuthGatewaySize) bytes, Modified: $($file.AuthGatewayModified)"
			Write-Host "  Hashes: Training=$($file.TrainingHash.Substring(0, 8))... AuthGateway=$($file.AuthGatewayHash.Substring(0, 8))..."
			Write-Host "  1 = Copy from Training, 2 = Copy from AuthGateway, S = Skip"

			$fileChoice = Read-Host "Choice"

			switch ($fileChoice.ToUpper()) {
				'1' {
					$source = Join-Path $trainingPath $file.File
					$dest = Join-Path $authgatewayPath $file.File
					Copy-Item -Path $source -Destination $dest -Force
					Write-Host "  [DONE] Copied from Training to AuthGateway" -ForegroundColor Green
					$syncedCount++
				}
				'2' {
					$source = Join-Path $authgatewayPath $file.File
					$dest = Join-Path $trainingPath $file.File
					Copy-Item -Path $source -Destination $dest -Force
					Write-Host "  [DONE] Copied from AuthGateway to Training" -ForegroundColor Green
					$syncedCount++
				}
				default {
					Write-Host "  [SKIP] Skipped" -ForegroundColor Gray
				}
			}
		} else {
			if ($Authoritative -eq 'Training') {
				$source = Join-Path $trainingPath $file.File
				$dest = Join-Path $authgatewayPath $file.File
				$destLabel = "AuthGateway"
			} else {
				$source = Join-Path $authgatewayPath $file.File
				$dest = Join-Path $trainingPath $file.File
				$destLabel = "Training"
			}

			# Verify before copying
			$sourceHash = (Get-FileHash -Path $source -Algorithm SHA256 -ErrorAction SilentlyContinue).Hash
			if ($sourceHash -ne $file.TrainingHash -and $sourceHash -ne $file.AuthGatewayHash) {
				Write-Host "[WARN] Source file hash mismatch for $($file.File) - skipping" -ForegroundColor Red
				continue
			}

			Copy-Item -Path $source -Destination $dest -Force
			Write-Host "[DONE] Synced: $($file.File) to $destLabel" -ForegroundColor Green
			$syncedCount++
		}
	}

	Write-Host ""
	Write-Host "Sync Summary:" -ForegroundColor Cyan
	Write-Host "  Total files to sync: $($different.Count)" -ForegroundColor Gray
	Write-Host "  Files synced: $syncedCount" -ForegroundColor Green
	if ($syncedCount -lt $different.Count) {
		Write-Host "  Files skipped: $($different.Count - $syncedCount)" -ForegroundColor Yellow
	}
} else {
	Write-Host "No files to sync - everything is already in sync!" -ForegroundColor Green
}

if (-not $Verbose) {
	if ($onlyInTraining.Count -gt 0 -or $onlyInAuthGateway.Count -gt 0) {
		Write-Host ""
		Write-Host "Note: Files only in one location were not synced." -ForegroundColor Yellow
		Write-Host "To sync these, you can manually delete them from the target, or use:" -ForegroundColor Gray
		Write-Host "  Sync-wwwroot.ps1 -Authoritative Training  # to copy missing files from Training to AuthGateway" -ForegroundColor Gray
	}
}

Write-Host ""
Write-Host "Sync complete!" -ForegroundColor Green
