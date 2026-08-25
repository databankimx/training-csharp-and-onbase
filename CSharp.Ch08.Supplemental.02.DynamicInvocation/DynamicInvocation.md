# Dynamic Object Creation and Invocation

## Introduction

Reflection isn't just for *looking at* code, it can create objects, read and write their data, and call their methods, all without a compile-time reference to any of it. This lesson builds up to a genuinely useful example: a small utility that copies matching properties between two unrelated objects, entirely at runtime.

---

## Creating Objects Without `new`

```csharp
var viaGeneric = Activator.CreateInstance<Product>();

var productType = typeof(Product);
var viaType = (Product)Activator.CreateInstance(productType);

var viaTypeWithArgs = (Product)Activator.CreateInstance(productType, 1, "Widget", 9.99m);
```

`Activator.CreateInstance()` creates an object from a `Type`, rather than a hardcoded class name in your source code. The generic version (`CreateInstance<Product>()`) is rarely needed in practice, if you already know the type well enough to write that, `new Product()` does the same thing more simply. The version that matters takes a `Type` object instead, useful when the type itself was only determined at runtime, loaded from a plugin, chosen from configuration, looked up by name. Passing extra arguments after the `Type` picks a matching constructor overload and calls it, just like calling that constructor directly would.

---

## Reading and Writing Properties by Name

```csharp
productType.GetProperty("Name")?.SetValue(product, "Gadget");
var name = productType.GetProperty("Name")?.GetValue(product);
```

`SetValue()` and `GetValue()` let you set or read a property purely by its name as a string, no `product.Name` anywhere in the code. This really does change the actual object, not some separate reflection-only view of it, you can confirm that by reading the property back through a normal reference afterward and seeing the same value.

---

## Calling a Method With Real Arguments

```csharp
var applyDiscountMethod = productType.GetMethod("ApplyDiscount");
var discountedPrice = applyDiscountMethod.Invoke(product, new object[] { 0.25m });
```

`Invoke()`'s second argument is an array of values, one per parameter the method expects, in order. Here, `ApplyDiscount(decimal percentage)` takes one `decimal`, so the array has exactly one entry. Get the number or types of arguments wrong, and `Invoke()` throws at runtime rather than the compiler catching the mistake beforehand, that's the real tradeoff reflection makes: flexibility to call things dynamically, at the cost of losing the compiler's usual safety net for that specific call.

---

## Putting It Together: A Reusable Property Mapper

```csharp
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
```

This copies every property that matches by name and type from one object onto another, no matter what those two objects' actual types are. It's a simplified version of what real mapping libraries (like AutoMapper) do internally. Try it with `Product` and `ProductDto`, two similarly-shaped but unrelated classes, `Id`, `Name`, and `Price` copy over correctly, while `ProductDto`'s extra `Source` property (which `Product` doesn't have at all) is left completely untouched, there's nothing on the source object to copy it from.

---

## Try It Yourself

Add a new property to both `Product` and `ProductDto` with the same name and type, and confirm `CopyMatchingProperties()` picks it up automatically, with no changes to the mapper itself. Then try adding a property to just one of the two classes and confirm it's correctly skipped rather than causing an error.
