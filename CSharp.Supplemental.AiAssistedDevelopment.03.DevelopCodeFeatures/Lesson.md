# Develop Code Features Using GitHub Copilot Tools

## What This Is

This section adapts the Microsoft Learn module on developing code features with Copilot. The central lesson is that AI-assisted feature work is not one prompt. It is a sequence: understand, plan, implement, verify, review.

The most useful Copilot workflow for feature work is often:

1. Use Ask to clarify the requirement.
2. Use Plan for the implementation approach.
3. Review the plan before code exists.
4. Use Agent or Inline Chat for a small implementation slice.
5. Run tests and review the diff.

Skipping the plan is tempting because it feels faster. It is also how you get code that technically does something, just not the thing anyone meant. Very productive, in the way a printer jam is productive of paper sculpture.

---

## Before You Start

Use a scratch C# console app or a disposable branch. For each step, you will first give Copilot a feature request, then compare its output to the runnable version in the lesson runner.

The goal is not to worship the runner sample. The goal is to practice converting a vague desire into a feature brief, letting Copilot draft from that brief, and then reviewing the result against the original constraints.

---

## Step 1: Implement a Small Feature

Begin with a written feature brief. Do not start with "write a filter method" and hope the details arrive by themselves. Hope is not a type system.

Paste this into Copilot Chat or Inline Chat:

```text
In C#, write a method named FilterDocuments.

Requirements:
- Accept IReadOnlyList<DocumentRecord> documents, string owner, and decimal? minimumAmount.
- Filter documents by optional owner when owner is not null or whitespace.
- Filter documents by optional minimum amount when minimumAmount has a value.
- Preserve input order.
- Do not mutate the original list.
- Throw ArgumentNullException when documents is null.
- Return IReadOnlyList<DocumentRecord>.
```

If Copilot needs the model type, type this first:

```csharp
private sealed class DocumentRecord
{
    public DocumentRecord(int id, string owner, decimal amount)
    {
        Id = id;
        Owner = owner;
        Amount = amount;
    }

    public int Id { get; }
    public string Owner { get; }
    public decimal Amount { get; }
}
```

> **Screenshot placeholder:** Add an image showing the feature prompt and Copilot's generated `FilterDocuments` method.

Run Step 1 in the lesson runner. Then compare the generated code to the requirements one line at a time.

Try these edits in your scratch file:

- Pass `owner: null` and confirm the owner filter is skipped.
- Pass `minimumAmount: null` and confirm the amount filter is skipped.
- Change `"Avery"` to `"avery"` and confirm the owner match is case-insensitive.
- Check whether the original `documents` list still has four records after filtering.

The feature is small, but the review habit scales. Requirements are not decoration. They are the measuring stick.

---

## Step 2: Add Feature Validation

Generated feature code often handles the happy path first. That is fine for a draft. It is not fine for a PR.

Start with this prompt:

```text
Write a C# method named CreateDocumentKey.

Requirements:
- Accept string documentType and int documentId.
- Throw ArgumentException named documentType when documentType is null, empty, or whitespace.
- Throw ArgumentOutOfRangeException named documentId when documentId is less than or equal to zero.
- Return the document type trimmed, uppercased invariantly, then a dash, then the document ID padded to six digits.
- Example: ("ap", 42) returns AP-000042.
```

When Copilot produces the method, add a small harness:

```csharp
Console.WriteLine(CreateDocumentKey("AP", 42));
TryCreateDocumentKey("", 42);
TryCreateDocumentKey("AP", 0);
```

> **Screenshot placeholder:** Add an image showing Copilot generating validation branches or exception handling.

Run Step 2 in the lesson runner and compare behavior.

Review the exception behavior:

- Empty document type throws `ArgumentException`.
- Non-positive document ID throws `ArgumentOutOfRangeException`.
- Valid input returns a normalized, padded key.

When asking Copilot to add validation, be specific about exception types and parameter names where your project has standards. Otherwise it may choose something plausible, and "plausible" is not the same as "matches the codebase."

---

## Step 3: Handle Feature Edge Cases

A feature is not done when the main example works. It is done when the boundaries behave intentionally.

Ask Copilot:

```text
Write a C# method named GetPage.

Requirements:
- Accept IReadOnlyList<int> values, int pageNumber, and int pageSize.
- Throw ArgumentNullException when values is null.
- Throw ArgumentOutOfRangeException when pageNumber is less than 1.
- Throw ArgumentOutOfRangeException when pageSize is less than 1.
- Return the requested page using one-based page numbers.
- Return an empty list when the requested page is beyond the end.
- Do not mutate the input list.
```

Then add calls for normal and boundary behavior:

```csharp
PrintPage(values, pageNumber: 1, pageSize: 5);
PrintPage(values, pageNumber: 3, pageSize: 5);
PrintPage(values, pageNumber: 9, pageSize: 5);
```

> **Screenshot placeholder:** Add an image showing Copilot's pagination implementation or the plan for handling edge cases.

Run Step 3 in the lesson runner. Then add a call for page `0` or page size `0` in your scratch file and observe the exception. Also notice that requesting page `9` of a twelve-item list returns an empty page rather than throwing. That is a design choice. If the product expects an exception instead, the prompt and implementation need to say so.

The question is not whether the edge case is handled the way you personally expected. The question is whether the behavior is intentional, documented, and tested.

---

## Step 4: Review the Feature Output

After Copilot implements a feature, review behavior before reviewing style. This step prints expected and actual values for a small formatter.

Ask Copilot for the method:

```text
Write a C# method named FormatBalanceMessage.

Requirements:
- Accept string name and decimal balance.
- Throw ArgumentException named name when name is null, empty, or whitespace.
- Trim the name.
- Return a message in the format: Avery owes $125.00
```

Then add explicit checks:

```csharp
Verify("Avery", 125m, "Avery owes $125.00");
Verify("Blair", 0m, "Blair owes $0.00");
```

> **Screenshot placeholder:** Add an image showing the generated formatter and verification calls.

Run Step 4 in the lesson runner. Then change the inputs:

- Try a name with leading and trailing spaces.
- Try an empty name and confirm it throws.
- Try a negative balance and decide whether that should be allowed.
- Try a different culture on your machine and think about whether currency formatting should be culture-specific.

This is the review mindset: generated code does not get a discount. It needs to prove the same behavior hand-written code would need to prove. Most production bugs are not fireworks; they are small assumptions wearing a fake mustache.
