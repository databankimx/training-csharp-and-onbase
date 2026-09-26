# Factory Pattern: 2 of 3 - Basic Factory

## What This Is

Ported from the loose `factory-pattern` repo's `02 - Basic Factory` example (Python). Python's `match`/`case` on `DataFormat` became a C# `switch` expression returning a `Func<Song, string>`; the `Enum` became a C# `enum`. Structurally a direct port - Interface/Creator/Products map cleanly onto the Python version's `#region` comments (which already named these same three roles).
