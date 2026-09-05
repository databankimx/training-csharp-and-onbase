# Chapter 8 Supplemental 03: CodeDOM Compile and Run

## What This Is

The main lesson's CodeDOM example stopped at rendering generated code as text. Interesting, but not yet useful — you had an object graph and a string, and nothing that could actually *run*.

This project goes the full distance:

1. Build a class as a CodeDOM object graph
2. **Compile it into a real, loadable in-memory assembly**
3. Use reflection to instantiate the generated type and call its methods

Those methods did not exist as compiled code until this program built and compiled them, moments earlier. This is the point where the chapter's two halves connect: **CodeDOM builds code, reflection runs it.**

---

## Two CodeDOM Pieces the Main Lesson Didn't Need

The generated `Calculator` class is more capable than `Greeter` was, which requires two node types the simpler example never used.

### Declaring Parameters

```csharp
addMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "a"));
addMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "b"));
```

`Greeter.Name` was a property and `Greet()` took no arguments, so parameters never came up. `CodeParameterDeclarationExpression` declares one — here producing `Add(int a, int b)`.

Note the pairing with `CodeArgumentReferenceExpression("a")` in the method body. Declaring a parameter and *referring* to it are separate node types, and the link between them is the **string name**. Misspell it in the reference and you get a compile error from the generated code, not from your own.

### One Generated Method Calling Another

```csharp
var sumVariable = new CodeVariableDeclarationStatement(typeof(int), "sum",
	new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Add",
		new CodeArgumentReferenceExpression("a"), new CodeArgumentReferenceExpression("b")));
```

That single expression produces `int sum = this.Add(a, b);`, combining two new node types:

- **`CodeVariableDeclarationStatement`** — declares a local variable, optionally with an initializer
- **`CodeMethodInvokeExpression`** — represents a method call

This is worth pausing on. The generated `Calculator` has two methods, and one calls the other, exactly like ordinary hand-written code — just built as data instead of typed as text. `AddThenDouble()` reads:

```csharp
// public int AddThenDouble(int a, int b) { int sum = this.Add(a, b); return sum * 2; }
```

The `return sum * 2;` then uses `CodeVariableReferenceExpression("sum")` — note this is a *different* node type from `CodeArgumentReferenceExpression`. CodeDOM distinguishes referring to a local variable from referring to a parameter, even though C# syntax makes them look identical.

---

## Actually Compiling It

Step 1 renders the source text for display, using the same technique as the main lesson, purely so you can see what's about to be compiled. Step 2 is the new part:

```csharp
var compilerParameters = new CompilerParameters
{
	GenerateInMemory = true,
	GenerateExecutable = false
};
compilerParameters.ReferencedAssemblies.Add("System.dll");

CompilerResults results = provider.CompileAssemblyFromDom(compilerParameters, compileUnit);
```

**`CompileAssemblyFromDom()` takes the exact same `CodeCompileUnit`** that was just rendered to text. Not the generated string — the object graph itself. The text preview was purely for human consumption; the compiler works directly from the graph.

The parameters matter:

- **`GenerateInMemory = true`** — the resulting assembly lives entirely in memory. No `.dll` is written to disk. (When `false`, you get a temp file you're responsible for cleaning up.)
- **`GenerateExecutable = false`** — produce a library, not an EXE. An executable would need a `Main` method.
- **`ReferencedAssemblies.Add("System.dll")`** — the generated code's own reference list. This is a completely separate compilation from the one that built *this* program, so it needs its own references. Forget one and you get a compile error about an unknown type.

### Check the Errors

```csharp
if (results.Errors.HasErrors)
{
	Console.WriteLine("Compilation failed:");
	foreach (CompilerError error in results.Errors)
	{
		Console.WriteLine($" - {error}");
	}
	return;
}
```

This is not defensive padding. **Generated code can fail to compile exactly like hand-written code can** — and it's arguably more likely to, since no one typed it and no IDE checked it. A misspelled method name in a `CodeMethodInvokeExpression`, a missing assembly reference, a type mismatch between a declared parameter and its use: all of these surface here and nowhere earlier.

Note the distinction between `Errors.HasErrors` and the collection itself. `CompilerResults.Errors` holds **warnings as well as errors**; `HasErrors` specifically reports whether any entry is a genuine error. There's a parallel `HasWarnings`. Iterating the collection prints both.

Accessing `results.CompiledAssembly` after a failed compile throws — so the early `return` here is load-bearing.

---

## The Full Circle: Reflection Loads and Runs What CodeDOM Just Built

```csharp
Assembly compiledAssembly = results.CompiledAssembly;
Type calculatorType = compiledAssembly.GetType("GeneratedCode.Calculator");
object calculatorInstance = Activator.CreateInstance(
	calculatorType ?? throw new DatabankException("Generated Calculator type not found!"));

var addMethod = calculatorType.GetMethod("Add");
var sum = addMethod?.Invoke(calculatorInstance, new object[] { 2, 3 });
Console.WriteLine($"Calculator.Add(2, 3) = {sum}");
```

**`CompilerResults.CompiledAssembly` is a genuine `System.Reflection.Assembly`** — the very same type `Assembly.GetExecutingAssembly()` returned in the main lesson. Nothing about it is special or second-class.

Which means everything from here is ordinary reflection, using techniques already covered:

- **`GetType("GeneratedCode.Calculator")`** — note the fully qualified name including namespace, the same requirement as the main lesson's `Assembly.CreateInstance()`. And the same failure mode: it returns `null` on a miss, which is why the `?? throw` is there.
- **`Activator.CreateInstance(Type)`** — the `Type`-based form from `Supplemental.02`, and here it's genuinely necessary. There is no compile-time type name to write; `Calculator` did not exist when this file was compiled.
- **`GetMethod("Add")` / `Invoke(instance, args)`** — argument array matching the parameter list, exactly as `Supplemental.02` covered.

The result is worth pausing on:

```csharp
Console.WriteLine($"{Environment.NewLine}Neither of those methods existed as compiled code until this program built and compiled them, moments ago.");
```

`Calculator.Add(2, 3)` prints `5`, and `Calculator.AddThenDouble(2, 3)` prints `10`. That second one is the better demonstration — it proves the *inter-method call* worked, that `AddThenDouble` really did invoke `Add` inside the generated assembly.

### Why `object` for the Instance

`calculatorInstance` is typed `object`, and it has to be. There is no `Calculator` type available at compile time to cast it to — that's the entire premise. This is the practical consequence of runtime code generation: everything you do with the result goes through reflection, or through an interface both sides already know about.

That last point is the standard production pattern. Rather than reflecting over every call, you define an interface in a shared assembly, have the generated class implement it, then cast the created instance to that interface. You pay reflection's cost once at creation and get normal, fast, type-checked calls afterward.

### A Note on Assembly Lifetime

An assembly loaded this way **cannot be unloaded** in .NET Framework, short of tearing down an entire `AppDomain`. A long-running process that generates and compiles code repeatedly will leak assemblies until it runs out of memory. This is a real operational hazard in rules engines that recompile on every configuration change.

---

## Where This Actually Matters

Generating and compiling code at runtime is real, if specialized:

- **Dynamic proxy generation** — mocking frameworks, ORM lazy-loading proxies, AOP interceptors
- **Rules engines** — compiling user-authored business rules into fast, executable code rather than interpreting them repeatedly
- **Serializers** — some generate and compile per-type read/write methods on first use, then reuse them

Note the shape common to all three: **pay a large one-time cost to eliminate a repeated one.** That's the same tradeoff `Supplemental.04.ReflectionPerformance` explores with cached delegates, taken to its logical extreme. Compiling a method is far more expensive than reflecting over one, but the compiled result runs at full speed forever after.

> **Modern equivalent:** new code should use **Roslyn** (`Microsoft.CodeAnalysis.CSharp`) for this, not CodeDOM. On .NET Core and later, `CompileAssemblyFromDom()` throws `PlatformNotSupportedException` outright — the CodeDOM *generator* still works, but the *compiler* does not. Roslyn also supports `AssemblyLoadContext` for collectible assemblies, solving the unloading problem described above. For cases known at build time, a **Source Generator** avoids runtime compilation entirely.

Still, seeing the full loop — generate, compile, load, invoke — work end to end is worth the exercise. It's the clearest possible illustration of what "code as data" actually means.

---

## What to Take Away

**`CompileAssemblyFromDom()` compiles the object graph, not the rendered text.** The text preview is for humans; the graph is the real artifact.

**The generated code has its own reference list.** `CompilerParameters.ReferencedAssemblies` is separate from your project's references, because it's a separate compilation.

**Always check `results.Errors.HasErrors` before touching `CompiledAssembly`.** Generated code fails to compile more readily than hand-written code, since nothing checked it as it was built, and accessing the assembly after a failure throws.

**The compiled result is an ordinary `Assembly`.** Every reflection technique from the rest of the chapter applies to it unchanged.

**Cast to a shared interface when you can.** Reflecting over every call to generated code is slow; implementing a known interface lets you pay reflection's cost once at instantiation.

**This specific API is .NET Framework only.** The concepts carry forward to Roslyn; the `CompileAssemblyFromDom()` call does not.
