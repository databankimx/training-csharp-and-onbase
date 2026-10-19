# Prompt Engineering for AI-Assisted Development

## What This Is

Prompt engineering is the practice of turning intent into instructions an AI coding assistant can actually use. That does not mean learning magic words. It means writing a small, useful specification: goal, context, constraints, examples, expected output, and verification criteria.

The goal of this chapter is practical. You will not learn to summon perfect code with a secret incantation whispered under a full moon. You will learn to give Copilot enough information to produce a useful draft, then tighten the result with evidence instead of vibes.

This chapter builds on the earlier AI Assisted Development material and the Microsoft Learn Copilot workflow concepts, but it focuses specifically on prompt construction and prompt iteration for C# development work.

---

## Before You Start

Open a scratch C# console app or a disposable branch with GitHub Copilot enabled. Keep the lesson runner open beside your editor.

For each step:

1. Read the prompt pattern.
2. Try the prompt in Copilot Chat or Inline Chat.
3. Capture the suggested output or plan.
4. Run the matching lesson runner step.
5. Compare the generated result to the executable sample.

The step files contain only runnable C# examples. The instructions live here because the runner changes code per step, not lesson text. We are respecting the tool instead of trying to teach it interpretive dance.

---

## The Prompt Shape

A good coding prompt usually includes these parts:


| Part | Purpose |
| :- | :- |
| Goal | What outcome you want |
| Context | What project, file, framework, or domain rules apply |
| Inputs and outputs | Method signatures, data shapes, expected return values |
| Constraints | Packages, performance, async, exceptions, style, security |
| Examples | Concrete inputs and expected outputs |
| Verification | How you will know the answer is correct |

You do not need all six parts for every request. A ghost suggestion for a guard clause does not need a novella. A multi-file feature absolutely does. The trick is matching prompt weight to task risk.

---

## Step 1: Build a Complete Prompt

Start with a weak prompt in Copilot Chat:

```text
Write a method that parses CSV rows.
```

Before reading the answer too closely, list what the assistant had to guess:

- Target framework.
- Method name.
- Input type.
- Output type.
- Whether there is a header row.
- Whether invalid rows throw, skip, or get reported.
- Whether third-party packages are allowed.
- Whether culture matters for decimal parsing.
- How the result will be tested.

Now replace the weak prompt with a complete one:

```text
Write a C# method named ParseInvoiceRows.

Target:
- C# targeting net10.0.

Input:
- IReadOnlyList<string> rows.
- Each row is in the format InvoiceId,CustomerName,Amount.
- There is no header row.

Output:
- Return IReadOnlyList<InvoiceRow>.

Rules:
- Throw ArgumentNullException when rows is null.
- Skip blank rows.
- Skip rows that do not have exactly three fields.
- Parse Amount with CultureInfo.InvariantCulture.
- Do not add third-party packages.

Also include:
- A small InvoiceRow class.
- A Main method with sample rows.
```

> **Screenshot placeholder:** Add an image showing the weak prompt response beside the complete prompt response.

Run Step 1 in the lesson runner. The runner sample scores a prompt specification and shows which pieces are present. This is not because prompts need grades like a nervous middle-school essay. It is because missing prompt parts become model guesses, and model guesses become code you now have to review.

After running the sample, revise your prompt one more time by adding expected examples:

```text
Example:
- "1001,Avery,42.50" becomes InvoiceRow(1001, "Avery", 42.50m).
- "bad row" is skipped.
```

The more precise the prompt, the less cleanup you buy for future-you, who has already been through enough.

---

## Step 2: Turn Vague Requests into Specifications

Prompt engineering often starts after someone says, "Can you make this better?" That is not a prompt. That is a haunted sticky note.

In Copilot Chat, ask:

```text
Convert this vague request into a concrete implementation prompt for a C# developer:

"Make the import faster and safer."

Ask clarifying questions first if required. Then produce a final prompt with goal, constraints, examples, and verification steps.
```

Review the clarifying questions. Good questions should separate performance, safety, and behavior:

- What import format?
- What is slow today?
- What does safer mean: validation, path traversal, logging, retry behavior?
- What behavior must remain unchanged?
- What tests or measurements prove success?

> **Screenshot placeholder:** Add an image showing Copilot turning the vague request into clarifying questions and a concrete prompt.

Run Step 2 in the lesson runner. The sample compares a vague request to a structured prompt and reports which specification parts were recovered.

Then practice with your own vague requests:

```text
Clean up this method.
```

```text
Add better error handling.
```

```text
Make this production ready.
```

Each of those might be reasonable in conversation with a teammate who knows the system. For an AI assistant, they are permission slips for guessing. Turn them into specifications before asking for implementation.

---

## Step 3: Iterate with Evidence

The first output is rarely the final output. The important skill is knowing how to iterate. Do not say:

```text
That is wrong. Try again.
```

That tells the assistant almost nothing, except that you are disappointed. The assistant was not emotionally prepared, but that is still not a debugging strategy.

Use evidence:

```text
The implementation fails this case:

Input: "alpha,,gamma"
Expected: 2
Actual: 3

Revise the method so empty CSV fields are not counted. Keep null and whitespace behavior unchanged.
```

> **Screenshot placeholder:** Add an image showing a correction prompt that includes input, expected output, and actual output.

Run Step 3 in the lesson runner. The sample compares a first draft and a revised implementation for counting non-empty CSV fields.

What to notice:

- The first draft has a specific, testable bug.
- The revision request names the failing case.
- The requested change preserves the behavior that already works.
- The verification harness proves the fix.

This is the difference between prompt iteration and prompt arguing. One gives the model information. The other gives it weather.

---

## Step 4: Use Examples to Shape Output

Examples are one of the strongest ways to steer an AI assistant. They remove ambiguity without requiring a lecture on your entire domain.

Ask Copilot:

```text
Write a C# method named ClassifyTicket that classifies a support ticket title.

Return:
- Billing
- Access
- Technical
- General

Use these examples:
- "Invoice total is wrong" => Billing
- "Cannot sign in" => Access
- "Export crashes on large file" => Technical
- "What are your support hours?" => General

Keep the implementation simple and deterministic. Do not use machine learning or external packages.
```

> **Screenshot placeholder:** Add an image showing Copilot using examples to generate deterministic classification logic.

Run Step 4 in the lesson runner. The sample uses a small deterministic classifier with example cases.

Then add another example to your prompt:

```text
"Password reset link expired" => Access
```

Ask Copilot to update the classifier. Notice whether it adds a targeted rule or rewrites the whole thing for no good reason. If it rewrites everything, ask for the smallest change. AI assistants sometimes treat a tiny edit like an audition for a kitchen renovation show. Encourage restraint.

Use examples when:

- The category boundaries are fuzzy.
- Formatting matters.
- You want a particular style of output.
- You need the assistant to preserve known behavior.

Do not use examples as a replacement for rules when rules exist. Use both when the work is important.

---

## Step 5: Add Guardrails and Negative Constraints

A prompt should say what to do. For professional work, it should also say what not to do.

Try this prompt:

```text
Write a C# method that resolves an import file path.

Guardrails:
- Do not accept absolute paths from the caller.
- Do not allow .. path traversal.
- Do not read files in this method.
- Do not add third-party packages.
- Throw clear exceptions for invalid input.
```

> **Screenshot placeholder:** Add an image showing Copilot responding to explicit negative constraints.

Run Step 5 in the lesson runner. The sample checks a prompt policy for common guardrails: no new packages, no secrets, no runtime code execution, no absolute paths, and no path traversal.

Use negative constraints for:

- Package restrictions.
- Security boundaries.
- Data privacy rules.
- Runtime behavior you explicitly do not want.
- APIs the project forbids.

Negative constraints are not paranoia. They are the little fences that keep a helpful assistant from solving the wrong problem with alarming enthusiasm.

---

## Summary

Good prompts are not fancy. They are specific.

Use this pattern:

```text
Goal:
Context:
Inputs:
Outputs:
Constraints:
Examples:
Verification:
Do not:
```

For small work, use a small prompt. For risky or broad work, write a real brief. If the assistant guesses, you review more. If you specify, you review better.

The point is not to become a prompt wizard. The point is to become a developer who can communicate requirements clearly to a very fast, very literal, occasionally overconfident assistant. Which, frankly, is also good practice for communicating with humans, except humans usually do not hallucinate NuGet packages before lunch.
