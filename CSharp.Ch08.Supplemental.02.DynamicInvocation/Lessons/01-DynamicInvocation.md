---
title: "Activator, GetValue/SetValue, Invoke With Arguments"
chapter: 8
index: 1
dependencies: []
---

```csharp
using System;
using System.Reflection;

public class Product
{
    public int     Id    { get; set; }
    public string  Name  { get; set; }
    public decimal Price { get; set; }

    public Product() { }
    public Product(int id, string name, decimal price) { Id = id; Name = name; Price = price; }

    public decimal ApplyDiscount(decimal pct) => Price * (1 - pct);
}

internal static class Program
{
    private static void Main()
    {
        // --- Activator.CreateInstance ---
        // Generic form (rarely needed -- "new Product()" is simpler when you know the type):
        var p1 = Activator.CreateInstance<Product>();
        Console.WriteLine($"Generic CreateInstance: Id={p1.Id}, Name={p1.Name ?? "(null)"}");

        // Non-generic form -- the common case when you only have a Type object at runtime:
        var productType = typeof(Product);
        var p2 = (Product)Activator.CreateInstance(productType);
        Console.WriteLine($"CreateInstance(Type):   Id={p2.Id}, Name={p2.Name ?? "(null)"}");

        // With constructor arguments -- throws MissingMethodException on no match (not null):
        var p3 = (Product)Activator.CreateInstance(productType, 1, "Widget", 9.99m);
        Console.WriteLine($"CreateInstance(args):   Id={p3.Id}, Name={p3.Name}, Price={p3.Price:C}");
        Console.WriteLine();

        // --- PropertyInfo.GetValue / SetValue ---
        // SetValue and GetValue operate on the ACTUAL object, not a copy.
        // GetProperty returns null on a typo -- the ?. is load-bearing.
        var p4 = new Product();
        productType.GetProperty("Name") ?.SetValue(p4, "Gadget");
        productType.GetProperty("Price")?.SetValue(p4, 24.99m);

        var name  = productType.GetProperty("Name") ?.GetValue(p4);
        var price = productType.GetProperty("Price")?.GetValue(p4);
        Console.WriteLine($"Set/get via reflection: Name={name}, Price={price:C}");
        Console.WriteLine($"Compile-time confirm:   Name={p4.Name}, Price={p4.Price:C}");
        Console.WriteLine();

        // --- MethodInfo.Invoke with real arguments ---
        // Arguments go in an object[], one entry per parameter, in declaration order.
        // Return value comes back as object -- cast it to use the actual type.
        // If the method throws, Invoke wraps it in TargetInvocationException.
        // The real exception is in .InnerException -- catching the original type directly won't match.
        var p5 = new Product(2, "Sprocket", 100m);
        var discountMethod  = productType.GetMethod("ApplyDiscount");
        var discounted      = discountMethod?.Invoke(p5, new object[] { 0.25m });
        Console.WriteLine($"ApplyDiscount(0.25) via Invoke: {discounted:C}");
        Console.WriteLine($"Original Price unchanged:       {p5.Price:C}");
        Console.WriteLine();

        // --- The property mapper pattern ---
        // Loops over source properties, finds matching ones on destination (same name + type),
        // copies values. This is what AutoMapper, JSON serializers, and ORMs do internally.
        Console.WriteLine("Reflection-based property mapping:");
        var src = new Product(3, "Thingamajig", 49.99m);
        var dst = new Product();
        foreach (var srcProp in typeof(Product).GetProperties())
        {
            var dstProp = typeof(Product).GetProperty(srcProp.Name);
            if (dstProp?.CanWrite == true)
                dstProp.SetValue(dst, srcProp.GetValue(src));
        }
        Console.WriteLine($"Copied: Id={dst.Id}, Name={dst.Name}, Price={dst.Price:C}");
        Console.WriteLine("Trade-off: renaming Product.Name breaks the mapper silently at runtime,");
        Console.WriteLine("with no compiler warning. Flexibility vs. compile-time safety.");
    }
}
```
