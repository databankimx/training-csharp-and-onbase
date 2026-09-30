# Chapter 5 Supplemental: Implementing Class Hierarchies

## What This Is About

The main Chapter 5 lesson uses deliberately illustrative examples: cars for sorting, faculty for hierarchy, org charts for enumeration. This project is the opposite kind of example on purpose -- an ordinary, boring class hierarchy for something genuinely mundane: an address book contact.

Sometimes the most useful example is the unglamorous one. You will write a `Contact` class at some point. You will probably never write a `TreeEnumerator`.

---

## The Shape of It

```
Person                  -- FirstName, MiddleName, LastName, FullName()
  └── Contact           -- + Email, HomePhone, WorkPhone, MobilePhone,
                              HomeAddress, WorkAddress

Address                 -- StreetAddress, City, State, ZipCode
  └── BusinessAddress   -- + CompanyName

Telephone               -- Number (self-validating)
```

Two inheritance chains, and the second one composes into the first rather than joining it.

---

## How to Write This Program

Build the types in dependency order -- the ones that other types need have to exist first.

### Step 1: Address and BusinessAddress

```csharp
public class Address
{
    public string StreetAddress { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
}

public class BusinessAddress : Address
{
    public string CompanyName { get; set; }
}
```

`BusinessAddress : Address` -- a business address IS-A regular address plus a company name. That "is a" sentence is the test for inheritance: say it out loud, and if it sounds right, inherit. If it sounds wrong ("a company name is an address" doesn't make sense), compose instead.

### Step 2: Telephone, With Built-In Validation

```csharp
public class Telephone
{
    private string number;

    public string Number
    {
        get => FormatPhoneNumber(number);
        set => number = SetPhoneNumber(value);
    }

    private string SetPhoneNumber(string raw)
    {
        string digits = new string(raw.Where(char.IsDigit).ToArray());
        if (digits.Length != 10)
            throw new InvalidDataException($"Phone number must be exactly 10 digits. Got: {raw}");
        return digits;
    }

    private string FormatPhoneNumber(string digits) =>
        string.IsNullOrEmpty(digits) ? "" : $"({digits[..3]}) {digits[3..6]}-{digits[6..]}";
}
```

```csharp
var phone = new Telephone { Number = "2145550234" };
Console.WriteLine(phone.Number); // (214) 555-0234

phone = new Telephone { Number = "(214) 555-0199" }; // strips formatting, same result
Console.WriteLine(phone.Number);

try
{
    phone = new Telephone { Number = "banana" }; // throws
}
catch (InvalidDataException ex)
{
    Console.WriteLine(ex.Message);
}
```

Run it. Two formatted numbers, then an error message.

The property setter never stores an invalid value in the first place -- `SetPhoneNumber` strips non-digit characters and throws if what's left isn't exactly ten digits. The getter formats on the way out, so callers always get a consistently formatted string regardless of how the number was entered. `"2145550234"`, `"(214) 555-0234"`, and `"214-555-0234"` all normalize to the same stored value.

This is the entire argument for properties over public fields: a public `string Number;` field can hold `"banana"`. A property can't, because there's a method body standing between the caller and the storage. Store canonical, format on display.

Notice also that throwing on invalid input means a `Telephone` object that exists is always a valid `Telephone`. Nothing downstream ever has to ask whether the number is usable. That property -- "if it constructed, it's valid" -- removes a lot of defensive checking everywhere else in the codebase.

### Step 3: Person

```csharp
public class Person
{
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }

    public string FullName(bool reverse = false, bool includeMiddle = false)
    {
        string middle = includeMiddle && !string.IsNullOrEmpty(MiddleName) ? $" {MiddleName}" : "";
        return reverse
            ? $"{LastName}, {FirstName}{middle}"
            : $"{FirstName}{middle} {LastName}";
    }
}
```

```csharp
var p = new Person { FirstName = "Jordan", MiddleName = "A", LastName = "Rivera" };
Console.WriteLine(p.FullName());
Console.WriteLine(p.FullName(reverse: true));
Console.WriteLine(p.FullName(includeMiddle: true));
```

Run it. `Jordan Rivera`, `Rivera, Jordan`, `Jordan A Rivera`.

The calls use named arguments (`reverse: true`) rather than positional ones. With multiple `bool` parameters in a signature, a bare `FullName(true)` tells the reader nothing about which flag is being set. Named arguments make the call self-documenting, and they become essential when you want to set the second optional parameter but not the first.

One caution worth carrying forward: optional parameter defaults are baked into the *calling* assembly at compile time, the same way `const` values are. Changing a default in a shared library doesn't take effect for consumers until they're rebuilt. For anything crossing an assembly boundary, overloads are safer than optional parameters.

### Step 4: Contact -- Inheritance Plus Composition

```csharp
public class Contact : Person
{
    public string Email { get; set; }
    public Telephone HomePhone { get; set; }
    public Telephone WorkPhone { get; set; }
    public Telephone MobilePhone { get; set; }
    public Address HomeAddress { get; set; }
    public BusinessAddress WorkAddress { get; set; }
}
```

```csharp
var someone = new Contact
{
    FirstName = "Jordan", MiddleName = "A", LastName = "Rivera",
    Email = "jrivera@databankimx.com",
    HomePhone = new Telephone { Number = "2145550234" },
    WorkPhone = new Telephone { Number = "2145550199" },
    HomeAddress = new Address
    {
        StreetAddress = "123 Main St", City = "Lewisville",
        State = "TX", ZipCode = "75067"
    },
    WorkAddress = new BusinessAddress
    {
        CompanyName = "DataBank IMX",
        StreetAddress = "456 Corporate Dr", City = "Lewisville",
        State = "TX", ZipCode = "75067"
    }
};

Console.WriteLine(someone.FullName());
Console.WriteLine(someone.Email);
Console.WriteLine(someone.HomePhone.Number);
Console.WriteLine($"{someone.HomeAddress.City}, {someone.HomeAddress.State}");
Console.WriteLine($"{someone.WorkAddress.CompanyName} -- {someone.WorkAddress.City}");
```

Run it. Name, email, formatted phone number, home city, work company and city.

`Contact : Person` is inheritance -- a `Contact` IS-A `Person`. `Contact.HomeAddress` (type `Address`) is composition -- a `Contact` HAS-AN `Address`, not IS-AN `Address`. Getting this distinction backwards is one of the most common early object-oriented design mistakes.

The standard test is the sentence. "A contact is a person" -- true, so inherit. "A contact is an address" -- obviously false, so compose. "A business address is an address" -- true, so inherit.

When the sentence sounds wrong, the inheritance is wrong. The industry shorthand for this is "prefer composition over inheritance," and the reason isn't that inheritance is bad -- it's that inheritance is a permanent, single-slot commitment (you get exactly one base class, forever), while composition can be changed, swapped, or extended at any time without restructuring the type.

---

## Takeaways

- "Is a" means inherit. "Has a" means compose. Say the sentence out loud before deciding.
- You get one base class forever, but unlimited composed members. Prefer composition when it's a close call.
- Properties earn their keep by validating and normalizing -- store canonical, format on display.
- Throw on invalid input at the boundary so nothing downstream has to re-check.
- Named arguments make optional-parameter call sites readable.
