---
title: "ADO.NET and Entity Framework - Run From VS"
chapter: 9
index: 1
dependencies: []
---

```csharp
using System;

// This project requires a live SQL Server database and cannot run
// in the LessonRunner. Run it directly from Visual Studio instead.
//
// Setup: restore ExternalData.bak into a SQL Server instance, then verify
// the connection string in App.config points to it. See the project's
// README.md for the full steps.
//
// What the project demonstrates once the database is connected:
//
// ADO.NET (direct, lowest-level):
//   SqlConnection  -- open/close, connection state, server metadata
//   ExecuteReader  -- forward-only result set, column access by ordinal or name
//   ExecuteScalar  -- single-value queries (COUNT, MAX, etc.)
//   ExecuteNonQuery -- INSERT / UPDATE / DELETE, returns rows affected
//   Parameters     -- parameterized queries; see Supplemental 02 for why this is mandatory
//   DataAdapter + DataSet -- disconnected result caching via Fill()
//   Stored procedure -- CommandType.StoredProcedure
//
// Entity Framework 6 (Code First against existing schema):
//   LINQ queries   -- deferred; nothing hits the database until ToList() forces evaluation
//   Add + SaveChanges    -- INSERT; identity columns populated on the entity after save
//   Change tracking      -- UPDATE only the columns that changed, automatically
//   Remove + SaveChanges -- DELETE
//   Database.SqlQuery<T> -- EF escape hatch for stored procedures and raw SQL;
//                           does NOT consult [Column] mapping metadata
//
// The source is in Program.cs alongside ExternalDataContext.cs and the model classes.

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("This project requires a live SQL Server connection.");
        Console.WriteLine("Run it from Visual Studio after restoring ExternalData.bak.");
        Console.WriteLine("See the project README.md for setup instructions.");
    }
}
```
