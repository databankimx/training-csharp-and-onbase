# wwwroot Sync Solution

This solution provides tools to keep the wwwroot folders synchronized between the Training Navigator and AuthGateway projects.

## Files Included

- **Sync-wwwroot.ps1** - Main PowerShell sync script
- **Sync-wwwroot.bat** - Batch wrapper for easy execution
- **README.md** - This file

## Usage

### Option 1: Run from Command Line (Easiest)

```bash
# Preview changes without making them
.\Sync-wwwroot.bat -Preview

# Sync from Training Navigator to AuthGateway (interactive - asks which is authoritative)
.\Sync-wwwroot.bat

# Specify authoritative source (no prompting)
# Use Training Navigator as source
.\Sync-wwwroot.ps1 -Authoritative Training

# Use AuthGateway as source
.\Sync-wwwroot.ps1 -Authoritative AuthGateway

# Interactive per-file selection
.\Sync-wwwroot.ps1 -Authoritative Manual
```

### Option 2: Integrate with Visual Studio

You can add the sync script as an External Tool in Visual Studio for quick access:

1. Go to **Tools > External Tools**
2. Click **Add**
3. Fill in:
   - **Title:** `wwwroot Sync (Training -> AuthGateway)`
   - **Command:** `powershell.exe`
   - **Arguments:** `-NoProfile -ExecutionPolicy Bypass -File "C:\Development\training\developer-training\Sync-wwwroot.ps1" -Authoritative Training`
   - **Initial directory:** `C:\Development\training\developer-training`
   - Check: **Use Output Window**
   - Check: **Close on exit**
4. Click **OK**

You can create multiple entries with different sources (Training vs AuthGateway).

### Option 3: Windows Task Scheduler (Automated)

Set up automatic sync on a schedule:

1. Open **Task Scheduler**
2. Create Basic Task
3. Set trigger (e.g., daily, on logon, etc.)
4. Set action: `powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Development\training\developer-training\Sync-wwwroot.ps1" -Authoritative Training`
5. Press OK

## Features

✓ **Excludes index.html** - This file is intentionally NOT synced (it differs between projects)

✓ **Comparison reporting** - Shows:
  - Files that differ between locations
  - File sizes and modification dates
  - Which file is newer
  - Files only in one location

✓ **Preview mode** - Run with `-Preview` flag to see what would change without making changes

✓ **Multiple sync modes**:
  - **Training** - Copy from Training Navigator to AuthGateway
  - **AuthGateway** - Copy from AuthGateway to Training Navigator
  - **Manual** - Choose for each file interactively
  - **Interactive** - Prompts you to choose the authoritative source

✓ **Safe operation** - Always backs up before overwriting (recommended to use source control)

## Excluded Files

By default, the following files are excluded from sync:
- `index.html` - Intentionally kept different; has ToU content in AuthGateway

To exclude additional patterns, edit the `$excludePatterns` array in Sync-wwwroot.ps1:

```powershell
$excludePatterns = @(
	"index.html",
	"*.log",
	"custom-file.css"
)
```

## Recommended Workflow

1. **Make changes** to files in either project's wwwroot (except index.html)
2. **Before committing**, run: `.\Sync-wwwroot.ps1 -Authoritative Training` (or your authoritative source)
3. **Review changes** in source control
4. **Commit both projects**

## Troubleshooting

**PowerShell execution policy error:**
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

**File locked error:**
- Close Visual Studio or iisexpress instance
- Or use `-Preview` mode to see what would change

**Want to manually exclude files from being synced:**
Edit the `$excludePatterns` array in `Sync-wwwroot.ps1`

## Alternative: Symbolic Links

If you prefer automatic real-time sync, you can use symbolic links (Windows 10+):

```powershell
# Run as Administrator
# Remove old directories first if needed
cmd /c mklink /D "C:\Development\eforms-navigator-authgateway\EFormsNavigator.AuthGateway\wwwroot\10-databank-branding" "C:\Development\training\developer-training\EForms.TrainingNavigator\wwwroot\10-databank-branding"
```

**Pros:** Real-time sync, single source of truth
**Cons:** Requires admin, both projects must be on same machine

## Alternative: Git Submodules

You could also create a shared repository for wwwroot content, but given the different folder structures, the script approach is simpler.

## Questions?

Check the script comments for more details on how it works.
