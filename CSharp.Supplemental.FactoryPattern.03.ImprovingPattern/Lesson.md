# Supplemental: Factory Pattern 03 -- Improving the Pattern

## What This Is

YAML support added to the factory from `02.BasicFactory`. The point is to read the diff, not to learn YAML.

What changes from the previous project: nothing structural. The factory from `02.BasicFactory` is extended with one new format. The value of this project is not in what it adds - it's in how little changes in `02.BasicFactory`'s code to accommodate it, compared to what `01.NoFactory` would have required.

---

## What Changed From 02

Three additions, nothing removed, nothing modified:

**1. A new enum value:**
```csharp
internal enum DataFormat { Undefined = 0, Json = 1, Xml = 2, Yaml = 3 }
```

**2. One new arm in the creator:**
```csharp
DataFormat.Yaml => SerializeToYaml,
```

**3. One new product method:**
```csharp
private static string SerializeToYaml(Song song)
{
    var serializer = new SerializerBuilder().Build();
    return serializer.Serialize(song);
}
```

---

## What Did Not Change

The `Serialize` interface method is byte-for-byte identical to `02.BasicFactory`. The JSON and XML products are untouched. A developer who only knows YAML wrote `SerializeToYaml`, added their enum value, and added their switch arm - without reading or risking the JSON or XML code.

Compare the full `Program.cs` files between `02` and `03` in a diff tool. The changes are exactly and only the three items above.

---

## The Pattern in Summary

The factory method pattern separates three concerns:

| Part | Responsibility | Changes when |
|---|---|---|
| **Interface** | What callers call | The public contract changes |
| **Creator** | Which product to use | A new product is added |
| **Product** | How one option works | That one option changes |

Adding a new format is an **extension** (new code) rather than a **modification** (changed existing code). This is what the Open/Closed Principle describes: open for extension, closed for modification.

---

## When to Use It

The factory method pattern is worth reaching for when:

- A method needs to return one of several implementations depending on a runtime condition.
- The set of implementations is expected to grow.
- Each implementation is non-trivial enough that grouping them all in one `switch` becomes unwieldy.

It's not worth the ceremony for two implementations that are unlikely to grow. `01.NoFactory` is genuinely fine for a two-format case that will never change. The pattern earns its place when the number of implementations grows and when the implementations are owned by different developers or teams.
