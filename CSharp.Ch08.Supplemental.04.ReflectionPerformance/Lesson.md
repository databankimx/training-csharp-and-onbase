# Chapter 8 Supplemental 04: Reflection Performance

## What This Is

Every project in this chapter repeats the same warning: reflection is resource-intensive, use it deliberately. This project puts an actual number behind that warning. Three timing comparisons, one million iterations each, direct code against equivalent reflection-based code. The results are impossible to miss.

The more useful finding isn't "reflection is slow" -- it's more specific than that: **the expensive part is the lookup**, not the subsequent call. `GetProperty()` and `GetMethod()` are the slow steps. Once you have the `PropertyInfo` or `MethodInfo` in hand, `GetValue()`, `SetValue()`, and `Invoke()` are much more reasonable. Looking up once and caching the result is the highest-impact optimization available when reflection is genuinely the right tool.

---

## How to Write This Program

`Counter` is already in the project. It has a `Value` property and an `Increment()` method, both trivially simple -- that's deliberate. The goal is to measure how you're accessing them, not what they do.

```csharp
private const int Iterations = 1_000_000;
```

Add a shared printer:

```csharp
private static void PrintComparison(string label1, TimeSpan time1, string label2, TimeSpan time2)
{
    Console.WriteLine($" - {label1}: {time1.TotalMilliseconds:N1} ms");
    Console.WriteLine($" - {label2}: {time2.TotalMilliseconds:N1} ms");
    if (time1.TotalMilliseconds > 0)
    {
        double ratio = time2.TotalMilliseconds / time1.TotalMilliseconds;
        Console.WriteLine($" - {label2} took roughly {ratio:N1}x as long as {label1}.");
    }
}
```

---

### Mini-Program 1: Direct vs. Reflected Property Access (PropertyInfo Cached)

Clear `Main()` and write:

```csharp
Console.WriteLine($"Setting a property {Iterations:N0} times: direct vs. reflection (PropertyInfo cached)...");

var counter = new Counter();

var directTimer = Stopwatch.StartNew();
for (int i = 0; i < Iterations; i++)
    counter.Value = i;
directTimer.Stop();

// The lookup happens ONCE, before the timed loop.
PropertyInfo valueProperty = typeof(Counter).GetProperty("Value");

var reflectedTimer = Stopwatch.StartNew();
for (int i = 0; i < Iterations; i++)
    valueProperty?.SetValue(counter, i);
reflectedTimer.Stop();

PrintComparison("Direct property set", directTimer.Elapsed,
                "Reflected property set (cached)", reflectedTimer.Elapsed);
GenericFunctions.Pause();
```

Run it. The reflected version will be slower -- typically 10-50x on .NET Framework, depending on hardware -- but not as catastrophically slower as the warning "hundreds of times" might suggest. That's because the `PropertyInfo` lookup is done once outside the loop. What you're measuring is the cost of `SetValue` itself, which includes boxing the value, the null check, and the internal dispatch -- real overhead, but not the most expensive part.

### Mini-Program 2: Direct vs. Reflected Method Calls (MethodInfo Cached)

Clear `Main()` and write:

```csharp
Console.WriteLine($"\nCalling a method {Iterations:N0} times: direct vs. reflection (MethodInfo cached)...");

var counter = new Counter();

var directTimer = Stopwatch.StartNew();
for (int i = 0; i < Iterations; i++)
    counter.Increment();
directTimer.Stop();

// Same principle: the lookup happens once, before the timed loop.
MethodInfo incrementMethod = typeof(Counter).GetMethod("Increment");

var reflectedTimer = Stopwatch.StartNew();
for (int i = 0; i < Iterations; i++)
    incrementMethod?.Invoke(counter, null);
reflectedTimer.Stop();

PrintComparison("Direct method call", directTimer.Elapsed,
                "Reflected method call (cached)", reflectedTimer.Elapsed);
GenericFunctions.Pause();
```

Run it. Similar story -- reflected is slower, but the cached `MethodInfo` version is nowhere near as bad as calling `GetMethod` on every iteration would be.

### Mini-Program 3: Cached vs. Uncached Lookup -- The Mistake That Matters

This is the comparison that explains where reflection's real-world performance cost actually lives.

Clear `Main()` and write:

```csharp
Console.WriteLine($"\nSetting a property {Iterations:N0} times: PropertyInfo cached once vs. looked up every iteration...");

var counter = new Counter();
var counterType = typeof(Counter);

// Cached: the lookup happens once.
PropertyInfo cachedProperty = counterType.GetProperty("Value");

var cachedTimer = Stopwatch.StartNew();
for (int i = 0; i < Iterations; i++)
    cachedProperty?.SetValue(counter, i);
cachedTimer.Stop();

// Uncached: GetProperty("Value") runs fresh inside the loop every single time.
// This is the pattern to avoid.
var uncachedTimer = Stopwatch.StartNew();
for (int i = 0; i < Iterations; i++)
    counterType.GetProperty("Value")?.SetValue(counter, i);
uncachedTimer.Stop();

PrintComparison("Cached PropertyInfo", cachedTimer.Elapsed,
                "Uncached (re-looked-up every iteration)", uncachedTimer.Elapsed);
GenericFunctions.Pause();
```

Run it. The uncached version is dramatically slower than the cached version -- often 10x or more -- and the cached version from this program matches the cached version from Mini-Program 1. The `SetValue` cost is the same. It's the `GetProperty` call inside the loop doing all the damage.

This is the pattern that causes most of reflection's real-world performance problems. It shows up as:

```csharp
// Every time this runs -- in a web request, in a loop, in a hot path:
var prop = type.GetProperty("Name");  // <-- this is the expensive part
prop.SetValue(obj, value);
```

The fix is always the same: move the lookup out of the hot path.

```csharp
// Once at startup or on first use:
private static readonly PropertyInfo _nameProp = typeof(MyClass).GetProperty("Name");

// Then in the hot path, just the call:
_nameProp.SetValue(obj, value);
```

Static readonly fields, a `ConcurrentDictionary<Type, PropertyInfo>`, or a lazy initializer all work. AutoMapper, JSON serializers, and ORMs all build caches of this kind at startup -- that's part of why they have a "warm-up" cost on first use and are fast on subsequent calls.

---

## Worth Knowing: Beyond Caching

Caching the `PropertyInfo` gets most of the performance back, but not all of it. `SetValue` still boxes value types and involves internal dispatch. For maximum performance on a hot path, the next step is compiling the reflection into a delegate:

```csharp
// Build a compiled setter delegate, once:
var setter = (Action<Counter, int>)Delegate.CreateDelegate(
    typeof(Action<Counter, int>),
    typeof(Counter).GetProperty("Value").GetSetMethod());

// Then call it at near-direct speed:
setter(counter, 42);
```

Or with expression trees:

```csharp
var param = Expression.Parameter(typeof(Counter));
var value = Expression.Parameter(typeof(int));
var setter = Expression.Lambda<Action<Counter, int>>(
    Expression.Assign(Expression.Property(param, "Value"), value),
    param, value).Compile();
```

Both approaches produce a delegate that the JIT can inline and optimize, bringing the per-call cost close to a direct call. This is what modern serializers and ORMs actually do. The reflection lookup happens once to build the delegate; the delegate is cached and reused.

---

## Takeaways

- The expensive part of reflection is the **lookup** (`GetProperty`, `GetMethod`), not the call (`SetValue`, `Invoke`).
- Never put a lookup inside a hot loop. Cache the `PropertyInfo` or `MethodInfo` and reuse it.
- Cached reflection is slower than direct code, but usually acceptable for non-hot paths.
- Uncached reflection inside a loop is the mistake that causes most real-world performance problems.
- For maximum performance, compile the reflection into a delegate using `Delegate.CreateDelegate` or expression trees.
- Modern serializers and ORMs do exactly this: reflect once at startup, cache compiled delegates, call at near-direct speed.
