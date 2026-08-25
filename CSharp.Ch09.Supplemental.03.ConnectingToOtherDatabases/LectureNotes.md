# Chapter 9 Supplemental 03: Connecting to Other Databases

## What This Is

Every other ADO.NET example in this chapter used `System.Data.SqlClient`, specifically for SQL Server. This project rounds that out: the same `Connection`/`Command`/`DataReader` pattern applied to five other relational databases, plus one genuinely different, non-relational one. **See `README.md`** for how to set up a real server for any of these, only `SQLite` is runnable without one.

---

## The Pattern Doesn't Change, Only the Names Do

```csharp
// SQL Server (covered in Supplemental 01)
using var connection = new SqlConnection(connectionString);
using var command = new SqlCommand(sql, connection);

// MySQL
using var connection = new MySqlConnection(connectionString);
using var command = new MySqlCommand(sql, connection);

// PostgreSQL
using var connection = new NpgsqlConnection(connectionString);
using var command = new NpgsqlCommand(sql, connection);

// Oracle
using var connection = new OracleConnection(connectionString);
using var command = new OracleCommand(sql, connection);
```

This is the real payoff of learning ADO.NET's shape once: `Connection.Open()`, build a `Command`, call `ExecuteReader()`/`ExecuteScalar()`/`ExecuteNonQuery()`, exactly as covered in `CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework`. Moving to a different relational database is mostly a matter of installing a different NuGet package and swapping the connection string, the actual C# code barely changes.

---

## SQLite: The One That Actually Runs

```csharp
string dbPath = Path.Combine(Path.GetTempPath(), $"ch09-sqlite-demo-{Guid.NewGuid():N}.db");
using var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
```

SQLite is fundamentally different from the others in one important way: it's **file-based and serverless**. There's no separate database process to install, configure, or connect to over a network, the "database" is just a file, and the `System.Data.SQLite` library reads and writes it directly. That's why this is the one method in this project that runs successfully with zero setup, it creates a temporary file, uses it, and deletes it when done. Worth knowing as a genuinely useful, lightweight option for local caching, configuration storage, or small self-contained applications that don't need a real database server at all.

---

## ODBC: A Bridge, Not a Database

```csharp
using var connection = new OdbcConnection("DSN=YourOdbcDataSourceName;...");
using var command = new OdbcCommand(sql, connection);
```

Unlike the others, ODBC isn't tied to one specific database engine. It's a generic standard, .NET Framework's built-in `System.Data.Odbc` can talk to *anything* with an ODBC driver installed, Microsoft Access, Excel files, or older legacy systems that never got a modern, dedicated .NET provider. Worth knowing as the fallback option when a system you need to connect to doesn't have anything more specific available.

---

## MongoDB: A Genuinely Different Shape

```csharp
var client = new MongoClient(connectionString);
var database = client.GetDatabase("ExternalData");
var collection = database.GetCollection<BsonDocument>("MurphysLaws");

var newLaw = new BsonDocument { { "LawName", "Murphy's Law" }, { "LawText", "..." } };
collection.InsertOne(newLaw);

var documents = collection.Find(new BsonDocument()).ToList();
```

Everything above this point followed the same `Connection`/`Command`/`DataReader` shape, just with different class name prefixes. MongoDB breaks that pattern entirely, deliberately included here to make that contrast obvious. There's no SQL text anywhere. No `Connection` or `Command` objects. No rows and columns, no schema to define ahead of time. Instead: a `MongoClient` connects to the server, an `IMongoDatabase` represents a database, and an `IMongoCollection<T>` holds documents (BSON, a binary JSON-like format) that can each have a completely different shape from one another if you want them to.

This is the real distinction between relational (SQL) and document (NoSQL) databases, worth understanding conceptually even if you never end up using MongoDB specifically: relational databases require you to define your schema (tables, columns, types) before you can store anything, document databases let each document carry its own shape, flexible, but without the same structural guarantees a relational schema enforces.

---

## Handling "It's Not Set Up" Gracefully

```csharp
try
{
    using var connection = new MySqlConnection(connectionString);
    connection.Open();
    ...
}
catch (Exception ex)
{
    PrintReferenceOnlyMessage("MySQL", ex);
}
```

Every method past `UsingSqlite()` is wrapped this way on purpose. Since most people running this lesson won't have five different database servers installed, letting a connection failure crash the whole program (or skip every method queued after the failed one) would make this project frustrating to actually read through. Each provider's failure is caught, reported clearly, and the demo moves on to the next one, worth noticing as a reasonable general pattern: a demo meant to be explored, not just executed top to bottom, should degrade gracefully when a piece of it genuinely can't run in the current environment.
