# Ch06 Textbook Code: Static and Instance Delegates

## What This Is

The direct source `CSharp.Ch06.Supplemental.01.NamedVersusAnonymousDelegates`'s `StaticAndInstanceDelegates()` method was adapted from. Same Alice/Bob demonstration, but this version displays the results directly in a textbox on load, no debugger or console needed to see what happened.

No bugs found. `Load` correctly wired.

---

## The Result, Right There on Screen

```csharp
alice.InstanceMethod = alice.GetName;
alice.StaticMethod = Person.StaticName;

bob.InstanceMethod = alice.GetName;
bob.StaticMethod = Person.StaticName;
```

Run it and read straight off the textbox:

```
Alice's InstanceMethod returns: Alice
Bob's InstanceMethod returns: Alice
Alice's StaticMethod returns: Static
Bob's StaticMethod returns: Static
```

"Bob's InstanceMethod returns: Alice" is the whole lesson in one line, `bob.InstanceMethod` was deliberately assigned `alice.GetName`, so calling it through `bob` still calls Alice's own instance method, an instance-method delegate carries its target object along with it, it doesn't matter which variable it's stored on. `StaticMethod` returns the same "Static" for both, since `Person.StaticName` has no instance to carry in the first place.

---

## Compare Against the Supplemental

Same setup, same conclusion, this version just skips the extra step of needing a debugger or console window to see it. Worth running both, `CSharp.Ch06.Supplemental.01.NamedVersusAnonymousDelegates` frames this as one section among several delegate topics covered in sequence, this project isolates it as the only thing happening.
