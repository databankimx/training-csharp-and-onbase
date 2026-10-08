---
title: "SQL Injection - Run From VS"
chapter: 9
index: 1
dependencies: []
---

```csharp
using System;

// This project requires a live SQL Server database and cannot run
// in the LessonRunner. Run it directly from Visual Studio instead.
//
// Setup: same ExternalData database as Supplemental 01. Keep ExternalData.bak
// handy -- the walkthrough ends with a step that deletes a table's rows on purpose.
//
// What the project demonstrates:
//
// The mechanism: if user input is concatenated directly into a SQL string, the database
// cannot distinguish "data the user typed" from "additional SQL the user wrote."
// Both arrive as characters; the database executes what the text says.
//
// SafeDatabaseUtility uses a parameterized query. The search value is sent to SQL Server
// separately from the query text, tagged as pure data. It can never be parsed as SQL.
//
// UnsafeDatabaseUtility concatenates the user input directly into the SQL string.
// Both classes look identical from the outside -- same constructor, same method signature.
//
// The console loop passes every search term through BOTH queries side by side.
// The moment they disagree is impossible to miss.
//
// The attack playbook (run against MurphysLaws, a sandbox that exists to be broken):
//   Normal lookup:       Murphy's Law
//   Bypass WHERE:        Murphy's Law' OR '1' = '1
//   Fingerprint engine:  Murphy's Law'; WAITFOR DELAY '0:0:5';--
//   List all tables:     ' UNION (SELECT TABLE_SCHEMA+'.'+TABLE_NAME FROM INFORMATION_SCHEMA.TABLES);--
//   Dump all columns:    ' UNION (SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='MurphysLaws');--
//   Dump all rows:       ' UNION (SELECT RTRIM(LawName)+'|'+RTRIM(LawText) FROM dbo.MurphysLaws);--
//   Delete everything:   '; DELETE FROM dbo.MurphysLaws;--  (restore from backup afterward)
//
// The fix is parameterization. It is not input sanitisation. Hand-rolled escaping loses
// eventually -- parameterization refuses to play that game at all.

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
