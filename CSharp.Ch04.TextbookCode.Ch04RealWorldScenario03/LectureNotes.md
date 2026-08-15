# Ch04 Textbook Code: StringBuilder Staircase (Real-World Scenario 3)

## What This Is

A small WinForms lab, no bugs, no crashes, just a single read-only multiline textbox that displays a "staircase" of the alphabet:

```
A
AB
ABC
ABCD
...
ABCDEFGHIJKLMNOPQRSTUVWXYZ
```

Built entirely with `StringBuilder`, run it and the output shows up immediately in the textbox, no interaction required, this one's genuinely safe to just launch and read.

---

## How the Staircase Actually Builds

```csharp
StringBuilder letters = new StringBuilder("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
StringBuilder line = new StringBuilder();
StringBuilder result = new StringBuilder();

for (int i = 0; i < 26; i++)
{
    line.Append(letters[i]);
    result.AppendLine(line.ToString());
}
```

`line` is never reset inside the loop, it just keeps growing, one letter longer every pass. Each time through, whatever `line` currently holds gets appended as a whole new line in `result`. That's the entire trick: `line` accumulates, `result` snapshots it at each stage. `letters[i]` is indexing directly into the `StringBuilder`, which supports the same `[]` indexer a `string` does, useful to know since it means you don't need to convert to a `char[]` first just to read one character at a time.
