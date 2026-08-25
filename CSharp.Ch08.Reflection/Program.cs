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
using System.Linq;
using System.Reflection;
using CSharp.Ch08.Reflection.Models.Attributes;
using CSharp.Ch08.Reflection.Models.Enumerations;
using CSharp.Ch08.Reflection.Models.Objects;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
using Microsoft.CSharp;
#endregion

namespace CSharp.Ch08.Reflection
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * Reflection allows visibility to read, modify, or invoke behavior for an assembly, module, or type.
         *
         * We've previously touched on this in our exception handlers where we interrogated the exception for its type name
         *      ex.GetType().Name
         *
         * !! WARNING !!
         * In general, using Reflection is a resource-intensive process, so while sometimes useful,
         *     we should always make sure that it is the best method to accomplish something before using it
         *
         * Common Reflection Classes and Their Properties/Methods:
         * - Assembly               Metadata about the DLL or EXE contents (classes, etc.)
         *   - CodeBase                     Path to assembly
         *   - FullName                     Assembly Name
         *   - GlobalAssemblyCache          True if loaded from GAC
         *   - ImageRuntimeVersion          CLR version used by assembly
         *   - Location                     Path or UNC
         *   - SecurityRuleSet              Identifies set of rules used by the CLR
         *   - GetTypes()                   List of types defined
         *   - GetExportedTypes()           Public types defined
         *   - GetModule()                  Returns specified module
         *   - GetModules()                 List of assembly modules
         *   - CreateInstance()             Creates an instance of a specified class
         *   - GetCustomAttributes()        List of custom attributes
         *   - GetExecutingAssembly()       Returns the currently executing assembly 
         *   - GetReferencedAssemblies()    Returns list of referenced assemblies
         *   - Load()                       Load specified assembly
         *   - LoadFile()                   Load content of assembly file
         *   - LoadFrom()                   Load assembly from file name or path
         *   - ReflectionOnlyLoad()         Load assembly but restrict reflection to types defined in the assembly
         *   - UnsafeLoadFrom()             Bypass security checks when loading assembly
         *
         * - EventInfo          Metadata about an event in a class
         * - FieldInfo          Metadata about a specific member field in a class
         * - MemberInfo         Metadata about any member of a class
         * - MethodInfo         Metadata about a class method
         * - Module             Metadata about the DLL or EXE file containing the Assembly
         * - ParameterInfo      Metadata about defined method parameters
         * - PropertyInfo       Metadata about a class property
         *
         * The System.Type Class:
         *   Type is the entry point into reflection for any given .NET type, obtained via typeof(SomeType),
         *     an instance's own .GetType(), or by looking it up by name from an Assembly. From a Type, you
         *     can reach every other kind of reflection metadata: constructors, fields, properties, methods,
         *     and (for enums specifically) the set of named values.
         *
         * Custom Attributes:
         *   An attribute attaches declarative metadata to code (a class, method, property, etc.) that can be
         *     read back at runtime via reflection. Unlike a comment, an attribute is real, structured data the
         *     compiler embeds into the assembly, code that never even instantiates your class can still ask
         *     "does this type have a CourseCatalogAttribute, and if so, what department does it say?"
         *
         * The CodeDOM (Code Document Object Model):
         *   A pre-Roslyn, source-language-agnostic way to represent and generate source code (C#, VB.NET,
         *     etc.) as an object graph, then render that graph as actual source text. It predates the modern
         *     Roslyn compiler APIs and is far less commonly used today, but it's still part of the .NET
         *     Framework BCL and this chapter's official curriculum, so we cover a minimal, real example.
         *
         * Lambda Expressions:
         *   Delegates, anonymous methods, and lambda expressions were already covered in depth back in
         *     Chapter 6 (see CSharp.Ch06.DelegatesEventsAndExceptions and its Supplemental projects), this
         *     chapter's own treatment of the topic is intentionally brief, see LambdaExpressionsRecap()
         *     below for a short pointer back rather than a re-teaching.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                #region Assembly
                // Look at details about this program assembly
                ExamineCurrentAssembly();
                GenericFunctions.Pause();

                // Look at details about a defined assembly
                ExamineRelatedAssembly();
                GenericFunctions.Pause();

                // Look at details about a global assembly
                ExamineGlobalAssembly();
                GenericFunctions.Pause();

                /*
                 * LoadFrom() vs LoadFile():
                 * - Load Context
                 *   - Found by probing the GAC, host assembly store, folder containing the executing assembly,
                 *     or that assembly's /bin folder
                 *   * Preferred method
                 * - Load-From Context
                 *   - Assemblies located in the path passed into LoadFrom()
                 *     * Disadvantages:
                 *       - In a name collision, the already loaded assembly is returned, not the one at the defined path
                 *       - Multiple assemblies in the probing path will result in an exception
                 *       - Requires FileIOPermissionAccess.Read and FileIOPermissionAccess.PathDiscovery permissions to the file path.
                 * - Reflection-Only Context
                 *   - Assemblies loaded using ReflectionOnlyLoad() or ReflectionOnlyLoadFrom()
                 */

                // Look at details about an assembly from file
                ExamineFileAssembly();
                GenericFunctions.Pause();

                InstantiateAssembly();
                GenericFunctions.Pause();
                #endregion

                #region Type
                // Demonstrate getting a Type variable
                TypeExample();
                GenericFunctions.Pause();

                // Demonstrate some properties of Type
                TypeDetails();
                GenericFunctions.Pause();

                // Demonstrate GetConstructors()
                ExamineConstructors();
                GenericFunctions.Pause();

                // Demonstrate GetEnumName(s)/GetEnumValues()
                ExamineEnum();
                GenericFunctions.Pause();

                // Demonstrate GetField(s), including non-public fields
                ExamineFields();
                GenericFunctions.Pause();

                // Demonstrate GetProperty/GetProperties(), including inherited properties
                ExamineProperties();
                GenericFunctions.Pause();

                // Demonstrate GetMethod/GetMethods(), including invoking a method via reflection
                ExamineMethods();
                GenericFunctions.Pause();

                // Demonstrate GetArrayRank()
                ExamineArrayRank();
                GenericFunctions.Pause();
                #endregion

                #region Custom Attributes
                // Demonstrate reading a custom attribute applied to a class
                ReadCourseCatalogAttribute();
                GenericFunctions.Pause();
                #endregion

                #region CodeDOM
                // Demonstrate generating C# source code from a CodeDOM object graph
                GenerateCodeWithCodeDom();
                GenericFunctions.Pause();
                #endregion

                #region Lambda Expressions
                // Brief pointer back to Chapter 6's thorough treatment of this topic
                LambdaExpressionsRecap();
                GenericFunctions.Pause();
                #endregion
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
        #region Assembly
        // Look at details about this program assembly
        private static void ExamineCurrentAssembly()
        {
            // GetExecutingAssembly() returns the currently executing DLL or EXE
            DisplayAssemblyDetails(Assembly.GetExecutingAssembly());
        }

        // Look at details about a defined assembly
        private static void ExamineRelatedAssembly()
        {
            // You can find a related assembly by name
            DisplayAssemblyDetails(Assembly.Load("CSharp.SharedLibrary"));
        }

        // Look at details about a global assembly
        private static void ExamineGlobalAssembly()
        {
            DisplayAssemblyDetails(
                Assembly.Load("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
        }

        // Look at details about an assembly from file
        private static void ExamineFileAssembly()
        {
            string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // Note: log4net is referenced via <PackageReference>, so its DLL is copied into
            //       this project's own output directory automatically at build time, no manual
            //       copying required (unlike the original 2021/2022 draft's approach).
            string dllPath = Path.Combine(exeDir ?? "", "log4net.dll");
            #pragma warning disable S3885 // For the lesson, LoadFile() is used to demonstrate loading an assembly from a file path, even though LoadFrom() is preferred in general.
            DisplayAssemblyDetails(Assembly.LoadFile(dllPath));
            #pragma warning restore S3885
        }

        // Look at details about specified assembly
        private static void DisplayAssemblyDetails(Assembly assembly)
        {
            Console.WriteLine($"CodeBase: {assembly.CodeBase}");
            Console.WriteLine($"FullName: {assembly.FullName}");
            Console.WriteLine($"GlobalAssemblyCache: {assembly.GlobalAssemblyCache}");
            Console.WriteLine($"ImageRuntimeVersion: {assembly.ImageRuntimeVersion}");
            Console.WriteLine($"Location: {assembly.Location}");
            Console.WriteLine($"SecurityRuleSet: {assembly.SecurityRuleSet}");
            GenericFunctions.Pause();
            Console.WriteLine("Defined Types:");
            foreach (var type in assembly.GetTypes()) Console.WriteLine($" - {type.Name}");
            Console.WriteLine("Exported Types:");
            foreach (var type in assembly.GetExportedTypes()) Console.WriteLine($" - {type.Name}");
            Console.WriteLine("Modules:");
            foreach (var mod in assembly.GetModules()) Console.WriteLine($" - {mod.Name}");
            Console.WriteLine("References:");
            foreach (var ras in assembly.GetReferencedAssemblies()) Console.WriteLine($" - {ras.Name}");
        }

        // Create an instance of a class defined in the loaded assembly
        private static void InstantiateAssembly()
        {
            // Get the assembly
            var sharedLib = Assembly.Load("CSharp.SharedLibrary");

            // Instantiate the object (note the full class name with namespace)
            // Note: An invalid class name passed to CreateInstance() will not throw an exception - it will just return NULL
            var item = (Item)sharedLib.CreateInstance("CSharp.SharedLibrary.Models.Item") ?? throw new DatabankException("Error creating Item object!");
            item.Name = "My item";
            Console.WriteLine($"Created instance of the {item.GetType()} class with Name = '{item.Name}'");
        }
        #endregion

        #region Type
        // Demonstrate getting a Type variable
        private static void TypeExample()
        {
            var intType = typeof(int);
            Console.WriteLine($"Found type [{intType.Name}]");

            int x = 1;
            intType = x.GetType();
            Console.WriteLine($"Found type [{intType.Name}]");
        }

        // Demonstrate some properties of Type
        private static void TypeDetails()
        {
            int x = 0;
            var intType = x.GetType();

            Console.WriteLine($"Name: {intType.Name}");
            Console.WriteLine($"Namespace: {intType.Namespace}");
            Console.WriteLine($"Assembly: {intType.Assembly}");
            Console.WriteLine($"AssemblyQualifiedName: {intType.AssemblyQualifiedName}");
            Console.WriteLine($"FullName: {intType.FullName}");
            Console.WriteLine($"IsValueType: {intType.IsValueType}");
        }

        // Demonstrate GetConstructors(), reflecting on Person's multiple overloaded constructors
        private static void ExamineConstructors()
        {
            var personType = typeof(Person);
            Console.WriteLine($"Constructors defined on {personType.Name}:");
            foreach (var ctor in personType.GetConstructors())
            {
                string parameterList = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                Console.WriteLine($" - {personType.Name}({parameterList})");
            }
        }

        // Demonstrate GetEnumNames()/GetEnumValues(), reflecting on the Degree enum
        private static void ExamineEnum()
        {
            var degreeType = typeof(Degree);
            Console.WriteLine($"IsEnum: {degreeType.IsEnum}");

            Console.WriteLine($"{Environment.NewLine}Names (GetEnumNames):");
            foreach (var name in degreeType.GetEnumNames()) Console.WriteLine($" - {name}");

            Console.WriteLine($"{Environment.NewLine}Values (GetEnumValues):");
            foreach (var value in degreeType.GetEnumValues()) Console.WriteLine($" - {(int)value}: {value}");
        }

        // Demonstrate GetFields(), including private fields via BindingFlags
        private static void ExamineFields()
        {
            var courseType = typeof(Course);

            // By default, GetFields() only returns PUBLIC fields. Course's own data is exposed
            //   entirely through auto-properties (which are backed by hidden compiler-generated
            //   fields, not the same thing as a field YOU declared), so this comes back empty.
            Console.WriteLine($"Public fields on {courseType.Name}: {courseType.GetFields().Length}");

            // Passing BindingFlags explicitly is required to see NON-public fields, here we ask
            //   for public AND non-public instance fields, which picks up Course's private,
            //   hand-declared gradeCriteria dictionary.
            Console.WriteLine($"{Environment.NewLine}All instance fields (public and non-public) on {courseType.Name}:");
            #pragma warning disable S3011 // For the lesson, we are intentionally using reflection to access non-public members, which is normally discouraged.
            foreach (var field in courseType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                Console.WriteLine($" - {field.FieldType.Name} {field.Name}");
            }
            #pragma warning restore S3011
        }

        // Demonstrate GetProperties(), including properties inherited from a base class
        private static void ExamineProperties()
        {
            var studentType = typeof(Student);
            Console.WriteLine($"Properties on {studentType.Name} (including inherited):");
            foreach (var property in studentType.GetProperties())
            {
                Console.WriteLine($" - {property.PropertyType.Name} {property.Name} (declared on {property.DeclaringType?.Name})");
            }
        }

        // Demonstrate GetMethods(), and actually invoking a method found via reflection
        private static void ExamineMethods()
        {
            var taType = typeof(TeachingAssistant);

            // DeclaredOnly limits this to methods TeachingAssistant itself defines, not everything
            //   it inherits from Faculty/Employee/Person or gets from IStudent.
            Console.WriteLine($"Public instance methods declared directly on {taType.Name}:");
            foreach (var method in taType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                Console.WriteLine($" - {method.ReturnType.Name} {method.Name}()");
            }

            // Now the real payoff: actually calling a method purely through its MethodInfo,
            //   with no compile-time reference to TeachingAssistant.Credentials() at all.
            var ta = new TeachingAssistant { FirstName = "Alex", LastName = "Rivera", Degree = Degree.Masters };
            var credentialsMethod = taType.GetMethod("Credentials");
            var result = credentialsMethod?.Invoke(ta, null);
            Console.WriteLine($"{Environment.NewLine}Invoked Credentials() via reflection: {result}");
        }

        // Demonstrate GetArrayRank()
        private static void ExamineArrayRank()
        {
            var oneDimensional = new int[5];
            var twoDimensional = new int[3, 4];
            var threeDimensional = new int[2, 2, 2];

            Console.WriteLine($"int[5].GetType().GetArrayRank(): {oneDimensional.GetType().GetArrayRank()}");
            Console.WriteLine($"int[3,4].GetType().GetArrayRank(): {twoDimensional.GetType().GetArrayRank()}");
            Console.WriteLine($"int[2,2,2].GetType().GetArrayRank(): {threeDimensional.GetType().GetArrayRank()}");
        }
        #endregion

        #region Custom Attributes
        // Demonstrate reading a custom attribute applied to a class (see Models/Attributes/CourseCatalogAttribute.cs
        //   for where it's defined, and Models/Objects/Course.cs for where it's applied)
        private static void ReadCourseCatalogAttribute()
        {
            var courseType = typeof(Course);

            // GetCustomAttribute<T>() returns null if the attribute isn't present, rather than throwing
            var catalogAttribute = courseType.GetCustomAttribute<CourseCatalogAttribute>();

            if (catalogAttribute != null)
            {
                Console.WriteLine($"{courseType.Name} is cataloged under {catalogAttribute.Department}, {catalogAttribute.CreditHours} credit hour(s).");
            }
            else
            {
                Console.WriteLine($"{courseType.Name} has no CourseCatalogAttribute applied.");
            }
        }
        #endregion

        #region CodeDOM
        // Demonstrate building a small class definition as a CodeDOM object graph, then
        //   rendering that graph as actual C# source text
        private static void GenerateCodeWithCodeDom()
        {
            var compileUnit = new CodeCompileUnit();

            var codeNamespace = new CodeNamespace("GeneratedCode");
            codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
            compileUnit.Namespaces.Add(codeNamespace);

            var classDeclaration = new CodeTypeDeclaration("Greeter")
            {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public
            };
            codeNamespace.Types.Add(classDeclaration);

            // A private backing field
            #pragma warning disable S1192 // IN this lesson, we'll leave the string literal
            var nameField = new CodeMemberField(typeof(string), "_name") { Attributes = MemberAttributes.Private };
            #pragma warning restore S1192
            classDeclaration.Members.Add(nameField);

            // A public auto-style property wrapping the field
            var nameProperty = new CodeMemberProperty
            {
                Name = "Name",
                Type = new CodeTypeReference(typeof(string)),
                Attributes = MemberAttributes.Public,
                HasGet = true,
                HasSet = true
            };
            nameProperty.GetStatements.Add(
                new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_name")));
            nameProperty.SetStatements.Add(
                new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_name"),
                    new CodePropertySetValueReferenceExpression()));
            classDeclaration.Members.Add(nameProperty);

            // A method returning a greeting built from the field
            var greetMethod = new CodeMemberMethod
            {
                Name = "Greet",
                Attributes = MemberAttributes.Public,
                ReturnType = new CodeTypeReference(typeof(string))
            };
            greetMethod.Statements.Add(new CodeMethodReturnStatement(
                new CodeBinaryOperatorExpression(
                    new CodePrimitiveExpression("Hello, "),
                    CodeBinaryOperatorType.Add,
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_name"))));
            classDeclaration.Members.Add(greetMethod);

            // Render the object graph as actual C# source text
            using var provider = new CSharpCodeProvider();
            using var writer = new StringWriter();
            provider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions { BracingStyle = "C" });

            Console.WriteLine("Generated source code:");
            Console.WriteLine(writer.ToString());
        }
        #endregion

        #region Lambda Expressions
        // A brief pointer back to Chapter 6's thorough treatment of delegates, anonymous
        //   methods, and lambda expressions, rather than re-teaching material already covered
        private static void LambdaExpressionsRecap()
        {
            Console.WriteLine("Delegates, anonymous methods, and lambda expressions were covered in depth in");
            Console.WriteLine("Chapter 6, see CSharp.Ch06.DelegatesEventsAndExceptions and its Supplemental");
            Console.WriteLine("projects (particularly Supplemental 01 and 02) for the full treatment.");
        }
        #endregion
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
