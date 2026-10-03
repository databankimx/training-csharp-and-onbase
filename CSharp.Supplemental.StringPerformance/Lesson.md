# Supplemental: String Performance

## What This Is

Two string topics that share a project because both come down to the same underlying question - does this code do what you think it does with strings?

The first half is about **comparison and search**: every string method that compares text has an overload accepting a `StringComparison` argument, and not passing one is how you get code that works on your machine, breaks on a Turkish locale, and takes three days to diagnose.

The second half is about **construction and modification**: strings in .NET are immutable. Every `+=` in a loop allocates a brand-new string. This is fine for a handful of concatenations, and genuinely painful at scale. `StringBuilder` exists to fix that.

---

## Part 1: StringComparison

Every string method that compares text has an overload that accepts a `StringComparison` enum value. When you don't pass one, the method uses the default - which is ordinal case-sensitive for most methods, but culture-sensitive for others, and the defaults differ between methods and across .NET versions.

Passing `StringComparison` explicitly:
- Makes the intent clear at the call site.
- Prevents behavior changes when code runs on a machine with a different locale.
- Avoids subtle bugs where the same string comparison returns different results on different machines or under different thread cultures.

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

`string.Compare` and `CompareTo` are for **sorting**, not equality. They return a negative number, zero, or positive number indicating ordering. Don't test for a zero return value to determine equality - use `string.Equals` instead. A zero from `Compare` means "same position in sort order" under the given `StringComparison`, which for case-insensitive comparisons means `"apple"` and `"APPLE"` are considered equal - even though `string.Equals` without the case-ignore option would return `false`.

### `string.IndexOf`

```csharp
"The Quick Brown Fox".IndexOf("quick");                                     // -1 (not found, case-sensitive)
"The Quick Brown Fox".IndexOf("quick", StringComparison.OrdinalIgnoreCase); // 4 (found)
```

Returns the zero-based index of the first occurrence, or -1 if not found.

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

### Which `StringComparison` to Use

| Scenario | Recommended |
|---|---|
| File names and paths | `OrdinalIgnoreCase` |
| URLs and URI components | `OrdinalIgnoreCase` |
| Internal identifiers, keys, codes | `Ordinal` or `OrdinalIgnoreCase` |
| User-visible text being sorted | `CurrentCulture` or `CurrentCultureIgnoreCase` |
| Storing and looking up in a dictionary | `Ordinal` (fastest, unambiguous) |

`Ordinal` comparisons compare byte values directly, with no culture rules applied. They're the fastest and most predictable. `CurrentCulture` applies locale-specific rules (the Turkish locale's distinct `I`/`i` distinction being the canonical example of where the difference matters). For anything that doesn't need to respect locale-specific collation, `Ordinal` or `OrdinalIgnoreCase` is the right default.

---

## Part 2: StringBuilder

### Why String Concatenation in a Loop Is Expensive

Strings in .NET are **immutable**. Once created, the content of a `string` object never changes. When you write:

```csharp
string result = "";
result += "Hello";
result += ", ";
result += "world";
```

...three new `string` objects are allocated. The original `""` is discarded; `"Hello"` is discarded; and the final `"Hello, world"` is what survives. In a short chain like this, the allocations are trivial.

Inside a loop the cost compounds:

```csharp
string log = "";
for (int i = 0; i < 10_000; i++)
    log += $"Line {i}\n";
```

This allocates 10,000 strings, each one character longer than the last. The total bytes allocated across all those discarded strings grows quadratically - the last string alone is ~100,000 characters, but the sum of all the discarded intermediate strings is roughly the square of that. At 100,000 iterations this is measurably slow. At 1,000,000 it is genuinely painful.

### The Fix: StringBuilder

`StringBuilder` maintains an internal buffer that it grows as needed. Appending to it doesn't allocate a new string - it writes into the existing buffer, resizing only when the buffer is exhausted:

```csharp
var sb = new StringBuilder();
for (int i = 0; i < 10_000; i++)
    sb.Append($"Line {i}\n");
string log = sb.ToString(); // one allocation, at the end
```

`ToString()` is called once at the end to produce the final immutable string. The loop itself allocates almost nothing.

### The Demo

```csharp
const int iterations = 100_000;
var sw = Stopwatch.StartNew();

string concatenated = "";
for (int i = 0; i < iterations; i++)
    concatenated += "x";

sw.Stop();
Console.WriteLine($"String concatenation ({iterations:N0} iterations): {sw.ElapsedMilliseconds} ms");
Console.WriteLine($"Result length: {concatenated.Length}");

sw.Restart();

var sb = new StringBuilder(capacity: iterations); // pre-size avoids internal resizes entirely
for (int i = 0; i < iterations; i++)
    sb.Append('x'); // char overload - no string allocation per call

string built = sb.ToString();
sw.Stop();
Console.WriteLine($"\nStringBuilder ({iterations:N0} appends): {sw.ElapsedMilliseconds} ms");
Console.WriteLine($"Result length: {built.Length}");
```

Run it. The `StringBuilder` version will be dramatically faster - typically 100-1000x at 100,000 iterations. The gap widens as iterations increase because string concatenation's cost is quadratic while `StringBuilder`'s cost is linear.

Two details worth noting in the `StringBuilder` version:

`new StringBuilder(capacity: iterations)` pre-allocates a buffer large enough to hold the final result. Without an initial capacity, `StringBuilder` starts with a default buffer (16 characters) and doubles it each time it fills - which works, but allocates several intermediate buffers along the way. Pre-sizing eliminates those internal reallocations entirely.

`sb.Append('x')` takes a `char` directly, not a `string`. The `string` overload would be `sb.Append("x")` - fine, but the `char` overload avoids even the tiny allocation of a one-character string literal on each iteration.

### When Not to Use StringBuilder

`StringBuilder` is the right tool when you're building a string incrementally across multiple operations, especially in a loop. For simple, fixed concatenations, it's unnecessary ceremony:

```csharp
// Fine as-is - three known pieces, no loop
string greeting = "Hello, " + firstName + "!";

// Also fine - the compiler optimizes known-at-compile-time concatenations
string path = folder + "\\" + fileName + extension;

// StringBuilder earns its place
var sb = new StringBuilder();
foreach (var item in items)
    sb.AppendLine(item.ToString());
string report = sb.ToString();
```

The rule of thumb: if the number of concatenations is bounded and small (say, under ten), plain `+` is readable and fast enough. If the concatenation is inside a loop, or if you're building something whose size depends on runtime data, reach for `StringBuilder`.

`string.Join` is worth knowing as a shortcut when the separator is constant:

```csharp
// Instead of looping with StringBuilder for this specific pattern:
string csv = string.Join(",", items);
```

`string.Join` is implemented with `StringBuilder` internally. Use it when it fits; use `StringBuilder` directly for anything more complex.

---

## Summary: Part 1 and Part 2

| Topic | Method | Pass `StringComparison`? |
|---|---|---|
| Equality | `string.Equals` | Always |
| Sort ordering | `string.Compare` / `CompareTo` | Always |
| Substring search | `string.IndexOf` | Always |
| Prefix check | `string.StartsWith` | Always |
| Suffix check | `string.EndsWith` | Always |

| Scenario | Use |
|---|---|
| 1-10 fixed concatenations | `+` or string interpolation |
| Building in a loop | `StringBuilder` |
| Joining a collection with a separator | `string.Join` |
| Known final size | `new StringBuilder(capacity)` |

---

## Takeaways

- Always pass an explicit `StringComparison`. The defaults vary by method and .NET version.
- `string.Equals` for equality tests. `string.Compare`/`CompareTo` for sort ordering.
- Don't test `Compare == 0` to check equality - use `Equals`.
- `OrdinalIgnoreCase` for file paths, URLs, identifiers. `CurrentCulture` only when locale-specific collation is genuinely needed.
- Strings are immutable. Every `+=` allocates a new string.
- String concatenation in a loop is O(n²) in total allocations. `StringBuilder` is O(n).
- Pre-size `StringBuilder` with the expected capacity when known - eliminates internal reallocations.
- `string.Join` is `StringBuilder` with a separator, built in.
- Use `+` freely for small, bounded concatenations. Reserve `StringBuilder` for loops and dynamic construction.
