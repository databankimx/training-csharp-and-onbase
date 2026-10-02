# Chapter 5 Supplemental: Configuration Classes

## What This Is

Custom `ConfigurationSection`, `ConfigurationElement`, and `ConfigurationElementCollection` classes for reading structured OnBase settings from App.config. This is the most directly job-applicable project in Chapter 5 -- the same pattern shows up in most existing DataBank OnBase projects, since it's how structured per-environment configuration (connection strings, document type IDs, keyword type definitions) is kept out of the code and in a config file where ops can change it without a rebuild.

## How to Write This Program

The program has two parts. Write Mini-Program 1 first and run it. Then build the four configuration classes and App.config structure (all described in the sections below), and come back to write Mini-Program 2.

### Mini-Program 1: Flat AppSettings

Before building the custom section, establish the baseline limitation. Add the following to `App.config`'s `<appSettings>` section and read it back:

```xml
<appSettings>
  <add key="subject" value="Configuration Classes"/>
</appSettings>
```

```csharp
string appSetting = ConfigurationManager.AppSettings["subject"];
Console.WriteLine($"Today, we are learning about [{appSetting}]");
```

**Run it.** This works fine for a single value. Now think about how you'd store a list of document types, each with its own list of keyword types, each with an ID, data type, and optional length -- all as flat key/value pairs. There's no clean way. That's what this project solves.

---

## Setting Up App.config

Every custom section needs a `<section>` declaration inside `<configSections>` before it can be used anywhere else in the file. The full structure:

```xml
<configuration>
  <configSections>
    <section name="onBaseSettings"
             type="CSharp.Ch05.Supplemental.ConfigurationClasses.Models.Configuration.OnBaseSettings,
                   CSharp.Ch05.Supplemental.ConfigurationClasses"/>
  </configSections>

  <appSettings>
    <add key="subject" value="Configuration Classes"/>
  </appSettings>

  <onBaseSettings>
    <documentTypes>
      <documentType name="TST - Image" id="101">
        <keywordTypes>
          <keywordType name="Description" id="1" dataType="Alphanumeric" dataLength="50"/>
          <keywordType name="Document Date" id="2" dataType="Date"/>
        </keywordTypes>
      </documentType>
    </documentTypes>

    <serviceLocation
      servicePath="http://localhost/appserver/service.asmx"
      dataSource="OnBase"
      licenseType="QueryMetering"
      useNTAuthentication="false"
      username="someUsername"
      password="somePassword"/>
  </onBaseSettings>
</configuration>
```

`name` is the XML element name (`onBaseSettings`). `type` is the fully-qualified class name plus assembly name for the `ConfigurationSection` that parses it. Get either wrong and `ConfigurationManager.GetSection()` throws a `ConfigurationErrorsException` at runtime -- not a compile error, a runtime one. Plain-text `username`/`password` work for running locally. See the encryption section below for what you'd do before this file went anywhere real.

---

## The Class Structure

### `OnBaseSettings : ConfigurationSection`

The root section class. Retrieved by name from `ConfigurationManager.GetSection()` and cast from `object`:

```csharp
public class OnBaseSettings : ConfigurationSection
{
    public const string SectionName = "onBaseSettings";

    [ConfigurationProperty("serviceLocation", IsRequired = true)]
    public ServiceLocationElement ServiceLocation
    {
        get => (ServiceLocationElement)base["serviceLocation"];
        set => base["serviceLocation"] = value;
    }

    [ConfigurationProperty("documentTypes", IsRequired = true)]
    [ConfigurationCollection(typeof(DocumentTypeElement),
        AddItemName = "documentType",
        ClearItemsName = "clear",
        RemoveItemName = "remove")]
    public DocumentTypeCollection DocumentTypes
    {
        get => (DocumentTypeCollection)base["documentTypes"];
        set => this["documentTypes"] = value;
    }

    public override bool IsReadOnly() => false;
}
```

`[ConfigurationProperty("xmlAttributeName")]` maps a C# property to an XML attribute or child element. Properties read and write through `base[...]`/`this[...]` rather than a plain backing field -- that's not a convention, that's how `System.Configuration` stores and retrieves values internally.

`[ConfigurationCollection(...)]` renames the child element from the framework's default `<add>` to something readable (`<documentType>`). Without it, your config file would need `<add name="..." id="..."/>` instead.

`IsReadOnly()` returning `false` deserves a full explanation because the default behavior is genuinely surprising. By default, `ConfigurationSection` and `ConfigurationElement` return `true` from `IsReadOnly()`, which causes `System.Configuration` to treat the object as immutable after loading. This sounds reasonable until you realize that "modification" in this context includes writing to any property through `base[...]` -- which is the same mechanism the property setters use. With the default `true`, trying to set any property after load (including in `PostDeserialize()`, which runs during loading) throws an exception. In other words: you can't populate the `decryptedUsername` cache field in `DecryptedUsername`'s getter, you can't assign anything in `PostDeserialize()`, and you can't make any programmatic adjustment to the loaded config at all. Returning `false` lifts that restriction. It does not mean the config file itself becomes writable -- it means the in-memory object the framework created from that file can be modified by your code. Every class in this hierarchy overrides it for the same reason.

### `DocumentTypeElement : ConfigurationElement`

One `<documentType>` entry with its own nested collection:

```csharp
public class DocumentTypeElement : ConfigurationElement
{
    [ConfigurationProperty("name", IsRequired = true)]
    public string Name
    {
        get => (string)base["name"];
        set => base["name"] = value;
    }

    [ConfigurationProperty("id", IsRequired = true)]
    public long Id
    {
        get => (long)base["id"];
        set => base["id"] = value;
    }

    [ConfigurationProperty("keywordTypes", IsRequired = true)]
    [ConfigurationCollection(typeof(KeywordTypeElement),
        AddItemName = "keywordType",
        ClearItemsName = "clear",
        RemoveItemName = "remove")]
    public KeywordTypeCollection KeywordTypes
    {
        get => (KeywordTypeCollection)base["keywordTypes"];
        set => base["keywordTypes"] = value;
    }

    public override bool IsReadOnly() => false;
}
```

Elements can nest other elements and collections arbitrarily deep. `DocumentTypeElement` nests a `KeywordTypeCollection` the same way `OnBaseSettings` nests a `DocumentTypeCollection`.

### `DocumentTypeCollection : ConfigurationElementCollection`

The collection that holds multiple `<documentType>` entries:

```csharp
public class DocumentTypeCollection : ConfigurationElementCollection
{
    protected override ConfigurationElement CreateNewElement()
        => new DocumentTypeElement();

    protected override object GetElementKey(ConfigurationElement element)
        => ((DocumentTypeElement)element).Name;

    public DocumentTypeElement this[int index]
    {
        get => (DocumentTypeElement)BaseGet(index);
        set
        {
            if (BaseGet(index) != null) BaseRemoveAt(index);
            BaseAdd(index, value);
        }
    }

    public void Add(DocumentTypeElement element) => BaseAdd(element);
    public void Remove(string name) => BaseRemove(name);
    public void Clear() => BaseClear();
}
```

`CreateNewElement()` and `GetElementKey()` are the two required overrides. They're what let the base class parse repeated XML elements into instances of your class, keyed by whichever property you choose (`Name`, here). Everything else wraps the inherited `Base*` methods in a typed surface.

### `KeywordTypeElement : ConfigurationElement`

One `<keywordType>` entry. `DataLength` is not required because numeric keyword types don't have one:

```csharp
public class KeywordTypeElement : ConfigurationElement
{
    [ConfigurationProperty("name", IsRequired = true)]
    public string Name { get => (string)base["name"]; set => base["name"] = value; }

    [ConfigurationProperty("id", IsRequired = true)]
    public long Id { get => (long)base["id"]; set => base["id"] = value; }

    [ConfigurationProperty("dataType", IsRequired = true)]
    public string DataType { get => (string)base["dataType"]; set => base["dataType"] = value; }

    [ConfigurationProperty("dataLength", IsRequired = false)]
    public long DataLength { get => (long)base["dataLength"]; set => base["dataLength"] = value; }

    public override bool IsReadOnly() => false;
}
```

### `ServiceLocationElement : ConfigurationElement`

The `<serviceLocation>` element. `PostDeserialize()` runs automatically after the XML is parsed -- the right place for cross-property validation:

```csharp
protected override void PostDeserialize()
{
    base.PostDeserialize();

    if (!UseNtAuthentication &&
        (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password)))
    {
        throw new ConfigurationErrorsException(
            "Username and Password are required when UseNtAuthentication is false.");
    }
}
```

This fires at config-load time, not at first use. A misconfigured file fails loudly and immediately rather than silently producing null credentials later.

---

## The Encrypted Credential Pattern

`ServiceLocationElement` stores credentials in App.config and decrypts them on read. The `Username` and `Password` properties hold the raw config value (either plain text or a `registry:...` reference). `DecryptedUsername` and `DecryptedPassword` hide the decision:

```csharp
public string DecryptedUsername
{
    get
    {
        if (!string.IsNullOrEmpty(decryptedUsername)) return decryptedUsername;
        decryptedUsername = Username.IsEncrypted()
            ? Username.DecryptRegistryKey()
            : Username;
        return decryptedUsername;
    }
}
```

`IsEncrypted()` recognizes the `registry:HKLM\...,valueName` format with a regex. `DecryptRegistryKey()` opens the registry path and decrypts the value via Windows DPAPI. Callers always get plaintext -- whether the config stores plain text or an encrypted registry reference is invisible to them.

### Encrypting Credentials with aspnet_setreg.exe

`aspnet_setreg.exe` lives in this solution's shared `Resources` folder. Run it from an elevated command prompt (writing to `HKLM` requires administrator rights):

```
aspnet_setreg.exe -k:SOFTWARE\DataBank\DeveloperTraining\Identity -u:yourUsername -p:yourPassword
```

`-k` is the registry key path under `HKEY_LOCAL_MACHINE`. `-u`/`-p` are the actual credentials to encrypt. Once run, update the config attributes:

```xml
username="registry:HKLM\SOFTWARE\DataBank\DeveloperTraining\Identity\ASPNET_SETREG,userName"
password="registry:HKLM\SOFTWARE\DataBank\DeveloperTraining\Identity\ASPNET_SETREG,password"
```

Everything before the final comma is the registry key path. Everything after is the value name (`userName` or `password`) within that key. Before trusting this description: run the command, open `regedit`, and confirm the exact key structure and value names `aspnet_setreg.exe` created. The implementation of `DecryptRegistryKey()` expects a specific structure, and it's worth verifying hands-on rather than assuming.

### Registry Permissions

The account that runs the application needs **Read** access to the registry key. `DecryptRegistryKey()` throws a `DatabankException` if `OpenSubKey()` returns `null` -- which is exactly what happens when the calling account can't read the key, indistinguishable from the key not existing. Grant access via `regedit`: right-click the key > Permissions > add the application's account with Read.

---

## Mini-Program 2: Reading the Full Section

With all four classes written and App.config set up, add the second block to `Main()`:

```csharp
var onBaseSettings = (OnBaseSettings)ConfigurationManager.GetSection(OnBaseSettings.SectionName);

foreach (DocumentTypeElement documentType in onBaseSettings.DocumentTypes)
{
    Console.WriteLine($"Name: [{documentType.Name}]  ID: [{documentType.Id}]");
    foreach (KeywordTypeElement keywordType in documentType.KeywordTypes)
    {
        Console.WriteLine($"  Name: [{keywordType.Name}]  ID: [{keywordType.Id}]  Data Type: [{keywordType.DataType}]");
    }
}

Console.WriteLine($"App Server: {onBaseSettings.ServiceLocation.ServicePath}");
Console.WriteLine($"Username:   {onBaseSettings.ServiceLocation.DecryptedUsername}");
Console.WriteLine($"Password:   {onBaseSettings.ServiceLocation.DecryptedPassword}");
```

**Run it.** The output reflects exactly what's in App.config -- no code changes needed to add a document type, change an ID, or point at a different environment. The credential properties return plaintext regardless of whether the config stores plain text or an encrypted registry reference. Try adding a second `<documentType>` entry to App.config and running again without touching the code.

---

## Why Custom Config Sections Instead of AppSettings

`ConfigurationManager.AppSettings["key"]` returns a flat string. For anything structured -- a list of document types, each with its own list of keyword types and IDs -- `AppSettings` forces either a fragile naming convention (`docType.1.name`, `docType.1.id`, ...) or parsing a JSON/XML string stored in a single value. Neither is maintainable.

A custom `ConfigurationSection` gives you:
- Typed, structured access -- no string parsing in calling code.
- Required-attribute enforcement -- a missing `IsRequired = true` property throws `ConfigurationErrorsException` with a clear message at load time, not a silent null later.
- Zero code changes to add a document type or change an ID -- edit App.config only.
- `PostDeserialize()` validation for cross-property rules that can't be expressed with `IsRequired` alone.

---

## Takeaways

- `ConfigurationSection`, `ConfigurationElement`, and `ConfigurationElementCollection` are the three base classes. Every custom class in this hierarchy needs `IsReadOnly() => false`.
- `[ConfigurationProperty("xmlName", IsRequired = true/false)]` maps each C# property to an XML attribute. Read/write through `base[...]`/`this[...]`, not a backing field.
- `[ConfigurationCollection(typeof(T), AddItemName = "elementName", ...)]` renames child elements from the default `<add>` to something meaningful.
- A collection class overrides `CreateNewElement()` (returns a new element instance) and `GetElementKey()` (returns the element's unique key). Everything else wraps the inherited `Base*` methods.
- `PostDeserialize()` runs immediately after XML parsing -- use it for cross-property validation that spans multiple attributes.
- The section name constant lives on the section class and gets passed to `ConfigurationManager.GetSection()`.
- `IsEncrypted()`/`DecryptRegistryKey()` in the `Decrypted*` properties keep credential handling invisible to callers. Verify the exact registry structure `aspnet_setreg.exe` produces before depending on it.
