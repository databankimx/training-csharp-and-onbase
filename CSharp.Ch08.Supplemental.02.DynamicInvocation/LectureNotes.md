# Chapter 8 Supplemental 02: Dynamic Object Creation and Invocation

## What This Is

The main lesson's `MethodInfo.Invoke()` example called a no-argument method, `TeachingAssistant.Credentials()`. This project rounds that out into the shape reflection actually takes in real code: creating objects without `new`, reading and writing properties by name, calling methods with real arguments, and, as the payoff, a small, genuinely reusable property-mapping utility built entirely from these pieces.

---

## `Activator.CreateInstance()`: Three Shapes

```csharp
var viaGeneric = Activator.CreateInstance<Product>();

var productType = typeof(Product);
var viaType = (Product)Activator.CreateInstance(productType);

var viaTypeWithArgs = (Product)Activator.CreateInstance(productType, 1, "Widget", 9.99m);
```

The generic form is rarely actually needed in practice, if you know the type at compile time well enough to write `Activator.CreateInstance<Product>()`, you almost always could have just written `new Product()` instead, simpler and slightly faster. The non-generic, `Type`-based form is the one that matters: it's what you reach for when the type itself was only discovered at runtime (looked up by name, loaded from a plugin assembly, chosen based on configuration). The overload taking constructor arguments works the same way `Type.GetConstructors()` (from the main lesson) suggested it would, .NET matches your argument list against the available constructor overloads and calls the one that fits.

---

## `PropertyInfo.GetValue()`/`SetValue()`

```csharp
productType.GetProperty("Name")?.SetValue(product, "Gadget");
var name = productType.GetProperty("Name")?.GetValue(product);
```

The main lesson's `ExamineProperties()` only ever *listed* properties. This is the other half: actually reading and writing a property's value, purely by name, no compile-time reference to `Product.Name` anywhere in the calling code. Worth confirming for yourself that this genuinely mutates the real object (the demo prints the same value both via reflection and via a normal `product.Name` reference immediately afterward) rather than operating on some reflection-only copy.

---

## `MethodInfo.Invoke()` With Real Arguments

```csharp
var applyDiscountMethod = productType.GetMethod("ApplyDiscount");
var discountedPrice = applyDiscountMethod?.Invoke(product, new object[] { 0.25m });
```

The main lesson's `Invoke(ta, null)` passed `null` for the argument array, since `Credentials()` takes no parameters. Here, `ApplyDiscount(decimal percentage)` takes one, and the argument array has to match its parameter list, one entry per parameter, in declaration order, boxed into `object`. Get the count or types wrong and `Invoke()` throws `TargetParameterCountException` or a similar mismatch error at runtime, this is exactly the tradeoff reflection makes: the compiler can no longer catch a mismatched call for you, since it never sees the call as a call at all, just a runtime lookup.

---

## The Payoff: A Real, Reusable Property Mapper

```csharp
public static class PropertyMapper
{
    public static void CopyMatchingProperties(object source, object destination)
    {
        var sourceProperties = source.GetType().GetProperties();
        var destinationProperties = destination.GetType().GetProperties();

        foreach (var sourceProperty in sourceProperties)
        {
            var destinationProperty = destinationProperties.FirstOrDefault(p =>
                p.Name == sourceProperty.Name &&
                p.PropertyType == sourceProperty.PropertyType &&
                p.CanWrite);

            if (destinationProperty == null) continue;

            object value = sourceProperty.GetValue(source);
            destinationProperty.SetValue(destination, value);
        }
    }
}
```

This is a simplified version of what libraries like AutoMapper actually do under the hood, copy every property that matches by name and type from one object onto another, entirely at runtime, with zero compile-time knowledge of either type's shape. `Product` and `ProductDto` are deliberately similar-but-not-identical: `ProductDto` has an extra `Source` property `Product` doesn't have at all, and the demo confirms `Source` is left untouched by the mapping, since there's nothing on `Product` to copy it from. This is the concrete answer to "why would real code ever want reflection": generic, type-agnostic utilities like this one are exactly the case where reflection earns its resource cost (see `Supplemental.04.ReflectionPerformance` for a direct look at that cost).
