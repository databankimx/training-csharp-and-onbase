# ImplementingClassHierarchies (Supplemental)

## Introduction

A small, ordinary class hierarchy: an address book contact. `Person → Contact`, with `Address`, `BusinessAddress`, and `Telephone` composed in rather than inherited. No interfaces, no generics gymnastics, just inheritance and composition used the way you'd actually use them in a real feature.

---

## Inheritance: Contact Is a Person

```csharp
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }

    public string FullName(bool reverse = false, bool includeMiddle = false)
    {
        string front = includeMiddle ? $"{FirstName} {MiddleName}" : FirstName;
        return reverse ? $"{LastName}, {front}" : $"{front} {LastName}";
    }
}

public class Contact : Person
{
    public Telephone HomePhone { get; set; }
    public Telephone WorkPhone { get; set; }
    public Telephone MobilePhone { get; set; }
    public string Email { get; set; }
    public Address HomeAddress { get; set; }
    public BusinessAddress WorkAddress { get; set; }
}
```

`Contact : Person` means a `Contact` *is a* `Person`, everything `Person` provides (the name properties, `FullName()`) is automatically available on `Contact` too, plus whatever `Contact` adds on top.

```csharp
Console.WriteLine(me.FullName());                          // Jordan Rivera
Console.WriteLine(me.FullName(reverse: true));              // Rivera, Jordan
Console.WriteLine(me.FullName(includeMiddle: true));        // Jordan A Rivera
```

`FullName()`'s two optional parameters combine independently, `reverse` controls last-name-first ordering, `includeMiddle` controls whether the middle name shows up at all. Neither depends on the other, so all four combinations are valid and produce different results.

---

## Composition: Contact Has an Address

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

`Contact.HomeAddress` and `Contact.WorkAddress` are properties *of type* `Address`/`BusinessAddress`, not base classes `Contact` inherits from. A `Contact` *has an* `Address`, it isn't *a kind of* `Address`. That's composition, building a class out of other objects as fields, rather than inheritance, building a class as a specialization of another class.

`BusinessAddress : Address` shows the same inheritance relationship one level down, a business address genuinely is a regular address, just with a company name added.

Telling these apart matters: reach for inheritance when the relationship is truly "is a," and composition when it's "has a." Mixing them up (inheriting where you should compose) is one of the most common early object-oriented design mistakes.

---

## A Self-Validating Property

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

    private static string FormatPhoneNumber(string phoneNumber)
    {
        return $"({phoneNumber.Substring(0, 3)}) {phoneNumber.Substring(3, 3)}-{phoneNumber.Substring(6, 4)}";
    }
}
```

```csharp
var phone = new Telephone { Number = "2145550172" };
Console.WriteLine(phone.Number); // (214) 555-0172
```

`Number`'s setter strips non-digit formatting characters and validates the result is exactly 10 digits before ever touching the backing field, an invalid value never gets stored in the first place. The getter formats on the way out, so no matter how the number was entered, reading `Number` back always gives you the same consistently formatted string.

---

## A Constrained Generic Extension Method

```csharp
public static string Initials<T>(this T t) where T : Person
{
    var person = t as Person;
    if (string.IsNullOrEmpty(person.FirstName) || string.IsNullOrEmpty(person.LastName))
        throw new DatabankException("Unable to produce initials. One or more required name(s) blank!");
    return $"{person.FirstName.Substring(0, 1)}{(string.IsNullOrEmpty(person.MiddleName) ? "" : person.MiddleName.Substring(0, 1))}{person.LastName.Substring(0, 1)}".ToUpper();
}
```

```csharp
var me = new Contact { FirstName = "Jordan", MiddleName = "A", LastName = "Rivera", ... };
Console.WriteLine(me.Initials()); // JAR
```

`Initials<T>()` is an extension method constrained to `Person` and its descendants (`where T : Person`), which lets it run on a `Contact`, a `Person`, or any future subclass. It builds the initials from whichever name parts are actually present, first, middle (if set), and last, all uppercased.
