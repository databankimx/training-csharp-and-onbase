# Chapter 8 Supplemental 03: CodeDOM Compile and Run

## What This Is

The main lesson's CodeDOM example stopped at rendering generated code as text. This project goes the full distance: build a class as a CodeDOM object graph, **compile it into a real, loadable in-memory assembly**, then use reflection to instantiate the generated type and actually call its methods, methods that didn't exist as compiled code until this very program built and compiled them, moments earlier. This is the point where CodeDOM and reflection connect: CodeDOM builds code, reflection runs it.

---

## Two CodeDOM Pieces the Main Lesson Didn't Need

```csharp
addMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "a"));
addMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "b"));
```

The main lesson's `Greeter.Name` property needed no parameters at all. `CodeParameterDeclarationExpression` declares a method parameter, here, two `int` parameters for `Add(int a, int b)`.

```csharp
var sumVariable = new CodeVariableDeclarationStatement(typeof(int), "sum",
    new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Add",
        new CodeArgumentReferenceExpression("a"), new CodeArgumentReferenceExpression("b")));
```

`CodeMethodInvokeExpression` represents a method call, right there inside the generated code. `AddThenDouble()` calls the *other generated method*, `Add()`, entirely within the code being built, `this.Add(a, b)`, assigned to a local variable declared with `CodeVariableDeclarationStatement`. This is worth noticing: the generated `Calculator` class has two methods that call each other, exactly like ordinary hand-written code would, just built as data instead of typed as text.

---

## Actually Compiling It

```csharp
var compilerParameters = new CompilerParameters
{
    GenerateInMemory = true,
    GenerateExecutable = false
};
compilerParameters.ReferencedAssemblies.Add("System.dll");

CompilerResults results = provider.CompileAssemblyFromDom(compilerParameters, compileUnit);

if (results.Errors.HasErrors)
{
    foreach (CompilerError error in results.Errors) Console.WriteLine(error);
    return;
}
```

`CompileAssemblyFromDom()` takes the exact same `CodeCompileUnit` object graph used to generate the text preview, and actually compiles it, `GenerateInMemory = true` means the result lives entirely in memory, no `.dll` file written to disk. `CompilerResults.Errors` is worth checking before trusting the result, exactly like any other compile step, generated code can fail to compile just like hand-written code can, and handling that gracefully (rather than assuming success) is the responsible way to use this API.

---

## The Full Circle: Reflection Loads and Runs What CodeDOM Just Built

```csharp
Assembly compiledAssembly = results.CompiledAssembly;
Type calculatorType = compiledAssembly.GetType("GeneratedCode.Calculator");
object calculatorInstance = Activator.CreateInstance(calculatorType);

var addMethod = calculatorType.GetMethod("Add");
var sum = addMethod.Invoke(calculatorInstance, new object[] { 2, 3 });
Console.WriteLine($"Calculator.Add(2, 3) = {sum}");
```

`CompilerResults.CompiledAssembly` is a genuine `System.Reflection.Assembly`, the exact same type the main lesson's `Assembly.GetExecutingAssembly()` returned. Everything from here on is ordinary reflection, `GetType()` by name, `Activator.CreateInstance()`, `GetMethod()`, `Invoke()`, all techniques covered elsewhere in this chapter, just applied to a type that was source text a few lines of code ago. Watching `Calculator.Add(2, 3)` actually print `5` is worth pausing on: that computation didn't exist as runnable code when this program started, it was built, compiled, and executed, all in one run.

---

## Where This Actually Matters

Generating and compiling code at runtime is a real (if fairly specialized) technique: dynamic proxy generation, certain kinds of rules engines, and some code-generation tooling all lean on ideas like this, though most modern .NET code reaches for Roslyn's compiler APIs instead of the older CodeDOM for this specific purpose. Still, seeing the full loop, generate, compile, load, invoke, work end to end with tools that have shipped in .NET Framework since the beginning is worth the exercise, and it's the clearest possible illustration of what "code as data" actually means in practice.
