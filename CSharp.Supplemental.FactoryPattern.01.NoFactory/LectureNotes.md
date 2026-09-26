# Factory Pattern: 1 of 3 - No Factory

## What This Is

Ported from the loose `factory-pattern` repo's `01 - No Factory` example (Python). Straightforward line-by-line conversion - `json.dumps`/`ElementTree` became `System.Text.Json`/`System.Xml.Linq`, and the loose Python `Song` class (fields assigned directly in `__init__`) became a small immutable C# class with read-only properties instead. Nothing structural changed - this stage isn't supposed to use the factory pattern yet.
