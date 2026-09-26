# String Comparisons - Performance Considerations

[From learn.microsoft.com](https://learn.microsoft.com/en-us/dotnet/standard/base-types/best-practices-strings)

When performing string comparisons in C#, there are a number of best practices worth knowing.

---

## Use `StringComparison` Options Where Possible

Most string comparison methods support a `StringComparison` parameter controlling how the comparison is performed.

> Side note: comparison methods are optimized over the use of operators, so `string.Equals` performs better than `==` when comparing strings.

## Methods Demonstrated

`Program.cs` runs a before/after comparison for each of these:

- `string.Equals`
- `string.Compare`
- `string.IndexOf`
- `string.StartsWith`
- `string.EndsWith`

Each demonstration runs the same operation twice - once with no explicit comparison rule, once with `StringComparison.OrdinalIgnoreCase` - so the difference in outcome is visible directly, not just asserted.

## Best-Practice Rules

- Use overloads that explicitly specify the string comparison rules for string operations - typically, an overload with a `StringComparison` parameter.
- Use `StringComparison.Ordinal` or `StringComparison.OrdinalIgnoreCase` as your safe default for culture-agnostic string matching, and for better performance.
- Use string operations based on `StringComparison.CurrentCulture` when displaying output to the user.
- Use the non-linguistic `StringComparison.Ordinal`/`OrdinalIgnoreCase` instead of `CultureInfo.InvariantCulture`-based operations when the comparison is linguistically irrelevant (symbolic data, for example).
- Use `String.ToUpperInvariant`, not `String.ToLowerInvariant`, when normalizing strings for comparison.
- Use an overload of `String.Equals` to test whether two strings are equal.
- Use `String.Compare`/`String.CompareTo` to sort strings, not to check for equality.
- Use culture-sensitive formatting to display non-string data (numbers, dates) in a UI; use invariant-culture formatting to persist non-string data in string form.

## Things to Avoid

- Don't use overloads that don't explicitly or implicitly specify comparison rules.
- Don't use `StringComparison.InvariantCulture`-based operations in most cases - one of the few exceptions is persisting linguistically meaningful but culturally agnostic data.
- Don't use `String.Compare`/`CompareTo` and test for a zero return value to determine whether two strings are equal - use `Equals` for that.
