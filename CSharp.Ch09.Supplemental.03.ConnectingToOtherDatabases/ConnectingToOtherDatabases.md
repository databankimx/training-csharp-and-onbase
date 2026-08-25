# Connecting to Other Databases

## Introduction

Every ADO.NET example so far in this chapter used SQL Server specifically. This lesson shows the same pattern applied to five other relational databases, plus one that works completely differently. **See `README.md`** if you want to actually set up a real server for any of these, only SQLite runs without one.

---

## Same Shape, Different Names

```csharp
// SQL Server
new SqlConnection(connectionString);
new SqlCommand(sql, connection);

// MySQL
new MySqlConnection(connectionString);
new MySqlCommand(sql, connection);

// PostgreSQL
new NpgsqlConnection(connectionString);
new NpgsqlCommand(sql, connection);

// Oracle
new OracleConnection(connectionString);
new OracleCommand(sql, connection);
```

Once you know the `Connection`/`Command`/`ExecuteReader()` pattern from one database, you basically know it for all of them. Switching providers is mostly: install a different NuGet package, change the class name prefix, update the connection string. The actual logic of your code barely changes.

---

## SQLite: No Server Needed at All

```csharp
using var connection = new SQLiteConnection("Data Source=mydata.db;Version=3;");
```

SQLite is different from the others in a genuinely useful way: it's just a file. No server to install, no network connection, nothing to configure. That's why this is the only method in this lesson that runs successfully with zero setup, it's a real, working database, just one that lives entirely in a single file on disk. Good to know for small local apps, caching, or anywhere you want "a real database" without wanting "a real database server."

---

## ODBC: Connects to Almost Anything

```csharp
using var connection = new OdbcConnection("DSN=SomeDataSource;...");
```

ODBC isn't its own database, it's a generic standard that any data source can implement a driver for. That means the same `OdbcConnection`/`OdbcCommand` code can talk to Microsoft Access, an Excel file, or an older system that never got its own modern .NET library, just by pointing it at a different driver. Worth knowing about as the "connects to almost anything" fallback.

---

## MongoDB: A Completely Different Approach

```csharp
var client = new MongoClient("mongodb://localhost:27017");
var database = client.GetDatabase("ExternalData");
var collection = database.GetCollection<BsonDocument>("MurphysLaws");

collection.InsertOne(new BsonDocument { { "LawName", "Murphy's Law" }, { "LawText", "..." } });
var documents = collection.Find(new BsonDocument()).ToList();
```

Everything above this is a relational database, tables, rows, columns, a fixed schema. MongoDB is a document database, there's no SQL, no `Connection`/`Command` objects, no predefined table structure. Instead, you store *documents* (flexible, JSON-like data) in a *collection*. Each document can even have a different shape from the others in the same collection if your application needs that. This is the core tradeoff between relational and document databases: relational databases enforce structure up front, document databases let structure be flexible, at the cost of the guarantees a fixed schema gives you.

---

## Try It Yourself

Run the project as-is, `SQLite` will succeed and every other method will print a clear "could not connect" message, that's expected. Then pick one provider from `README.md`, set up a real (even temporary, e.g. via Docker) server for it, update its connection string, and watch that one method succeed too.
