# Implement Code Improvements Using GitHub Copilot Tools

## What This Is

This section adapts the Microsoft Learn module on code improvements and refactoring. Copilot can suggest readability, reliability, performance, and security improvements. Start by asking for observations, not edits.

The safe workflow is:

1. Ask for possible improvements without changing code.
2. Choose the one improvement that matches the goal.
3. Apply one change at a time.
4. Run tests.
5. Review the diff for behavior changes disguised as cleanup.

"Make this better" is not a refactoring prompt. It is an invitation to discover how many unrelated changes can fit in one diff. The answer is always "too many."

---

## Step 1: Refactor Without Changing Behavior

Start by asking Copilot for analysis instead of code:

```text
Review this C# method for readability improvements. Do not change behavior yet. List possible refactorings and call out any behavior that must be preserved.
```

Then provide the loop-based implementation:

```csharp
private static int SumPositiveNumbersBefore(IEnumerable<int> values)
{
    int total = 0;
    foreach (int value in values)
    {
        if (value > 0)
        {
            total = total + value;
        }
    }
    return total;
}
```

After reviewing Copilot's suggestions, ask for one specific refactor:

```text
Refactor this method using LINQ. Preserve behavior for the provided sample input. If you add validation, call it out explicitly as a behavior change.
```

> **Screenshot placeholder:** Add an image showing Copilot listing refactoring options before making edits.

Run Step 1 in the lesson runner. The runner compares the original and refactored implementations side by side. If the results diverge, the refactor is not done. It may be prettier. It may be clever. It may deserve a small award from the Committee for Elegant Regressions. It is still wrong.

One subtlety: the refactored version adds a null check. That changes behavior for null input, but intentionally. If a refactor includes this kind of improvement, call it out explicitly in the prompt, the commit message, and the review notes. "Refactor" should not be a trench coat full of secret feature changes.

---

## Step 2: Improve Reliability

Reliability improvements are often small: validate input, make failure modes explicit, and stop pretending a malformed row is fine.

Start with this prompt:

```text
Improve this row parser for reliability.

Requirements:
- Accept rows in the format Id,Name.
- Reject rows with the wrong number of fields.
- Reject rows with a non-numeric ID.
- Reject rows with a blank name.
- Report rejected rows instead of silently skipping them.
- Do not add third-party packages.
```

Then paste a simple parser or ask Copilot to draft one from the requirements. Add sample rows:

```csharp
var rows = new[] { "1,Avery", "bad", "2,Blair", "3," };
```

> **Screenshot placeholder:** Add an image showing Copilot adding explicit rejection paths for malformed rows.

Run Step 2 in the lesson runner and review the accepted and rejected rows. Then try adding:

- A row with too many fields.
- A row with a blank name.
- A row with whitespace around the name.
- A row with a non-numeric ID.

The improvement is not that the code is fancy. The improvement is that malformed input produces deliberate, visible behavior. A Copilot refactor that silently swallows those rows may be shorter, but shorter is not automatically better. Sometimes shorter is just where the missing incident report went to hide.

---

## Step 3: Improve Performance After Measuring

Do not ask Copilot to optimize a hot path until you know what is hot. Ask for a measurement harness first:

```text
Write a small C# console measurement harness that compares repeated string concatenation with StringBuilder for building a comma-delimited string of 5000 integers. Use Stopwatch and print elapsed milliseconds and output length.
```

Review the generated code. It should measure comparable work and should not accidentally optimize one side away. Then ask:

```text
Based on this measurement, explain when StringBuilder is a useful improvement and when the extra code is unnecessary.
```

> **Screenshot placeholder:** Add an image showing Copilot's measurement harness or explanation of the performance result.

Run Step 3 in the lesson runner more than once. The exact times will vary, but the shape should be clear: repeated string concatenation allocates heavily, while `StringBuilder` is designed for this job.

Performance review questions:

- What workload was measured?
- Is this workload representative?
- Does the change improve the measured bottleneck?
- Did the change preserve behavior?
- Is the added complexity worth it?

Copilot can suggest performance changes quickly. Your job is to make sure they are connected to actual evidence, not vibes wearing running shoes.

---

## Step 4: Improve Security Posture

Security-oriented improvements should make unsafe input impossible or explicit. Name the threat in the prompt.

Ask Copilot:

```text
Write a C# method named ResolveImportPath.

Requirements:
- Accept a trusted baseDirectory and an untrusted fileName.
- Throw ArgumentException when either value is null, empty, or whitespace.
- Combine the paths safely.
- Resolve full paths before comparing.
- Prevent path traversal so the final path cannot escape the base directory.
- Throw InvalidOperationException when the requested file is outside the import directory.
```

Then test it with:

```csharp
TryResolve(@"C:\DataBank\Imports", "today.csv");
TryResolve(@"C:\DataBank\Imports", @"..\secrets.txt");
```

> **Screenshot placeholder:** Add an image showing Copilot's path traversal prevention logic.

Run Step 4 in the lesson runner and inspect `ResolveImportPath`:

- It requires a base directory.
- It requires a file name.
- It resolves both paths to full paths.
- It rejects any candidate path outside the import directory.

AI-generated file handling code often forgets this. It looks clean right up until someone asks for `..\secrets.txt`, which is not a feature request we should honor. When asking Copilot to improve security, do not ask vaguely. Name the threat. That prompt gives the assistant a target, and it gives you a review checklist.
