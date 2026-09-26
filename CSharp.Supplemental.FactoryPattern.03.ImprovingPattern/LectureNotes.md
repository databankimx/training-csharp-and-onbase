# Factory Pattern: 3 of 3 - Improving Pattern

## What This Is

Ported from the loose `factory-pattern` repo's `03 - Improving Pattern` example - except the Python source for this stage was byte-for-byte identical to `02 - Basic Factory`. `Notes.md`'s setup instructions include `py -m pip install PyYAML`, which is a strong signal the intended difference for this stage was adding YAML as a third format - the entire point of introducing the factory pattern in stage 2 - but that addition was never actually written before the repo was set aside.

This C# port completes that intended addition rather than porting the gap forward: `SerializeToYaml`, backed by the `YamlDotNet` NuGet package, plus the one enum value and one switch arm needed to wire it in. `Lesson.md` explains why that specific, minimal diff is the actual point of this stage.

## Package Version

`YamlDotNet` 16.2.1 - I don't have a way to confirm this is still the current version from here; worth checking for anything newer when this gets built.
