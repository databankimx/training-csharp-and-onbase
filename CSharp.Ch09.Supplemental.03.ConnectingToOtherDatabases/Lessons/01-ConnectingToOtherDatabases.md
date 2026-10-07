---
title: "Connecting to Other Databases - ADO.NET Provider Pattern"
chapter: 9
index: 1
dependencies: []
---

```csharp
using System;

// This project uses provider-specific NuGet packages not available in the LessonRunner.
// Run it directly from Visual Studio. Only the SQLite demo actually connects to a
// database -- every other provider prints a "could not connect" message unless you
// have a matching server running, which is expected and intentional.
//
// The core lesson -- every relational provider follows the same ADO.NET shape:
//
//   Provider      NuGet package              Connection class     Command class
//   SQL Server    (built into .NET Framework) SqlConnection        SqlCommand
//   SQLite        System.Data.SQLite          SQLiteConnection     SQLiteCommand
//   MySQL         MySql.Data                  MySqlConnection      MySqlCommand
//   PostgreSQL    Npgsql                      NpgsqlConnection     NpgsqlCommand
//   Oracle        Oracle.ManagedDataAccess     OracleConnection     OracleCommand
//   ODBC          (built into .NET Framework) OdbcConnection       OdbcCommand
//
// Swap the NuGet package and the connection string; the Open/Command/Execute/Read
// pattern is muscle memory from Supplemental 01.
//
// Key per-provider notes:
//   SQLite:      File-based, serverless -- connection string is a file path.
//   PostgreSQL:  Folds unquoted identifiers to lowercase; quote them if case matters.
//   ODBC:        Generic bridge via DSN configured in Windows ODBC Data Source Administrator.
//   MongoDB:     No Connection, Command, or DataReader. No SQL. Documents in collections,
//                accessed via MongoClient -> GetDatabase -> GetCollection -> Find/Insert.
//                Deliberately included to show that the ADO.NET pattern does NOT apply everywhere.
//
// The runnable SQLite demo creates a temp .db file, inserts a row, reads it back,
// and deletes the file -- no server required.

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("Run this project from Visual Studio to see the provider demos.");
        Console.WriteLine("Only the SQLite section connects to a real database.");
        Console.WriteLine("All other providers print a 'could not connect' message unless");
        Console.WriteLine("you have a matching server running locally.");
    }
}
```
