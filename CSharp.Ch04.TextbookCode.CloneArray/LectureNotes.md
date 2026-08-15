# Ch04 Textbook Code: Clone Array

## What This Is

`CastingArrays`' twin: a WinForms lab, blank window, all the logic in `Form1_Load()`, no `try`/`catch`. Meant to be stepped through in the debugger, watching `array3` and `array4` in the Locals window as each line executes, not read from console output.

This mirrors the same logic already ported into `CloningArrays()` in `CSharp.Ch04.UsingTypes`, worth comparing side by side the same way `CastingArrays` is worth comparing against its console counterpart.

---

## Kept Exactly As Downloaded

```csharp
int[] array3 = (int[])array1.Clone();
array3[5] = 55;

dynamic array4 = array1.Clone();
array4[6] = 66;

array4[7] = "This won't work";
```

The last line is the intentional failure. `array4` is `dynamic`, so the compiler doesn't check the assignment at all, `array4[7] = "This won't work";` compiles cleanly. At runtime, though, `array4` still actually points at an `int[]` under the hood (`Clone()` on an `int[]` returns another `int[]`), and trying to put a `string` into an `int[]` element throws. `dynamic` didn't remove the type mismatch, it just moved the moment you find out about it from compile time to runtime.

No try/catch here on purpose, same reasoning as `CastingArrays`, running this without a debugger attached crashes the instant the form loads, and that's the intended way to experience it.
