# Chapter 8 Supplemental 04: Reflection Performance

## What This Is

Every project in this chapter has repeated the same warning from the main lesson's Chapter Notes: reflection is resource-intensive. This project is the one that puts an actual number behind that claim, timing direct code against equivalent reflection-based code across a million iterations each, using `Counter`, a class deliberately trivial enough that the timing differences measured come from *how* its property and method are accessed, not from any real work they do.

---

## Direct vs. Reflected, With the Lookup Already Cached

```csharp
var directTimer = Stopwatch.StartNew();
for (int i = 0; i < Iterations; i++) counter.Value = i;
directTimer.Stop();

PropertyInfo valueProperty = typeof(Counter).GetProperty("Value");   // looked up ONCE

var reflectedTimer = Stopwatch.StartNew();
for (int i = 0; i < Iterations; i++) valueProperty.SetValue(counter, i);
reflectedTimer.Stop();
```

Run this and reflection is still measurably slower, `SetValue()`/`Invoke()` themselves carry real overhead compared to a direct call, there's no getting around that entirely. But notice the shape of this specific comparison: the `PropertyInfo` lookup (`GetProperty("Value")`) happens **once**, outside both timed loops. The same structure repeats for `CompareDirectVsReflectedMethodCalls()`, comparing a direct `counter.Increment()` call against a cached `MethodInfo.Invoke()`.

---

## The Real Finding: Lookups, Not Calls, Are What Actually Hurts

```csharp
PropertyInfo cachedProperty = counterType.GetProperty("Value");
var cachedTimer = Stopwatch.StartNew();
for (int i = 0; i < Iterations; i++) cachedProperty.SetValue(counter, i);
cachedTimer.Stop();

var uncachedTimer = Stopwatch.StartNew();
for (int i = 0; i < Iterations; i++)
{
    // GetProperty("Value") runs fresh, every single iteration:
    counterType.GetProperty("Value")?.SetValue(counter, i);
}
uncachedTimer.Stop();
```

This is the comparison worth paying closest attention to. Both loops call `SetValue()` the same number of times, the only difference is whether `GetProperty("Value")` runs once beforehand or once *per iteration*, inside the loop. The uncached version comes out dramatically slower, often by a much larger margin than the direct-vs-cached-reflection gap in the first two comparisons. This is the actual, practical lesson: reflection's real-world cost usually isn't the `Invoke()`/`GetValue()`/`SetValue()` call itself, it's re-doing the `GetType()`/`GetProperty()`/`GetMethod()` lookup over and over inside a hot path, when it could have been done once and reused.

---

## The Practical Takeaway

If reflection is genuinely the right tool (a plugin loader, a generic serializer, the property mapper from `Supplemental.02.DynamicInvocation`), the single highest-impact thing you can do for performance is: **look up the `Type`/`PropertyInfo`/`MethodInfo`/`ConstructorInfo` once, and cache it**, a static readonly field, a dictionary keyed by type, whatever fits the situation, rather than calling `GetProperty()`/`GetMethod()` fresh every time you need it. That single habit closes most of the gap between "reflection is unacceptably slow" and "reflection has a real but manageable cost", it doesn't eliminate reflection's overhead entirely, but it eliminates the *avoidable* part of it, which in practice tends to be the larger part.
