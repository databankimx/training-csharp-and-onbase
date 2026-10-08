---
title: "CodeDOM - Generate, Compile, and Invoke"
chapter: 8
index: 1
launchMode: external
---

```csharp
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using Microsoft.CSharp;

internal static class Program
{
    private static void Main()
    {
        // Step 1: build the Calculator class as a CodeDOM object graph
        var unit = BuildCalculatorCompileUnit();

        using var provider = new CSharpCodeProvider();

        // Step 2: render as C# source text so we can see what we're about to compile
        using (var writer = new StringWriter())
        {
            provider.GenerateCodeFromCompileUnit(unit, writer, new CodeGeneratorOptions { BracingStyle = "C" });
            Console.WriteLine("Generated source code:");
            Console.WriteLine(writer.ToString());
        }

        // Step 3: compile into a real in-memory assembly
        var compilerParams = new CompilerParameters
        {
            GenerateInMemory    = true,
            GenerateExecutable  = false
        };
        compilerParams.ReferencedAssemblies.Add("System.dll");

        // CompileAssemblyFromDom does NOT throw on compilation failure.
        // Always check Errors.HasErrors before touching CompiledAssembly.
        var results = provider.CompileAssemblyFromDom(compilerParams, unit);

        if (results.Errors.HasErrors)
        {
            Console.WriteLine("Compilation failed:");
            foreach (CompilerError error in results.Errors)
                Console.WriteLine($"  {error}");
            return;
        }
        Console.WriteLine("Compilation succeeded.\n");

        // Step 4: use reflection to load the generated type and call its methods.
        // These are the exact same reflection APIs from the rest of the chapter --
        // the only difference is the assembly was compiled moments ago, not at solution build time.
        Assembly asm           = results.CompiledAssembly;
        Type     calcType      = asm.GetType("GeneratedCode.Calculator")
                                 ?? throw new InvalidOperationException("Calculator type not found!");
        object   calcInstance  = Activator.CreateInstance(calcType);

        var sum     = calcType.GetMethod("Add")         ?.Invoke(calcInstance, new object[] { 2, 3 });
        var doubled = calcType.GetMethod("AddThenDouble")?.Invoke(calcInstance, new object[] { 2, 3 });
        Console.WriteLine($"Calculator.Add(2, 3)          = {sum}");
        Console.WriteLine($"Calculator.AddThenDouble(2, 3) = {doubled}");
        Console.WriteLine("\nNeither method existed as compiled code until this program built and compiled it.");
    }

    private static CodeCompileUnit BuildCalculatorCompileUnit()
    {
        var unit = new CodeCompileUnit();
        var ns   = new CodeNamespace("GeneratedCode");
        ns.Imports.Add(new CodeNamespaceImport("System"));
        unit.Namespaces.Add(ns);

        var cls = new CodeTypeDeclaration("Calculator") { IsClass = true, TypeAttributes = System.Reflection.TypeAttributes.Public };
        ns.Types.Add(cls);

        // public int Add(int a, int b) { return a + b; }
        var add = new CodeMemberMethod { Name = "Add", Attributes = MemberAttributes.Public, ReturnType = new CodeTypeReference(typeof(int)) };
        add.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "a"));
        add.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "b"));
        add.Statements.Add(new CodeMethodReturnStatement(
            new CodeBinaryOperatorExpression(new CodeArgumentReferenceExpression("a"), CodeBinaryOperatorType.Add, new CodeArgumentReferenceExpression("b"))));
        cls.Members.Add(add);

        // public int AddThenDouble(int a, int b) { int sum = this.Add(a, b); return sum * 2; }
        var atd = new CodeMemberMethod { Name = "AddThenDouble", Attributes = MemberAttributes.Public, ReturnType = new CodeTypeReference(typeof(int)) };
        atd.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "a"));
        atd.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "b"));
        atd.Statements.Add(new CodeVariableDeclarationStatement(typeof(int), "sum",
            new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Add",
                new CodeArgumentReferenceExpression("a"), new CodeArgumentReferenceExpression("b"))));
        atd.Statements.Add(new CodeMethodReturnStatement(
            new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("sum"), CodeBinaryOperatorType.Multiply, new CodePrimitiveExpression(2))));
        cls.Members.Add(atd);

        return unit;
    }
}
```
