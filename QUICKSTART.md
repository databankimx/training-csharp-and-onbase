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

## Run the setup script

Open **PowerShell as Administrator**, go to the folder where you saved `setup.ps1`, and run:

```pwsh
powershell -ExecutionPolicy Bypass -File .\setup.ps1 -InstallMissing
```

The `-ExecutionPolicy Bypass` part lets this one script run. It doesn't change any setting on your machine.

With `-InstallMissing`, the script:

- installs **Git** if it's missing, then clones the repository into `C:\Development\training\training-csharp-and-onbase`
- installs **Visual Studio Community** with the .NET desktop development and ASP.NET and web development workloads, if it's missing
- installs the **.NET SDK 10**, if it's missing
- checks the **.NET Framework 4.8 targeting pack**, and the **DataBank NuGet source**
- restores the whole solution with Visual Studio's MSBuild, then builds the lesson runner

The Visual Studio install is large and can take a long time. If Windows asks for a restart, restart, then run the script again. Steps already done are skipped.

If you ran the script elevated and the restore fails with `401 (Unauthorized)`, your elevated window may be running under a different account, or it may not see your NuGet settings. Run the script from a normal PowerShell window, using the same command without the elevation, to restore and build. Your normal account is the one with the feed credentials.

When the script finishes, every line in the summary should say `ok`.

---

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
