# Quickstart

This guide gets the C# training solution ready to use in Visual Studio. Follow the steps in order.

---

## Before you start: get NuGet access from the Development Team

The solution uses packages from DataBank's private NuGet feeds, including `Hyland.Unity`, `DBIMX.Extensions`, and `Databank.Logging`. Restore can't download them without credentials, and the setup script stops at its NuGet step until they're configured.

**Contact the DataBank Development Team** (development@databankimx.com) before you run the script. They will:

- confirm your account has access to the DataBank NuGet feed on GitHub Enterprise
- help you add the feed to your user-level `NuGet.config`, with your own credentials

The script doesn't store credentials, so this step has to be done by you, with the team's help.

**Check:** the setup script's step 4 reports `A DataBank NuGet source is configured.`

---

## Quick setup with the script

### What gets installed

By default, the setup installs:

* Notepad++
* Git
* PowerShell 7
* .NET SDK 10
* Visual Studio 17 Community with: ManagedDesktop, NetWeb, 4.8.TargetingPack

The setup then runs these steps: Restore the solution; Build LessonRunner and LessonRunner.Wpf. At the end it opens the solution in Visual Studio and starts the lesson runner program.

### Getting started

There are three methods available to set up this tutorial. They are currently Windows-only.

#### Full Automation (if you don't have Git currently set up)

This process will install Git and copy the repository to a subfolder of `C:\Development\training` and install all necessary software and extensions for the tutorial's lesson runner.

1. Obtain the tutorial's GitHub slug name, which is `training-csharp-and-onbase`
2. Open PowerShell using "Run as Administrator"
3. Run the script below

```pwsh
# Obtain the load tutorial script
irm https://raw.githubusercontent.com/databankimx/training-shared-template/main/load-tutorial.ps1 -OutFile "$env:TEMP\load-tutorial.ps1"

# Perform automated setup
powershell -NoProfile -ExecutionPolicy Bypass -File "$env:TEMP\load-tutorial.ps1" -RepoUrl https://github.com/databankimx/training-csharp-and-onbase.git
```

#### Automated Setup from Local Repo (assumes you already have Git installed and set up)

In this process, you pull the existing repository and then run its setup script, which installs all necessary software and extensions for the tutorial's lesson runner.

1. Obtain the tutorial's GitHub slug name, which is `training-csharp-and-onbase`
2. Open PowerShell using "Run as Administrator"
3. If you will use the default base directory `C:\Development\training`, run the following:

```pwsh
# Create the base folder if it doesn't exist yet
New-Item -ItemType Directory -Force -Path C:\Development\training | Out-Null
```

4. Clone the repository. If you're using a different base directory, replace `C:\Development\training` with your path:

```pwsh
# Go there and clone the tutorial
cd C:\Development\training
git clone https://github.com/databankimx/training-csharp-and-onbase.git
cd training-csharp-and-onbase
```

5. If desired, edit the setup configuration, which determines what software is installed automatically (see "Change what gets installed" below)

```pwsh
# Optional: edit setup.json before installations
notepad setup.json
```

6. Save the configuration if you edited it, then run the setup script

##### If using the default base path, run this:

```pwsh
# Run the setup
powershell -NoProfile -ExecutionPolicy Bypass -File .\setup.ps1
```

##### If using a different base path, replace `<BASE_DIR>` with your path and run this:

```pwsh
# Run the setup
powershell -NoProfile -ExecutionPolicy Bypass -File .\setup.ps1 -BaseDirectory <BASE_DIR>
```

#### Manual Setup from Local Repo

If you want to do everything manually, you can pull the repository and set up software on your own.

1. Obtain the tutorial's GitHub slug name, which is `training-csharp-and-onbase`
2. Run the script below

```pwsh
git clone https://github.com/databankimx/training-csharp-and-onbase.git
```

3. Review README.md and QUICKSTART.md in the repository for instructions for manual setup.

### Switches and notes

* `-SkipInstall` checks what's installed and installs nothing.
* `-NoLaunch` runs the setup but doesn't open VS Code or start the runner.
* Installing uses winget. If winget isn't available, install **App Installer** from the Microsoft Store first. Company policy can block installs, in which case the script stops and you'll need IT.
* If an install needs administrator rights, the script stops before installing anything and asks you to run it again from an Administrator PowerShell window.

If you ran the script elevated and the restore fails with `401 (Unauthorized)`, your elevated window may be running under a different account, or it may not see your NuGet settings. Run the script from a normal PowerShell window, without elevation, to restore and build. Your normal account is the one with the feed credentials.

### Change what gets installed

The settings are in `setup.json`, in the repository folder. Open it in any text editor:

* Set an install's `"install"` to `true` to include it, or `false` to skip it.
* `"pinned"` sets the version. `"extensions"` lists the VS Code extensions to add.
* Save the file, then run `setup.ps1` again. It skips what's already installed.

## Open the lessons

1. Open `DataBank.DeveloperTraining.sln` in Visual Studio.
2. Right-click **LessonRunner.Wpf**, choose **Set as Startup Project**, then press **F5**.

---

## Optional: database and OnBase

- **SQL Server with ExternalData.bak.** The database samples and the EForms navigator need a running SQL Server with `Resources\ExternalData.bak` restored. The core lessons don't. The setup script reports whether SQL Server is running.
- **A live OnBase system.** The Unity and REST API tracks need one. Without it, those projects are reference only.

---

## Troubleshooting

| What you see | What to do |
|---|---|
| `No DataBank NuGet source is configured` | Contact the Development Team, as described above. Then run the script again. |
| Restore fails with `401 (Unauthorized)` | The feed credentials aren't set for the account running the script. Check with the Development Team, and run the script from your normal account. |
| `The ASP.NET and web development workload isn't installed` | Run the script with `-InstallMissing`, or tick the workload in the Visual Studio Installer. |
| `The .NET Framework 4.8 targeting pack wasn't found` | In the Visual Studio Installer, open Modify, and tick **.NET Framework 4.8 development tools** under Individual components. |
| `MSBuild wasn't found` | Visual Studio isn't complete. Repair it from the Visual Studio Installer, then run the script again. |
| `Windows needs a restart` | Restart, then run the script again. |

If a problem isn't listed here, note the step and the full error text, and report it to the Development Team.
