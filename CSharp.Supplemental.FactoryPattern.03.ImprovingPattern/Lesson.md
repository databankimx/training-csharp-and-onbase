# Factory Pattern: 3 of 3 - Improving Pattern

## The Payoff

`02.BasicFactory` set up the Interface/Creator/Product structure, but with only JSON and XML, it doesn't actually look any better than `01.NoFactory` - same two formats, same output either way. This lesson is where that structure earns its keep: **a third format, YAML, gets added.**

Compare `Program.cs` here to `02.BasicFactory\Program.cs` directly. The `Interface` method (`Serialize`) is byte-for-byte identical. Nothing about it changed to make room for YAML. The entire addition is:

```diff
  internal enum DataFormat
  {
      Undefined = 0,
      Json = 1,
      Xml = 2,
+     Yaml = 3
  }
```

```diff
  private static Func<Song, string> GetSerializer(DataFormat dataFormat) => dataFormat switch
  {
      DataFormat.Json => SerializeToJson,
      DataFormat.Xml => SerializeToXml,
+     DataFormat.Yaml => SerializeToYaml,
      _ => throw new ArgumentException($"Unknown data format: {dataFormat}")
  };
```

```diff
+ private static string SerializeToYaml(Song song)
+ {
+     var serializer = new SerializerBuilder().Build();
+     return serializer.Serialize(song);
+ }
```

One new enum value, one new switch arm, one new method. `SerializeToJson` and `SerializeToXml` weren't touched, and neither was anything that calls `Serialize` - as far as any caller is concerned, YAML just started working.

## The Comparison That Matters

Go back to `01.NoFactory` and imagine adding YAML there instead: a third `if` block, inserted into the same method that already handles JSON and XML, all three now sharing one growing method. That's the difference this pattern is actually for - not making two formats look nicer, but keeping a third (or fourth, or tenth) format from becoming everyone's problem at once.
