# Chapter 5 Supplemental: Implementing Class Hierarchies

## What This Is

A whole lesson added beyond the textbook (see the `Supplemental` naming convention in the solution README) — a small, focused example of building a class hierarchy for something genuinely ordinary: an address book contact. `Person → Contact`, with `Address`, `BusinessAddress`, and `Telephone` composed in rather than inherited.

No bugs found in this one. It traced clean.

---

## Why This Exists

The main `CSharp.Ch05.ImplementingClassHierarchies` lesson covers the mechanics (`IComparable`, `IEquatable`, `ICloneable`, `IEnumerable`, `IDisposable`) using deliberately illustrative examples — cars, faculty, org charts. This one is the opposite kind of example on purpose: no interfaces, no generics gymnastics, just an ordinary, boring inheritance-plus-composition hierarchy that looks like something you'd actually write for a real feature.

Sometimes the most useful example is the unglamorous one. You will write a `Contact` class at some point. You will probably never write a `TreeEnumerator`.

---

## The Shape of It

```csharp
Person                  // FirstName, MiddleName, LastName, FullName()
  └── Contact           // + Email, HomePhone, WorkPhone, MobilePhone,
						//   HomeAddress, WorkAddress

Address                 // StreetAddress, City, State, ZipCode
  └── BusinessAddress   // + CompanyName

Telephone               // Number (self-validating)
```

Two inheritance chains, and the second one composes into the first rather than joining it.

---

## Inheritance vs. Composition, Side by Side

`Contact : Person` is **inheritance** — a `Contact` *is a* `Person`, with extra fields tacked on (phone numbers, addresses, email).

`Contact.HomeAddress` (type `Address`) and `Contact.WorkAddress` (type `BusinessAddress`) are **composition** — a `Contact` *has an* `Address`, not *is an* `Address`. `BusinessAddress : Address` shows the same inheritance relationship one level down: a business address is a regular address plus a company name.

Worth noticing which relationship got used where. `Contact` inheriting from `Person` makes sense because everything `Person` has (first/middle/last name, `FullName()`) is genuinely still true of a `Contact`. `Address` isn't a field *of* `Person`; it composes into `Contact` instead, because an address isn't a kind of person — it's something a contact *has*.

Getting this distinction backwards (inheriting where you should compose, or vice versa) is one of the most common early object-oriented design mistakes, and this hierarchy is small enough to see the correct call made twice, clearly, in one file.

The standard test is the sentence itself. Say it out loud:

- "A contact **is a** person." — true, so inherit.
- "A contact **is an** address." — obviously false, so compose.
- "A business address **is an** address." — true, so inherit.

When the sentence sounds wrong, the inheritance is wrong. The industry shorthand for this is "prefer composition over inheritance," and the reason isn't that inheritance is bad — it's that inheritance is a permanent, single-slot commitment (you get exactly one base class, forever), while composition can be changed, swapped, or added to at any time without restructuring the type.

---

## Optional Parameters on `FullName()`

```csharp
someone.FullName()                        // Jordan Rivera
someone.FullName(reverse: true)           // Rivera, Jordan
someone.FullName(includeMiddle: true)     // Jordan A Rivera
```

One method, three outputs, driven by optional parameters with sensible defaults. Note the calls use **named arguments** (`reverse: true`) rather than positional ones. That's not decoration — with multiple `bool` parameters in a signature, a bare `FullName(true)` at the call site tells the reader nothing about which flag is being set. Named arguments make the call self-documenting, and they're required in practice once you want to set the second optional parameter but not the first.

A caution worth carrying forward: optional parameter defaults are baked into the *calling* assembly at compile time, the same way `const` values are. Changing a default in a shared library doesn't take effect for consumers until they're rebuilt. For anything crossing an assembly boundary, overloads are safer than optional parameters.

---

## A Self-Validating Property

```csharp
public string Number
{
	get => FormatPhoneNumber(number);
	set => number = SetPhoneNumber(value);
}
```

`Telephone.Number` never stores an invalid value in the first place. `SetPhoneNumber()` strips non-digit formatting characters and throws `InvalidDataException` if what's left isn't exactly 10 digits, *before* the backing field is ever touched.

The getter formats on the way out (`(214) 718-8383`), so callers always get a consistently formatted string regardless of how it was entered. `"2145550234"`, `"(214) 555-0234"`, and `"214-555-0234"` all normalize to the same stored value and the same displayed format.

This is the entire argument for properties over public fields, in one small class. A public `string Number;` field can hold `"banana"`. A property can't, because there's a method body standing between the caller and the storage. The related principle: **store canonical, format on display.** The backing field holds ten bare digits — the one representation that's unambiguous and easy to compare — and formatting is a presentation concern applied at the boundary.

Note also that validation failure throws rather than silently correcting or storing a flag. A `Telephone` object that exists is always a valid `Telephone`, so no code downstream ever has to ask whether it's usable. That property — "if it constructed, it's valid" — removes an enormous amount of defensive checking everywhere else.

---

## A Constrained Generic Extension Method

```csharp
public static string Initials<T>(this T t) where T : Person
{
	var person = t as Person;
	...
}
```

`Initials<T>()` is written as a generic method constrained to `Person` (`where T : Person`) rather than simply taking a `Person` parameter directly. Since every call site in this lesson already has a concrete `Person`-or-descendant reference, the practical behavior is identical to a plain `Person` parameter — the generic constraint doesn't unlock any additional capability here.

Worth treating as a demonstration of the syntax (`<T> where T : Person` on an extension method) rather than a pattern to copy reflexively. A plain `this Person person` parameter would do the same job with less ceremony unless there's a concrete reason to need the actual runtime type `T` — typically returning `T` so the caller keeps the derived type instead of getting a `Person` back, which is the case that genuinely justifies the constraint.

The extension method mechanism itself is worth understanding, though: `this T t` in a `static` method inside a `static` class makes the method appear as if it were declared on `Person`, so `someone.Initials()` compiles even though `Person` has no such member. It's the pattern behind all of LINQ. Useful for adding behavior to types you don't own; less appropriate for types you do own, where a real method is clearer.

---

## Takeaways

- "Is a" means inherit. "Has a" means compose. Say the sentence out loud before deciding.
- You get one base class forever, but unlimited composed members. Prefer composition when it's a close call.
- Properties earn their keep by validating and normalizing — store canonical, format on display.
- Throw on invalid input at the boundary so nothing downstream has to re-check.
- Named arguments make optional-parameter call sites readable.
- Generic constraints on extension methods are real, but don't reach for them without a reason.
