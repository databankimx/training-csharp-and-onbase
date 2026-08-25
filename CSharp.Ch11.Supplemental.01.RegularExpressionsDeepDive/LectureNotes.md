# Chapter 11 Supplemental 01: Regular Expressions Deep Dive

## What This Is

The main lesson used `Regex.IsMatch()` for a simple yes/no validity check. This Supplemental covers everything else regular expressions are actually good for: pulling specific pieces out of a match, finding every match in a string (not just testing the whole thing), search-and-replace using matched pieces, case-insensitive matching, and a genuinely common bug source, greedy vs. lazy quantifiers.

---

## Breaking Down a Real Pattern

```
^([A-Z][a-z]*[-' ]?)+$
```

Worth reading character by character at least once: `^`/`$` anchor the match to the *entire* string (without them, the pattern would happily match just a piece of a longer string containing something name-shaped anywhere inside it). `(...)＋` means the group inside repeats one or more times. Inside that group: `[A-Z]` one uppercase letter, `[a-z]*` zero or more lowercase letters after it, `[-' ]?` an *optional* hyphen, apostrophe, or space. Together, this matches "Mary", "Mary-Jane", "O'Brien", and "Van Der Berg", each capitalized word optionally followed by a hyphen/apostrophe/space before the next one starts.

---

## Extracting Pieces: Named Groups

```csharp
const string emailPattern = @"^(?<user>[^@\s]+)@(?<domain>[^@\s]+\.[^@\s]+)$";
Match match = Regex.Match(candidate, emailPattern);
string user = match.Groups["user"].Value;
```

`(?<name>...)` names a capture group, read back later via `match.Groups["name"]` instead of counting parentheses to figure out whether something is "group 1" or "group 2", far more readable and far less fragile once a pattern has more than one or two groups in it.

---

## `Regex.Matches()`: Every Match, Not Just the First

```csharp
MatchCollection matches = Regex.Matches(text, phonePattern);
foreach (Match match in matches) { ... }
```

`Regex.IsMatch()` answers yes/no. `Regex.Match()` finds the *first* match and stops. `Regex.Matches()` finds *every non-overlapping match* in the input and returns them all, the right tool when a string might contain more than one thing worth extracting (every phone number in a block of text, for instance).

---

## `Regex.Replace()`: Search-and-Replace Using What Was Matched

```csharp
string result = Regex.Replace(text, @"(\d{2})/(\d{2})/(\d{4})", "$3-$1-$2");
```

`$1`, `$2`, `$3` in the replacement string refer back to the numbered capture groups from the pattern, letting a replace operation *rearrange* matched pieces, not just delete or substitute them wholesale. Here, `MM/DD/YYYY` becomes `YYYY-MM-DD`, the three captured pieces (month, day, year) reordered directly in the output.

---

## `RegexOptions.IgnoreCase`

```csharp
Regex.IsMatch("Hello World", "hello", RegexOptions.IgnoreCase);   // true
```

Regex matching is case-sensitive by default, worth knowing since this is an easy thing to forget when validating something a user typed (names, search terms) where case genuinely shouldn't matter.

---

## Greedy vs. Lazy: A Real, Common Bug Source

```csharp
Regex.Match("<b>bold</b> and <i>italic</i>", "<.*>");    // matches "<b>bold</b> and <i>italic</i>"
Regex.Match("<b>bold</b> and <i>italic</i>", "<.*?>");   // matches only "<b>"
```

`.*` is **greedy** by default: it grabs as *much* text as possible while still letting the overall pattern succeed. Against `"<b>bold</b> and <i>italic</i>"`, that means matching from the very first `<` all the way to the very *last* `>`, swallowing everything in between, almost certainly not what was intended if the goal was "match one HTML tag." Adding `?` after a quantifier (`*?`, `+?`, `??`) makes it **lazy** instead: grab as *little* as possible, stopping at the first opportunity. This single character is the entire difference between a pattern that matches exactly what you meant and one that silently matches far more than you meant, worth testing directly against real sample input any time a pattern involves `.*` or `.+` and the input might contain more than one instance of whatever comes after it.

---

## Reusing a Compiled `Regex` for Performance

```csharp
var compiledRegex = new Regex(pattern, RegexOptions.Compiled);
// ... call compiledRegex.IsMatch(input) many times ...
```

Worth knowing precisely, not just vaguely: the static `Regex.IsMatch(string, string)` overload isn't naively re-parsing the pattern from scratch on every single call, .NET internally caches a small, limited number (15, by default) of recently-used patterns already. Building and reusing your own `Regex` instance still measures faster for repeated use, and `RegexOptions.Compiled` goes a step further, compiling the pattern to actual IL (via `Reflection.Emit`) rather than interpreting it. That compilation step has real up-front cost of its own, worth reaching for specifically when the *same* pattern will run a large number of times (validating every row of a large import, for instance), not for a pattern that only ever runs once or twice.
