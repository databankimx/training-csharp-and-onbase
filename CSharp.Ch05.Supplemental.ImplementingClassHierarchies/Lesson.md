# Chapter 5 Supplemental: Implementing Class Hierarchies

## What This Is

The main chapter covers the mechanics of inheritance using deliberately illustrative examples -- cars, faculty, org charts. This supplemental is the opposite kind of example on purpose: no interfaces, no generics gymnastics, just an ordinary, boring inheritance-plus-composition hierarchy for an address book contact. Sometimes the most useful example is the unglamorous one.

The hierarchy: `Person` (name utilities) -> `Contact` (adds address, phone, and email) -> with `Address`, `BusinessAddress`, and `Telephone` composed in. No exam-style interfaces. Something you'd actually write for a real feature.

---

## How to Write This Program

The types live in `Models/Objects/`. Build them first -- the mini-program below won't compile without them. Read through each class before writing it, because the point of this project is the structural decisions, not the demo output.

### The Type Hierarchy

`Person` holds name parts and provides two formatting utilities:

```csharp
public class Person
{
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }

    public string FullName(bool reverse = false, bool includeMiddle = false)
    {
        var middle = includeMiddle && !string.IsNullOrEmpty(MiddleName) ? $" {MiddleName}" : "";
        return reverse
            ? $"{LastName}, {FirstName}{middle}"
            : $"{FirstName}{middle} {LastName}";
    }

}
```

`Contact : Person` adds address, phone, and email fields -- all composed in rather than inherited:

```csharp
public class Contact : Person
{
    public Address HomeAddress { get; set; }
    public BusinessAddress WorkAddress { get; set; }
    public Telephone HomePhone { get; set; }
    public Telephone WorkPhone { get; set; }
    public Telephone MobilePhone { get; set; }
    public string Email { get; set; }
}
```

`Address` is a plain value holder:

```csharp
public class Address
{
    public string StreetAddress { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
}
```

`BusinessAddress : Address` adds exactly one thing:

```csharp
public class BusinessAddress : Address
{
    public string CompanyName { get; set; }
}
```

`Telephone` wraps a phone number string with validation and formatting:

```csharp
public class Telephone
{
    private string number;
    private static readonly Regex NonDigits = new Regex(@"\D");

    public string Number
    {
        get => FormatPhoneNumber(number);
        set => number = SetPhoneNumber(value);
    }

    private static string SetPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            throw new InvalidDataException("Phone number cannot be blank!");

        string temp = NonDigits.Replace(phoneNumber, "");
        if (temp.Length != 10)
            throw new InvalidDataException($"Phone number {phoneNumber} does not contain ten digits!");

        return temp;
    }

    private static string FormatPhoneNumber(string phoneNumber) =>
        $"({phoneNumber.Substring(0, 3)}) {phoneNumber.Substring(3, 3)}-{phoneNumber.Substring(6, 4)}";
}
```

### Mini-Program: Building a Contact

```csharp
var someone = new Contact
{
    FirstName = "Jordan", MiddleName = "A", LastName = "Rivera",
    HomeAddress = new Address
    {
        StreetAddress = "123 Main St", City = "Lewisville", State = "TX", ZipCode = "75067"
    },
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
Console.WriteLine(someone.HomePhone.Number);       // formatted: (214) 555-0234
Console.WriteLine(someone.HomeAddress.StreetAddress);
Console.WriteLine(someone.WorkAddress.CompanyName);
```

**Run it.** Three name formats, initials, formatted phone number, home address, company name.

Now try entering a bad phone number:

```csharp
try
{
    var bad = new Telephone { Number = "12345" }; // too short
}
catch (InvalidDataException ex)
{
    Console.WriteLine(ex.Message);
}
```

**Run it.** The exception message fires during the property set -- before the backing field is ever touched.

---

## What's Worth Noticing

### Inheritance vs. Composition, Made Concrete

This hierarchy uses both mechanisms, one right next to the other, which makes it easy to see why each got chosen:

`Contact : Person` is **inheritance** -- a `Contact` IS-A `Person`. Every name-utility method on `Person` (`FullName`, `Initials`) is genuinely still true of a `Contact`. Inheritance is the right call.

`Contact.HomeAddress` (type `Address`) is **composition** -- a `Contact` HAS-A home address. An address isn't a kind of person. Inheriting `Address` into `Contact` would be wrong -- it would make `Contact` IS-A `Address`, which it isn't. Composition is the right call.

`BusinessAddress : Address` is **inheritance again** -- a business address IS-A address (same street/city/state/zip fields, same formatting logic) plus a company name. The IS-A test passes, so inheritance applies.

Getting this distinction backwards -- inheriting where you should compose, or composing where you should inherit -- is one of the most common early object-oriented design mistakes. This hierarchy is small enough to see the correct call made twice, in two different directions, in one file.

### The `Telephone` Self-Validating Property

```csharp
public string Number
{
    get => FormatPhoneNumber(number);
    set => number = SetPhoneNumber(value);
}
```

`Telephone.Number` never stores an invalid value. `SetPhoneNumber()` strips non-digit formatting characters and throws `InvalidDataException` if what remains isn't exactly 10 digits -- before the backing field is ever touched. The getter formats on the way out, so callers always see a consistently formatted string regardless of how the number was entered. `"2145550234"`, `"(214) 555-0234"`, and `"214-555-0234"` all normalize to the same stored digits and the same displayed format.

This pattern -- validate and normalize on the way in, format on the way out -- keeps the invariant enforced at the property boundary so no other code in the class needs to defend against invalid state.

### `FullName`'s Optional Parameters

`FullName(bool reverse = false, bool includeMiddle = false)` gives callers three useful combinations without requiring three overloads. The named-parameter call syntax (`FullName(reverse: true)`) makes the intent clear at the call site without counting positional arguments -- compare `FullName(true, false)` against `FullName(reverse: true)` and the second one is unambiguous at a glance.

### The `Initials` Extension Method

In the actual project, `Initials()` is a constrained generic extension method rather than an instance method on `Person`:

```csharp
public static string Initials<T>(this T t) where T : Person
{
    var person = t as Person;
    if (string.IsNullOrEmpty(person.FirstName) || string.IsNullOrEmpty(person.LastName))
        throw new DatabankException("Unable to produce initials. One or more required name(s) blank!");
    return $"{person.FirstName.Substring(0, 1)}{(string.IsNullOrEmpty(person.MiddleName) ? "" : person.MiddleName.Substring(0, 1))}{person.LastName.Substring(0, 1)}".ToUpper();
}
```

The `where T : Person` constraint means this extension only appears on `Person` instances and descendants. Worth knowing the syntax exists -- but a plain `this Person person` parameter would do the same job with less ceremony here. The generic constraint doesn't unlock any additional capability in this specific case; it's a demonstration of the pattern rather than a reason to reach for it routinely.

---

## Takeaways

- The IS-A test decides between inheritance and composition. If you can't say "`X` is a `Y`" truthfully, don't inherit -- compose.
- Validate and normalize in the property setter, format in the getter. The invariant is enforced once, at the boundary.
- Optional parameters with defaults give callers flexibility without overload proliferation. Named arguments at the call site keep the intent readable.
- A constrained generic extension method (`where T : Person`) restricts which types see the method without changing its runtime behavior. Use it when the generic type `T` itself is needed; otherwise a plain typed parameter is simpler.
