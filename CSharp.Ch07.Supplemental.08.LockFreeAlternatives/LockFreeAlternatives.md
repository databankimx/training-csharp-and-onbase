# Lock-Free Alternatives

## Introduction

Locking (`Monitor`, `Mutex`, `Semaphore`) is powerful but comes with real costs: overhead, and the risk of deadlocks if used carelessly. For simple operations, like incrementing a counter, .NET offers a lighter-weight alternative: the `Interlocked` class, a set of static methods that perform basic operations atomically (as a single, uninterruptible step), with no lock required at all.

---

## The Problem, One More Time

```csharp
counter++;
```

This looks like one operation, but it's really three: read `counter`, add 1, write the result back. If two threads do this at the same time, they can both read the same starting value, and one thread's increment quietly disappears. Run enough threads doing this enough times, and the final count comes out lower than expected, every time.

---

## `Interlocked.Increment` and `Interlocked.Decrement`

```csharp
Interlocked.Increment(ref counter);
Interlocked.Decrement(ref counter);
```

These perform the read-add-write (or read-subtract-write) as one atomic step, no other thread can interleave in the middle. Run the same "100 threads increment 1,000 times each" test with `Interlocked.Increment` instead of `counter++`, and the result is always exactly correct, no lock needed anywhere.

---

## `Interlocked.Add`

```csharp
Interlocked.Add(ref total, 10);
```

Like `Increment`, but for any value, not just 1.

---

## `Interlocked.Exchange`

```csharp
string previous = Interlocked.Exchange(ref currentLeader, "Alice");
```

Atomically sets a variable to a new value and returns whatever it held immediately before. Without `Interlocked`, "check the old value, then set the new one" would be two separate steps, with a gap another thread could interfere in. `Exchange` closes that gap.

---

## `Interlocked.CompareExchange`

```csharp
int original = Interlocked.CompareExchange(ref flag, 1, 0);
bool didWeSetIt = original == 0;
```

Reads as: "if `flag` currently equals `0`, set it to `1`." It always returns whatever `flag` held right before the call, so comparing that return value against what you expected tells you whether your update actually took effect, or whether some other thread got there first.

This is the real building block behind most lock-free code: read a value, figure out what you want the new value to be, then try to `CompareExchange` it in. If the value that comes back doesn't match what you started with, someone else changed it in the meantime, so you try again with the fresh value. `Interlocked.Increment` is essentially this exact dance, already done for you, for the specific case of "add 1 to a number."

---

## `lock` Is Shorthand for `Monitor`

```csharp
lock (syncObject)
{
    // protected code
}
```

is exactly equivalent to:

```csharp
Monitor.Enter(syncObject);
try
{
    // protected code
}
finally
{
    Monitor.Exit(syncObject);
}
```

Worth knowing both forms exist, `lock` is just the shorter way to write the same thing.

---

## When to Use Which

| Situation | Tool |
|---|---|
| One simple update to one variable | `Interlocked` |
| Multiple related values that need to change together | `Monitor`/`lock` |
| A whole block of code that must run without interruption | `Monitor`/`lock` |

`Interlocked` is faster and can't deadlock, but it only handles single, simple operations. Anything more involved than that needs a real lock.

---

## Try It Yourself

Run the unprotected-vs-`Interlocked` comparison a few times. The unprotected version's wrongness isn't consistent, run it repeatedly and watch the actual (wrong) number change each time, while the `Interlocked` version always lands on exactly the right answer.
