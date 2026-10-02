# Supplemental: String Performance

## What This Is

Five string comparison and search methods, each demonstrated with and without an explicit `StringComparison` argument. The lesson isn't exotic -- it's the one rule that prevents a specific, common class of bugs: always pass a `StringComparison` when you mean to compare strings.

---

## The Rule

Every string method that compares text has an overload that accepts a `StringComparison` enum value. When you don't pass one, the method uses the default -- which is ordinal case-sensitive for most methods, but culture-sensitive for others, and the defaults differ between methods and across .NET versions.

Passing `StringComparison` explicitly:
- Makes the intent clear at the call site.
- Prevents behavior changes when code runs on a machine with a different locale.
- Avoids subtle bugs where the same string comparison returns different results on different machines or under different thread cultures.

---

## The Demos

### `string.Equals`

```csharp
string.Equals(url.Scheme, "HTTPS");                                     // false - "https" != "HTTPS"
string.Equals(url.Scheme, "HTTPS", StringComparison.OrdinalIgnoreCase); // true
```

`string.Equals` is the recommended way to test whether two strings are equal. Use it instead of `==` whenever culture or case might be relevant.

### `string.Compare`

```csharp
string.Compare("apple", "Apple");                                     // non-zero (case-sensitive)
string.Compare("apple", "Apple", StringComparison.OrdinalIgnoreCase); // 0 (case-ignored)
```

`string.Compare` and `CompareTo` are for **sorting**, not equality. They return a negative number, zero, or positive number indicating ordering. Don't test for a zero return value to determine equality -- use `string.Equals` instead. A zero from `Compare` means "same position in sort order" under the given `StringComparison`, which for case-insensitive comparisons means `"apple"` and `"APPLE"` are considered equal, even though `string.Equals` would return `false` without the case-ignore option.

### `string.IndexOf`

```csharp
"The Quick Brown Fox".IndexOf("quick");                                     // -1 (not found, case-sensitive)
"The Quick Brown Fox".IndexOf("quick", StringComparison.OrdinalIgnoreCase); // 4 (found)
```

Returns the zero-based index of the first occurrence, or -1 if not found. Without `OrdinalIgnoreCase`, `"quick"` doesn't match `"Quick"`.

### `string.StartsWith`

```csharp
url.Scheme.StartsWith("HTTP");                                     // false - "http" doesn't start with "HTTP"
url.Scheme.StartsWith("HTTP", StringComparison.OrdinalIgnoreCase); // true
```

### `string.EndsWith`

```csharp
"REPORT.PDF".EndsWith(".pdf");                                     // false
"REPORT.PDF".EndsWith(".pdf", StringComparison.OrdinalIgnoreCase); // true
```

---

## Which `StringComparison` to Use

| Scenario | Recommended |
|---|---|
| File names and paths | `OrdinalIgnoreCase` |
| URLs and URI components | `OrdinalIgnoreCase` |
| Internal identifiers, keys, codes | `Ordinal` or `OrdinalIgnoreCase` |
| User-visible text being sorted | `CurrentCulture` or `CurrentCultureIgnoreCase` |
| Storing and looking up in a dictionary | `Ordinal` (fastest, unambiguous) |

`Ordinal` comparisons compare byte values directly, with no culture rules applied. They're the fastest and most predictable. `CurrentCulture` applies locale-specific rules (Turkish locale's `İ`/`i` distinction being the classic example of where the difference matters). For anything that doesn't need to respect locale-specific collation, `Ordinal` or `OrdinalIgnoreCase` is the right default.

---

## Takeaways

- Always pass an explicit `StringComparison`. The defaults vary by method and .NET version.
- `string.Equals` for equality tests. `string.Compare`/`CompareTo` for sort ordering.
- Don't test `Compare == 0` to check equality -- use `Equals`.
- `OrdinalIgnoreCase` for file paths, URLs, identifiers. `CurrentCulture` only when locale-specific collation is genuinely needed.
