# Chapter 9 Supplemental 03: Connecting to Other Databases

## What This Is

Everything in `Supplemental.01` used `System.Data.SqlClient` - `SqlConnection`, `SqlCommand`, `SqlDataReader` - because it was talking to SQL Server. Every other relational database has its own equivalent provider library with its own prefixed class names, but the exact same shape: a Connection, a Command, a DataReader. Once you've learned the pattern once, applying it to a new provider is almost entirely a matter of swapping the NuGet package and the connection string.

What's being abstracted here is the provider itself. ADO.NET defines a common interface (`IDbConnection`, `IDbCommand`, `IDataReader`) that every provider implements. That's why the code looks nearly identical regardless of which database sits behind it. MongoDB is deliberately included as the exception to demonstrate that the pattern doesn't apply everywhere - document databases have fundamentally different access models.

**Only SQLite actually runs without setup.** It's a file-based, serverless database - this project creates a temporary one, uses it, and deletes it automatically. Every other provider method will print a "could not connect" message rather than crashing the rest of the demo if you haven't set up a server. That's expected. The code is what matters here, not a live connection.

| Provider | NuGet Package | Connection Class | Command Class |
|---|---|---|---|
| SQL Server | (built into .NET Framework) | `SqlConnection` | `SqlCommand` |
| SQLite | `System.Data.SQLite` | `SQLiteConnection` | `SQLiteCommand` |
| MySQL | `MySql.Data` | `MySqlConnection` | `MySqlCommand` |
| PostgreSQL | `Npgsql` | `NpgsqlConnection` | `NpgsqlCommand` |
| Oracle | `Oracle.ManagedDataAccess` | `OracleConnection` | `OracleCommand` |
| ODBC | (built into .NET Framework) | `OdbcConnection` | `OdbcCommand` |

---

## How to Write This Program

Add a shared helper to `Program.cs` - every provider method that might fail calls this instead of crashing:

```csharp
private static void PrintReferenceOnlyMessage(string providerName, Exception ex)
{
    Console.WriteLine($"Could not connect to {providerName} (expected unless you've set one up, see README.md): {ex.Message}");
}
```

---

### Mini-Program 1: SQLite (Actually Runs)

Clear `Main()` and write:

```csharp
string dbPath = Path.Combine(Path.GetTempPath(), $"ch09-sqlite-demo-{Guid.NewGuid():N}.db");
string connectionString = $"Data Source={dbPath};Version=3;";

try
{
    using var connection = new SQLiteConnection(connectionString);
    connection.Open();

    using (var createCmd = new SQLiteCommand(
        "CREATE TABLE MurphysLaws (LawName TEXT, LawText TEXT)", connection))
    {
        createCmd.ExecuteNonQuery();
    }

    using (var insertCmd = new SQLiteCommand(
        "INSERT INTO MurphysLaws (LawName, LawText) VALUES (@LawName, @LawText)", connection))
    {
        insertCmd.Parameters.AddWithValue("@LawName", "Murphy's Law");
        insertCmd.Parameters.AddWithValue("@LawText", "Anything that can go wrong will go wrong.");
        insertCmd.ExecuteNonQuery();
    }

    using var selectCmd = new SQLiteCommand("SELECT LawName, LawText FROM MurphysLaws", connection);
    using var reader = selectCmd.ExecuteReader();

    Console.WriteLine($"Rows in a temporary SQLite database ({dbPath}):");
    while (reader.Read())
        Console.WriteLine($" - {reader.GetString(0)}: {reader.GetString(1)}");
}
finally
{
    if (File.Exists(dbPath)) File.Delete(dbPath);
}

GenericFunctions.Pause();
```

Run it. Notice that `SQLiteConnection`, `SQLiteCommand`, and `SQLiteCommand.ExecuteReader()` are structurally identical to their `Sql*` counterparts from `Supplemental.01`. Create, Open, Command, ExecuteNonQuery/ExecuteReader, Read - same pattern, different class names.

SQLite's connection string is a file path rather than a server address, which is the only real novelty. The temporary path with a `Guid` suffix avoids collisions if you run the program multiple times in quick succession.

### Mini-Program 2: MySQL (Reference)

Clear `Main()` and write:

```csharp
const string connectionString = "Server=localhost;Database=ExternalData;Uid=your_username;Pwd=your_password;";

try
{
    using var connection = new MySqlConnection(connectionString);
    connection.Open();

    using var command = new MySqlCommand("SELECT LawName, LawText FROM MurphysLaws", connection);
    using var reader = command.ExecuteReader();

    Console.WriteLine("Rows from MySQL:");
    while (reader.Read())
        Console.WriteLine($" - {reader.GetString(0)}: {reader.GetString(1)}");
}
catch (Exception ex)
{
    PrintReferenceOnlyMessage("MySQL", ex);
}

GenericFunctions.Pause();
```

`MySqlConnection`, `MySqlCommand` - same shape, different prefix. The connection string format is MySQL-specific (`Server=`, `Database=`, `Uid=`, `Pwd=`). Everything else is identical.

### Mini-Program 3: PostgreSQL (Reference)

Clear `Main()` and write:

```csharp
const string connectionString = "Host=localhost;Database=ExternalData;Username=your_username;Password=your_password;";

try
{
    using var connection = new NpgsqlConnection(connectionString);
    connection.Open();

    // PostgreSQL folds unquoted identifiers to lowercase. "MurphysLaws" as created
    // in SQL Server would be referenced as "murphyslaws" here unless quoted.
    using var command = new NpgsqlCommand("SELECT LawName, LawText FROM MurphysLaws", connection);
    using var reader = command.ExecuteReader();

    Console.WriteLine("Rows from PostgreSQL:");
    while (reader.Read())
        Console.WriteLine($" - {reader.GetString(0)}: {reader.GetString(1)}");
}
catch (Exception ex)
{
    PrintReferenceOnlyMessage("PostgreSQL", ex);
}

GenericFunctions.Pause();
```

`NpgsqlConnection`, `NpgsqlCommand`. The identifier-folding note in the comment is a real PostgreSQL behavior worth internalizing: `SELECT * FROM MurphysLaws` and `SELECT * FROM murphyslaws` are the same query, but `SELECT * FROM "MurphysLaws"` is case-sensitive. SQL Server doesn't do this by default.

### Mini-Program 4: Oracle (Reference)

Clear `Main()` and write:

```csharp
const string connectionString = "User Id=your_username;Password=your_password;Data Source=localhost:1521/XEPDB1;";

try
{
    using var connection = new OracleConnection(connectionString);
    connection.Open();

    using var command = new OracleCommand("SELECT LawName, LawText FROM MurphysLaws", connection);
    using var reader = command.ExecuteReader();

    Console.WriteLine("Rows from Oracle:");
    while (reader.Read())
        Console.WriteLine($" - {reader.GetString(0)}: {reader.GetString(1)}");
}
catch (Exception ex)
{
    PrintReferenceOnlyMessage("Oracle", ex);
}

GenericFunctions.Pause();
```

`OracleConnection`, `OracleCommand`. Oracle's connection string format uses `Data Source` for host and service name. The underlying pattern is the same.

### Mini-Program 5: ODBC (Reference)

Clear `Main()` and write:

```csharp
const string connectionString = "DSN=YourOdbcDataSourceName;Uid=your_username;Pwd=your_password;";

try
{
    using var connection = new OdbcConnection(connectionString);
    connection.Open();

    using var command = new OdbcCommand("SELECT LawName, LawText FROM MurphysLaws", connection);
    using var reader = command.ExecuteReader();

    Console.WriteLine("Rows via ODBC:");
    while (reader.Read())
        Console.WriteLine($" - {reader.GetString(0)}: {reader.GetString(1)}");
}
catch (Exception ex)
{
    PrintReferenceOnlyMessage("ODBC", ex);
}

GenericFunctions.Pause();
```

ODBC is a generic bridge layer - the same `OdbcConnection`/`OdbcCommand` classes work against SQL Server, Access, Excel, legacy mainframe systems, and anything else that exposes an ODBC driver. The `DSN` (Data Source Name) is configured in Windows's ODBC Data Source Administrator, outside of the application itself.

### Mini-Program 6: MongoDB (No SQL, No DataReader)

Clear `Main()` and write:

```csharp
const string connectionString = "mongodb://localhost:27017";

try
{
    var client = new MongoClient(connectionString);
    var database = client.GetDatabase("ExternalData");
    var collection = database.GetCollection<BsonDocument>("MurphysLaws");

    var newLaw = new BsonDocument
    {
        { "LawName", "Murphy's Law" },
        { "LawText", "Anything that can go wrong will go wrong." }
    };
    collection.InsertOne(newLaw);

    var documents = collection.Find(new BsonDocument()).ToList();

    Console.WriteLine("Documents in the MurphysLaws collection:");
    foreach (var document in documents)
        Console.WriteLine($" - {document["LawName"]}: {document["LawText"]}");
}
catch (Exception ex)
{
    PrintReferenceOnlyMessage("MongoDB", ex);
}

GenericFunctions.Pause();
```

No `Connection`, `Command`, or `DataReader`. No SQL text. No rows and columns.

MongoDB stores **documents** - BSON (a binary JSON-like format) objects in a **collection**, analogous to a table but without a fixed schema. Every document in a collection can have different fields. A `MongoClient` connects to the server; `GetDatabase()` and `GetCollection<T>()` navigate to the collection; `InsertOne()` and `Find()` operate on it.

This is genuinely different from everything above, and that's the reason it's included. Recognizing when a problem calls for a document database versus a relational one - and knowing that the access pattern looks nothing like ADO.NET - is worth more than being able to run a query against a live MongoDB server.

---

## Try It Yourself

Run the project as-is - SQLite will succeed and every other method will print a clear "could not connect" message. That's expected. Then pick one provider from `README.md`, set up a real (even temporary, e.g. via Docker) server for it, update its connection string, and watch that one method succeed too.

---

## Summary: Relational vs. Document

| | Relational (SQL) | Document (MongoDB) |
|---|---|---|
| Access pattern | `Connection`/`Command`/`DataReader` | `MongoClient`/`GetCollection`/`Find` |
| Query language | SQL | Query filter documents |
| Schema | Fixed, defined ahead of time | Flexible, per-document |
| ADO.NET pattern | Yes | No |
| Best for | Structured data with relationships | Flexible or hierarchical data |

SQLite is worth knowing specifically as the "no server needed" option for local caching, testing, and small self-contained applications. ODBC is the fallback bridge when a system doesn't have a dedicated .NET provider.

---

## Takeaways

- Every relational database provider follows the same Connection/Command/DataReader shape. Swap the NuGet package and the connection string; the rest is muscle memory.
- SQLite is serverless and file-based - ideal for local development, testing, and small applications that don't need a server.
- PostgreSQL folds unquoted identifiers to lowercase; quote them if case matters.
- ODBC is a generic bridge that works against anything with an ODBC driver.
- MongoDB is a fundamentally different paradigm: documents in collections, no SQL, no rows and columns.
- A provider that can't connect should fail gracefully and print a clear message, not crash the whole demo.
