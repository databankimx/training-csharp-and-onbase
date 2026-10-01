# Chapter 8 Supplemental 03: CodeDOM Compile and Run

## What This Is

The main lesson's CodeDOM example built a class as an object graph, rendered it as C# source text, and stopped there. This project takes the step that makes CodeDOM actually useful: it compiles the generated code into a real, loadable in-memory assembly, then uses reflection to instantiate the generated type and call its methods. Methods that didn't exist as compiled code until this program built and compiled them, at runtime, moments before calling them.

It also introduces two CodeDOM pieces the simpler main lesson example didn't need: `CodeParameterDeclarationExpression` (declaring a method parameter) and `CodeMethodInvokeExpression` (one generated method calling another).

---

## How to Write This Program

This project has a single lesson method: build the object graph, show the generated source, compile it, and call it. There's no useful checkpoint before the object graph is complete, so write the whole thing and run it once.

### Mini-Program 1: GenerateCompileAndRun()

Add `BuildCalculatorCompileUnit()` to `Program.cs` -- this builds the CodeDOM graph for a two-method `Calculator` class:

```csharp
private static CodeCompileUnit BuildCalculatorCompileUnit()
{
    var compileUnit = new CodeCompileUnit();

    var codeNamespace = new CodeNamespace("GeneratedCode");
    codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
    compileUnit.Namespaces.Add(codeNamespace);

    var classDeclaration = new CodeTypeDeclaration("Calculator")
    {
        IsClass = true,
        TypeAttributes = TypeAttributes.Public
    };
    codeNamespace.Types.Add(classDeclaration);

    // public int Add(int a, int b) { return a + b; }
    var addMethod = new CodeMemberMethod
    {
        Name = "Add",
        Attributes = MemberAttributes.Public,
        ReturnType = new CodeTypeReference(typeof(int))
    };
    addMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "a"));
    addMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "b"));
    addMethod.Statements.Add(new CodeMethodReturnStatement(
        new CodeBinaryOperatorExpression(
            new CodeArgumentReferenceExpression("a"),
            CodeBinaryOperatorType.Add,
            new CodeArgumentReferenceExpression("b"))));
    classDeclaration.Members.Add(addMethod);

    // public int AddThenDouble(int a, int b) { int sum = this.Add(a, b); return sum * 2; }
    var addThenDoubleMethod = new CodeMemberMethod
    {
        Name = "AddThenDouble",
        Attributes = MemberAttributes.Public,
        ReturnType = new CodeTypeReference(typeof(int))
    };
    addThenDoubleMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "a"));
    addThenDoubleMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "b"));

    // int sum = this.Add(a, b);
    var sumVariable = new CodeVariableDeclarationStatement(typeof(int), "sum",
        new CodeMethodInvokeExpression(
            new CodeThisReferenceExpression(), "Add",
            new CodeArgumentReferenceExpression("a"),
            new CodeArgumentReferenceExpression("b")));
    addThenDoubleMethod.Statements.Add(sumVariable);

    // return sum * 2;
    addThenDoubleMethod.Statements.Add(new CodeMethodReturnStatement(
        new CodeBinaryOperatorExpression(
            new CodeVariableReferenceExpression("sum"),
            CodeBinaryOperatorType.Multiply,
            new CodePrimitiveExpression(2))));
    classDeclaration.Members.Add(addThenDoubleMethod);

    return compileUnit;
}
```

Now add `GenerateCompileAndRun()` and call it from `Main()`:

```csharp
private static void GenerateCompileAndRun()
{
    var compileUnit = BuildCalculatorCompileUnit();

    using var provider = new CSharpCodeProvider();

    // Step 1: render as C# source text so we can see what we're about to compile.
    using (var writer = new StringWriter())
    {
        provider.GenerateCodeFromCompileUnit(compileUnit, writer,
            new CodeGeneratorOptions { BracingStyle = "C" });
        Console.WriteLine("Generated source code:");
        Console.WriteLine(writer.ToString());
    }
    GenericFunctions.Pause();

    // Step 2: compile the same object graph into a real, in-memory assembly.
    var compilerParameters = new CompilerParameters
    {
        GenerateInMemory = true,
        GenerateExecutable = false
    };
    compilerParameters.ReferencedAssemblies.Add("System.dll");

    CompilerResults results = provider.CompileAssemblyFromDom(compilerParameters, compileUnit);

    if (results.Errors.HasErrors)
    {
        Console.WriteLine("Compilation failed:");
        foreach (CompilerError error in results.Errors)
            Console.WriteLine($" - {error}");
        return;
    }
    Console.WriteLine("Compilation succeeded.");
    GenericFunctions.Pause();

    // Step 3: use reflection to instantiate the freshly-compiled type and call its methods.
    Assembly compiledAssembly = results.CompiledAssembly;
    Type calculatorType = compiledAssembly.GetType("GeneratedCode.Calculator");
    object calculatorInstance = Activator.CreateInstance(
        calculatorType ?? throw new DatabankException("Generated Calculator type not found!"));

    var addMethod = calculatorType.GetMethod("Add");
    var sum = addMethod?.Invoke(calculatorInstance, new object[] { 2, 3 });
    Console.WriteLine($"Calculator.Add(2, 3) = {sum}");

    var addThenDoubleMethod = calculatorType.GetMethod("AddThenDouble");
    var doubled = addThenDoubleMethod?.Invoke(calculatorInstance, new object[] { 2, 3 });
    Console.WriteLine($"Calculator.AddThenDouble(2, 3) = {doubled}");

    Console.WriteLine($"\nNeither of those methods existed as compiled code until this program built and compiled them, moments ago.");
    GenericFunctions.Pause();
}
```

Run it. Read the generated source in Step 1, confirm compilation succeeds in Step 2, and watch the calls work in Step 3.

**Step 1 -- the generated source.** Two new CodeDOM pieces beyond the main lesson:

`CodeParameterDeclarationExpression(typeof(int), "a")` declares a method parameter. Compare to the main lesson's `Greeter.Greet()`, which was a no-parameter method -- this is how you add them.

`CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Add", ...)` calls one generated method from another, entirely within the generated code. The object graph represents a method body that calls `this.Add(a, b)` and stores the result in a local variable, all expressed as nested `Code*` objects.

`CodeVariableDeclarationStatement` declares a local variable (`int sum = ...`). The initializer expression is the `Add` call above.

**Step 2 -- compilation.** `CompilerParameters` controls the compilation:
- `GenerateInMemory = true` -- the resulting assembly lives in memory, no `.dll` file written.
- `GenerateExecutable = false` -- we're building a class library, not an EXE.
- `ReferencedAssemblies` -- the generated code imports `System`, so `System.dll` must be referenced.

`CompileAssemblyFromDom` returns a `CompilerResults`. Always check `results.Errors.HasErrors` before proceeding -- the method does not throw on compilation failure, it just populates the error collection. Ignoring errors and then calling `results.CompiledAssembly` when compilation failed gives you a broken or null assembly, and the resulting runtime errors will be confusing.

**Step 3 -- reflection.** `compiledAssembly.GetType("GeneratedCode.Calculator")` finds the type by its fully qualified name. From there, `Activator.CreateInstance` and `GetMethod` / `Invoke` are exactly the same reflection techniques covered in the main lesson and `Supplemental.02`. The only difference is the assembly they're inspecting was just created at runtime, rather than being compiled as part of the solution.

---

## Worth Knowing: Where This Is Actually Used

This combination -- generate code, compile it, load and call it -- is how .NET's own infrastructure handles some genuinely demanding problems:

**Regular expressions.** `Regex.CompileToAssembly()` takes a set of patterns and produces an optimized assembly. The compiled regex avoids the interpretation overhead of the interpreted version at the cost of startup time and memory.

**Serializers and ORMs.** Many generate IL or C# at startup for each type they handle, so that subsequent serialization and deserialization is direct code rather than reflection. This is how `System.Text.Json` achieves its performance after the first use.

**Template engines and expression evaluators.** Anything that needs to run user-supplied code -- CSHTML templates, business rule engines, workflow systems -- may compile fragments to IL to avoid the overhead of running an interpreter.

The pattern is always the same: pay a one-time generation and compilation cost, then get direct-call performance for all subsequent uses. `Supplemental.04.ReflectionPerformance` quantifies exactly what that performance difference looks like.

---

## Takeaways

- `CompileAssemblyFromDom` does not throw on compilation failure -- always check `results.Errors.HasErrors`.
- `GenerateInMemory = true` keeps the assembly in memory; no file is written.
- Referenced assemblies must be explicitly listed in `CompilerParameters`.
- A type's fully qualified name (namespace + class name) is required for `GetType()` on a loaded assembly.
- Once loaded, a dynamically compiled assembly is used with exactly the same reflection APIs as any other.
- Code generation + compile + reflection is a real pattern in serializers, ORMs, regex engines, and template systems.
- For new work, use Roslyn or Source Generators. CodeDOM has no support for C# features added after roughly 2005.
