---
title: "Anonymous Methods and Events"
chapter: 6
index: 2
dependencies: []
---

```csharp
using System;

internal static class Program
{
    // Event declaration using a custom delegate type
    public delegate void MyEventHandler();
    public static event MyEventHandler MyEvent;

    private static int clicks = 0;

    private static void Main()
    {
        // Wire up the event with a lambda -- a named method could do the same job,
        // but the handler is simple and used in exactly one place, so inline wins
        MyEvent = () => Console.WriteLine("Too many clicks! Event fired.");

        // Anonymous method as the click handler -- no separately declared method needed
        Action handleClick = delegate (/* object o, EventArgs e -- omitted, unused */)
        {
            clicks++;
            if (clicks > 3)
                MyEvent?.Invoke();      // ?.Invoke() is null-safe and thread-safe
            else
                Console.WriteLine($"I'm anonymous! - Clicked [{clicks}/3] times");
        };

        // Simulate five button clicks
        for (int i = 0; i < 5; i++)
            handleClick();
    }
}
```
