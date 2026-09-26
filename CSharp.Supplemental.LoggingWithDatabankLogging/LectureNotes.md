# Logging with Databank.Logging

## What This Is

A new lesson, not a direct port - added to round out the log4net/Serilog comparison with what using DataBank's actual internal libraries looks like in practice. Uses `Databank.Logging` 2.0.1 (which itself depends on `Databank.Exceptions` 2.0.1 as a transitive package reference - only `Databank.Logging` is referenced directly here).

## Requires the DataBank GHE NuGet Feed

Both packages are published to `https://nuget.databankimx.ghe.com/NuGet/index.json`, not nuget.org. This solution has no repo-level `NuGet.config` - Scott already has this feed configured in his own global NuGet config (`%AppData%\Roaming\NuGet`), so nothing further was added at the solution level. Anyone else restoring this project needs the same: a DataBank GHE account with access to the feed, and that feed configured in their own NuGet settings (globally, or in a personal `NuGet.config` - not something to check into source control, since it would need to carry credentials).

## Target Framework

`net48`, inherited from this solution's default (`Databank.Logging`/`Databank.Exceptions` both multi-target `net48` and `net8.0`, so either would have worked) - kept consistent with the other two lessons in this comparison.

## One Thing Worth a Second Look

The package's own `serilog.json` template (shipped inside the `Databank.Logging` package itself) sets the debug sink's `restrictedToMinimumLevel` to `"Debug"`, but the package's own README explicitly warns that this should be `"Verbose"`, not `"Debug"` - otherwise Verbose-level traces (including anything from `Databank.LogInjection`, per the README) won't show up in the debug log at all. This lesson's `serilog.json` follows the README's documented guidance (`"Verbose"`), not the package's own shipped template - worth flagging in case that's an oversight worth fixing in the package itself rather than something intentional.
