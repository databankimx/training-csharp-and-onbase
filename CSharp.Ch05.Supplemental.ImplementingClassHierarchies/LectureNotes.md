# Chapter 5 Supplemental: Implementing Class Hierarchies

## What This Is

A whole lesson added beyond the textbook (see the `Supplemental` naming convention in the solution README), a small, focused example of building a class hierarchy for something genuinely ordinary: an address book contact. `Person → Contact`, with `Address`, `BusinessAddress`, and `Telephone` composed in rather than inherited.

No bugs found.

---

## Why This Exists

The main `CSharp.Ch05.ImplementingClassHierarchies` lesson covers the mechanics (`IComparable`, `IEquatable`, `ICloneable`, `IEnumerable`, `IDisposable`) using deliberately illustrative examples (cars, faculty, org charts). This one is the opposite kind of example on purpose: no interfaces, no generics gymnastics, just an ordinary, boring inheritance-plus-composition hierarchy that looks like something you'd actually write for a real feature. Sometimes the most useful example is the unglamorous one.

---

## Inheritance vs. Composition, Side by Side

`Contact : Person` is inheritance, a `Contact` *is a* `Person`, with extra fields tacked on (phone numbers, addresses, email).

`Contact.HomeAddress` (type `Address`) and `Contact.WorkAddress` (type `BusinessAddress`) are composition, a `Contact` *has an* `Address`, not *is an* `Address`. `BusinessAddress : Address` shows the same inheritance relationship one level down, a business address is a regular address plus a company name.

Worth noticing which relationship got used where. `Contact` inheriting from `Person` makes sense because everything `Person` has (first/middle/last name, `FullName()`) is genuinely still true of a `Contact`. `Address` isn't a field *of* `Person`, it composes into `Contact` instead, because an address isn't a kind of person, it's something a contact *has*. Getting this distinction backwards (inheriting where you should compose, or vice versa) is one of the most common early object-oriented design mistakes, and this hierarchy is small enough to see the correct call made twice, clearly, in one file.

---

## A Self-Validating Property

```csharp
public string Number
{
    get => FormatPhoneNumber(number);
    set => number = SetPhoneNumber(value);
}
```

`Telephone.Number` never stores an invalid value in the first place, `SetPhoneNumber()` strips non-digit formatting characters and throws `InvalidDataException` if what's left isn't exactly 10 digits, before the backing field is ever touched. The getter formats on the way out (`(214) 718-8383`), so callers always get a consistently formatted string regardless of how it was entered (`"2147188383"`, `"(214) 718-8383"`, `"214-718-8383"` all normalize to the same stored value and the same displayed format).

---

## A Constrained Generic Extension Method

```csharp
public static string Initials<T>(this T t) where T : Person
{
    var person = t as Person;
    ...
}
```

`Initials<T>()` is written as a generic method constrained to `Person` (`where T : Person`) rather than simply taking a `Person` parameter directly. Since every call site in this lesson already has a concrete `Person`-or-descendant reference, the practical behavior is identical to a plain `Person` parameter, the generic constraint doesn't unlock any additional capability here. Worth treating as a demonstration of the syntax (`<T> where T : Person` on an extension method) rather than a pattern to copy reflexively, a plain `this Person person` parameter would do the same job with less ceremony unless there's a concrete reason to need the actual runtime type `T`.
