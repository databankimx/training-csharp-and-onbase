# Chapter 5 Supplemental: Configuration Classes

## What This Is

A whole lesson added beyond the textbook (see the `Supplemental` naming convention in the solution README), covering `System.Configuration`: custom `ConfigurationSection`/`ConfigurationElement`/`ConfigurationElementCollection` classes, and a real, DataBank-relevant example, reading OnBase document type and keyword type definitions out of `App.config`, including credentials optionally encrypted into the Windows registry via DPAPI.

---

## Why This Exists

The default `appSettings` section (`ConfigurationManager.AppSettings["subject"]`) is fine for flat key/value pairs, but real configuration is rarely flat. OnBase connection settings need a nested structure: a list of document types, each with its own nested list of keyword types, plus a separate connection-details block. Custom configuration sections are how you model that structure with actual typed C# classes instead of manually parsing raw XML every time you need a setting.

---

## The Four Pieces

- **`OnBaseSettings : ConfigurationSection`** — the root. `ConfigurationManager.GetSection(OnBaseSettings.SectionName)` returns this, cast from `object`. It exposes `ServiceLocation` (a single nested element) and `DocumentTypes` (a nested collection).
- **`ConfigurationElement`** (`DocumentTypeElement`, `KeywordTypeElement`, `ServiceLocation`) — a single XML element's worth of typed properties, each backed by `[ConfigurationProperty("xmlAttributeName")]` and read through `this["xmlAttributeName"]` or `base["xmlAttributeName"]`.
- **`ConfigurationElementCollection`** (`DocumentTypeCollection`, `KeywordTypeCollection`) — a strongly-typed collection of elements. The two required overrides, `CreateNewElement()` and `GetElementKey()`, are what let the base class parse repeated XML elements (`<documentType>`, `<keywordType>`) into instances of your own class, keyed by whatever property you designate (`Name`, here).
- **`[ConfigurationCollection(...)]`** on the property that exposes a collection — this is what lets you rename the XML child element from the default `<add>` to something readable, `<documentType>` and `<keywordType>` instead of a generic `<add>` for both.

Every one of these classes overrides `IsReadOnly()` to return `false`. Without that, `System.Configuration`'s default behavior locks configuration objects after they're loaded, which is fine for read-only scenarios but gets in the way the moment you want to modify and re-save configuration programmatically.

---

## Credentials, Optionally Encrypted

`ServiceLocation.Username`/`Password` can be either a plain-text value or a string in the form `registry:HKLM\SOFTWARE\DataBank\...\ASPNET_SETREG,userName`, encrypted into the registry ahead of time via `aspnet_setreg.exe` (the same tool ASP.NET has used for decades to keep credentials out of plain-text config files). `RegistryExtensions.IsEncrypted()` checks the string shape with a regex, and `DecryptRegistryKey()` opens the referenced registry key and runs the stored bytes through Windows DPAPI (`CryptUnprotectData`, via P/Invoke into `Crypt32.dll`) to get the real value back. `ServiceLocation.DecryptedUsername`/`DecryptedPassword` are the properties that actually make this decision transparently, callers never need to know or care whether a given config value was encrypted.

`PostDeserialize()` on `ServiceLocation` runs validation immediately after the XML is parsed: if `useNTAuthentication` is `false`, a username and password are required; if it's `true`, either all of domain/username/password must be specified (for an alternate-user login) or none of them (falling back to the identity already running the application).

---

## A Real Dependency Worth Knowing About

`ServiceLocation.LicenseType` is typed as `Hyland.Unity.LicenseType`, a real enum from OnBase's proprietary Unity API. Unlike every other dependency in this training set, this isn't available on public NuGet, it's referenced via `Hyland.Unity.v25` from DataBank's internal GHE feed. Resolves from the same `DataBank GitHub` source already configured in your user-level `NuGet.config`, no solution-level override needed or wanted here, an earlier attempt at adding one with a guessed feed URL actually broke restore by shadowing the correct, working source with `<clear />`. If this project fails to restore, check that your user-level `NuGet.config` still has the `DataBank GitHub` source and that `DATABANKIMX_NUGET_PAT` is set, and confirm nothing at the project or solution level is overriding it.
