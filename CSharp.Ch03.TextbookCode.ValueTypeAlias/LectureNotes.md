# Ch03 Textbook Code: Value Type Alias

## What This Is

A small standalone lab, three ways of declaring an `int`-shaped value, and what each one does or doesn't give you by default.

```csharp
int myInt;                                   // declared, unassigned, unusable until set
int myNewInt = new int();                    // declared via "new", gets the default value (0)
System.Int32 myInt32 = new System.Int32();   // same thing, spelled out with the full .NET type name
```

`myInt` is commented out on purpose in the one line that would use it, uncomment `Console.WriteLine(myInt);` and the compiler stops you cold with "use of unassigned local variable." That's the entire lesson in one line: C# won't let you read a local variable that was only declared, never given a value, not even a default one. Locals don't get default values the way fields do.

`myNewInt` and `myInt32` are the exact same idea, `int` and `System.Int32` are literally the same type, `int` is just a keyword alias for it, calling `new int()` or `new System.Int32()` explicitly constructs the value rather than leaving it unassigned, which is why both print `0` without complaint.

No bugs to fix here, this one arrived clean. Only the usual project-structure and standards pass, plus stripping a disclaimer that was no longer accurate.
