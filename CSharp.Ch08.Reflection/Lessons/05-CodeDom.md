---
title: "CodeDOM - Generating C# Source From an Object Graph"
chapter: 8
index: 5
dependencies: []
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
        // CodeDOM represents source code as an object graph, then renders it as source text.
        // One graph, multiple language providers -- swap CSharpCodeProvider for VBCodeProvider
        // and the same graph emits equivalent VB.NET. That substitutability was its purpose.
        // For new work use Roslyn or Source Generators; CodeDOM predates C# 3.0.

        var unit = new CodeCompileUnit();

        var ns = new CodeNamespace("GeneratedCode");
        ns.Imports.Add(new CodeNamespaceImport("System"));
        unit.Namespaces.Add(ns);

        var cls = new CodeTypeDeclaration("Greeter")
        {
            IsClass = true,
            TypeAttributes = TypeAttributes.Public
        };
        ns.Types.Add(cls);

        // private string _name;
        cls.Members.Add(new CodeMemberField(typeof(string), "_name")
            { Attributes = MemberAttributes.Private });

        // public string Name { get { return _name; } set { _name = value; } }
        var prop = new CodeMemberProperty
        {
            Name = "Name",
            Type = new CodeTypeReference(typeof(string)),
            Attributes = MemberAttributes.Public,
            HasGet = true,
            HasSet = true
        };
        prop.GetStatements.Add(new CodeMethodReturnStatement(
            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_name")));
        prop.SetStatements.Add(new CodeAssignStatement(
            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_name"),
            new CodePropertySetValueReferenceExpression()));
        cls.Members.Add(prop);

        // public string Greet() { return "Hello, " + _name; }
        var method = new CodeMemberMethod
        {
            Name = "Greet",
            Attributes = MemberAttributes.Public,
            ReturnType = new CodeTypeReference(typeof(string))
        };
        method.Statements.Add(new CodeMethodReturnStatement(
            new CodeBinaryOperatorExpression(
                new CodePrimitiveExpression("Hello, "),
                CodeBinaryOperatorType.Add,
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_name"))));
        cls.Members.Add(method);

        // Render the graph as C# source text
        using var provider = new CSharpCodeProvider();
        using var writer   = new StringWriter();
        provider.GenerateCodeFromCompileUnit(unit, writer, new CodeGeneratorOptions { BracingStyle = "C" });

        Console.WriteLine("Generated source:");
        Console.WriteLine(writer.ToString());
        Console.WriteLine("Notice how verbose the object graph is.");
        Console.WriteLine("'return _name;' required three nested objects: return statement,");
        Console.WriteLine("field reference, 'this' reference.");
        Console.WriteLine("Supplemental 03 compiles and runs this kind of output.");
    }
}
```
