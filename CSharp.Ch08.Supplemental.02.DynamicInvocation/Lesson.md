# Chapter 8 Supplemental 02: Dynamic Object Creation and Invocation

## What This Is

The main lesson's `MethodInfo.Invoke()` example called a no-argument method, `TeachingAssistant.Credentials()`. That demonstrates the mechanism but not the point — nobody uses reflection to call a method they could have called directly.

This project rounds it out into the shape reflection actually takes in real code:

1. **Creating objects** without `new`
2. **Reading and writing properties** by name
3. **Calling methods** with real arguments
4. **The payoff** — a small, genuinely reusable property-mapping utility built entirely from those pieces

That fourth item is the answer to "why would real code ever want this."

---

## `Activator.CreateInstance()`: Three Shapes

```csharp
var viaGeneric = Activator.CreateInstance<Product>();

var productType = typeof(Product);
var viaType = (Product)Activator.CreateInstance(productType);

var viaTypeWithArgs = (Product)Activator.CreateInstance(productType, 1, "Widget", 9.99m);
```

### The Generic Form Is Almost Never What You Want

```csharp
// Generic form: you know the type at compile time. Rarely actually needed, since
//   "new Product()" does the same thing more simply, shown here for comparison.
var viaGeneric = Activator.CreateInstance<Product>();
```

The source comment is blunt about this, and it's correct. If you know the type well enough at compile time to write `Activator.CreateInstance<Product>()`, you could have written `new Product()` — simpler, faster, and checked by the compiler.

It's shown here for contrast, not as a recommendation. (The one place the generic form legitimately appears is inside a generic method with a `where T : new()` constraint, where `T` isn't known to the author but is known to the caller.)

### The `Type`-Based Form Is the Real One

```csharp
// Non-generic form: you only have a Type object, the genuinely common case, e.g.
//   when the type was itself looked up by name (see the main lesson's
//   Assembly.CreateInstance(), which works similarly).
var viaType = (Product)Activator.CreateInstance(productType);
```

This is the one that matters, because it's the one you *can't* replace with `new`. You reach for it when the type itself was discovered at runtime — looked up by name from a config file, loaded from a plugin assembly, or selected by a factory based on incoming data.

In this demo `productType` comes from `typeof(Product)`, which is admittedly a compile-time source. That's for clarity. In real use it would come from `assembly.GetType(nameFromConfig)` or a scan of `assembly.GetTypes()`.

### With Constructor Arguments

```csharp
// With constructor arguments: selects and calls the matching constructor overload.
var viaTypeWithArgs = (Product)Activator.CreateInstance(productType, 1, "Widget", 9.99m);
```

This connects back to `ExamineConstructors()` in the main lesson. Having seen that a `Type` exposes its constructor overloads and their parameter lists, this is the natural consequence: .NET matches your argument list against the available overloads and calls the one that fits. `Product` has both a parameterless constructor and `Product(int, string, decimal)`, and the three arguments here select the latter.

Two failure modes to know:

- **No parameterless constructor** and you call the no-args overload anyway → `MissingMethodException`. This bites when adding a constructor to a class that a serializer or DI container instantiates, since declaring any constructor removes the implicit default one.
- **No overload matches** your argument types → `MissingMethodException` as well. Overload resolution here happens at runtime with no compiler assistance.

### Relationship to `Assembly.CreateInstance()`

The main lesson used `sharedLib.CreateInstance("CSharp.SharedLibrary.Models.Item")`. The difference is worth keeping straight:

- **`Assembly.CreateInstance(string)`** — you have an assembly and a type *name*. Returns `null` if the name doesn't resolve.
- **`Activator.CreateInstance(Type)`** — you already have a resolved `Type` object. **Throws** rather than returning `null` on failure.

That inconsistency is a genuine trap. `Activator` is one of the few reflection APIs that does *not* follow the "return null on miss" rule described in the main lesson.

---

## `PropertyInfo.GetValue()` / `SetValue()`

The main lesson's `ExamineProperties()` only ever *listed* properties. This is the other half — actually reading and writing values:

```csharp
// Set properties purely by name, as strings, no compile-time reference to
//   Product.Name or Product.Price anywhere in this method.
productType.GetProperty("Name")?.SetValue(product, "Gadget");
productType.GetProperty("Price")?.SetValue(product, 24.99m);

// Read them back the same way
var name = productType.GetProperty("Name")?.GetValue(product);
var price = productType.GetProperty("Price")?.GetValue(product);
```

The signatures follow the same shape as `MethodInfo.Invoke()`: the first argument is the **instance** to operate on (`null` for a static property).

### It Mutates the Real Object

The demo makes a point of proving this:

```csharp
Console.WriteLine($"Set via reflection, then read back via reflection: Name={name}, Price={price:C}");

// Confirm this actually changed the real object, not just some reflection-only copy
Console.WriteLine($"Same values via a normal compile-time reference: Name={product.Name}, Price={product.Price:C}");
```

Both lines print the same values. There is no separate "reflection copy" of the object — `SetValue()` calls the property's real setter, including any validation or side effects inside it. A property that throws on invalid input will throw here too (wrapped in `TargetInvocationException`, as always).

### Things That Bite

**`GetValue()` returns `object`.** Value types get boxed. Note the demo's `{price:C}` still formats as currency — that works because `string.Format` checks for `IFormattable` at runtime, which the boxed `decimal` still implements. Assigning to a typed variable requires an explicit cast.

**`SetValue()` is not type-checked at compile time.** Passing a `string` where the property expects a `decimal` compiles fine and throws `ArgumentException` at runtime.

**Read-only properties fail.** A property with no setter throws on `SetValue()`. Check `CanWrite` first — which is exactly what the mapper below does.

**The `?.` is load-bearing again.** `GetProperty("Naem")` returns `null`, and with `?.` the whole statement silently does nothing. That's arguably worse than throwing, since a typo produces no output at all. In production code, prefer an explicit null check with a real error message.

---

## `MethodInfo.Invoke()` With Real Arguments

```csharp
var applyDiscountMethod = productType.GetMethod("ApplyDiscount");

// Unlike the main lesson's parameterless Invoke(ta, null), this passes a real
//   argument array, one entry per parameter, in declaration order.
var discountedPrice = applyDiscountMethod?.Invoke(product, [0.25m]);
```

The main lesson passed `null` for the argument array because `Credentials()` takes no parameters. `ApplyDiscount(decimal percentage)` takes one, so the array must match its parameter list: **one entry per parameter, in declaration order, boxed into `object`.**

(Note the `[0.25m]` collection expression syntax — a modern C# shorthand for `new object[] { 0.25m }`.)

The demo also confirms the method's semantics:

```csharp
Console.WriteLine($"(original Price is unchanged, ApplyDiscount() returns a new value rather than mutating): {product.Price:C}");
```

`ApplyDiscount` computes `Price * (1 - percentage)` and returns it without touching `Price`. Invoking through reflection doesn't change that — reflection calls the method exactly as written.

### The Tradeoff, Stated Plainly

The lecture notes put it well:

> Get the count or types wrong and `Invoke()` throws `TargetParameterCountException` or a similar mismatch error at runtime. This is exactly the tradeoff reflection makes: the compiler can no longer catch a mismatched call for you, since it never sees the call as a call at all, just a runtime lookup.

That last clause is the precise formulation. To the compiler, `Invoke(product, [0.25m])` is a call to `Invoke` — a method taking an `object` and an `object[]`. Both arguments are valid. The compiler has no idea that a *different* method is about to be called with them.

Also worth remembering from the main lesson: `ApplyDiscount` throws `ArgumentOutOfRangeException` for a percentage outside 0–1. Invoked reflectively, that surfaces as a `TargetInvocationException` with the real exception in `.InnerException`. A `catch (ArgumentOutOfRangeException)` around this call will **not** match.

---

## The Payoff: A Real, Reusable Property Mapper

Everything above is mechanism. `PropertyMapper` is the justification:

```csharp
public static void CopyMatchingProperties(object source, object destination)
{
	var sourceProperties = source.GetType().GetProperties();
	var destinationProperties = destination.GetType().GetProperties();

	foreach (var sourceProperty in sourceProperties)
	{
		// Only copy a property when the destination has a property with the SAME
		//   name, the SAME type, and a public setter.
		var destinationProperty = destinationProperties.FirstOrDefault(p =>
			p.Name == sourceProperty.Name &&
			p.PropertyType == sourceProperty.PropertyType &&
			p.CanWrite);

		if (destinationProperty == null) continue;

		object value = sourceProperty.GetValue(source);
		destinationProperty.SetValue(destination, value);
	}
}
```

Note the signature: both parameters are `object`. **This method has no compile-time knowledge of either type**, yet it does real work on both. That is something you simply cannot write without reflection.

This is a simplified version of what AutoMapper and similar libraries do internally.

### The Three-Part Match

The matching predicate is doing careful work, and each clause earns its place:

- **`p.Name == sourceProperty.Name`** — the obvious one. Note this is case-sensitive and exact; a real mapper usually supports configurable naming conventions.
- **`p.PropertyType == sourceProperty.PropertyType`** — prevents copying an `int Id` onto a `string Id`, which would throw. Note this demands *exact* type equality — no implicit conversions, no assignable-to. An `int` source will not populate a `long` destination.
- **`p.CanWrite`** — skips read-only destination properties instead of throwing on them.

Anything that fails the match is **silently skipped**, not treated as an error. That's the right call for a mapper: the whole point is tolerating shape mismatches between types.

### What the Demo Proves

`Product` has `Id`, `Name`, `Price`. `ProductDto` has those three **plus** `Source`, which defaults to `"Unknown"` and has no counterpart on `Product`:

```csharp
var product = new Product(3, "Thingamajig", 49.99m);
var dto = new ProductDto { Source = "Imported from legacy system" };

PropertyMapper.CopyMatchingProperties(product, dto);
```

After mapping, `Id`, `Name`, and `Price` carry `Product`'s values, and `Source` still reads `"Imported from legacy system"`:

```csharp
Console.WriteLine($"Note: dto.Source was left alone, Product has no Source property to copy from.");
```

That's the design being verified, not just demonstrated. A mapper that blanked out unmatched destination properties would destroy data.

### Honest Limitations

Understanding where this simplified version falls short is as valuable as the version itself:

- **Shallow copy.** Reference-typed properties are copied by reference, so both objects then share the same instance.
- **No conversion.** `int` → `long` or `int` → `string` won't map, despite being safe.
- **No nested mapping.** A `Product.Category` won't map to a `ProductDto.CategoryDto`.
- **Slow.** `GetProperties()` runs on every single call, with no caching. `Supplemental.04.ReflectionPerformance` measures exactly this cost and shows how to eliminate most of it with cached delegates.

That last point is the one to carry forward. This mapper is a fine illustration and a reasonable tool for a few hundred objects. Run it in a loop over a million rows and the reflection overhead dominates everything else.

---

## What to Take Away

**`Activator.CreateInstance(Type)` is the form that matters.** The generic version is nearly always just a slower `new`. Note that `Activator` **throws** on failure rather than returning `null` — an exception to the usual reflection convention.

**`GetValue()`/`SetValue()` operate on the real object.** They invoke the actual accessors, with all their validation and side effects. `GetValue()` returns `object`, so value types are boxed.

**The argument array must match the parameter list exactly** — count, order, and type. The compiler cannot help you, because it doesn't see a method call at all.

**Reflection earns its cost in type-agnostic utilities.** `PropertyMapper` is the concrete case: a genuinely useful function that cannot be written any other way. That's the standard to apply before reaching for reflection — not "can I do this with reflection," but "is there any alternative."
