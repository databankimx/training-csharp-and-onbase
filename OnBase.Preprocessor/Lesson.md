# OnBase Preprocessor

## What This Is

OnBase's COLD/DIP import process can hand a text file off to an external "preprocessor" executable before importing it - a small, non-interactive command-line tool that cleans up or transforms the file, then hands control back. This project is a working example of one: it strips or replaces characters OnBase can't handle cleanly (control characters, characters outside a configured character set, and specific characters you tell it to replace), and reports back with an integer exit code OnBase uses to know whether the run succeeded.

## The Contract OnBase Expects

Three rules, all enforced in `Program.cs`:

1. **No user interaction required.** This has to run unattended, kicked off by OnBase itself.
2. **At least two command-line arguments**: an input file path and an output file path (`%I` and `%O`, in OnBase's own configuration syntax).
3. **An integer exit code** on the way out, so OnBase knows what happened - see `Models/Enumerations/ExitStatus.cs` for the specific codes this project returns (`Success`, `MissingArgument`, `InputFileNotFound`, and so on).

## What It Actually Cleans Up

`FileCleaner.CleanFile()` reads the input file one character at a time and, for each one, checks:

- Is it outside the configured character set (`Ansi`/`Ascii`/`Unicode`, set in `App.config`)? Replace it with the configured substitute character.
- Is it one of the non-printable ASCII control characters `FileCleaner` already knows about? Same substitution.
- Is it a form-feed character, and is `replaceFormFeeds` turned on? Replace it with a line break instead.
- Is it in the explicit `characterReplacements` list in `App.config`? Use that specific replacement - this check runs *after* the general substitution check, so an explicit rule always wins over the generic "this is out of range" substitute.

## Try It Yourself

`sample-input.txt` is included, three rows of CSV-ish data with an en dash, an em dash, and a curly apostrophe mixed in - none of them plain ASCII. The project's `App.config` has explicit replacement rules for the two dashes (both become a plain hyphen); the curly apostrophe has no explicit rule, so it falls through to the generic out-of-range substitute (`?`, by default).

Build the project, then run it from a command line with the input and a destination path:

```
OnBase.Preprocessor.exe sample-input.txt cleaned-output.txt
```

Open `cleaned-output.txt` afterward and compare it against `sample-input.txt` - the dashes should read as plain hyphens, and the curly apostrophes should have become `?`. Check the log file (path set by `logFilePath` in `App.config`, `.\logs\preprocessor.log` by default) for a record of the run.

Try renaming or deleting `sample-input.txt` and running it again to see the `InputFileNotFound` path, or run it with no arguments at all to see the `MissingArgument` path - both exit codes are exactly what OnBase itself would see if either of those things happened during a real import.
