# CodeDOM: Compile and Run

## Introduction

The main lesson showed the CodeDOM building a class as an object graph and rendering it as text. This lesson takes the next step, the one that actually makes it useful: compiling that generated code into a real assembly, then using reflection to run it. Generate, compile, load, invoke, all in one program.

---

## Declaring Method Parameters

```csharp
var addMethod = new CodeMemberMethod { Name = "Add", ReturnType = new CodeTypeReference(typeof(int)) };
addMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "a"));
addMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "b"));
```

`CodeParameterDeclarationExpression` adds a parameter to a generated method, here building `int Add(int a, int b)`.

---

## One Generated Method Calling Another

```csharp
var sumVariable = new CodeVariableDeclarationStatement(typeof(int), "sum",
    new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Add",
        new CodeArgumentReferenceExpression("a"), new CodeArgumentReferenceExpression("b")));
```

This builds the equivalent of `int sum = this.Add(a, b);`, a local variable declaration whose value comes from calling another method. `CodeMethodInvokeExpression` represents that method call; `CodeVariableDeclarationStatement` declares the local variable holding its result. The generated `Calculator` class ends up with two methods, `Add()` and `AddThenDouble()`, and `AddThenDouble()` genuinely calls `Add()` internally, just like ordinary hand-written code would.

---

## Compiling the Generated Code

```csharp
var compilerParameters = new CompilerParameters { GenerateInMemory = true, GenerateExecutable = false };
compilerParameters.ReferencedAssemblies.Add("System.dll");

CompilerResults results = provider.CompileAssemblyFromDom(compilerParameters, compileUnit);

if (results.Errors.HasErrors)
{
    foreach (CompilerError error in results.Errors) Console.WriteLine(error);
    return;
}
```

`CompileAssemblyFromDom()` takes the same object graph used to preview the source text and actually compiles it into a working assembly, held entirely in memory (`GenerateInMemory = true`). Always check `Errors.HasErrors` before trusting the result, generated code can fail to compile too.

---

## Running What Was Just Compiled

```csharp
Assembly compiledAssembly = results.CompiledAssembly;
Type calculatorType = compiledAssembly.GetType("GeneratedCode.Calculator");
object calculatorInstance = Activator.CreateInstance(calculatorType);

var addMethod = calculatorType.GetMethod("Add");
var sum = addMethod.Invoke(calculatorInstance, new object[] { 2, 3 });
Console.WriteLine($"Calculator.Add(2, 3) = {sum}");
```

`CompilerResults.CompiledAssembly` is an ordinary `Assembly`, exactly the kind of object the rest of this chapter worked with. From here it's the same reflection toolkit covered everywhere else: look up the type by name, create an instance, find the method, invoke it. The result really does print `5`, computed by code that was only source text a moment before this program ran.

---

## Try It Yourself

Add a third generated method, `Subtract(int a, int b)`, following the same pattern as `Add()`. Compile and run the program again, no changes needed anywhere else, reflection will find and call whatever methods actually ended up on the generated type.
