# Introduction to Vibe Coding

## What This Is

This section adapts the Microsoft Learn introduction to vibe coding and combines it with the stricter DataBank guidance from the introductory AI Assisted Development lesson.

Vibe coding means describing desired behavior in natural language and using an AI assistant to draft code. It is useful for prototypes, boilerplate, unfamiliar APIs, and getting unstuck. It is not a substitute for understanding, review, security judgment, or compliance. The professional obligation remains stubbornly attached to the human. Rude, but fair.

---

## Agentic Workflow

A good agentic workflow has checkpoints:

1. Write a clear task brief.
2. Attach relevant context documents.
3. Ask for a plan before broad edits.
4. Review the plan.
5. Let the agent implement a small slice.
6. Run the build and tests.
7. Review every line before shipping.

Stop when the agent is not converging, when the domain is too risky, or when you cannot explain the output.

---

## Step 1: Review an Agent Draft

In this step, you will treat an AI-generated implementation as a pull request. The runner contains a finished draft; your job is to learn how to produce and review that draft responsibly.

Start with a clear agent prompt:

```text
Write a C# console sample that processes raw CSV rows.

Requirements:
- Accept raw CSV rows in the format Id,Name,Amount.
- Skip invalid rows.
- Return total valid row count, sum of Amount, and the Name of the record with the highest Amount.
- Use no third-party packages.
- Use CultureInfo.InvariantCulture for decimal parsing.
- Include a small Main method with valid and invalid sample rows.
```

Read Copilot's draft before running it. Look for invented packages, unclear parsing, culture-sensitive decimal handling, and vague behavior for empty results. If the assistant adds a CSV library after being told not to, that is not initiative. That is the software equivalent of bringing a leaf blower to a library.

> **Screenshot placeholder:** Add an image showing the agent prompt and the generated data processor draft.

Run Step 1 in the lesson runner. Then review the code like a pull request:

- Does it validate the input collection?
- Does it handle blank rows?
- Does it require exactly three fields?
- Does it parse decimals with a deliberate culture?
- What happens when no valid rows exist?

Agent output is allowed to be a draft. It is not allowed to be a mystery. If you cannot explain the parsing, invalid row handling, or empty result behavior, the next step is not "ship it"; the next step is understanding it.

---

## Step 2: Use a Context Document

Agents do better when they know local rules. In real work, the context document is Markdown: coding standards, testing framework, package rules, naming conventions, security constraints, and local "please stop doing this" notes discovered through pain.

Start a new Copilot Chat conversation and paste:

```text
Treat the following as project context for this session:
- No new NuGet packages unless explicitly approved.
- Use NUnit for tests.
- Thread CancellationToken through async work.
- Follow existing C# naming and file organization.
- Ask before inventing types, APIs, or dependencies.

Acknowledge these rules, then wait for the task.
```

Then ask:

```text
Plan an implementation for an async document import method that reads rows, validates them, and writes accepted records. Do not write code yet.
```

Review whether the plan obeys the context. The lesson runner models this with a tiny `ContextRules` object and a generated plan. The real lesson is that rules must exist somewhere outside your head. If the agent does not know the rules, it will invent a cheerful little universe where your standards do not apply.

> **Screenshot placeholder:** Add an image showing Copilot acknowledging the context document or applying it to a plan.

Run Step 2 in the lesson runner. Inspect how the generated plan is checked against the rules:

- No new NuGet packages.
- Use NUnit.
- Thread `CancellationToken` through async work.

The matching logic is intentionally simple because this is not a natural-language processing lesson. It is a supervision lesson.

---

## Step 3: Checkpoint Agent Work

Agentic work needs checkpoints. Do not give an agent a broad task, go make coffee, and come back to a thousand-line diff with the emotional texture of a tax audit.

Give Copilot or your agent this kind of staged instruction:

```text
We will do this in checkpoints.

Task: add a small CSV import feature.

Checkpoint 1: inspect the relevant files and propose a plan. Do not edit files.
Checkpoint 2: after I approve the plan, implement only the parser.
Checkpoint 3: run the build and tests.
Checkpoint 4: summarize the diff and list anything I must review manually.
```

After the agent provides the plan, stop and review it. Do not approve implementation until you understand the files it intends to touch and the behavior it intends to add.

> **Screenshot placeholder:** Add an image showing an agent plan before implementation approval.

Run Step 3 in the lesson runner. The sample simulates a task moving through plan, implementation, build, tests, and review. Notice that the final answer is "Do not ship yet" because human review is incomplete. That is intentional.

Useful checkpoints:

- Plan reviewed.
- Implementation scoped.
- Build passed.
- Tests passed.
- Standards checker passed.
- Human review complete.

The agent may report success after writing files or fixing build errors. That is useful progress. It is not a merge decision.

---

## Step 4: Know When to Stop

Sometimes the correct AI workflow is to stop using AI for the current task. This is not failure. This is engineering judgment wearing its grown-up shoes.

Before using an agent, classify the risk:

```text
Assess whether this task is appropriate for AI assistance.

Task:
[describe the task]

Answer with:
- Safe to prototype with AI, or stop and implement manually.
- The main risks.
- What evidence would be required before accepting the output.
```

Use this especially when the work touches security, compliance, data privacy, runtime code execution, or an unfamiliar domain. If you cannot validate the output, you cannot responsibly accept it.

> **Screenshot placeholder:** Add an image showing Copilot identifying stop conditions or risk signals for a task.

Run Step 4 in the lesson runner. The sample evaluates a few risk signals:

- The work is security-sensitive.
- The work is compliance-sensitive.
- You cannot validate the output.
- The agent has failed to converge after several iterations.

Add another signal in your scratch file:

```csharp
var deadlinePanic = new TaskRisk(false, false, false, 5);
PrintDecision("Deadline panic", deadlinePanic);
```

That exact constructor call may not represent a real production rubric, but the human signal is real: accepting code because you are tired of arguing with it is where bad decisions put on a nice shirt and walk into production. The remedy is not another prompt; it is taking control of the implementation.

---

## Summary

The runner steps in this chapter are not instructions for making AI write code while you watch approvingly from a swivel chair. They are checkpoints for a supervised workflow:

- Write a clear task brief.
- Provide context before asking for output.
- Ask for a plan before broad edits.
- Review at every checkpoint.
- Run the code.
- Know when to stop.

The agent can get you to a draft faster. The distance from draft to production is still yours to cross.
