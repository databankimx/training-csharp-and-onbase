# Ch03 Textbook Code: Using Value Types

## What This Is

A small standalone lab: declare one variable of each built-in value type, assign it, and print its value, its runtime type, and its size in bytes.

---

## The Bug That Was Here

```csharp
myByte = 254;
Console.WriteLine(sizeof(byte));   // correct, this one measures byte

myChar = 'r';
Console.WriteLine(sizeof(byte));   // still measuring byte, not char

myDecimal = 20987.89756M;
Console.WriteLine(sizeof(byte));   // still byte, not decimal

// ...and so on through float, long, and short
```

Once the pattern gets copy-pasted for `myByte`, `sizeof(byte)` never gets updated again for the rest of the file. Every block after that prints `1`, the size of a `byte`, regardless of what type it's supposedly measuring. `char` should print `2`, `decimal` should print `16`, `float` should print `4`, `long` should print `8`, `short` should print `2`. Only `bool` happens to land on the right answer by coincidence, since a `bool` and a `byte` are both 1 byte, so that one line looks correct while actually being wrong for the same reason as the others.

This is the textbook definition of a copy-paste bug: the first correct line becomes the template, and the one part that needed to change each time (the type name inside `sizeof()`) quietly stays frozen while everything else around it updates. Fixed by giving each block its own matching type inside `sizeof()`.

## Worth Noticing

`sizeof(char)` returning `2`, not `1`, catches people off guard the first time. C# `char` represents a UTF-16 code unit, not a single byte the way it might in C, so it's twice the size a lot of people assume going in.
