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
