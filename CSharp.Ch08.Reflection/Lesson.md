# Chapter 8: Reflection

## What This Is

Reflection is the ability to inspect -- and sometimes invoke or modify -- the metadata of an assembly, module, or type at runtime, without compile-time knowledge of what you're looking at. It's the mechanism behind test runners finding `[TestMethod]`s, JSON serializers populating properties, DI containers selecting constructors, and ORMs materializing entities.

The chapter opens with a warning in the source, and the warning deserves to be in the lesson too:

```
!! WARNING !!
In general, using Reflection is a resource-intensive process, so while sometimes useful,
    we should always make sure that it is the best method to accomplish something before using it
```

This is not boilerplate caution. Every reflective member lookup is a string-based search through metadata tables, with no JIT inlining, no type checking, and no compiler assistance. A reflective method call can be hundreds of times slower than a direct one. `Supplemental.04.ReflectionPerformance` measures exactly how much and shows how to recover most of it. Read that before putting reflection on a hot path.

The other cost is subtler: reflection defeats compile-time safety. Rename the method you're looking up by string and the compiler says nothing -- you find out at runtime, when `GetMethod` returns `null`.

You've already been using reflection, by the way. Every `ex.GetType().Name` in every catch block since Chapter 1 is reaching into runtime metadata. This chapter just makes the mechanism explicit and shows how far it goes.

---

## How to Write This Program

The models used throughout -- `Person`, `Course`, `Student`, `TeachingAssistant`, `Degree`, `CourseCatalogAttribute` -- are already in the project under `Models/`. Read them briefly before starting. The hierarchy is Person -> Employee -> Faculty -> TeachingAssistant, and Student : Person. `Course` carries the `[CourseCatalog]` attribute.

---

## Part 1: Assembly

An `Assembly` is the metadata about a DLL or EXE: what types it defines, what it references, where it lives, which CLR version built it.

### Mini-Program 1: Examining Assemblies

Add a shared display helper, then examine four different assemblies in sequence:

```csharp
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
```

Clear `Main()` and call `DisplayAssemblyDetails` four times, with a pause between each:

```csharp
// 1. The currently executing assembly
Console.WriteLine("=== Executing Assembly ===");
DisplayAssemblyDetails(Assembly.GetExecutingAssembly());
GenericFunctions.Pause();

// 2. A referenced assembly, by simple name
Console.WriteLine("=== Related Assembly ===");
DisplayAssemblyDetails(Assembly.Load("CSharp.SharedLibrary"));
GenericFunctions.Pause();

// 3. A GAC assembly, by fully qualified strong name
Console.WriteLine("=== GAC Assembly ===");
DisplayAssemblyDetails(
    Assembly.Load("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
GenericFunctions.Pause();

// 4. An assembly loaded from a file path
Console.WriteLine("=== File Assembly ===");
string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
string dllPath = Path.Combine(exeDir ?? "", "log4net.dll");
DisplayAssemblyDetails(Assembly.LoadFile(dllPath));
GenericFunctions.Pause();
```

Run it. Compare what each approach returns. The GAC assembly's `FullName` includes version, culture, and public key token -- the long string you passed in is a **strong name**. The `PublicKeyToken` is what lets the GAC hold multiple versions of `System.Data` simultaneously without collision. Chapter 12 Supplemental 04 covers where those tokens come from.

Note `GetTypes()` returns all types including internal ones; `GetExportedTypes()` returns only public ones. The difference is visible on `CSharp.SharedLibrary` if any of its types are internal.

The `LoadFile()` call for log4net carries a suppression:

```csharp
#pragma warning disable S3885 // For the lesson, LoadFile() is used to demonstrate loading
                              // from a file path, even though LoadFrom() is preferred.
```

SonarLint flags `LoadFile()` with good reason. Four load contexts exist:

- **`Load()`** -- probes GAC and standard paths. Preferred. Plays well with everything.
- **`LoadFrom()`** -- loads from an arbitrary path. Useful for plugin scenarios, but: in a name collision the already-loaded assembly is returned silently, ignoring your path. Multiple assemblies in the probing path throw. Requires filesystem permissions.
- **`LoadFile()`** -- loads with almost no context, bypassing resolution rules. Even the loaded assembly's own dependencies won't resolve automatically. Use only when you genuinely need this behavior and understand why.
- **`ReflectionOnlyLoad()`** -- inspect metadata without executing any code from the assembly. The safe choice for examining untrusted DLLs.

The critical gotcha: **the same DLL loaded through two different contexts produces two incompatible sets of types**. A type's identity includes its assembly and its load context. So this can happen:

```
InvalidCastException: Unable to cast object of type 'MyLib.Widget' to type 'MyLib.Widget'.
```

That is not a joke. It means you have the same type loaded twice from two contexts. The error message looks impossible until you know this exists.

### Mini-Program 2: Instantiating From a Loaded Assembly

Clear `Main()` and write:

```csharp
var sharedLib = Assembly.Load("CSharp.SharedLibrary");

// Note: CreateInstance() returns NULL on a bad class name -- it does not throw.
// The full namespace-qualified name is required.
var item = (Item)sharedLib.CreateInstance("CSharp.SharedLibrary.Models.Item")
           ?? throw new DatabankException("Error creating Item object!");

item.Name = "My item";
Console.WriteLine($"Created instance of {item.GetType()} with Name = '{item.Name}'");
GenericFunctions.Pause();
```

Run it. The object is created and populated through its type alone, with no compile-time `new Item()` anywhere.

Two things to internalize from this.

The name must be **fully namespace-qualified**. `"Item"` alone returns `null`. `"CSharp.SharedLibrary.Models.Item"` works.

A typo returns `null`. It does not throw. This is the compile-time-safety loss in its most concrete form. The `?? throw` is not defensive padding -- it's the only thing standing between a misspelled string and a `NullReferenceException` three stack frames later. Every reflection method that looks things up by name -- `CreateInstance`, `GetMethod`, `GetProperty`, `GetCustomAttribute` -- can return `null`, and all of them will do so in complete silence.

---

## Part 2: Type

`Type` is the entry point to all other reflection metadata. You get one via `typeof(T)` at compile time, or via `instance.GetType()` at runtime.

### Mini-Program 3: Getting a Type

Clear `Main()` and write:

```csharp
// typeof() -- resolved at compile time, reflects the declared type
var intType = typeof(int);
Console.WriteLine($"typeof(int) -> [{intType.Name}]");

// GetType() -- resolved at runtime, reflects the actual type
int x = 1;
intType = x.GetType();
Console.WriteLine($"(1).GetType() -> [{intType.Name}]");
GenericFunctions.Pause();
```

Run it. Both print `Int32`. The difference matters when you have a derived type:

```csharp
Person p = new Student();
Console.WriteLine(typeof(Person).Name);  // Person
Console.WriteLine(p.GetType().Name);     // Student
GenericFunctions.Pause();
```

`typeof(Person)` is resolved at compile time and reflects the declared type. `p.GetType()` is resolved at runtime and reflects what the object actually is. That distinction is how polymorphic serializers and JSON converters know to emit the concrete type rather than the base type.

Also: `intType.Name` is `Int32`, not `int`. `int` is a C# language alias. The CLR only knows `System.Int32`.

### Mini-Program 4: Type Properties

Clear `Main()` and write:

```csharp
int x = 0;
var intType = x.GetType();

Console.WriteLine($"Name:                  {intType.Name}");
Console.WriteLine($"Namespace:             {intType.Namespace}");
Console.WriteLine($"Assembly:              {intType.Assembly}");
Console.WriteLine($"AssemblyQualifiedName: {intType.AssemblyQualifiedName}");
Console.WriteLine($"FullName:              {intType.FullName}");
Console.WriteLine($"IsValueType:           {intType.IsValueType}");
GenericFunctions.Pause();
```

Run it. `AssemblyQualifiedName` is the full strong name you'd pass to `Assembly.Load` to get this type's assembly back out.

### Mini-Program 5: Constructors

Clear `Main()` and write:

```csharp
var personType = typeof(Person);
Console.WriteLine($"Constructors on {personType.Name}:");
foreach (var ctor in personType.GetConstructors())
{
    string paramList = string.Join(", ",
        ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    Console.WriteLine($" - {personType.Name}({paramList})");
}
GenericFunctions.Pause();
```

Run it. You'll see three constructors: the parameterless one, one taking `firstName`, and one taking `firstName` and `lastName`.

`ParameterInfo` preserves parameter **names**, not just types. That metadata survives compilation -- it's what makes named-argument binding, DI container constructor selection, and ASP.NET model binding possible.

### Mini-Program 6: Enums

Clear `Main()` and write:

```csharp
var degreeType = typeof(Degree);
Console.WriteLine($"IsEnum: {degreeType.IsEnum}");

Console.WriteLine($"\nNames (GetEnumNames):");
foreach (var name in degreeType.GetEnumNames())
    Console.WriteLine($" - {name}");

Console.WriteLine($"\nValues (GetEnumValues):");
foreach (var value in degreeType.GetEnumValues())
    Console.WriteLine($" - {(int)value}: {value}");
GenericFunctions.Pause();
```

Run it. `GetEnumValues()` returns values boxed as `object`. Printing `value` alone calls `Enum.ToString()` which does a name lookup, giving you the name. Casting to `int` reveals the underlying numeric value. Both side by side show the pairing: `Associates` is `0`, `Doctorate` is `3`, because no explicit values were declared and the enum counts from zero.

### Mini-Program 7: Fields and BindingFlags

Clear `Main()` and write:

```csharp
var courseType = typeof(Course);

// Default: public fields only. Course's data is all auto-properties,
// whose backing fields are compiler-generated, private, and unspeakably named.
Console.WriteLine($"Public fields on {courseType.Name}: {courseType.GetFields().Length}");

// To see anything, you must ask explicitly. Passing BindingFlags at all
// replaces the defaults entirely -- you must specify both a visibility flag
// (Public/NonPublic) AND a scope flag (Instance/Static) or you get nothing.
Console.WriteLine($"\nAll instance fields on {courseType.Name}:");
foreach (var field in courseType.GetFields(
    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
{
    Console.WriteLine($" - {field.FieldType.Name} {field.Name}");
}
GenericFunctions.Pause();
```

Run it. The default call returns 0. The explicit call returns the private `gradeCriteria` dictionary, plus the compiler-generated backing fields for the auto-properties (named something like `<Name>k__BackingField`).

The `BindingFlags` rule that catches everyone: the moment you pass any flags explicitly, you replace the defaults entirely. You must specify at least one visibility flag (`Public`, `NonPublic`) **and** at least one scope flag (`Instance`, `Static`). Forgetting `Instance` when you're looking for instance members is the classic version of this bug -- it returns an empty array with no error, and you'll spend quality time wondering why the fields aren't there.

Reading private state through reflection breaks encapsulation deliberately -- you're reaching past a boundary the author established, and nothing obligates them to keep that private field stable between versions. It's the right tool for a debugger or a serializer. It's the wrong tool for ordinary application logic.

### Mini-Program 8: Properties and Inheritance

Clear `Main()` and write:

```csharp
var studentType = typeof(Student);
Console.WriteLine($"Properties on {studentType.Name} (including inherited):");
foreach (var property in studentType.GetProperties())
{
    Console.WriteLine($" - {property.PropertyType.Name} {property.Name} " +
                      $"(declared on {property.DeclaringType?.Name})");
}
GenericFunctions.Pause();
```

Run it. `GetProperties()` walks the inheritance chain by default, so you'll see properties declared on `Person` showing up on `Student`, with `DeclaringType` identifying where each one actually lives.

This is an asymmetry worth remembering: public property and method lookups traverse base types; non-public member lookups don't. If you need a private field from a base class, you have to walk up `Type.BaseType` yourself.

### Mini-Program 9: Methods and Invoking One

Clear `Main()` and write:

```csharp
var taType = typeof(TeachingAssistant);

// DeclaredOnly: only methods TeachingAssistant itself defines, not everything
// it inherits from Faculty/Employee/Person or gets from IStudent.
// Without DeclaredOnly you'd also get property accessors as get_Name/set_Name.
Console.WriteLine($"Methods declared directly on {taType.Name}:");
foreach (var method in taType.GetMethods(
    BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
{
    Console.WriteLine($" - {method.ReturnType.Name} {method.Name}()");
}

// The real payoff: calling a method with no compile-time reference to it at all.
var ta = new TeachingAssistant { FirstName = "Alex", LastName = "Rivera", Degree = Degree.Masters };
var credentialsMethod = taType.GetMethod("Credentials");
var result = credentialsMethod?.Invoke(ta, null);
Console.WriteLine($"\nInvoked Credentials() via reflection: {result}");
GenericFunctions.Pause();
```

Run it. The method is found by name, called on the instance, and the return value comes back as `object`.

`Invoke(target, parameters)` takes the instance to call on (`null` for static methods) and an `object[]` of arguments (`null` for no arguments). It returns `object`, so non-void returns need a cast.

Two hazards. The `?.` is load-bearing -- `GetMethod("Credentials")` returns `null` on a typo, silently. And if the invoked method throws, `Invoke` wraps it in a `TargetInvocationException`. The real exception is in `.InnerException`. Catching for the original exception type directly will not match.

### Mini-Program 10: Array Rank

Clear `Main()` and write:

```csharp
var oneDimensional   = new int[5];
var twoDimensional   = new int[3, 4];
var threeDimensional = new int[2, 2, 2];

Console.WriteLine($"int[5]:       GetArrayRank() = {oneDimensional.GetType().GetArrayRank()}");
Console.WriteLine($"int[3,4]:     GetArrayRank() = {twoDimensional.GetType().GetArrayRank()}");
Console.WriteLine($"int[2,2,2]:   GetArrayRank() = {threeDimensional.GetType().GetArrayRank()}");
GenericFunctions.Pause();
```

Run it. Rank is the number of dimensions, independent of length. Note that `int[][]` (jagged array) has rank 1 -- it's an array whose elements happen to be arrays, not a two-dimensional rectangular array.

---

## Part 3: Custom Attributes

An attribute attaches declarative metadata to code -- a class, method, property, parameter -- that can be read back at runtime via reflection. Unlike a comment, it's real structured data the compiler embeds into the assembly. Code that never instantiates your class can still ask "does this type have a `CourseCatalogAttribute`, and if so, what department does it say?"

### Mini-Program 11: Defining and Applying an Attribute

`CourseCatalogAttribute` is already defined in `Models/Attributes/`. Read it -- it's short:

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class CourseCatalogAttribute : Attribute
{
    public string Department { get; }
    public int CreditHours { get; }

    public CourseCatalogAttribute(string department, int creditHours)
    {
        Department = department;
        CreditHours = creditHours;
    }
}
```

Three conventions at work. Inheriting from `Attribute` is what makes it an attribute rather than an ordinary class. The `Attribute` suffix is dropped at the usage site -- the class is `CourseCatalogAttribute` but applied as `[CourseCatalog(...)]`; the compiler tries both spellings. `[AttributeUsage]` constrains the attribute itself:

- `AttributeTargets.Class` -- applying this to a method or property is a *compile error*, not a runtime surprise.
- `Inherited = false` -- a class deriving from `Course` will not inherit this attribute.
- `AllowMultiple = false` -- applying it twice to the same target is also a compile error.

Properties are **get-only**, initialized through the constructor. Attribute values are baked into assembly metadata at compile time; they must be compile-time constants. You can't pass a computed value or a `new` object into an attribute argument.

`Course` already has the attribute applied: `[CourseCatalog("Computer Science", 3)]`. Now read it back.

### Mini-Program 12: Reading an Attribute

Clear `Main()` and write:

```csharp
var courseType = typeof(Course);

// GetCustomAttribute<T>() returns null if the attribute isn't present -- it doesn't throw.
var catalogAttribute = courseType.GetCustomAttribute<CourseCatalogAttribute>();

if (catalogAttribute != null)
{
    Console.WriteLine($"{courseType.Name} is cataloged under {catalogAttribute.Department}, " +
                      $"{catalogAttribute.CreditHours} credit hour(s).");
}
else
{
    Console.WriteLine($"{courseType.Name} has no CourseCatalogAttribute applied.");
}
GenericFunctions.Pause();
```

Run it. The attribute data is recovered and printed.

The key insight: `GetCustomAttribute<T>()` **instantiates** the attribute object. The constructor arguments written in the source were stored in metadata, and reading the attribute constructs a real `CourseCatalogAttribute` from them. Attributes are lazily created on read, not held in memory alongside the type.

The `null` check follows the same rule as every other reflection lookup. Use `GetCustomAttributes<T>()` (plural) for `AllowMultiple = true` attribute types -- it returns an empty collection rather than null.

Attributes are the foundation of declarative programming in .NET: `[Serializable]`, `[Obsolete]`, `[TestMethod]`, `[Required]`, `[JsonProperty]`, `[HttpGet]`. `Supplemental.01.CustomAttributes` goes considerably deeper into defining and consuming your own.

---

## Part 4: CodeDOM

The CodeDOM (Code Document Object Model) is a pre-Roslyn, source-language-agnostic way to represent source code as an object graph, then render that graph as actual source text. You build one graph describing a class, hand it to a `CSharpCodeProvider` or a `VBCodeProvider`, and get valid C# or VB.NET out of the same graph. That substitutability was genuinely useful before Roslyn existed. It's less commonly reached for in new code, but it's still in the BCL and on the curriculum, so here's a minimal real example.

### Mini-Program 13: Generating Source With CodeDOM

Clear `Main()` and write the whole graph in one go -- there's no useful checkpoint before it's complete:

```csharp
var compileUnit = new CodeCompileUnit();

// Namespace and imports
var codeNamespace = new CodeNamespace("GeneratedCode");
codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
compileUnit.Namespaces.Add(codeNamespace);

// Class declaration
var classDeclaration = new CodeTypeDeclaration("Greeter")
{
    IsClass = true,
    TypeAttributes = TypeAttributes.Public
};
codeNamespace.Types.Add(classDeclaration);

// Private backing field
var nameField = new CodeMemberField(typeof(string), "_name")
{
    Attributes = MemberAttributes.Private
};
classDeclaration.Members.Add(nameField);

// Public property wrapping the field
var nameProperty = new CodeMemberProperty
{
    Name = "Name",
    Type = new CodeTypeReference(typeof(string)),
    Attributes = MemberAttributes.Public,
    HasGet = true,
    HasSet = true
};
nameProperty.GetStatements.Add(
    new CodeMethodReturnStatement(
        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_name")));
nameProperty.SetStatements.Add(
    new CodeAssignStatement(
        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_name"),
        new CodePropertySetValueReferenceExpression()));
classDeclaration.Members.Add(nameProperty);

// Method returning a greeting
var greetMethod = new CodeMemberMethod
{
    Name = "Greet",
    Attributes = MemberAttributes.Public,
    ReturnType = new CodeTypeReference(typeof(string))
};
greetMethod.Statements.Add(
    new CodeMethodReturnStatement(
        new CodeBinaryOperatorExpression(
            new CodePrimitiveExpression("Hello, "),
            CodeBinaryOperatorType.Add,
            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_name"))));
classDeclaration.Members.Add(greetMethod);

// Render the object graph as C# source text
using var provider = new CSharpCodeProvider();
using var writer = new StringWriter();
provider.GenerateCodeFromCompileUnit(compileUnit, writer,
    new CodeGeneratorOptions { BracingStyle = "C" });

Console.WriteLine("Generated source code:");
Console.WriteLine(writer.ToString());
GenericFunctions.Pause();
```

Run it. You get a complete, compilable C# source file as output.

The verbosity of the CodeDOM is immediately obvious. `return _name;` requires three nested objects: a return statement, wrapping a field reference, wrapping a `this` reference. `CodePropertySetValueReferenceExpression` is the contextual `value` keyword inside a setter. The entire tree expressing `return "Hello, " + _name;` is five nested constructor calls.

`BracingStyle = "C"` puts the opening brace on its own line, matching normal C# convention. The default puts it on the same line, which is valid C# but will make you sad.

Changing `CSharpCodeProvider` to `VBCodeProvider` with no other modification emits equivalent VB.NET. That's the point of the abstraction and the one moment it earns its verbosity.

This method only *generates* text. `Supplemental.03.CodeDomCompileAndRun` takes the next step: compiling the generated source into a live in-memory assembly and calling it, which is where CodeDOM and reflection meet and produce something genuinely interesting.

For new work: use Roslyn (`Microsoft.CodeAnalysis.CSharp`) for analysis and generation, or Source Generators for compile-time generation. Both understand C# semantics rather than just surface syntax. CodeDOM has no support for any C# feature added after roughly 2005 -- no generics-heavy constructs, no `async`, no pattern matching, no records.

---

## Part 5: Lambda Expressions (Deliberately Brief)

Clear `Main()` and write:

```csharp
Console.WriteLine("Delegates, anonymous methods, and lambda expressions were covered in depth in");
Console.WriteLine("Chapter 6. See CSharp.Ch06.DelegatesEventsAndExceptions and its supplementals");
Console.WriteLine("(particularly Supplemental 01 and 02) for the full treatment.");
GenericFunctions.Pause();
```

The textbook groups lambdas into this chapter, but they were covered more thoroughly in Chapter 6. A weaker second pass helps no one. There is a genuine connection between lambdas and reflection worth knowing, though it's beyond this chapter's scope: `Expression<Func<T>>` represents a lambda as an inspectable expression tree rather than compiled IL. That's how LINQ providers translate C# lambda expressions into SQL, and how strongly-typed member-reference helpers avoid magic strings. Chapter 10's `Supplemental.04.IQueryableVsIEnumerable` covers the practical consequences.

---

## Takeaways

- Reflection trades performance and compile-time safety for runtime flexibility. The trade is worth it for frameworks, tooling, and serializers; rarely worth it for ordinary application logic.
- Every reflective lookup returns `null` on a miss, not an exception. Handle it at the call site.
- `BindingFlags` replaces the defaults entirely. Specify both a visibility flag and a scope flag every time.
- Load context is part of type identity. The same DLL loaded twice through different mechanisms yields incompatible types.
- `typeof(T)` reflects the declared type at compile time; `instance.GetType()` reflects the actual runtime type.
- Attributes are data embedded in assembly metadata, not documentation. They're instantiated lazily on read.
- `[AttributeUsage]` constrains where and how your attribute can be applied, turning misuse into a compile error.
- CodeDOM generates source text from an object graph. It's legacy -- reach for Roslyn or Source Generators in new work.
- Exceptions thrown inside `Invoke()` are wrapped in `TargetInvocationException`. The real exception is in `.InnerException`.

---

## Also in Chapter 8

Four supplemental projects accompany this one, each documented separately:

1. `CSharp.Ch08.Supplemental.01.CustomAttributes`
2. `CSharp.Ch08.Supplemental.02.DynamicInvocation`
3. `CSharp.Ch08.Supplemental.03.CodeDomCompileAndRun`
4. `CSharp.Ch08.Supplemental.04.ReflectionPerformance`
