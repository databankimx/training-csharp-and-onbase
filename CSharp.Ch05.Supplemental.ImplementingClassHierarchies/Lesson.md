# Chapter 5 Supplemental: Implementing Class Hierarchies

## What This Is

A plain, practical class hierarchy for an address book contact -- no exam concepts, no interfaces to memorize, just a realistic use of inheritance and composition for something a real application would actually build.

The hierarchy: `Person` (name utilities) -> `Contact` (adds address, phone, and email) -> `BusinessAddress` (extends `Address` with a company name). Two supporting value types, `Address` and `Telephone`, round it out.

---

## How to Write This Program

The types live in `Models/Objects/`. Read through them before running anything -- the point of this project is the structure, not a mini-program exercise.

### The Hierarchy at a Glance

`Person` holds name parts and provides formatting utilities:

```csharp
public string FullName(bool reverse = false, bool includeMiddle = false)
public string Initials()
```

`Contact : Person` adds the things a contact record actually needs:

```csharp
public Address HomeAddress { get; set; }
public BusinessAddress WorkAddress { get; set; }
public Telephone HomePhone { get; set; }
public Telephone WorkPhone { get; set; }
public Telephone MobilePhone { get; set; }
public string Email { get; set; }
```

`BusinessAddress : Address` adds one thing `Address` doesn't have: a company name.

`Telephone` is a simple value holder with a formatted phone number property.

### Run It

```csharp
var someone = new Contact
{
    FirstName = "Jordan", MiddleName = "A", LastName = "Rivera",
    HomeAddress = new Address { StreetAddress = "123 Main St", City = "Lewisville", State = "TX", ZipCode = "75067" },
    HomePhone = new Telephone { Number = "2145550234" },
    Email = "jrivera@databankimx.com",
    WorkAddress = new BusinessAddress
    {
        CompanyName = "DataBank IMX",
        StreetAddress = "456 Corporate Dr", City = "Lewisville", State = "TX", ZipCode = "75067"
    }
};

Console.WriteLine(someone.FullName());
Console.WriteLine(someone.FullName(reverse: true));
Console.WriteLine(someone.FullName(includeMiddle: true));
Console.WriteLine(someone.Initials());
Console.WriteLine(someone.HomeAddress.StreetAddress);
Console.WriteLine(someone.WorkAddress.CompanyName);
```

Run it. Three name formats, initials, home address, company name.

---

## What's Worth Noticing

`BusinessAddress : Address` is the composition-versus-inheritance question in miniature. A business address IS-A address (same street/city/state/zip fields, same formatting logic) plus a company name -- inheritance is the right call. Contrast this with `Contact`'s relationship to `Address`: a contact HAS-A home address, not IS-A address. The relationship decides the mechanism.

`FullName` uses optional parameters with defaults (`reverse = false`, `includeMiddle = false`) to give callers flexibility without requiring an overload for every combination. The named-parameter call syntax (`FullName(reverse: true)`) makes the intent clear at the call site without having to count positional arguments.

The `Telephone` class wraps a phone number string rather than storing it as a raw `string` directly on `Contact`. The payoff is that formatting, validation, and any future display logic live in one place. If the formatting rule changes, it changes once.
