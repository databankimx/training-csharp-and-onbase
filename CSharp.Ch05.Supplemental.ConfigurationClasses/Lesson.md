# Chapter 5 Supplemental: Configuration Classes

## What This Is

A whole lesson added beyond the textbook (see the `Supplemental` naming convention in the solution README), covering `System.Configuration`: custom `ConfigurationSection`/`ConfigurationElement`/`ConfigurationElementCollection` classes, and a real, DataBank-relevant example — reading OnBase document type and keyword type definitions out of `App.config`, including credentials optionally encrypted into the Windows registry via DPAPI.

This is the most directly job-applicable project in Chapter 5. The class-hierarchy mechanics are the same ones the main lesson covers; the difference is that here they're solving an actual problem you will encounter.

---

## Why This Exists

The default `appSettings` section is fine for flat key/value pairs:

```csharp
var subject = ConfigurationManager.AppSettings["subject"];
```

But real configuration is rarely flat. OnBase connection settings need a nested structure: a list of document types, each with its own nested list of keyword types, plus a separate connection-details block. Custom configuration sections are how you model that structure with actual typed C# classes instead of manually parsing raw XML every time you need a setting.

The payoff is that a typo in the config file becomes a load-time error with a line number, rather than a `null` that surfaces three layers deep at runtime.

---

## What the Config Looks Like

```xml
<configSections>
  <section name="onBaseSettings"
		   type="CSharp.Ch05.Supplemental.ConfigurationClasses.Models.Configuration.OnBaseSettings, CSharp.Ch05.Supplemental.ConfigurationClasses"/>
</configSections>

<onBaseSettings>
  <documentTypes>
	<documentType name="TST - Image" id="101">
	  <keywordTypes>
		<keywordType name="Description" id="1" dataType="Alphanumeric" dataLength="50"/>
		<keywordType name="TST - Alpha 10" id="101" dataType="Alphanumeric" dataLength="10"/>
	  </keywordTypes>
	</documentType>
  </documentTypes>

  <serviceLocation servicePath="http://localhost/appserver/service.asmx"
				   dataSource="OnBase"
				   licenseType="QueryMetering"
				   useNTAuthentication="false"
				   domain=""
				   username="..."
				   password="..."/>
</onBaseSettings>
```

The `<configSections>` declaration at the top is mandatory and easy to forget. It maps the XML element name to the class that knows how to parse it, in `"Namespace.Class, AssemblyName"` form. Miss it and .NET throws a `ConfigurationErrorsException` complaining that the section is unrecognized — which is at least an honest error, if not an obvious one.

Note also that `<configSections>` must be the **first** child of `<configuration>`. The parser rejects the file otherwise.

---

## The Four Pieces

- **`OnBaseSettings : ConfigurationSection`** — the root. `ConfigurationManager.GetSection(OnBaseSettings.SectionName)` returns this, cast from `object`. It exposes `ServiceLocation` (a single nested element) and `DocumentTypes` (a nested collection).

- **`ConfigurationElement`** (`DocumentTypeElement`, `KeywordTypeElement`, `ServiceLocation`) — a single XML element's worth of typed properties, each backed by `[ConfigurationProperty("xmlAttributeName")]` and read through `this["xmlAttributeName"]` or `base["xmlAttributeName"]`.

- **`ConfigurationElementCollection`** (`DocumentTypeCollection`, `KeywordTypeCollection`) — a strongly-typed collection of elements. The two required overrides, `CreateNewElement()` and `GetElementKey()`, are what let the base class parse repeated XML elements (`<documentType>`, `<keywordType>`) into instances of your own class, keyed by whatever property you designate (`Name`, here).

- **`[ConfigurationCollection(...)]`** on the property that exposes a collection — this is what lets you rename the XML child element from the default `<add>` to something readable: `<documentType>` and `<keywordType>` instead of a generic `<add>` for both.

The property pattern itself is worth internalizing, because it repeats everywhere in this project:

```csharp
[ConfigurationProperty("name", IsRequired = true, IsKey = true)]
public string Name => (string)this["name"];
```

The attribute declares the XML contract (`IsRequired` gets you a load-time failure for a missing attribute; `IsKey` marks the collection key), and the indexer reads from the base class's internal property bag. The typed property is a thin façade over that bag — which is why the cast is always necessary, and why the string in the attribute and the string in the indexer must match exactly. A mismatch between those two strings compiles fine and fails at runtime, which is the single most common bug in this style of code.

Every one of these classes overrides `IsReadOnly()` to return `false`. Without that, `System.Configuration`'s default behavior locks configuration objects after they're loaded, which is fine for read-only scenarios but gets in the way the moment you want to modify and re-save configuration programmatically.

---

## Why This Is a Class Hierarchy Lesson

It's here in Chapter 5 rather than somewhere more obviously "configuration-shaped" because every piece of it is inheritance doing real work:

- You inherit from `ConfigurationSection`/`ConfigurationElement`/`ConfigurationElementCollection` and get the entire XML parsing engine for free.
- You override `CreateNewElement()` and `GetElementKey()` — abstract members the base class *requires* you to supply, because they're the two things it genuinely cannot know about your type.
- You override `PostDeserialize()` — a virtual hook the base class calls at a specific point in its own lifecycle.

That's the Template Method pattern: the base class owns the algorithm and calls down into your overrides at the points where behavior varies. It's exactly the "abstract class for shared implementation" case from the main Chapter 5 lesson, applied to a framework you didn't write.

---

## Credentials, Optionally Encrypted

`ServiceLocation.Username`/`Password` can be either a plain-text value or a string in this form:

```text
registry:HKLM\SOFTWARE\DataBank\DeveloperTraining\Identity\ASPNET_SETREG,userName
```

The encrypted values are written ahead of time with `aspnet_setreg.exe` — the same tool ASP.NET has used for decades to keep credentials out of plain-text config files:

```text
aspnet_setreg.exe -k:SOFTWARE\DataBank\DeveloperTraining\Identity -u:username -p:password
```

`RegistryExtensions.IsEncrypted()` checks the string shape with a regex, and `DecryptRegistryKey()` opens the referenced registry key and runs the stored bytes through Windows DPAPI (`CryptUnprotectData`, via P/Invoke into `Crypt32.dll`) to get the real value back.

`ServiceLocation.DecryptedUsername`/`DecryptedPassword` are the properties that actually make this decision transparently. Callers never need to know or care whether a given config value was encrypted — they read the `Decrypted*` property and get a usable credential either way. That's good API design: the complexity is real, but it's absorbed by the type rather than pushed onto every consumer.

DPAPI is worth understanding at a high level, because it defines the operational constraints. Windows derives the encryption key from the machine (or user) account, so the encrypted value is only decryptable on the machine where it was encrypted. That's the security benefit — a stolen config file is useless elsewhere — and simultaneously the deployment gotcha: **you must run `aspnet_setreg.exe` on every server**, and the account running the application needs read access to that registry key. "It works on my machine" has a very specific and very literal meaning here.

Note the P/Invoke into `Crypt32.dll` is exactly the `[DllImport]`/`extern` mechanism from Chapter 4, showing up in production code rather than a demo.

---

## Validation at Load Time

`PostDeserialize()` on `ServiceLocation` runs validation immediately after the XML is parsed:

- If `useNTAuthentication` is `false`, a username and password are **required**.
- If it's `true`, either **all** of domain/username/password must be specified (for an alternate-user login) or **none** of them (falling back to the identity already running the application).

This is the same "if it constructed, it's valid" principle from the other supplemental lessons, applied at the configuration boundary. Catching a half-specified credential set at startup, with a message naming the problem, is dramatically better than a failed OnBase login thirty seconds into a batch job. Fail fast, fail loudly, fail where the fix is obvious.

`PostDeserialize()` is the correct hook for this specifically because it runs after *all* attributes are populated. Validating inside individual property getters can't work — you can't check "domain requires username" before you know whether username was supplied.

---

## A Real Dependency Worth Knowing About

`ServiceLocation.LicenseType` is typed as `Hyland.Unity.LicenseType`, a real enum from OnBase's proprietary Unity API. Unlike every other dependency in this training set, this isn't available on public NuGet — it's referenced via `Hyland.Unity.v25` from DataBank's internal GHE feed.

It resolves from the same `DataBank GitHub` source already configured in your user-level `NuGet.config`. No solution-level override is needed or wanted here; an earlier attempt at adding one with a guessed feed URL actually broke restore by shadowing the correct, working source with `<clear />`.

If this project fails to restore:

1. Confirm your user-level `NuGet.config` still has the `DataBank GitHub` source.
2. Confirm `DATABANKIMX_NUGET_PAT` is set in your environment.
3. Confirm nothing at the project or solution level is overriding the source list.

Worth noting what the strong typing buys: `licenseType="QueryMetering"` in the XML is parsed straight into the enum, so an invalid license type is a configuration load failure rather than a mysterious rejection from the OnBase Application Server later. Typed configuration moves errors earlier, which is the whole reason to write these classes instead of reading strings.

---

## Takeaways

- `appSettings` is for flat values. Anything nested or repeated wants a custom `ConfigurationSection`.
- Declare the section in `<configSections>` first, or nothing works.
- The attribute name and the indexer string must match exactly — that mismatch is the classic bug here.
- `CreateNewElement()` and `GetElementKey()` are the two things the framework can't infer; everything else you inherit.
- `[ConfigurationCollection]` is what gets you readable `<documentType>` elements instead of generic `<add>`.
- Override `IsReadOnly()` to `false` if you ever intend to write config back out.
- Validate in `PostDeserialize()`, where the whole element is populated.
- DPAPI-encrypted credentials are machine-bound. Run `aspnet_setreg.exe` on every server.
- Typed configuration turns runtime surprises into startup errors. That's the entire point.
