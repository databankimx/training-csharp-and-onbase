---
title: "IQueryable vs. IEnumerable - Run From VS"
chapter: 10
index: 1
dependencies: []
---

```csharp
using System;

// This project requires a live SQL Server connection (same ExternalData database
// as Ch09 Supplemental 01) and cannot run in the LessonRunner.
// Run it directly from Visual Studio.
//
// What the project demonstrates:
//
// IEnumerable<T> (LINQ to Objects):
//   db.MurphysLaws.ToList().Where(law => ...)
//   Where() takes a Func<T, bool> -- a compiled C# delegate.
//   Filtering runs IN THIS PROCESS, one object at a time, in C#.
//   .ToList() fetches every row first; the Where() discards most of them in memory.
//
// IQueryable<T> (LINQ to Entities / EF):
//   db.MurphysLaws.Where(law => ...)
//   Where() takes an Expression<Func<T, bool>> -- a DATA STRUCTURE describing the lambda.
//   EF walks that structure and translates it into a SQL WHERE clause.
//   Only the matching rows ever leave the database.
//   .ToString() on an EF IQueryable<T> prints the actual SQL EF generated.
//
// Untranslatable expression:
//   db.MurphysLaws.Where(law => IsPalindrome(law.LawName))
//   IsPalindrome() is an arbitrary C# method -- EF cannot translate it to SQL.
//   Throws NotSupportedException.
//
// .AsEnumerable() -- the deliberate switch point:
//   db.MurphysLaws
//     .Where(law => law.LawText.Length < 60)   // IQueryable -- translated to SQL
//     .AsEnumerable()                           // switch: rest runs client-side
//     .Where(law => IsPalindrome(law.LawName))  // IEnumerable -- ordinary C#
//   Server does the cheap filter; client does the untranslatable one.
//   Order matters: server-translatable filters BEFORE .AsEnumerable().
//
// The same C# syntax produces fundamentally different execution paths depending
// on whether the source is IEnumerable<T> or IQueryable<T>.
// This is why Expression<Func<T, bool>> exists as a distinct type from Func<T, bool>.

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("This project requires a live SQL Server connection.");
        Console.WriteLine("Run it from Visual Studio after restoring ExternalData.bak.");
        Console.WriteLine("See the project README.md and Lesson.md for the full walkthrough.");
    }
}
```
