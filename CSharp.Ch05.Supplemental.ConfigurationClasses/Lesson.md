# Chapter 5 Supplemental: Configuration Classes

## What This Is About

The default `appSettings` section is fine for flat key/value pairs:

```csharp
var subject = ConfigurationManager.AppSettings["subject"];
```

But real configuration is rarely flat. OnBase connection settings need a nested structure: a list of document types, each with its own nested list of keyword types, plus a separate connection block. Custom configuration sections are how you model that structure with actual typed C# classes instead of manually parsing raw XML every time you need a setting.

The payoff is that a typo in the config file becomes a load-time error with a line number, rather than a `null` that surfaces three layers deep at runtime. This is also a class hierarchy lesson in disguise -- every piece of it is inheritance doing real work, which is exactly why it lives in Chapter 5.

---

## How to Write This Program

The types in this project compose together in layers, so build them in dependency order, smallest to largest.

### Step 1: The Config File First

Before writing a single class, write what you want to be able to put in `App.config`. Having the target XML in front of you makes the class structure obvious rather than something you have to reverse-engineer later.

```xml
<configuration>
  <configSections>
    <section name="onBaseSettings"
             type="YourNamespace.OnBaseSettings, YourAssemblyName"/>
  </configSections>

  <appSettings>
    <add key="subject" value="Configuration Classes"/>
  </appSettings>

  <onBaseSettings>
    <documentTypes>
      <documentType name="TST - Image" id="101">
        <keywordTypes>
          <keywordType name="Description"     id="1"   dataType="Alphanumeric" dataLength="50"/>
          <keywordType name="TST - Alpha 10"  id="101" dataType="Alphanumeric" dataLength="10"/>
        </keywordTypes>
      </documentType>
    </documentTypes>
    <serviceLocation servicePath="http://localhost/appserver/service.asmx"
                     dataSource="OnBase"
                     licenseType="QueryMetering"
                     useNTAuthentication="false"
                     username="myuser"
                     password="mypass"/>
  </onBaseSettings>
</configuration>
```

Two things worth committing to memory right now:

- **`<configSections>` must be the first child of `<configuration>`.** The parser rejects the file if it isn't. This is the most common config-class setup error.
- **The `type` attribute must be `"Namespace.ClassName, AssemblyName"`, exactly.** A typo here produces a `ConfigurationErrorsException` at load time with a message that doesn't point at the problem.

### Step 2: A Single Element -- KeywordTypeElement

```csharp
public class KeywordTypeElement : ConfigurationElement
{
    [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
    public string Name => (string)this["name"];

    [ConfigurationProperty("id", IsRequired = true)]
    public int Id => (int)this["id"];

    [ConfigurationProperty("dataType", IsRequired = true)]
    public string DataType => (string)this["dataType"];

    [ConfigurationProperty("dataLength", IsRequired = false, DefaultValue = 0)]
    public int DataLength => (int)this["dataLength"];

    public override bool IsReadOnly() => false;
}
```

The pattern repeats verbatim for every property on every element class in this project, so make sure it's clear before moving on. The `[ConfigurationProperty("name")]` attribute declares the XML attribute name and its contract (`IsRequired`, `IsKey`, `DefaultValue`). The property reads from `this["name"]` -- the base class's internal property bag. The string in the attribute and the string in the indexer **must match exactly**. A mismatch compiles fine and fails at runtime, quietly, which is the single most common bug in this style of code.

`IsKey = true` marks the property that uniquely identifies elements within a collection -- the base class uses it to enforce uniqueness and to look up elements.

`IsReadOnly() => false` must be overridden on every element class. Without it, the framework locks the object after loading, preventing any programmatic modification.

### Step 3: A Collection -- KeywordTypeCollection

```csharp
[ConfigurationCollection(typeof(KeywordTypeElement), AddItemName = "keywordType")]
public class KeywordTypeCollection : ConfigurationElementCollection
{
    protected override ConfigurationElement CreateNewElement() => new KeywordTypeElement();
    protected override object GetElementKey(ConfigurationElement element) =>
        ((KeywordTypeElement)element).Name;

    public override bool IsReadOnly() => false;
}
```

Two abstract methods you must supply, because these are the two things the framework genuinely cannot infer about your type:

- `CreateNewElement()` -- how to make a new instance of whatever goes in this collection.
- `GetElementKey(element)` -- what uniquely identifies each element (returns the value of the `IsKey` property).

The `[ConfigurationCollection]` attribute on the class, combined with `AddItemName = "keywordType"`, is what lets you write `<keywordType .../>` in the XML instead of the default `<add .../>`. Without it, every collection element would be named `<add>`, which is unreadable when you have multiple collections in the same section.

Run the program now with just this much in place and verify that the config file loads and the keyword types print. If something's wrong with the section declaration or the `type` attribute, you'll find out here rather than halfway through building the remaining classes.

### Step 4: DocumentTypeElement and DocumentTypeCollection

```csharp
public class DocumentTypeElement : ConfigurationElement
{
    [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
    public string Name => (string)this["name"];

    [ConfigurationProperty("id", IsRequired = true)]
    public int Id => (int)this["id"];

    [ConfigurationProperty("keywordTypes", IsRequired = true)]
    [ConfigurationCollection(typeof(KeywordTypeElement), AddItemName = "keywordType")]
    public KeywordTypeCollection KeywordTypes => (KeywordTypeCollection)this["keywordTypes"];

    public override bool IsReadOnly() => false;
}

[ConfigurationCollection(typeof(DocumentTypeElement), AddItemName = "documentType")]
public class DocumentTypeCollection : ConfigurationElementCollection
{
    protected override ConfigurationElement CreateNewElement() => new DocumentTypeElement();
    protected override object GetElementKey(ConfigurationElement element) =>
        ((DocumentTypeElement)element).Name;

    public override bool IsReadOnly() => false;
}
```

Notice `KeywordTypes` is a nested collection property -- a `ConfigurationElementCollection` property on a `ConfigurationElement`. This is how you get the `<keywordType>` elements nested inside `<documentType>` in the XML. The `[ConfigurationCollection]` attribute on the property specifies the item type and element name, just like it does on the collection class itself.

### Step 5: ServiceLocation

```csharp
public class ServiceLocation : ConfigurationElement
{
    [ConfigurationProperty("servicePath", IsRequired = true)]
    public string ServicePath => (string)this["servicePath"];

    [ConfigurationProperty("dataSource", IsRequired = true)]
    public string DataSource => (string)this["dataSource"];

    [ConfigurationProperty("useNTAuthentication", IsRequired = true)]
    public bool UseNtAuthentication => (bool)this["useNTAuthentication"];

    [ConfigurationProperty("username", IsRequired = false, DefaultValue = "")]
    public string Username => (string)this["username"];

    [ConfigurationProperty("password", IsRequired = false, DefaultValue = "")]
    public string Password => (string)this["password"];

    // Credentials can optionally be stored encrypted in the Windows registry
    // via aspnet_setreg.exe. DecryptedUsername/Password transparently handle both cases.
    public string DecryptedUsername => IsEncrypted(Username) ? DecryptRegistryKey(Username) : Username;
    public string DecryptedPassword => IsEncrypted(Password) ? DecryptRegistryKey(Password) : Password;

    protected override void PostDeserialize()
    {
        base.PostDeserialize();
        Validate();
    }

    private void Validate()
    {
        if (!UseNtAuthentication && string.IsNullOrEmpty(Username))
            throw new ConfigurationErrorsException(
                "Username is required when useNTAuthentication is false.");
    }

    public override bool IsReadOnly() => false;
}
```

`PostDeserialize()` is a virtual hook the base class calls immediately after all attributes are populated. This is the right place to validate cross-property constraints -- you can't check "username is required when NT auth is off" before you know whether NT auth was supplied, and individual property getters run in unpredictable order during parsing. Catch it here, at load time, with a message naming the problem.

`DecryptedUsername`/`DecryptedPassword` are the properties callers use. The encrypted value format is:

```
registry:HKLM\SOFTWARE\DataBank\DeveloperTraining\Identity\ASPNET_SETREG,userName
```

Generated ahead of time with:
```
aspnet_setreg.exe -k:SOFTWARE\DataBank\DeveloperTraining\Identity -u:username -p:password
```

Callers never need to know or care whether a given credential is encrypted -- they read `DecryptedUsername` and get the right value either way. The complexity is absorbed by the type rather than pushed onto every consumer, which is good API design.

One operational constraint worth understanding: DPAPI encrypts using the machine (or user) account as the key source, so the encrypted value is only decryptable on the machine where it was created. A stolen config file is useless elsewhere, which is the security benefit. The deployment implication: you must run `aspnet_setreg.exe` on every server, and the account running the application needs read access to the registry key.

### Step 6: The Root Section -- OnBaseSettings

```csharp
public class OnBaseSettings : ConfigurationSection
{
    public const string SectionName = "onBaseSettings";

    [ConfigurationProperty("documentTypes", IsRequired = true)]
    [ConfigurationCollection(typeof(DocumentTypeElement), AddItemName = "documentType")]
    public DocumentTypeCollection DocumentTypes => (DocumentTypeCollection)this["documentTypes"];

    [ConfigurationProperty("serviceLocation", IsRequired = true)]
    public ServiceLocation ServiceLocation => (ServiceLocation)this["serviceLocation"];

    public override bool IsReadOnly() => false;
}
```

`ConfigurationSection` is the root of the hierarchy -- the class `ConfigurationManager.GetSection()` instantiates and returns. Everything else hangs from it. The read in `Main()` is as simple as:

```csharp
var settings = (OnBaseSettings)ConfigurationManager.GetSection(OnBaseSettings.SectionName);

foreach (DocumentTypeElement docType in settings.DocumentTypes)
{
    Console.WriteLine($"Document Type: {docType.Name} (ID: {docType.Id})");
    foreach (KeywordTypeElement kwType in docType.KeywordTypes)
        Console.WriteLine($"  Keyword: {kwType.Name} ({kwType.DataType})");
}

Console.WriteLine($"Server: {settings.ServiceLocation.ServicePath}");
Console.WriteLine($"User:   {settings.ServiceLocation.DecryptedUsername}");
```

Run it. Document types, keyword types, connection details, all read from typed C# objects with no manual XML parsing anywhere.

---

## Why This Is a Chapter 5 Lesson

The hierarchy mechanics at work here:

- You **inherit** from `ConfigurationSection`, `ConfigurationElement`, and `ConfigurationElementCollection` and get the entire XML parsing engine for free.
- You **override** `CreateNewElement()` and `GetElementKey()` -- abstract members the base class requires you to supply, because they're the two things it genuinely cannot know about your type.
- You **override** `PostDeserialize()` -- a virtual hook the base class calls at a specific point in its own lifecycle.

That's the Template Method pattern: the base class owns the algorithm and calls down into your overrides at the points where behavior varies. It's exactly the "abstract class for shared implementation" case from the main Chapter 5 lesson, applied to a framework you didn't write.

---

## Takeaways

- `appSettings` is for flat values. Anything nested or repeated wants a custom `ConfigurationSection`.
- `<configSections>` must be the first child of `<configuration>`, or nothing works.
- The attribute name string and the indexer string must match exactly -- that mismatch is the classic bug here.
- `CreateNewElement()` and `GetElementKey()` are the two things the framework can't infer; everything else you inherit.
- `[ConfigurationCollection(AddItemName = "...")]` is what gets you readable element names instead of generic `<add>`.
- Override `IsReadOnly()` to `false` if you ever intend to write config back out.
- Validate cross-property constraints in `PostDeserialize()`, where the whole element is populated.
- DPAPI-encrypted credentials are machine-bound. Run the encryption tool on every server.
- Typed configuration turns runtime surprises into startup errors. That's the entire point.
