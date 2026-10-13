# Get Started with GitHub Copilot

## What This Is

This section adapts the Microsoft Learn module [Get started with GitHub Copilot](https://learn.microsoft.com/en-us/training/modules/get-started-github-copilot/) into the style of this course. The Microsoft module introduces Copilot plans, setup, settings, code completion, chat, inline chat, smart actions, and agent modes. Here, we care less about memorizing every button and more about choosing the right interaction surface for the work in front of you.

Copilot is not just autocomplete anymore. It is a family of related tools: ghost suggestions, next edit suggestions, Chat view, Quick Chat, Inline Chat, Ask, Plan, Agent, code review, PR summaries, CLI support, and cloud agents. That sentence is exhausting, which is why we need a mental model instead of a feature inventory.

---

## Before You Start

Open a scratch C# console app or a disposable file in Visual Studio or VS Code with GitHub Copilot enabled. The lesson runner will show the finished code for each step, but the learning happens when you reproduce the workflow in the editor: type a little, wait for Copilot, accept or reject deliberately, then compare your result to the runner sample.

Use the runner as the answer key and execution harness. Use your editor as the place where you practice interacting with Copilot.

When you see a screenshot placeholder, add a screenshot from your own editor showing the Copilot surface being discussed. The placeholders are intentionally plain so you can drop images in without needing to rewrite the lesson later.

---

## The Practical Model

Use the smallest useful surface:

| Surface | Use it when |
|---|---|
| Ghost suggestions | You are typing and the next few lines are obvious |
| Inline Chat | You need a small local edit in the current file |
| Quick Chat | You have a short question and do not want to leave the editor flow |
| Ask | You need explanation or comparison without file edits |
| Plan | The work is broad enough that the approach should be reviewed first |
| Agent | The task is scoped, you have a clear brief, and you are ready to supervise |
| Code review / PR summary | You want assistance communicating or checking changes, not a replacement for review |

If completion can solve the next step, do not summon an autonomous agent to wander the codebase with a tiny backpack and a dream. That is how you get a diff you did not ask for and a new respect for source control.

---

## Step 1: Ghost Suggestion, Simple Helper

The first Copilot interaction should be almost boring. Boring is excellent here. We want a tiny helper where the method name and signature provide enough context for Copilot to predict the body.

In your scratch file, type this much and then pause:

```csharp
private static int AddInt(int a, int b)
{
```

Wait for the ghost text. Copilot should suggest something close to:

```csharp
    return a + b;
}
```

Read the suggestion before accepting it. If it is correct, press `Tab` to accept the suggestion. If it is not correct, keep typing manually or press `Esc` and try again. The key habit is that `Tab` means "I reviewed this and I accept responsibility for it," not "sure, whatever, the robot seems confident."

> **Screenshot placeholder:** Add an image showing the ghost text for `return a + b;` before it is accepted.

After accepting the suggestion, add a small `Main` method that calls it:

```csharp
Console.WriteLine(AddInt(2, 3));
Console.WriteLine(AddInt(-4, 10));
```

Now run Step 1 in the lesson runner. The runner sample executes the finished helper. Compare the runner code to what Copilot produced in your editor.

Review points:

- The method name and parameters provide almost all the needed context.
- The implementation is short enough to verify by inspection.
- The sample runs more than one input, including a negative number.

This is the right home for ghost suggestions: tiny local work with an obvious answer and a very small blast radius. The point is not that adding two integers is difficult. The point is that small, obvious code is where inline suggestions are least risky and most useful. We are building habits, not doing arithmetic cosplay.

---

## Step 2: Ghost Suggestion with Edge Cases

Now repeat the workflow with logic that has real edge cases. A descriptive method name can guide a larger suggestion, but larger suggestions need more review.

In your scratch file, start with the call site first:

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5, 9, 11, 12 };
Console.WriteLine(AddPrimeNumbersInNumericList(numbers));
```

Then type the method signature and pause:

```csharp
private static int AddPrimeNumbersInNumericList(List<int> numbers)
{
```

Copilot may suggest the summing loop and may also create an `IsPrime` helper. If it only suggests part of the method, accept the part you can verify and keep typing. Completion often arrives in chunks; apparently even the machine enjoys suspense.

> **Screenshot placeholder:** Add an image showing Copilot suggesting either the prime-number loop or the `IsPrime` helper.

Before accepting the final version, inspect the edge cases:

- `1` is not prime.
- `2` is prime.
- Even numbers above `2` are not prime.
- Composite odd numbers such as `9` are not prime.
- The divisor loop should stop when `divisor * divisor <= number`.

Run Step 2 in the lesson runner. The expected sum for the provided list is `21` because `2 + 3 + 5 + 11 = 21`.

Review checklist:

- Does the code reject numbers less than or equal to `1`?
- Does it accept `2`?
- Does it reject even numbers greater than `2`?
- Does it reject `9`?
- Does the sample input prove more than the happy path?

This is still a reasonable Copilot completion task, but only because the behavior is easy to verify. Once a suggestion contains real logic, the Tab key is no longer a review process.

---

## Step 3: Choose the Smallest Useful Surface

Copilot gives you several ways to ask for help. This step is about deciding which surface to use before you start asking for code. The wrong surface can make a small task noisy or a broad task under-specified.

In Copilot Chat, ask:

```text
I am learning GitHub Copilot. Compare ghost suggestions, Inline Chat, Ask, Plan, and Agent mode. Give one C# development scenario where each is the smallest useful surface.
```

Read the answer and translate it into a decision rule. Then type a small model like this:

```csharp
private static string ChooseSurface(WorkScenario scenario)
{
```

Let Copilot suggest the branching logic. If the logic jumps straight to Agent for every multi-file task, revise it. A plan should usually come before an agentic edit. We are developers, not people handing chainsaws to interns and hoping the hedge comes out symmetrical.

> **Screenshot placeholder:** Add an image showing Copilot Chat's comparison of Copilot surfaces or the inline suggestion for `ChooseSurface`.

Run Step 3 in the lesson runner and review the output. The important behavior is:

- Local and obvious work maps to ghost suggestions or Inline Chat.
- Explanation-only work maps to Ask.
- Multi-file work maps to Plan until the plan is reviewed.
- Multi-file work can move to Agent after the plan is reviewed.

After the runner works, add another scenario in your scratch file:

```csharp
new WorkScenario("Rename a method in one file", false, false, false, false)
```

Decide whether the fallback to Quick Chat is right, or whether your decision model needs another property. That little discomfort is the useful part. Tool choice is judgment, not a lookup table from Mount Sinai.

---

## Step 4: Validate Before Accepting

Inline suggestions are strongest when you can validate them quickly. In this step, you will prompt Copilot with a small method and immediately surround it with known inputs.

Start by typing the verification calls:

```csharp
Verify("alpha,beta,gamma", 3);
Verify("alpha,,gamma", 2);
Verify("", 0);
Verify(null, 0);
```

Then type the method signature:

```csharp
private static int CountNonEmptyCsvFields(string row)
{
```

Wait for Copilot to suggest the implementation. Before accepting, compare the suggestion to the examples you already wrote. Does it handle `null`? Does it ignore blank fields? Does it count whitespace-only fields? If the answer is "I have no idea," do not press `Tab` yet. Mystery code is not a productivity gain; it is a future meeting.

> **Screenshot placeholder:** Add an image showing the suggested `CountNonEmptyCsvFields` implementation before acceptance.

Run Step 4 in the lesson runner. Then extend your scratch sample with:

```csharp
Verify(" alpha , beta ", 2);
Verify(",", 0);
Verify("one,two,three,", 3);
```

If the output does not match the expectation, decide whether the code is wrong or the expectation needs to be clarified. That last phrase is doing a lot of work: "the expectation." Copilot does not know your requirement unless you state it. Testing the output is how you discover whether the assistant guessed correctly or just guessed confidently.
