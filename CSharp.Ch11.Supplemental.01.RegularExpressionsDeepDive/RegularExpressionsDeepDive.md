# Regular Expressions Deep Dive

## Introduction

The main lesson used regular expressions for a simple yes/no check. This lesson goes further: pulling specific pieces out of matched text, finding every match (not just the first), search-and-replace, and a genuinely common mistake worth knowing about before it bites you.

---

## Pulling Pieces Out With Named Groups

```csharp
Match match = Regex.Match(email, @"^(?<user>[^@\s]+)@(?<domain>[^@\s]+\.[^@\s]+)$");
string user = match.Groups["user"].Value;
```

`(?<name>...)` names part of your pattern so you can read it back later by that name, much clearer than trying to remember which numbered group is which.

---

## Finding Every Match, Not Just One

```csharp
MatchCollection matches = Regex.Matches(text, phonePattern);
foreach (Match match in matches) { ... }
```

If a string might contain more than one thing you're looking for, use `Regex.Matches()` (plural) instead of `Regex.Match()`, which stops after the first.

---

## Search-and-Replace Using What Was Matched

```csharp
string result = Regex.Replace("08/25/2026", @"(\d{2})/(\d{2})/(\d{4})", "$3-$1-$2");
// result: "2026-08-25"
```

`$1`, `$2`, `$3` refer back to the pieces you captured, letting you rearrange them in the output, not just delete or swap them.

---

## Case-Insensitive Matching

```csharp
Regex.IsMatch("Hello", "hello", RegexOptions.IgnoreCase);   // true
```

Regex matching cares about capitalization by default, easy to forget when it shouldn't matter.

---

## A Real Gotcha: Greedy vs. Lazy

```csharp
Regex.Match("<b>bold</b> and <i>italic</i>", "<.*>");    // matches WAY more than expected!
Regex.Match("<b>bold</b> and <i>italic</i>", "<.*?>");   // matches just "<b>"
```

`.*` grabs as much text as it possibly can. If your text has more than one of whatever you're looking for, this can match from the first one all the way through the last one, swallowing everything between. Adding a `?` (making it `.*?`) flips this to grab as little as possible instead. This single character difference is worth remembering any time your pattern uses `.*` or `.+`.

---

## Try It Yourself

Run `GreedyVsLazyQuantifiers()` and compare the two outputs directly, side by side, they run against the exact same input text with almost the exact same pattern, just one character different.
