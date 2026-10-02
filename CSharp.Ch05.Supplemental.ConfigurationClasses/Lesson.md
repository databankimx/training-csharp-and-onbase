# Chapter 5 Supplemental: Configuration Classes

## What This Is

Custom `ConfigurationSection`, `ConfigurationElement`, and `ConfigurationElementCollection` classes for reading structured OnBase settings from App.config. This is the most directly job-applicable project in Chapter 5 -- the same pattern shows up in most existing DataBank OnBase projects, since it's how structured per-environment configuration (connection strings, document type IDs, keyword type definitions) is kept out of the code and in a config file where ops can change it without a rebuild.

---

## What the Config Structure Looks Like

The App.config in this project defines a custom section named `onBaseSettings` with nested elements. Abbreviated shape:

```xml
<onBaseSettings>
  <documentTypes>
    <documentType name="Invoice" id="1001">
      <keywordTypes>
        <keywordType name="Vendor" id="2001" dataType="Alpha" dataLength="50" />
        <keywordType name="Amount" id="2002" dataType="Numeric" />
      </keywordTypes>
    </documentType>
  </documentTypes>
  <serviceLocation
    servicePath="http://onbase-server/AppNetWebService/Service.asmx"
    dataSource="ONBASE_DB"
    username="enc:..."
    password="enc:..." />
</onBaseSettings>
```

---

## The Class Structure

Three configuration classes model this structure:

**`OnBaseSettings : ConfigurationSection`** -- the root. Holds the `DocumentTypeElementCollection` and the `ServiceLocationElement`. Also exposes the section name constant (`"onBaseSettings"`) used to retrieve it.

**`DocumentTypeElement : ConfigurationElement`** -- one `<documentType>` entry. Exposes `Name`, `Id`, and a `KeywordTypeElementCollection`.

**`KeywordTypeElement : ConfigurationElement`** -- one `<keywordType>` entry. Exposes `Name`, `Id`, `DataType`, and `DataLength`.

**`ServiceLocationElement : ConfigurationElement`** -- the `<serviceLocation>` element. Exposes `ServicePath`, `DataSource`, and encrypted-username/password properties with decryption handled in the property getter so callers always get plaintext.

Each property is backed by a `[ConfigurationProperty(...)]` attribute that names the XML attribute it maps to and whether it's required.

---

## How to Read It

```csharp
var onBaseSettings = (OnBaseSettings)ConfigurationManager.GetSection(OnBaseSettings.SectionName);

foreach (DocumentTypeElement docType in onBaseSettings.DocumentTypes)
{
    Console.WriteLine($"Document Type: {docType.Name} ({docType.Id})");
    foreach (KeywordTypeElement kwType in docType.KeywordTypes)
    {
        Console.WriteLine($"  Keyword Type: {kwType.Name} ({kwType.Id}) - {kwType.DataType}");
    }
}

Console.WriteLine($"App Server: {onBaseSettings.ServiceLocation.ServicePath}");
Console.WriteLine($"Username:   {onBaseSettings.ServiceLocation.DecryptedUsername}");
```

Run it. The output reflects whatever is in App.config -- no code changes needed to point at a different environment or add a document type.

---

## Why Custom Config Sections Instead of AppSettings

`ConfigurationManager.AppSettings["key"]` returns a flat string. For anything more structured -- a list of document types, each with its own list of keyword types and IDs -- `AppSettings` forces you to either invent a fragile naming convention (`docType.1.name`, `docType.1.id`, ...) or parse a JSON/XML string stored in a single value. Neither is maintainable.

A custom `ConfigurationSection` gives you typed, structured access. The XML schema is enforced by the element classes; a missing required attribute throws a `ConfigurationErrorsException` with a clear message rather than silently returning `null`. Adding a new document type or keyword type means editing App.config only, with no code change anywhere.

---

## The Encrypted Credential Pattern

`ServiceLocationElement` stores connection credentials encrypted in App.config and decrypts them on read:

```csharp
public string DecryptedUsername =>
    CryptoHelper.Decrypt(Username);
```

The encryption/decryption lives entirely inside the configuration class. Callers get plaintext; the config file never stores plaintext. This is not production-grade credential management (a secrets manager or certificate-backed approach is better for that), but it's a meaningful step above storing plaintext passwords in a config file that gets committed to version control.

---

## Takeaways

- `ConfigurationSection`, `ConfigurationElement`, and `ConfigurationElementCollection` are the three base classes for custom structured configuration.
- Each property needs a `[ConfigurationProperty(...)]` attribute naming its XML attribute and specifying `IsRequired` where appropriate.
- A collection element type overrides `CreateNewElement()` and `GetElementKey()`.
- The section name constant lives on the section class and gets passed to `ConfigurationManager.GetSection()`.
- Decryption in the property getter keeps callers clean and ensures credentials are never exposed through a plain property read.
