---
title: "Custom Configuration Sections"
chapter: 5
index: 1
dependencies: []
launchMode: external
targetFramework: net48
---

```csharp
using System;
using System.Configuration;
using System.IO;
using System.Reflection;

// ---------------------------------------------------------------------------
// The real pattern: ConfigurationSection / ConfigurationElementCollection /
// ConfigurationElement with [ConfigurationProperty] attributes.
// ---------------------------------------------------------------------------

public class KeywordTypeElement : ConfigurationElement
{
    [ConfigurationProperty("name",       IsRequired = true)]
    public string Name       { get => (string)base["name"];       set => base["name"]       = value; }

    [ConfigurationProperty("id",         IsRequired = true)]
    public long   Id         { get => (long)base["id"];           set => base["id"]         = value; }

    [ConfigurationProperty("dataType",   IsRequired = true)]
    public string DataType   { get => (string)base["dataType"];   set => base["dataType"]   = value; }

    [ConfigurationProperty("dataLength", IsRequired = true)]
    public int    DataLength { get => (int)base["dataLength"];    set => base["dataLength"] = value; }

    public KeywordTypeElement() { }

    public KeywordTypeElement(string name, long id, string dataType, int dataLength)
    { Name = name; Id = id; DataType = dataType; DataLength = dataLength; }

    public override bool IsReadOnly() => false;
}

public class KeywordTypeCollection : ConfigurationElementCollection
{
    protected override ConfigurationElement CreateNewElement()                   => new KeywordTypeElement();
    protected override object               GetElementKey(ConfigurationElement e) => ((KeywordTypeElement)e).Name;

    public KeywordTypeElement this[int i]
    {
        get => (KeywordTypeElement)BaseGet(i);
        set { if (BaseGet(i) != null) BaseRemoveAt(i); BaseAdd(i, value); }
    }
    public new KeywordTypeElement this[string name] => (KeywordTypeElement)BaseGet(name);

    public void Add(KeywordTypeElement e)    => BaseAdd(e);
    public void Remove(KeywordTypeElement e) { if (BaseIndexOf(e) >= 0) BaseRemove(e.Name); }
    public void Remove(string name)          => BaseRemove(name);
    public void RemoveAt(int i)              => BaseRemoveAt(i);
    public void Clear()                      => BaseClear();

    protected override void BaseAdd(ConfigurationElement e) => BaseAdd(e, false);
    public override bool IsReadOnly() => false;
}

public class DocumentTypeElement : ConfigurationElement
{
    [ConfigurationProperty("name", IsRequired = true)]
    public string Name { get => (string)base["name"]; set => base["name"] = value; }

    [ConfigurationProperty("id",   IsRequired = true)]
    public long   Id   { get => (long)base["id"];     set => base["id"]   = value; }

    [ConfigurationProperty("keywordTypes", IsRequired = true)]
    [ConfigurationCollection(typeof(KeywordTypeElement),
        AddItemName = "keywordType", ClearItemsName = "clear", RemoveItemName = "remove")]
    public KeywordTypeCollection KeywordTypes
    {
        get => (KeywordTypeCollection)base["keywordTypes"];
        set => base["keywordTypes"] = value;
    }

    public DocumentTypeElement() { }
    public override bool IsReadOnly() => false;
}

public class DocumentTypeCollection : ConfigurationElementCollection
{
    protected override ConfigurationElement CreateNewElement()                   => new DocumentTypeElement();
    protected override object               GetElementKey(ConfigurationElement e) => ((DocumentTypeElement)e).Name;

    public DocumentTypeElement this[int i]
    {
        get => (DocumentTypeElement)BaseGet(i);
        set { if (BaseGet(i) != null) BaseRemoveAt(i); BaseAdd(i, value); }
    }
    public new DocumentTypeElement this[string name] => (DocumentTypeElement)BaseGet(name);

    public void Add(DocumentTypeElement e)    => BaseAdd(e);
    public void Remove(DocumentTypeElement e) { if (BaseIndexOf(e) >= 0) BaseRemove(e.Name); }
    public void Remove(string name)           => BaseRemove(name);
    public void RemoveAt(int i)               => BaseRemoveAt(i);
    public void Clear()                       => BaseClear();

    protected override void BaseAdd(ConfigurationElement e) => BaseAdd(e, false);
    public override bool IsReadOnly() => false;
}

// ServiceLocation uses plain strings.
// The real project uses Hyland.Unity.LicenseType for LicenseType, but the
// ConfigurationElement pattern is identical regardless of the property type.
public class ServiceLocation : ConfigurationElement
{
    [ConfigurationProperty("servicePath",         IsRequired = true)]
    public string ServicePath         => (string)this["servicePath"];

    [ConfigurationProperty("dataSource",          IsRequired = true)]
    public string DataSource          => (string)this["dataSource"];

    [ConfigurationProperty("licenseType",         IsRequired = true)]
    public string LicenseType         => (string)this["licenseType"];

    [ConfigurationProperty("useNTAuthentication", IsRequired = true)]
    public bool   UseNtAuthentication => (bool)this["useNTAuthentication"];

    [ConfigurationProperty("username", IsRequired = false)]
    public string Username            => (string)this["username"];

    [ConfigurationProperty("password", IsRequired = false)]
    public string Password            => (string)this["password"];

    public override bool IsReadOnly() => false;
}

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
        AddItemName = "documentType", ClearItemsName = "clear", RemoveItemName = "remove")]
    public DocumentTypeCollection DocumentTypes
    {
        get => (DocumentTypeCollection)base["documentTypes"];
        set => this["documentTypes"] = value;
    }

    public OnBaseSettings() { }
    public override bool IsReadOnly() => false;
}

// ---------------------------------------------------------------------------
// Program
// ---------------------------------------------------------------------------

internal static class Program
{
    private static void Main()
    {
        // The runner compiles this to a temp .exe on disk (launchMode: external),
        // so the assembly exists as a file. ConfigurationManager therefore needs
        // a config file alongside it -- we write one to the same temp directory.
        string exePath      = Assembly.GetExecutingAssembly().Location;
        string configPath   = exePath + ".config";
        string assemblyName = Path.GetFileNameWithoutExtension(exePath);

        File.WriteAllText(configPath,
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
            "<configuration>\n" +
            "  <configSections>\n" +
            "    <!-- The type attribute must match the class name and the assembly name -->\n" +
            "    <section name=\"onBaseSettings\"\n" +
           $"             type=\"OnBaseSettings, {assemblyName}\"/>\n" +
            "  </configSections>\n" +
            "  <appSettings>\n" +
            "    <add key=\"subject\" value=\"Configuration Classes\"/>\n" +
            "  </appSettings>\n" +
            "  <onBaseSettings>\n" +
            "    <documentTypes>\n" +
            "      <documentType name=\"TST - Image\" id=\"101\">\n" +
            "        <keywordTypes>\n" +
            "          <keywordType name=\"Description\"    id=\"1\"   dataType=\"Alphanumeric\" dataLength=\"50\"/>\n" +
            "          <keywordType name=\"TST - Alpha 10\" id=\"101\" dataType=\"Alphanumeric\" dataLength=\"10\"/>\n" +
            "        </keywordTypes>\n" +
            "      </documentType>\n" +
            "      <documentType name=\"TST - Word\" id=\"102\">\n" +
            "        <keywordTypes>\n" +
            "          <keywordType name=\"Description\" id=\"1\"   dataType=\"Alphanumeric\" dataLength=\"50\"/>\n" +
            "          <keywordType name=\"TST - Date\"  id=\"102\" dataType=\"Date\"         dataLength=\"0\"/>\n" +
            "        </keywordTypes>\n" +
            "      </documentType>\n" +
            "      <documentType name=\"TST - PDF\" id=\"103\">\n" +
            "        <keywordTypes>\n" +
            "          <keywordType name=\"TST - Date\"     id=\"102\" dataType=\"Date\"         dataLength=\"0\"/>\n" +
            "          <keywordType name=\"Description\"    id=\"1\"   dataType=\"Alphanumeric\" dataLength=\"50\"/>\n" +
            "          <keywordType name=\"TST - Alpha 10\" id=\"101\" dataType=\"Alphanumeric\" dataLength=\"10\"/>\n" +
            "        </keywordTypes>\n" +
            "      </documentType>\n" +
            "    </documentTypes>\n" +
            "    <serviceLocation servicePath=\"http://localhost/appserver/service.asmx\"\n" +
            "                     dataSource=\"OnBase\"\n" +
            "                     licenseType=\"QueryMetering\"\n" +
            "                     useNTAuthentication=\"false\"\n" +
            "                     username=\"svc_training\"\n" +
            "                     password=\"P@ssw0rd\"/>\n" +
            "  </onBaseSettings>\n" +
            "</configuration>");

        // Exactly the same reading code as the real project.
        var map    = new ExeConfigurationFileMap { ExeConfigFilename = configPath };
        var config = ConfigurationManager.OpenMappedExeConfiguration(
            map, ConfigurationUserLevel.None);

        string subject = config.AppSettings.Settings["subject"]?.Value ?? "(not set)";
        Console.WriteLine($"Today, we are learning about [{subject}]{Environment.NewLine}");

        var settings = (OnBaseSettings)config.GetSection(OnBaseSettings.SectionName);

        foreach (DocumentTypeElement docType in settings.DocumentTypes)
        {
            Console.WriteLine($"Document Type:\n  Name: [{docType.Name}]\n  ID: [{docType.Id}]\n  Keyword Types:");
            foreach (KeywordTypeElement kwType in docType.KeywordTypes)
            {
                string lengthLine = kwType.DataType.ToLower().StartsWith("alpha")
                    ? $"\n      Length: [{kwType.DataLength}]" : "";
                Console.WriteLine(
                    $"    Keyword Type:\n      Name: [{kwType.Name}]\n      ID: [{kwType.Id}]" +
                    $"\n      Data Type: [{kwType.DataType}]{lengthLine}");
            }
        }
        Console.WriteLine();

        Console.WriteLine("OnBase Connection Settings:");
        Console.WriteLine("--------------------------");
        Console.WriteLine($"App Server URL: {settings.ServiceLocation.ServicePath}");
        Console.WriteLine($"Data Source:    {settings.ServiceLocation.DataSource}");
        Console.WriteLine($"License Type:   {settings.ServiceLocation.LicenseType}");
        Console.WriteLine($"NT Auth:        {settings.ServiceLocation.UseNtAuthentication}");
        Console.WriteLine($"Username:       {settings.ServiceLocation.Username}");
        Console.WriteLine($"Password:       {settings.ServiceLocation.Password}");
    }
}
```
