# Chapter 8 Supplemental 02: Dynamic Invocation

## What This Is

The main lesson demonstrated one narrow slice of dynamic invocation: calling a no-argument method by name. This project rounds that out into the shape reflection actually takes in real code -- creating objects dynamically, reading and writing their properties by name, calling methods with real arguments, and a small reusable utility built entirely from these pieces.

The models are already in the project. `Product` has three properties (`Id`, `Name`, `Price`), a parameterless constructor, a three-argument constructor, and an `ApplyDiscount(decimal)` method. `ProductDto` has the same three properties plus a `Source` property that `Product` doesn't have. `PropertyMapper` is the reusable utility -- read it before running the last mini-program.

---

## How to Write This Program

### Mini-Program 1: Activator.CreateInstance()

Clear `Main()` and write:

```csharp
// Generic form: you know the type at compile time. Rarely needed -- "new Product()" is simpler.
// Shown here for completeness.
var viaGeneric = Activator.CreateInstance<Product>();
Console.WriteLine($"CreateInstance<Product>(): Id={viaGeneric.Id}, Name={viaGeneric.Name ?? "(null)"}");

// Non-generic form: you only have a Type object. This is the actually common case.
var productType = typeof(Product);
var viaType = (Product)Activator.CreateInstance(productType);
Console.WriteLine($"CreateInstance(typeof(Product)): Id={viaType.Id}, Name={viaType.Name ?? "(null)"}");

// With constructor arguments: matches and calls the appropriate constructor overload.
var viaTypeWithArgs = (Product)Activator.CreateInstance(productType, 1, "Widget", 9.99m);
Console.WriteLine($"CreateInstance(..., 1, \"Widget\", 9.99m): " +
    $"Id={viaTypeWithArgs.Id}, Name={viaTypeWithArgs.Name}, Price={viaTypeWithArgs.Price:C}");

GenericFunctions.Pause();
```

Run it. Three products created three ways.

The generic form is mostly a curiosity -- when you can write `Activator.CreateInstance<Product>()` you can also write `new Product()`, and `new` is faster and clearer. The non-generic form is where `Activator` earns its place: when the type is only known at runtime, loaded from a plugin assembly or looked up by name, you can't write `new` because you don't know the type name at compile time.

Constructor argument matching with the array form works by trying to find a constructor whose parameter types are compatible with the arguments you pass. If no match is found, `MissingMethodException` is thrown -- not `null`. This is one of the few places the reflection API throws on a miss rather than returning `null`, so no `?? throw` needed here.

### Mini-Program 2: PropertyInfo.GetValue() and SetValue()

Clear `Main()` and write:

```csharp
var product = new Product();
var productType = typeof(Product);

// Set properties by name, with no compile-time reference to Product.Name or Product.Price.
productType.GetProperty("Name")?.SetValue(product, "Gadget");
productType.GetProperty("Price")?.SetValue(product, 24.99m);

// Read them back the same way.
var name  = productType.GetProperty("Name")?.GetValue(product);
var price = productType.GetProperty("Price")?.GetValue(product);
Console.WriteLine($"Set and read via reflection: Name={name}, Price={price:C}");

// Confirm this changed the real object, not some reflection-internal copy.
Console.WriteLine($"Same values via compile-time reference: Name={product.Name}, Price={product.Price:C}");

GenericFunctions.Pause();
```

Run it. Both output lines show the same values -- reflection operates on the actual object, not a snapshot.

`SetValue(target, value)` takes the value as `object`, so you can pass any type. The runtime will try to convert or unbox it; passing the wrong type throws `ArgumentException`. `GetValue(target)` returns `object`, so the caller is responsible for casting -- or living with the dynamic dispatch.

The `?.` on `GetProperty` is load-bearing. A misspelled property name returns `null` and the `?.` prevents the downstream `NullReferenceException`. This is the reflection pattern in miniature: always assume lookups can return null, always handle it at the call site.

### Mini-Program 3: Invoking a Method With Parameters

Clear `Main()` and write:

```csharp
var product = new Product(2, "Widget", 100m);
var productType = typeof(Product);

var applyDiscountMethod = productType.GetMethod("ApplyDiscount");

// Invoke with a real argument array -- one entry per parameter, in declaration order.
// Unlike the main lesson's null (no parameters), this passes actual data.
var discountedPrice = applyDiscountMethod?.Invoke(product, [0.25m]);

Console.WriteLine($"ApplyDiscount(0.25m) via reflection: {discountedPrice:C}");
Console.WriteLine($"Original Price unchanged (method returns a new value): {product.Price:C}");

GenericFunctions.Pause();
```

Run it. $75.00 discounted price, $100.00 original unchanged.

`Invoke(target, object[] args)` maps argument positions to parameter positions. Pass arguments in the same order they're declared. The return value comes back as `object`; cast it to use it as the actual type.

If `ApplyDiscount` throws -- say you passed `1.5m` -- `Invoke` wraps the original exception in `TargetInvocationException`. The real exception is in `.InnerException`. Catching `ArgumentOutOfRangeException` directly won't match; you'd catch `TargetInvocationException` and inspect `InnerException`. That wrapping catches people every time.

### Mini-Program 4: The Property Mapper

Read `HelperClasses/PropertyMapper.cs` before running this. The mapper loops over the source's properties, finds matching properties on the destination (same name, same type, has a public setter), and copies the values. It's a simplified version of what AutoMapper does under the hood.

Clear `Main()` and write:

```csharp
var product = new Product(3, "Thingamajig", 49.99m);
var dto = new ProductDto { Source = "Imported from legacy system" };

Console.WriteLine("Before mapping:");
Console.WriteLine($" - dto.Id={dto.Id}, dto.Name={dto.Name ?? "(null)"}, " +
    $"dto.Price={dto.Price:C}, dto.Source={dto.Source}");

PropertyMapper.CopyMatchingProperties(product, dto);

Console.WriteLine($"\nAfter mapping:");
Console.WriteLine($" - dto.Id={dto.Id}, dto.Name={dto.Name}, " +
    $"dto.Price={dto.Price:C}, dto.Source={dto.Source}");

Console.WriteLine("\ndto.Source was left alone -- Product has no Source property to copy from.");

GenericFunctions.Pause();
```

Run it. `Id`, `Name`, and `Price` are copied. `Source` is untouched.

`PropertyMapper.CopyMatchingProperties` works on any two objects -- no interfaces required, no attributes needed, no configuration. The cost is that it's purely runtime: rename `Product.Name` to `Product.Title` and the mapper silently copies nothing for that property, with no compiler warning. That tradeoff -- flexibility versus safety -- is the central tension of reflection-based code.

This is also the answer to the "why would I ever use this" question that comes up every time reflection is introduced. The answer is: when the mapping between two types is not known at compile time, or when you'd otherwise write hundreds of lines of `dto.X = source.X` by hand across dozens of types. AutoMapper, Entity Framework, JSON serializers, and DI containers all rely on variations of this pattern.

---

## Takeaways

- `Activator.CreateInstance<T>()` is rarely needed -- use `new T()` when you know the type.
- `Activator.CreateInstance(Type)` is the common form when you only have a `Type` at runtime.
- Constructor argument matching throws `MissingMethodException` on no match, not `null`.
- `SetValue` and `GetValue` operate on the actual object. The `?.` guard is essential -- wrong property names return `null`.
- `Invoke` argument arrays map to parameters by position, in declaration order.
- Exceptions thrown inside `Invoke` are wrapped in `TargetInvocationException`. The real exception is in `.InnerException`.
- Reflection-based property mapping trades compile-time safety for runtime flexibility. Both costs and benefits are real.
