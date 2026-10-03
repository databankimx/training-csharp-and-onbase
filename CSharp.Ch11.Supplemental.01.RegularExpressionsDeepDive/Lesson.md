# Chapter 11 Supplemental 01: Regular Expressions Deep Dive

## What This Is

The main lesson used `Regex.IsMatch()` for a yes/no validity check - the simplest slice of what regular expressions can do. This project covers the rest: extracting pieces of a match with groups, finding every match in a string, search-and-replace using the matched pieces, case-insensitive matching, the greedy-vs-lazy quantifier distinction, and reusing a compiled `Regex` instance for performance.

What's being extended here is the regex API surface. The main lesson's `IsMatch()` answers "does this string conform to this pattern?" The tools here answer "what parts of this string match, where are all the matches, and how do I transform them?" The performance section also makes the compiled-vs-static tradeoff concrete - the right choice depends on how often the same pattern runs.

---

## How to Write This Program

Add a shared timeout constant:

```csharp
private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(5);
```

### Mini-Program 1: Breaking Down the Name Pattern

Clear `Main()` and write:

```csharp
const string namePattern = @"^([A-Z][a-z]*[-' ]?)+$";

Console.WriteLine($"Pattern: {namePattern}");
Console.WriteLine("Piece by piece:");
Console.WriteLine("  ^           anchor: must match from the very beginning");
Console.WriteLine("  (...)+      the group repeats one or more times");
Console.WriteLine("  [A-Z]       exactly one uppercase letter");
Console.WriteLine("  [a-z]*      zero or more lowercase letters");
Console.WriteLine("  [-' ]?      optional hyphen, apostrophe, or space");
Console.WriteLine("  $           anchor: must match all the way to the end");
Console.WriteLine();

string[] candidates = ["Mary", "Mary-Jane", "O'Brien", "Van Der Berg", "mary", "Mary123", ""];
foreach (string candidate in candidates)
    Console.WriteLine($" - \"{candidate}\" -> {Regex.IsMatch(candidate, namePattern, RegexOptions.Compiled, RegexTimeout)}");

GenericFunctions.Pause();
```

Run it. Read through which candidates match and which don't, and trace why each result makes sense against the piece-by-piece breakdown above.

`^` and `$` are anchors - without them, the pattern would match any substring within the input, and `"mary123"` would match because `"mary"` appears in it. With them, the entire string from start to finish must conform.

### Mini-Program 2: Extracting Data With Named Groups

Clear `Main()` and write:

```csharp
// (?<name>...) defines a named capture group
const string emailPattern = @"^(?<user>[^@\s]+)@(?<domain>[^@\s]+\.[^@\s]+)$";

string[] candidates = ["jane.doe@example.com", "not-an-email"];

foreach (string candidate in candidates)
{
    Match match = Regex.Match(candidate, emailPattern, RegexOptions.Compiled, RegexTimeout);

    if (match.Success)
        Console.WriteLine($" - \"{candidate}\" -> user: \"{match.Groups["user"].Value}\", " +
                          $"domain: \"{match.Groups["domain"].Value}\"");
    else
        Console.WriteLine($" - \"{candidate}\" -> no match");
}

GenericFunctions.Pause();
```

Run it. `match.Groups["user"]` and `match.Groups["domain"]` give you the pieces of the match, named.

Named groups (`(?<user>...)`) are far more readable than numbered ones (`$1`, `$2`) in complex patterns. The alternative would be counting parentheses to figure out which group is "group 2" - an activity that reliably produces wrong answers when anyone modifies the pattern later.

### Mini-Program 3: Finding All Matches

Clear `Main()` and write:

```csharp
const string phonePattern = @"\d{3}-\d{3}-\d{4}";
const string text = "Call the office at 555-123-4567, or reach Jane directly at 555-987-6543.";

MatchCollection matches = Regex.Matches(text, phonePattern, RegexOptions.Compiled, RegexTimeout);

Console.WriteLine($"Found {matches.Count} phone number(s):");
foreach (Match match in matches)
    Console.WriteLine($" - {match.Value} (at position {match.Index})");

GenericFunctions.Pause();
```

Run it. `Regex.Matches()` finds every non-overlapping match in a string and returns a `MatchCollection`. `Regex.Match()` (singular) finds only the first. `Regex.IsMatch()` just tells you whether at least one exists.

`match.Index` is the zero-based character position in the original string where the match starts - useful when you need to replace or annotate matches in context.

### Mini-Program 4: Search and Replace

Clear `Main()` and write:

```csharp
// Three capture groups: month, day, year (MM/DD/YYYY)
const string datePattern = @"(\d{2})/(\d{2})/(\d{4})";
const string text = "The invoice is dated 08/25/2026.";

// $1/$2/$3 refer back to the captured groups, reordered into ISO 8601 (YYYY-MM-DD)
string result = Regex.Replace(text, datePattern, "$3-$1-$2", RegexOptions.Compiled, RegexTimeout);

Console.WriteLine($"Original: {text}");
Console.WriteLine($"Replaced: {result}");

GenericFunctions.Pause();
```

Run it. The date is reformatted from `08/25/2026` to `2026-08-25` in one call, without parsing or string manipulation.

`$1`, `$2`, `$3` in the replacement string refer back to the captured groups in the matched text. Named groups can also be referenced as `${name}` in the replacement.

### Mini-Program 5: RegexOptions

Clear `Main()` and write:

```csharp
const string pattern = "hello";
const string input = "Hello World";

Console.WriteLine($"Case-sensitive (default):    {Regex.IsMatch(input, pattern, RegexOptions.Compiled, RegexTimeout)}");
Console.WriteLine($"RegexOptions.IgnoreCase:     {Regex.IsMatch(input, pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase, RegexTimeout)}");

GenericFunctions.Pause();
```

Run it. `RegexOptions` is a flags enum - combine options with `|`. The useful ones beyond `IgnoreCase`:

- `Multiline` - `^` and `$` match the start and end of each line, not just the whole string.
- `Singleline` - `.` matches newline characters (by default it doesn't).
- `IgnorePatternWhitespace` - whitespace in the pattern is ignored, letting you format a complex pattern across multiple lines with comments.

### Mini-Program 6: Greedy vs Lazy Quantifiers

Clear `Main()` and write:

```csharp
const string html = "<b>bold</b> and <i>italic</i>";

// ".*" is GREEDY: grabs as MUCH as possible while still letting the pattern succeed.
// Here it matches from the first "<" all the way to the LAST ">".
Match greedyMatch = Regex.Match(html, "<.*>", RegexOptions.Compiled, RegexTimeout);
Console.WriteLine($"Greedy \"<.*>\" matched:  \"{greedyMatch.Value}\"");

// ".*?" is LAZY: grabs as LITTLE as possible, stopping at the first ">" it can.
Match lazyMatch = Regex.Match(html, "<.*?>", RegexOptions.Compiled, RegexTimeout);
Console.WriteLine($"Lazy \"<.*?>\" matched:   \"{lazyMatch.Value}\"");

Console.WriteLine("\nSame input, same base pattern, wildly different results.");
Console.WriteLine("\".*\" defaults to greedy. Add \"?\" after a quantifier to make it lazy: *?, +?, ??");

GenericFunctions.Pause();
```

Run it. Greedy `<.*>` matches from `<b>` to the final `</i>`, swallowing everything in between. Lazy `<.*?>` stops at the first `>` it can and returns just `<b>`.

This is one of the most common sources of "my pattern matched way more than I expected" bugs. The fix is almost always adding `?` after the quantifier.

### Mini-Program 7: Compiled Regex for Performance

Clear `Main()` and write:

```csharp
const string pattern = @"^\d{3}-\d{3}-\d{4}$";
const string input = "555-123-4567";
const int iterations = 200_000;

// Static method: internally caches a limited number of recently-used patterns,
// but still does more work per call than a reused instance.
var sw = Stopwatch.StartNew();
for (int i = 0; i < iterations; i++)
    Regex.IsMatch(input, pattern, RegexOptions.Compiled, RegexTimeout);
sw.Stop();
Console.WriteLine($"Static Regex.IsMatch(), {iterations:N0} calls: {sw.ElapsedMilliseconds} ms");

// Single instance, built once, called many times.
var compiledRegex = new Regex(pattern, RegexOptions.Compiled, RegexTimeout);
sw.Restart();
for (int i = 0; i < iterations; i++)
    compiledRegex.IsMatch(input);
sw.Stop();
Console.WriteLine($"Reused compiled instance, {iterations:N0} calls:  {sw.ElapsedMilliseconds} ms");

GenericFunctions.Pause();
```

Run it. The reused compiled instance is faster, often substantially.

Two distinct optimizations are at play. First, creating the `Regex` object once avoids parsing the pattern on each call. Second, `RegexOptions.Compiled` uses `Reflection.Emit` to compile the pattern to native IL rather than interpreting it - that compilation has a real up-front cost, so it's only worth it for a pattern that will run many times. For a pattern used once or twice, `RegexOptions.None` is actually faster overall.

---

## Try It Yourself

Run `GreedyVsLazyQuantifiers()` and compare the two outputs directly - same input text, almost the same pattern, just one `?` different. That single character is the entire difference between a pattern that grabs one HTML tag and one that grabs everything from the first tag to the last.

---

## Summary: Three Methods, One Use Case Each

| Method | Returns | Use when |
|---|---|---|
| `Regex.IsMatch()` | `bool` | You only need yes/no |
| `Regex.Match()` | `Match` | You need the first (or only) match and its groups |
| `Regex.Matches()` | `MatchCollection` | There may be multiple matches |
| `Regex.Replace()` | `string` | You need to transform matched text |

---

## Takeaways

- `^` and `$` anchor a pattern to the start and end of the string. Without them, partial substring matches succeed.
- Named groups (`(?<name>...)`) are far more maintainable than numbered groups for anything complex.
- `Regex.Matches()` finds all occurrences; `Regex.Match()` finds the first; `Regex.IsMatch()` answers yes/no.
- `$1`, `$2`, `${name}` in a replacement string reference captured groups from the match.
- `.*` is greedy (grabs as much as possible). `.*?` is lazy (grabs as little as possible). Add `?` to any quantifier to make it lazy.
- A reused `Regex` instance is faster than repeated static calls. `RegexOptions.Compiled` compiles to IL - worth it for high-frequency patterns, not for one-shot use.
- Always pass a timeout. Certain patterns on certain inputs cause catastrophic backtracking.
