#region Copyright
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
#endregion

#region Using Directives
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
using Microsoft.CSharp;
#endregion

namespace CSharp.Ch08.Supplemental._03.CodeDomCompileAndRun
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * The main Chapter 8 lesson's CodeDOM example builds a class as an object graph and
         *   renders it as C# source text, and stops there. This project goes one step further,
         *   the step that actually makes the CodeDOM useful for something: it COMPILES the
         *   generated code into a real, loadable in-memory assembly, then uses reflection
         *   (tying this back to the rest of the chapter) to instantiate the generated type and
         *   call its methods, methods that didn't exist as compiled code until this program
         *   built and compiled them itself, at runtime.
         *
         * This also demonstrates two CodeDOM pieces the main lesson's simpler example didn't
         *   need: CodeParameterDeclarationExpression (declaring a method parameter) and
         *   CodeMethodInvokeExpression (one generated method calling another).
         *
         * New classes used here, beyond what the main lesson already covered:
         * - CompilerParameters         Settings controlling how the generated code compiles
         *                                 (GenerateInMemory, referenced assemblies, etc.)
         * - CompilerResults             The outcome of a compile: any errors, and (on success)
         *                                 the resulting Assembly
         * - CodeVariableDeclarationStatement   Declares a local variable inside a method body
         * - CodeMethodInvokeExpression  Calls a method (here, one generated method calling
         *                                 another, entirely within the generated code)
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                GenerateCompileAndRun();
                GenericFunctions.Pause();
                #endregion
            }
            catch (Exception ex)
            {
                new DatabankException("Error Caught!", ex).Log();
                GenericFunctions.Pause();
            }
            finally
            {
                GenericFunctions.Pause(final: true);
            }
        }
        #endregion

        #region Lesson Methods
        // Build a class via CodeDOM, compile it in memory, then invoke its methods via reflection
        private static void GenerateCompileAndRun()
        {
            var compileUnit = BuildCalculatorCompileUnit();

            using var provider = new CSharpCodeProvider();

            // Step 1: render the object graph as C# source text, purely so we can see what
            //   we're about to compile, same technique the main lesson used.
            using (var writer = new StringWriter())
            {
                provider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions { BracingStyle = "C" });
                Console.WriteLine("Generated source code:");
                Console.WriteLine(writer.ToString());
            }

            GenericFunctions.Pause();

            // Step 2: actually compile that same object graph into a real, in-memory assembly.
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
                {
                    Console.WriteLine($" - {error}");
                }
                return;
            }

            Console.WriteLine("Compilation succeeded.");
            GenericFunctions.Pause();

            // Step 3: use reflection, the same tools the rest of this chapter covered, to
            //   instantiate the freshly-compiled type and call its methods.
            Assembly compiledAssembly = results.CompiledAssembly;
            Type calculatorType = compiledAssembly.GetType("GeneratedCode.Calculator");
            object calculatorInstance = Activator.CreateInstance(calculatorType ?? throw new DatabankException("Generated Calculator type not found!"));

            var addMethod = calculatorType.GetMethod("Add");
#pragma warning disable IDE0300 // Simplify collection initialization
            var sum = addMethod?.Invoke(calculatorInstance, new object[] { 2, 3 });
#pragma warning restore IDE0300 // Simplify collection initialization
            Console.WriteLine($"Calculator.Add(2, 3) = {sum}");

            var addThenDoubleMethod = calculatorType.GetMethod("AddThenDouble");
#pragma warning disable IDE0300 // Simplify collection initialization
            var doubled = addThenDoubleMethod?.Invoke(calculatorInstance, new object[] { 2, 3 });
#pragma warning restore IDE0300 // Simplify collection initialization
            Console.WriteLine($"Calculator.AddThenDouble(2, 3) = {doubled}");

            Console.WriteLine($"{Environment.NewLine}Neither of those methods existed as compiled code until this program built and compiled them, moments ago.");
        }

        // Build the CodeDOM object graph for a small "Calculator" class with two methods
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


#pragma warning disable S125
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

            var sumVariable = new CodeVariableDeclarationStatement(typeof(int), "sum",
                new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Add",
                    new CodeArgumentReferenceExpression("a"), new CodeArgumentReferenceExpression("b")));
            addThenDoubleMethod.Statements.Add(sumVariable);

            addThenDoubleMethod.Statements.Add(new CodeMethodReturnStatement(
                new CodeBinaryOperatorExpression(
                    new CodeVariableReferenceExpression("sum"),
                    CodeBinaryOperatorType.Multiply,
                    new CodePrimitiveExpression(2))));
            classDeclaration.Members.Add(addThenDoubleMethod);

            return compileUnit;
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
