# Chapter 8: Reflection

## What This Is

The main Chapter 8 project. Reflection is the ability to inspect — and in some cases modify or invoke — the metadata of an assembly, module, or type **at runtime**, without having compile-time knowledge of what you're looking at.

Unlike most chapters, this one has no `LectureNotes.md`; the Chapter Notes live in the `#region Chapter Notes` block at the top of `Program.cs`. The project covers four distinct topics that the textbook groups together under "Reflection":

1. **Assembly** — loading and inspecting assemblies
2. **Type** — the entry point to all other reflection metadata
3. **Custom Attributes** — declarative metadata you define and read back
4. **CodeDOM** — programmatic source-code generation

Plus a deliberate pointer back to Chapter 6 for lambda expressions.

---

## The Warning Comes First

The Chapter Notes lead with this, and it deserves to stay at the top of the lesson too:

```
!! WARNING !!
In general, using Reflection is a resource-intensive process, so while sometimes useful,
	we should always make sure that it is the best method to accomplish something before using it
```

This is not boilerplate caution. Reflection bypasses everything the compiler and JIT normally do for you: there's no type checking, no inlining, no devirtualization, and every member lookup is a string-based search through metadata tables. A reflective method call can be **hundreds of times** slower than a direct one.

`Supplemental.04.ReflectionPerformance` measures exactly how much slower, and shows how to claw most of it back. Read that one before you put reflection on a hot path.

The other half of the warning is subtler: **reflection defeats compile-time safety.** When you call `taType.GetMethod("Credentials")`, the string `"Credentials"` is not checked by anything. Rename that method and the compiler stays silent — you find out at runtime, when `GetMethod` returns `null`.

### You've Already Used It

The notes make a nice observation:

```
We've previously touched on this in our exception handlers where we interrogated the exception
for its type name
	 ex.GetType().Name
```

That's reflection. Every `GetType()` call in the previous seven chapters was reaching into runtime metadata. This chapter just makes the mechanism explicit and shows how far it goes.

---

## Part 1: Assembly

An `Assembly` is the metadata about a DLL or EXE — what types it defines, what it references, where it came from, which CLR version built it.

### Key Members

```
- CodeBase                     Path to assembly
- FullName                     Assembly Name
- GlobalAssemblyCache          True if loaded from GAC
- ImageRuntimeVersion          CLR version used by assembly
- Location                     Path or UNC
- SecurityRuleSet              Identifies set of rules used by the CLR
- GetTypes()                   List of types defined
- GetExportedTypes()           Public types defined
- GetModules()                 List of assembly modules
- CreateInstance()             Creates an instance of a specified class
- GetCustomAttributes()        List of custom attributes
- GetExecutingAssembly()       Returns the currently executing assembly
- GetReferencedAssemblies()    Returns list of referenced assemblies
```

### Four Ways In

The project demonstrates four different routes to an `Assembly` object, each in its own method, all funneling into the shared `DisplayAssemblyDetails()` printer.

**`ExamineCurrentAssembly()`** — the simplest case:

```csharp
DisplayAssemblyDetails(Assembly.GetExecutingAssembly());
```

**`ExamineRelatedAssembly()`** — load a referenced assembly by simple name:

```csharp
DisplayAssemblyDetails(Assembly.Load("CSharp.SharedLibrary"));
```

**`ExamineGlobalAssembly()`** — load by *fully qualified* name, including version, culture, and public key token:

```csharp
Assembly.Load("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
```

That long string is a **strong name**. The `PublicKeyToken` is what makes it unambiguous — it proves the assembly came from the publisher who holds the matching private key. This is the mechanism that lets the GAC hold multiple versions of `System.Data` side by side without collision. Chapter 12 (`Supplemental.04.StrongNamingAndTheGacDeepDive`) covers where those tokens come from.

**`ExamineFileAssembly()`** — load from a raw file path:

```csharp
string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
string dllPath = Path.Combine(exeDir ?? "", "log4net.dll");
DisplayAssemblyDetails(Assembly.LoadFile(dllPath));
```

Note the comment in the source about how `log4net.dll` gets there:

```csharp
// Note: log4net is referenced via <PackageReference>, so its DLL is copied into
//       this project's own output directory automatically at build time, no manual
//       copying required (unlike the original 2021/2022 draft's approach).
```

There's also a suppression here worth reading rather than skipping:

```csharp
#pragma warning disable S3885 // For the lesson, LoadFile() is used to demonstrate loading an assembly
							 // from a file path, even though LoadFrom() is preferred in general.
```

SonarLint flags `LoadFile()` on purpose, which leads directly into the next topic.

### Load Contexts: `LoadFrom()` vs `LoadFile()`

This is the part of the chapter most likely to bite you in production, and the Chapter Notes give it a full breakdown:

```
- Load Context
  - Found by probing the GAC, host assembly store, folder containing the executing assembly,
	or that assembly's /bin folder
- LoadFrom Context
  - Assemblies located in the path passed into LoadFrom()
	* Disadvantages:
	  - In a name collision, the already loaded assembly is returned, not the one at the defined path
	  - Multiple assemblies in the probing path will result in an exception
	  - Requires FileIOPermissionAccess.Read and FileIOPermissionAccess.PathDiscovery permissions
- Reflection-Only Context
  - Assemblies loaded using ReflectionOnlyLoad() or ReflectionOnlyLoadFrom()
```

The critical takeaway is that **the same DLL loaded into two different contexts produces two different, incompatible `Type` objects.** A type is identified by its assembly *and its load context*. So this can happen:

```
InvalidCastException: Unable to cast object of type 'MyLib.Widget' to type 'MyLib.Widget'.
```

An error message that looks like nonsense until you know about load contexts. It means you have the same type loaded twice from two contexts, and the runtime correctly considers them unrelated.

The practical rules:

- **`Load()`** — the default. Use it whenever you can.
- **`LoadFrom()`** — plugin scenarios where you must load from an arbitrary directory. Note the first disadvantage carefully: if an assembly with that name is *already* loaded, your path is silently ignored and you get the existing one.
- **`LoadFile()`** — loads with essentially no context, bypassing the resolution rules entirely. It won't even resolve the target's own dependencies automatically. This is why Sonar flags it, and why the project suppresses the warning explicitly rather than pretending it isn't a problem.
- **`ReflectionOnlyLoad()`** — inspect metadata without running any code from the assembly. The right choice for a tool that examines untrusted DLLs, since nothing in the loaded assembly can execute.

> **Note for modern .NET:** load contexts were redesigned in .NET Core and later. `AppDomain` is gone, and `AssemblyLoadContext` replaces this whole model with something explicit and collectible. `GlobalAssemblyCache` always returns `false`, and `CodeBase` / `SecurityRuleSet` are obsolete. The concepts transfer; the specific API surface here is .NET Framework.

### Creating Objects From a Loaded Assembly

`InstantiateAssembly()` closes the loop — going from an assembly you loaded by name to a live object:

```csharp
var sharedLib = Assembly.Load("CSharp.SharedLibrary");

// Note: An invalid class name passed to CreateInstance() will not throw an exception
//       - it will just return NULL
var item = (Item)sharedLib.CreateInstance("CSharp.SharedLibrary.Models.Item")
		   ?? throw new DatabankException("Error creating Item object!");
item.Name = "My item";
```

Two things to internalize:

**The name must be fully qualified with its namespace.** `"Item"` alone will not work; it must be `"CSharp.SharedLibrary.Models.Item"`.

**A typo returns `null`, it does not throw.** This is exactly the compile-time-safety loss described earlier, in its most concrete form. The `?? throw` is not defensive padding — it is the only thing standing between a misspelled string and a `NullReferenceException` somewhere far away from the actual mistake. Any time you call `CreateInstance()`, `GetMethod()`, or `GetProperty()`, assume the result can be `null` and handle it at the call site.

---

## Part 2: Type

From the Chapter Notes:

```
Type is the entry point into reflection for any given .NET type, obtained via typeof(SomeType),
  an instance's own .GetType(), or by looking it up by name from an Assembly. From a Type, you
  can reach every other kind of reflection metadata: constructors, fields, properties, methods,
  and (for enums specifically) the set of named values.
```

The supporting metadata classes all hang off of `Type`:

```
- EventInfo          Metadata about an event in a class
- FieldInfo          Metadata about a specific member field in a class
- MemberInfo         Metadata about any member of a class
- MethodInfo         Metadata about a class method
- Module             Metadata about the DLL or EXE file containing the Assembly
- ParameterInfo      Metadata about defined method parameters
- PropertyInfo       Metadata about a class property
```

### Getting a Type

`TypeExample()` shows both routes:

```csharp
var intType = typeof(int);      // compile-time: I know the type, give me its metadata
Console.WriteLine($"Found type [{intType.Name}]");

int x = 1;
intType = x.GetType();          // runtime: I have an object, what IS it?
```

The distinction matters more than it first appears. `typeof(T)` is resolved at compile time and reflects the **declared** type. `obj.GetType()` is resolved at runtime and reflects the **actual** type. For a `Person p = new Student()`, `typeof(Person)` and `p.GetType()` give different answers — the latter returns `Student`.

`TypeDetails()` then prints the descriptive properties: `Name`, `Namespace`, `Assembly`, `AssemblyQualifiedName`, `FullName`, `IsValueType`. Running it against `int` is instructive — `Name` is `Int32`, not `int`. `int` is a C# language alias; the CLR only knows `System.Int32`.

### Constructors

`ExamineConstructors()` reflects over `Person`, which has three overloads:

```csharp
foreach (var ctor in personType.GetConstructors())
{
	string parameterList = string.Join(", ",
		ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
	Console.WriteLine($" - {personType.Name}({parameterList})");
}
```

Note that `ParameterInfo` preserves **parameter names**, not just types. That metadata survives compilation, which is what makes named-argument binding, DI container resolution, and model binding in ASP.NET possible.

### Enums

`ExamineEnum()` uses the enum-specific members against `Degree`:

```csharp
foreach (var name in degreeType.GetEnumNames()) Console.WriteLine($" - {name}");

foreach (var value in degreeType.GetEnumValues()) Console.WriteLine($" - {(int)value}: {value}");
```

The `(int)value` cast is the interesting part. `GetEnumValues()` returns the values boxed as `object`, so printing `value` alone gives the *name* (because `Enum.ToString()` does the lookup), while casting to `int` reveals the underlying numeric value. Printing both side by side shows the pairing — `Associates` is `0`, `Doctorate` is `3` — since `Degree` declares no explicit values and defaults to sequential numbering from zero.

### Fields and `BindingFlags`

`ExamineFields()` is where the chapter introduces the single most important concept in the `Type` API:

```csharp
// By default, GetFields() only returns PUBLIC fields. Course's own data is exposed
//   entirely through auto-properties (which are backed by hidden compiler-generated
//   fields, not the same thing as a field YOU declared), so this comes back empty.
Console.WriteLine($"Public fields on {courseType.Name}: {courseType.GetFields().Length}");
```

That prints **0**, which surprises people. `Course` has `Name` and `RawGrade`, but those are *properties*. Their compiler-generated backing fields exist, but they're private and have unspeakable names like `<Name>k__BackingField`.

To see anything else, you must ask explicitly:

```csharp
#pragma warning disable S3011 // For the lesson, we are intentionally using reflection to access
							 // non-public members, which is normally discouraged.
foreach (var field in courseType.GetFields(
	BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
```

This picks up `gradeCriteria`, the private hand-declared `Dictionary<string, double>`.

**The rule that catches everyone:** the moment you pass `BindingFlags` explicitly, you replace the defaults entirely — you do not add to them. You must specify at least one of `Public`/`NonPublic` **and** at least one of `Instance`/`Static`, or you get an empty result with no error. Forgetting `Instance` is the classic version of this bug.

The `S3011` suppression is again worth pausing on. Reading private state through reflection breaks encapsulation deliberately: you are reaching past a boundary the author established, and nothing obligates them to keep that field stable. It's the right tool for a debugger, a serializer, or a test harness — and the wrong tool for ordinary application logic.

### Properties and Inheritance

`ExamineProperties()` reflects over `Student`:

```csharp
foreach (var property in studentType.GetProperties())
{
	Console.WriteLine($" - {property.PropertyType.Name} {property.Name} " +
					  $"(declared on {property.DeclaringType?.Name})");
}
```

Unlike fields, `GetProperties()` **does** walk the inheritance chain by default. Printing `DeclaringType` alongside each name makes the hierarchy visible — you'll see properties declared on `Person` and `Employee` showing up on `Student`.

This asymmetry is worth remembering: public property and method lookups traverse base types, but **non-public members are never inherited into a reflection query.** If you need a private field from a base class, you have to walk up `Type.BaseType` yourself.

### Methods, and Actually Calling One

`ExamineMethods()` is the payoff of the whole `Type` section. First, narrowing the result set:

```csharp
// DeclaredOnly limits this to methods TeachingAssistant itself defines, not everything
//   it inherits from Faculty/Employee/Person or gets from IStudent.
foreach (var method in taType.GetMethods(
	BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
```

Without `DeclaredOnly` you'd also get everything from `Faculty`, `Employee`, `Person`, and `object` itself — including the property accessors, which appear as `get_Name` / `set_Name` methods.

Then the real demonstration:

```csharp
// Now the real payoff: actually calling a method purely through its MethodInfo,
//   with no compile-time reference to TeachingAssistant.Credentials() at all.
var ta = new TeachingAssistant { FirstName = "Alex", LastName = "Rivera", Degree = Degree.Masters };
var credentialsMethod = taType.GetMethod("Credentials");
var result = credentialsMethod?.Invoke(ta, null);
```

`Invoke(target, parameters)` takes the instance to call on (`null` for a static method) and an `object[]` of arguments (`null` for none). It returns `object`, so a non-void return needs casting.

This is the mechanism behind an enormous amount of infrastructure you already use: test runners finding `[TestMethod]`s, JSON serializers populating properties, DI containers selecting constructors, ORMs materializing entities.

Two hazards to know:

- **The `?.` is load-bearing.** `GetMethod("Credentials")` returns `null` on a typo — no exception, no warning.
- **Exceptions get wrapped.** If the invoked method throws, `Invoke` wraps it in a `TargetInvocationException`. The real exception is in `.InnerException`. Catching for the original type directly will not match.

### Array Rank

`ExamineArrayRank()` is a small aside showing that arrays carry their dimensionality in metadata:

```csharp
var oneDimensional   = new int[5];
var twoDimensional   = new int[3, 4];
var threeDimensional = new int[2, 2, 2];
// GetArrayRank() returns 1, 2, and 3 respectively
```

Rank is the number of dimensions, independent of length. Note this is about *rectangular* arrays (`int[3,4]`) — a jagged array `int[][]` has rank 1, because it's an array whose elements happen to also be arrays.

---

## Part 3: Custom Attributes

From the Chapter Notes:

```
An attribute attaches declarative metadata to code (a class, method, property, etc.) that can be
  read back at runtime via reflection. Unlike a comment, an attribute is real, structured data the
  compiler embeds into the assembly, code that never even instantiates your class can still ask
  "does this type have a CourseCatalogAttribute, and if so, what department does it say?"
```

That contrast with comments is the whole idea. A comment is discarded at compile time and can only be read by humans. An attribute is preserved in the assembly's metadata and can be read by *code*.

### Defining One

`Models/Attributes/CourseCatalogAttribute.cs`:

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

Three conventions are at work:

**Inherit from `Attribute`.** That's what makes it an attribute rather than an ordinary class.

**The `Attribute` suffix is dropped at the usage site.** The class is `CourseCatalogAttribute`, but it's applied as `[CourseCatalog(...)]`. The compiler tries both spellings.

**`[AttributeUsage]` constrains your attribute** — itself an attribute applied to an attribute:
- `AttributeTargets.Class` — applying this to a method or property is now a *compile error*, not a runtime surprise.
- `Inherited = false` — a class deriving from `Course` will not report having this attribute.
- `AllowMultiple = false` — it can be applied only once per target.

Also note the properties are **get-only**, set through the constructor. Attribute values are baked into metadata at compile time; they must be compile-time constants. This is why you can't pass a computed value or a `new` object into an attribute.

### Reading One

`ReadCourseCatalogAttribute()`:

```csharp
// GetCustomAttribute<T>() returns null if the attribute isn't present, rather than throwing
var catalogAttribute = courseType.GetCustomAttribute<CourseCatalogAttribute>();

if (catalogAttribute != null)
{
	Console.WriteLine($"{courseType.Name} is cataloged under {catalogAttribute.Department}, " +
					  $"{catalogAttribute.CreditHours} credit hour(s).");
}
```

Against `Course`, which is decorated `[CourseCatalog("Computer Science", 3)]`, this prints the department and credit hours.

The key insight: **`GetCustomAttribute<T>()` instantiates the attribute object.** The constructor arguments written in the source were stored in metadata, and reading the attribute constructs a real `CourseCatalogAttribute` from them. Attributes are lazily created on read, not held in memory alongside the type.

The `null` check follows the same rule as every other reflection lookup — absence is reported as `null`, never as an exception. The `else` branch here isn't padding; use `AllowMultiple = true` types with `GetCustomAttributes<T>()` (plural) instead, which returns an empty collection rather than `null`.

Attributes are the foundation of declarative programming in .NET: `[Serializable]`, `[Obsolete]`, `[TestMethod]`, `[Required]`, `[JsonProperty]`, `[HttpGet]`. `Supplemental.01.CustomAttributes` goes considerably deeper.

---

## Part 4: The CodeDOM

From the Chapter Notes:

```
A pre-Roslyn, source-language-agnostic way to represent and generate source code (C#, VB.NET,
  etc.) as an object graph, then render that graph as actual source text. It predates the modern
  Roslyn compiler APIs and is far less commonly used today, but it's still part of the .NET
  Framework BCL and this chapter's official curriculum, so we cover a minimal, real example.
```

That's an honest framing. This is legacy technology, included because it's on the exam objectives and still present in the BCL — not because you should reach for it in new work.

The "source-language-agnostic" part is the actual idea. You build one object graph describing a class, then hand it to a `CSharpCodeProvider` or a `VBCodeProvider` and get valid C# or valid VB.NET out of the *same* graph. That was genuinely valuable in the pre-Roslyn era, when designers had to emit code for multiple languages.

### `GenerateCodeWithCodeDom()`

The structure mirrors the shape of a source file. Start with a compile unit and namespace:

```csharp
var compileUnit = new CodeCompileUnit();

var codeNamespace = new CodeNamespace("GeneratedCode");
codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
compileUnit.Namespaces.Add(codeNamespace);
```

Add a class:

```csharp
var classDeclaration = new CodeTypeDeclaration("Greeter")
{
	IsClass = true,
	TypeAttributes = TypeAttributes.Public
};
codeNamespace.Types.Add(classDeclaration);
```

Note `TypeAttributes` comes from `System.Reflection` — the same enum used to describe *existing* types is reused to specify a type being *generated*.

Then a private field, a property wrapping it, and a method:

```csharp
var nameField = new CodeMemberField(typeof(string), "_name") { Attributes = MemberAttributes.Private };

var nameProperty = new CodeMemberProperty
{
	Name = "Name",
	Type = new CodeTypeReference(typeof(string)),
	Attributes = MemberAttributes.Public,
	HasGet = true,
	HasSet = true
};
nameProperty.GetStatements.Add(new CodeMethodReturnStatement(
	new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_name")));
nameProperty.SetStatements.Add(new CodeAssignStatement(
	new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_name"),
	new CodePropertySetValueReferenceExpression()));
```

This is where the CodeDOM's verbosity becomes obvious. `return _name;` requires three nested objects: a return statement, wrapping a field reference, wrapping a `this` reference. `CodePropertySetValueReferenceExpression` is the contextual `value` keyword inside a setter.

The `Greet()` method shows expression composition:

```csharp
greetMethod.Statements.Add(new CodeMethodReturnStatement(
	new CodeBinaryOperatorExpression(
		new CodePrimitiveExpression("Hello, "),
		CodeBinaryOperatorType.Add,
		new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_name"))));
```

That entire tree expresses `return "Hello, " + _name;`.

Finally, render the graph to text:

```csharp
using var provider = new CSharpCodeProvider();
using var writer = new StringWriter();
provider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions { BracingStyle = "C" });

Console.WriteLine("Generated source code:");
Console.WriteLine(writer.ToString());
```

`BracingStyle = "C"` puts the opening brace on its own line, matching normal C# convention; the default puts it on the same line.

Swapping `CSharpCodeProvider` for `VBCodeProvider` on that one line — with no other change — emits the equivalent VB.NET. That substitutability is the entire point of the abstraction.

Note that this method only *generates* source text. `Supplemental.03.CodeDomCompileAndRun` takes the next step: compiling generated source into a live in-memory assembly and executing it, which is where CodeDOM and reflection meet.

> **Modern equivalent:** for new work, use Roslyn (`Microsoft.CodeAnalysis.CSharp`) for analysis and generation, or a **Source Generator** for compile-time code generation. Both are dramatically more capable, and both understand C# semantics rather than just its syntax tree shape. CodeDOM has no support for any C# feature added after roughly 2005 — no generics-heavy constructs, no `async`, no pattern matching.

---

## Part 5: Lambda Expressions (Deliberately Brief)

`LambdaExpressionsRecap()` prints three lines pointing back to Chapter 6, and the Chapter Notes explain why:

```
Delegates, anonymous methods, and lambda expressions were already covered in depth back in
  Chapter 6 (see CSharp.Ch06.DelegatesEventsAndExceptions and its Supplemental projects), this
  chapter's own treatment of the topic is intentionally brief, see LambdaExpressionsRecap()
  below for a short pointer back rather than a re-teaching.
```

The textbook groups lambdas into this chapter, but the material was covered more thoroughly in Chapter 6 — particularly `Supplemental.01.NamedVersusAnonymousDelegates` and `Supplemental.02.LambdaExpressions`. Rather than a weaker second pass, this is a cross-reference.

There *is* a genuine connection between lambdas and reflection worth knowing, though it's beyond this chapter's scope: `Expression<Func<T>>` represents a lambda as an inspectable **expression tree** rather than compiled IL. That's how LINQ providers translate C# into SQL, and how strongly-typed member references like `nameof`-style helpers avoid magic strings. Chapter 10's `Supplemental.04.IQueryableVsIEnumerable` covers the practical consequences.

---

## What to Take Away

**Reflection trades performance and safety for flexibility.** Every reflective operation is slower than its direct equivalent and unchecked by the compiler. That trade is worth it for frameworks, tooling, serializers, and plugin hosts — and rarely worth it inside ordinary application logic.

**Everything reflective returns `null` on a miss, not an exception.** `CreateInstance()`, `GetMethod()`, `GetProperty()`, `GetCustomAttribute<T>()` — all of them. Handle it at the call site, as `InstantiateAssembly()` does with `?? throw`.

**`BindingFlags` replaces the defaults, it doesn't extend them.** Specify visibility *and* scope, every time.

**Load context is part of type identity.** The same DLL loaded twice through different mechanisms yields two mutually incompatible sets of types, and the resulting error message will look impossible.

**Attributes are data, not documentation.** They survive compilation and are readable by code that has no other knowledge of your type.

**Prefer the modern tool where one exists.** `AssemblyLoadContext` over `AppDomain` juggling, Roslyn or Source Generators over CodeDOM, and — per `Supplemental.04` — cached delegates over repeated `Invoke()` calls.
