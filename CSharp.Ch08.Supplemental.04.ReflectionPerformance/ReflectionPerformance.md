# Reflection Performance

## Introduction

Every lesson in this chapter has repeated a warning: reflection is resource-intensive. This lesson puts an actual number behind that, timing direct code against equivalent reflection-based code over a million iterations, so the difference is impossible to miss, and more importantly, so you can see exactly *where* the cost actually comes from.

---

## Setting Up a Fair Comparison

```csharp
public class Counter
{
    public int Value { get; set; }
    public void Increment() { Value++; }
}
```

`Counter` does as little work as possible, on purpose. That way, any timing difference measured comes from *how* `Value` and `Increment()` are accessed, not from the cost of whatever they actually do.

---

## Direct Access vs. Reflection

```csharp
// Direct:
counter.Value = i;

// Reflection, with the lookup already done beforehand:
PropertyInfo valueProperty = typeof(Counter).GetProperty("Value");
valueProperty.SetValue(counter, i);
```

Run both a million times, and reflection is measurably slower, `SetValue()` genuinely carries more overhead than a direct property assignment. That's expected. The same holds for calling a method directly versus `MethodInfo.Invoke()`.

---

## The Real Question: Where Does the Slowness Actually Come From?

```csharp
// Cached: look up the property once, reuse it a million times
PropertyInfo cachedProperty = typeof(Counter).GetProperty("Value");
for (int i = 0; i < 1_000_000; i++) cachedProperty.SetValue(counter, i);

// Uncached: look up the property fresh, every single time
for (int i = 0; i < 1_000_000; i++)
{
    typeof(Counter).GetProperty("Value").SetValue(counter, i);
}
```

This is the comparison that matters most. Both versions call `SetValue()` the same number of times, the only difference is whether `GetProperty("Value")` runs once before the loop, or once *inside* the loop, every iteration. The uncached version comes out dramatically slower than even the cached-reflection-vs-direct comparison above. **The lookup, not the call, is where most of reflection's real-world cost hides.**

---

## What This Actually Means for Your Code

If you genuinely need reflection (loading plugins, writing a generic mapper like the one in the Dynamic Invocation lesson, reading configuration once at startup), the single most effective thing you can do is: **look up the `Type`, `PropertyInfo`, or `MethodInfo` once, and hold onto it**, rather than calling `GetProperty()` or `GetMethod()` every time you need it. Storing that lookup in a field, a `static readonly` variable, or a small cache dictionary turns "reflection in a hot loop" from a genuine performance problem into something with a real but manageable cost.

The rule isn't "never use reflection." It's "never repeat the expensive part of reflection when you don't have to."

---

## Try It Yourself

Change `Iterations` from 1,000,000 to 10,000,000 and run the project again. All three comparisons should scale up roughly proportionally, but pay attention to which comparison's *ratio* changes the most, that tells you which gap actually matters most as the workload grows.
