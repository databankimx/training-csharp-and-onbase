# Ch07 Textbook Code: Barrier Sample

## What This Is

The publisher's original `Barrier` example, but with a critical difference from what a truly "unedited" download would contain: it's already been corrected. The embedded "Instructor Note" (signed "-SWM") explains why: the book's original code was "badly broken", and it was split into two projects, this one and the separate `CSharp.Ch07.TextbookCode.BarrierWithTasks`.

No bugs found, code is unchanged from this (already-corrected) download aside from the project file format.

---

## This Confirms the Bug Found in Supplemental 06

`CSharp.Ch07.Supplemental.06.Barriers` had a real bug: tasks that called `RemoveParticipant()` would still fall through to shared code calling `SignalAndWait()` again, throwing `InvalidOperationException`. Reading this file's corrected version confirms exactly what that fix needed to be:

```csharp
else
{
    Console.WriteLine("Task {0} changed its mind and went back!", localCopy);
    barrier.RemoveParticipant();
    return;
}
```

Notice the explicit `return;` right after `RemoveParticipant()`. That's the fix, in a different (but equally valid) shape than the one applied in `Supplemental.06.Barriers` (which moved the trailing code inside the `if` branch instead of returning early from the `else`). Both approaches solve the identical problem: a task that has permanently left the barrier must never call `SignalAndWait()` again.

Seeing the "Instructor Note" explicitly say the book's code was "badly broken" is a good confirmation that this wasn't a subtle misreading on my part, it's the same conclusion reached independently, twice.

---

## Worth Noticing: A Namespace That Doesn't Match the Project Folder

Every file in this project uses `namespace BarrierSample`, not `CSharp.Ch07.TextbookCode.BarrierSample`. Preserved exactly as authored (the `.csproj`'s `RootNamespace`/`AssemblyName` still follow this solution's naming convention without needing to touch the `.cs` file's own namespace declaration), matching the same situation documented for `CSharp.Ch06.TextbookCode.TreeEnumerator` and `UniversityClasses`.
