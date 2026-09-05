# Chapter 8 Supplemental 04: Reflection Performance

## What This Is

Every project in this chapter has repeated the same warning from the main lesson's Chapter Notes: reflection is resource-intensive. This project is the one that puts an actual number behind that claim.

It times direct code against equivalent reflection-based code across a million iterations each, using `Counter` — a class deliberately trivial enough that the differences measured come from *how* its property and method are accessed, not from any real work they do.

The finding is more specific and more useful than "reflection is slow":

> The expensive part is usually the **lookup** (`GetProperty()`, `GetMethod()`), not the actual `GetValue()`/`SetValue()`/`Invoke()` call once you already have the `PropertyInfo`/`MethodInfo` in hand.

---

## The Setup

```csharp
private const int Iterations = 1_000_000;
```

A million iterations of an operation that does essentially nothing. That's deliberate — the goal is to isolate access overhead. If `Counter.Increment()` did real work, that work would dominate the timings and hide the effect being measured.

This also means **the ratios reported here are an upper bound, not a typical case.** Real methods do real work, so reflection's relative overhead shrinks as the method being called gets more substantial. A 100x penalty on a method that does nothing is a very different problem from a 100x penalty on a method that queries a database (where it would be unmeasurable).

---

## Comparison 1: Direct vs. Reflected Property Access

```csharp
var directTimer = Stopwatch.StartNew();
for (int i = 0; i < Iterations; i++)
{
	counter.Value = i;
}
directTimer.Stop();

// The PropertyInfo lookup happens ONCE, here, before the timed loop starts.
PropertyInfo valueProperty = typeof(Counter).GetProperty("Value");

var reflectedTimer = Stopwatch.StartNew();
for (int i = 0; i < Iterations; i++)
{
	valueProperty?.SetValue(counter, i);
}
reflectedTimer.Stop();
```

Reflection loses, substantially. `SetValue()` carries real overhead that a direct assignment does not — there's no getting around that entirely.

But note the structure carefully: **the `GetProperty("Value")` lookup happens once, outside both timed loops.** This comparison is deliberately generous to reflection. It measures the cost of `SetValue()` alone, with the lookup already paid for.

Where does that overhead come from? A direct `counter.Value = i` compiles to a property setter call the JIT will very likely inline into a single field write. `SetValue()` cannot be inlined — it must validate the target object's type, check accessibility, **box** the `int` argument into an `object`, verify the boxed value is assignable to the property type, and then dispatch to the setter through an indirection.

That boxing is worth noting on its own. A million `SetValue()` calls with an `int` means a million heap allocations, and therefore GC pressure that the direct version never creates.

---

## Comparison 2: Direct vs. Reflected Method Calls

```csharp
var directTimer = Stopwatch.StartNew();
for (int i = 0; i < Iterations; i++)
{
	counter.Increment();
}
directTimer.Stop();

// Same principle: the MethodInfo lookup happens ONCE, before the timed loop.
MethodInfo incrementMethod = typeof(Counter).GetMethod("Increment");

var reflectedTimer = Stopwatch.StartNew();
for (int i = 0; i < Iterations; i++)
{
	incrementMethod?.Invoke(counter, null);
}
reflectedTimer.Stop();
```

Same structure, same conclusion, applied to method calls instead of property access. `Invoke()` does the same validation-and-dispatch work `SetValue()` does.

Note that `Invoke(counter, null)` passes `null` for arguments, so this case avoids the array allocation a parameterized call would incur. `Supplemental.02`'s `Invoke(product, [0.25m])` allocates an `object[]` **per call** — in a hot loop that's another million allocations on top of the boxing.

---

## Comparison 3: The Real Finding

This is the comparison worth paying closest attention to:

```csharp
PropertyInfo cachedProperty = counterType.GetProperty("Value");

var cachedTimer = Stopwatch.StartNew();
for (int i = 0; i < Iterations; i++)
{
	cachedProperty?.SetValue(counter, i);
}
cachedTimer.Stop();

var uncachedTimer = Stopwatch.StartNew();
for (int i = 0; i < Iterations; i++)
{
	// GetProperty("Value") runs fresh on every single iteration here, this is the
	//   pattern to avoid: doing the expensive lookup inside a hot loop.
	counterType.GetProperty("Value")?.SetValue(counter, i);
}
uncachedTimer.Stop();
```

**Both loops call `SetValue()` exactly the same number of times.** The only difference is whether `GetProperty("Value")` runs once beforehand or once per iteration.

The uncached version comes out dramatically slower — often by a larger margin than the direct-vs-reflection gap in the first two comparisons. That's the headline: **the avoidable cost is bigger than the unavoidable one.**

### Why the Lookup Is So Expensive

`GetProperty("Value")` is not a cheap dictionary hit. It performs a **string-based search** through the type's metadata tables, applying default `BindingFlags` (public, instance, and — as the main lesson covered — walking the inheritance chain for properties). Then it allocates and returns a `PropertyInfo` object describing what it found.

Every single iteration. A million times. Compare that to the cached version, which does it once and reuses one reference.

Note that `MethodInfo` lookups can be worse still, since `GetMethod("Name")` must also perform overload resolution when multiple candidates share a name — and throws `AmbiguousMatchException` if it can't pick one.

---

## The Practical Takeaway

If reflection is genuinely the right tool — a plugin loader, a generic serializer, the property mapper from `Supplemental.02.DynamicInvocation` — the single highest-impact optimization available is:

> **Look up the `Type`/`PropertyInfo`/`MethodInfo`/`ConstructorInfo` once, and cache it.**

A `static readonly` field, a `Dictionary<Type, PropertyInfo[]>`, a lazily-populated `ConcurrentDictionary` — whatever fits the situation. Anything other than calling `GetProperty()` fresh every time you need it.

That habit doesn't eliminate reflection's overhead, but it eliminates the *avoidable* part, which in practice is usually the larger part. It's the difference between "reflection is unacceptably slow" and "reflection has a real but manageable cost."

### Applying This to `PropertyMapper`

`Supplemental.02`'s mapper is exactly the code this lesson is warning about:

```csharp
public static void CopyMatchingProperties(object source, object destination)
{
	var sourceProperties = source.GetType().GetProperties();
	var destinationProperties = destination.GetType().GetProperties();
	...
}
```

Both `GetProperties()` calls run on **every invocation**, plus a `FirstOrDefault()` scan per property. Map one object: fine. Map a million rows in a loop: this is comparison 3, with extra steps.

The fix follows directly from the finding. Cache the property arrays — or better, the whole computed match list — keyed by the source/destination type pair:

```csharp
private static readonly ConcurrentDictionary<(Type, Type), List<(PropertyInfo Source, PropertyInfo Dest)>> MapCache = new();
```

The first call for a given type pair does the reflection; every subsequent call just walks a prepared list. Note that a `ConcurrentDictionary` is the right choice here rather than a plain one — a static cache is shared across threads, which is precisely the situation Chapter 7's `Supplemental.09.ConcurrentCollections` covered.

### Going Further: Compiled Delegates

Caching removes the lookup cost but leaves `SetValue()`'s per-call overhead. Production libraries take one more step: converting the cached `MethodInfo`/`PropertyInfo` into a **compiled delegate**, once, then invoking the delegate thereafter.

```csharp
// Roughly, using expression trees:
var setter = (Action<object, object>)/* compiled from an Expression tree */;
```

The delegate call is nearly as fast as direct code, because after compilation it *is* direct code. This is the same "pay a large one-time cost to eliminate a repeated one" trade that `Supplemental.03.CodeDomCompileAndRun` demonstrated with runtime compilation — AutoMapper, Dapper, and System.Text.Json all work this way internally.

That's why the main lesson's takeaway names "cached delegates" specifically: caching the `MethodInfo` is the easy 80%, and compiling a delegate is the remaining stretch.

---

## A Note on the Measurement Itself

`PrintComparison()` reports both absolute times and a ratio:

```csharp
double ratio = time2.TotalMilliseconds / time1.TotalMilliseconds;
Console.WriteLine($" - {label2} took roughly {ratio:N1}x as long as {label1}.");
```

The word "roughly" is doing honest work. This is a simple `Stopwatch` benchmark, and the numbers will vary between runs and machines. It's more than adequate for demonstrating an order-of-magnitude difference, which is the point here.

It is not, however, a rigorous benchmark. Note what it doesn't do: no warmup iterations (so the first loop absorbs JIT compilation cost), no multiple rounds, no statistical analysis, and no defense against the JIT optimizing away work whose result is never observed. For real performance work, use **BenchmarkDotNet**, which handles all of that.

The ordering here also slightly favors the direct loop in comparisons 1 and 2, since it runs first and pays the JIT warmup — which means the reported ratios, if anything, *understate* reflection's disadvantage.

---

## What to Take Away

**Reflection's unavoidable cost is real but bounded.** `SetValue()`/`Invoke()` must validate, box, and dispatch indirectly, and cannot be inlined.

**Reflection's avoidable cost is usually larger.** Repeating `GetProperty()`/`GetMethod()` inside a hot path is the mistake that actually shows up in profiles, and comparison 3 isolates it precisely.

**Cache the metadata, always.** A `static readonly` field or a `ConcurrentDictionary` keyed by type closes most of the gap for almost no effort.

**Compile a delegate when it matters.** Caching removes the lookup; a compiled delegate removes most of what's left.

**Measure before optimizing.** These ratios come from a method that does nothing. Against real work, reflection's relative overhead may be irrelevant — and the right response to "is this fast enough" is a profiler, not an assumption.
