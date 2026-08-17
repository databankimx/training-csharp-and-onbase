# ConfigurationClasses

## Introduction

`System.Configuration`: building custom, strongly-typed configuration sections instead of relying on flat `appSettings` key/value pairs. This is genuinely useful the moment your configuration needs any structure beyond a single list of settings.

---

## The Default appSettings Section

```csharp
string appSetting = ConfigurationManager.AppSettings["subject"];
```

```xml
<appSettings>
  <add key="subject" value="Configuraton Classes"/>
</appSettings>
```

Fine for flat key/value pairs. It doesn't scale to anything with real structure, like a list of document types, each with its own nested list of keyword types.

---

## A Custom Configuration Section

```csharp
public class OnBaseSettings : ConfigurationSection
{
    public const string SectionName = "onBaseSettings";

    [ConfigurationProperty("serviceLocation", IsRequired = true)]
    public ServiceLocation ServiceLocation
    {
        get => (ServiceLocation)base["serviceLocation"];
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

```csharp
var onBaseSettings = (OnBaseSettings)ConfigurationManager.GetSection(OnBaseSettings.SectionName);
```

`ConfigurationSection` is the root of a custom config section, retrieved by name via `ConfigurationManager.GetSection()` and cast from `object`. `[ConfigurationProperty("xmlAttributeName")]` maps a C# property to an XML attribute or child element, `get`/`set` read and write through `this[...]`/`base[...]` rather than a plain backing field.

`[ConfigurationCollection(...)]` on a collection property lets you rename the XML child element to something readable, `<documentType>` instead of the framework's default `<add>`.

Every configuration class here overrides `IsReadOnly()` to return `false`. Without it, `System.Configuration` locks the object after loading, which blocks programmatic modification.

---

## A Configuration Element

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
}
```

A `ConfigurationElement` represents one XML element's worth of typed properties. `DocumentTypeElement` also holds a nested `KeywordTypeCollection`, elements can nest other elements and collections arbitrarily deep.

---

## A Configuration Element Collection

```csharp
public class DocumentTypeCollection : ConfigurationElementCollection
{
    protected override ConfigurationElement CreateNewElement()
    {
        return new DocumentTypeElement();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
        return ((DocumentTypeElement)element).Name;
    }

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

`CreateNewElement()` and `GetElementKey()` are the two required overrides, they're what let the base class parse repeated XML elements into instances of your own class, keyed by whichever property you choose (`Name`, here). Everything else (`Add`, `Remove`, indexers) just wraps the inherited `Base*` methods in a strongly-typed, easier-to-use surface.

---

## Iterating the Result

```csharp
foreach (DocumentTypeElement documentType in onBaseSettings.DocumentTypes)
{
    Console.WriteLine($"Name: [{documentType.Name}]  ID: [{documentType.Id}]");
    foreach (KeywordTypeElement keywordType in documentType.KeywordTypes)
    {
        Console.WriteLine($"    Name: [{keywordType.Name}]  ID: [{keywordType.Id}]  Data Type: [{keywordType.DataType}]");
    }
}
```

Once loaded, the whole structure behaves like ordinary C# objects and collections, nested `foreach` loops, property access, no manual XML parsing anywhere in the calling code.

---

## Optionally Encrypted Values

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

A configuration value can be either a plain-text string or a special `registry:HKLM\...,keyName` reference pointing at a value encrypted into the registry (via Windows DPAPI, using the same `aspnet_setreg.exe` tool ASP.NET has long used to keep credentials out of plain-text config files). `IsEncrypted()` recognizes the special format with a regex, `DecryptRegistryKey()` reads the registry key and decrypts it. `DecryptedUsername`/`DecryptedPassword` hide that decision entirely, callers just read a property and get the real value back, encrypted or not.

```csharp
protected override void PostDeserialize()
{
    base.PostDeserialize();

    if (UseNtAuthentication == false && (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password)))
    {
        throw new ConfigurationErrorsException("...");
    }
    // ...
}
```

`PostDeserialize()` runs immediately after the XML is parsed, a good place for validation that spans multiple properties (here, making sure username/password are present when they're actually required, given the authentication mode).

---

## Setting Up the Config File

Getting `App.config` working end to end for real: creating an actual `onBaseSettings` section, and encrypting real credentials into it instead of leaving them in plain text.

### The `onBaseSettings` Section Itself

```xml
<configuration>
  <configSections>
    <section name="onBaseSettings" type="CSharp.Ch05.Supplemental.ConfigurationClasses.Models.Configuration.OnBaseSettings, CSharp.Ch05.Supplemental.ConfigurationClasses"/>
  </configSections>

  <onBaseSettings>
    <documentTypes>
      <documentType name="TST - Image" id="101">
        <keywordTypes>
          <keywordType name="Description" id="1" dataType="Alphanumeric" dataLength="50"/>
        </keywordTypes>
      </documentType>
    </documentTypes>

    <serviceLocation servicePath="http://localhost/appserver/service.asmx"
                     dataSource="OnBase"
                     licenseType="QueryMetering"
                     useNTAuthentication="false"
                     domain=""
                     username="someUsername"
                     password="somePassword"/>
  </onBaseSettings>
</configuration>
```

Every custom section needs a `<section>` declaration inside `<configSections>` before it can be used anywhere else in the file, `name` is the XML element name you'll actually write (`onBaseSettings`), `type` is the fully qualified class name plus assembly name for the `ConfigurationSection` that parses it. Get this declaration wrong (typo the type name, forget the assembly) and you'll get a `ConfigurationErrorsException` the moment `ConfigurationManager.GetSection()` is called, not at compile time.

With `useNTAuthentication="false"`, plain-text `username`/`password` attributes work fine as-is, this is the fastest way to get the lesson running locally. Encrypting them, below, is what you'd actually do before this config file went anywhere real.

### Encrypting Credentials with aspnet_setreg.exe

`aspnet_setreg.exe` lives in this solution's shared `Resources` folder. It's a long-standing ASP.NET tool for exactly this problem, keeping credentials out of a plain-text config file by encrypting them into the Windows registry via DPAPI instead.

```
aspnet_setreg.exe -k:SOFTWARE\DataBank\DeveloperTraining\Identity -u:yourUsername -p:yourPassword
```

`-k` is the registry key path (under `HKEY_LOCAL_MACHINE`) to encrypt into, `-u`/`-p` are the actual credentials. This needs to be run from an elevated command prompt, writing to `HKLM` requires administrator rights.

Once that's run, point the config attributes at the encrypted values instead of plain text:

```xml
username="registry:HKLM\SOFTWARE\DataBank\DeveloperTraining\Identity\ASPNET_SETREG,userName"
password="registry:HKLM\SOFTWARE\DataBank\DeveloperTraining\Identity\ASPNET_SETREG,password"
```

The `registry:` prefix and comma-separated suffix are what `IsEncrypted()` recognizes (see the code walkthrough above), everything before the last comma is the registry path to open, everything after is the specific value name to read out of it (`userName` or `password`). `DecryptedUsername`/`DecryptedPassword` then transparently decrypt these the moment they're read.

**Before treating this as gospel**: the exact registry structure `aspnet_setreg.exe` produces (specifically, whether it creates `userName` and `password` as two separate named values under an `ASPNET_SETREG` key, which is what `DecryptRegistryKey()` expects to find) is worth confirming by actually running the command and checking the result in `regedit` yourself, rather than trusting this description blindly. This is standard, well-documented ASP.NET tooling, but its exact behavior is worth verifying hands-on before depending on it.

### Permissions

Whichever account actually runs the application needs **Read** access to that registry key, `DecryptRegistryKey()` throws a `DatabankException` if `OpenSubKey()` comes back `null`, which is exactly what happens when the calling account can't see the key at all, indistinguishable at that point from the key simply not existing. Grant this via `regedit`, right-click the key, Permissions, add the account running the application with Read access.
