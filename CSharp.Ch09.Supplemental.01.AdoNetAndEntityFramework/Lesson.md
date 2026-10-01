# Chapter 9 Supplemental 01: ADO.NET and Entity Framework

## What This Is

This project needs a real SQL Server database before any of it runs. See `README.md` in this folder for setup instructions -- restoring `ExternalData.bak`, verifying the connection string, and creating one stored procedure. The rest of this lesson assumes that's done.

ADO.NET is the foundational .NET data access API. Entity Framework is built on top of it. Every other .NET data library is built on top of it. It's turtles most of the way down, and ADO.NET is the turtle at the bottom.

---

## How to Write This Program

The models are already in the project:

- `MurphysLaw` -- maps to `dbo.MurphysLaws`, with `[Key]`, `[Column]`, `[Table]` annotations
- `ZipCodeRecord` -- maps to `dbo.ZipCodes`
- `ExternalDataContext : DbContext` -- the EF context, with `Database.SetInitializer<>(null)` because the database already exists and EF should never try to create or alter its schema

The connection string lives in `App.config` under the name `"ExternalData"`.

---

## Part 1: ADO.NET

The ADO.NET shape is always the same: open a Connection, create a Command, execute it, and process the results. The exact class names depend on the provider; here they're all `Sql*` for SQL Server.

### Mini-Program 1: Connection

Clear `Main()` and write:

```csharp
string connectionString = ConfigurationManager.ConnectionStrings["ExternalData"].ConnectionString;

using var connection = new SqlConnection(connectionString);
Console.WriteLine($"State before Open(): {connection.State}");

connection.Open();
Console.WriteLine($"State after Open(): {connection.State}");
Console.WriteLine($"Database: {connection.Database}");
Console.WriteLine($"DataSource: {connection.DataSource}");
Console.WriteLine($"ServerVersion: {connection.ServerVersion}");

connection.Close();
Console.WriteLine($"State after Close(): {connection.State}");

GenericFunctions.Pause();
```

Run it. `State` transitions from `Closed` to `Open` to `Closed`. The `using` statement ensures `Dispose()` runs even if an exception occurs between `Open()` and `Close()` -- releasing the underlying connection back to the pool. Always use `using` with database connections.

### Mini-Program 2: ExecuteReader()

Clear `Main()` and write:

```csharp
string connectionString = ConfigurationManager.ConnectionStrings["ExternalData"].ConnectionString;

using var connection = new SqlConnection(connectionString);
connection.Open();

using var command = new SqlCommand(
    "SELECT LawID, LawName, LawText FROM dbo.MurphysLaws ORDER BY LawID", connection);
using var reader = command.ExecuteReader();

Console.WriteLine("Murphy's Laws:");
while (reader.Read())
{
    short lawId    = reader.GetInt16(0);
    string lawName = reader.GetString(1);
    string lawText = reader.GetString(2);
    Console.WriteLine($" - [{lawId}] {lawName}: {lawText}");
}

GenericFunctions.Pause();
```

Run it. `ExecuteReader()` returns a `SqlDataReader` -- a fast, forward-only, read-only stream of results. It doesn't hold the whole result set in memory; it fetches rows as you call `Read()`. The connection must stay open for the lifetime of the reader.

Columns are accessed by ordinal position (0, 1, 2) or by name (`reader["LawName"]`). By-ordinal is faster but fragile against column reordering; by-name is more readable but slightly slower. Pick one consistently.

### Mini-Program 3: ExecuteScalar()

Clear `Main()` and write:

```csharp
string connectionString = ConfigurationManager.ConnectionStrings["ExternalData"].ConnectionString;

using var connection = new SqlConnection(connectionString);
connection.Open();

using var command = new SqlCommand("SELECT COUNT(*) FROM dbo.ZipCodes", connection);
object result = command.ExecuteScalar();
int zipCodeCount = Convert.ToInt32(result);

Console.WriteLine($"Total ZipCodes rows: {zipCodeCount}");
GenericFunctions.Pause();
```

Run it. `ExecuteScalar()` is the right tool specifically when a query returns one row and one column -- a `COUNT(*)`, a `MAX()`, a single computed value. It's more efficient than `ExecuteReader()` for that narrow case since it doesn't set up the full reader machinery for a single value. It returns `object`, hence the `Convert.ToInt32` -- the type depends on the query, not on a generic type parameter.

### Mini-Program 4: Parameterized INSERT with ExecuteNonQuery()

Clear `Main()` and write:

```csharp
string connectionString = ConfigurationManager.ConnectionStrings["ExternalData"].ConnectionString;

using var connection = new SqlConnection(connectionString);
connection.Open();

// Parameter placeholders (@LawName, @LawText) instead of string concatenation.
// See Supplemental.02.SqlInjection for a hands-on demonstration of why this matters.
const string sql = "INSERT INTO dbo.MurphysLaws (LawName, LawText) VALUES (@LawName, @LawText)";

using var command = new SqlCommand(sql, connection);
command.Parameters.Add(new SqlParameter("@LawName", SqlDbType.VarChar, 50)
    { Value = "Segal's Law" });
command.Parameters.Add(new SqlParameter("@LawText", SqlDbType.VarChar, 250)
    { Value = "A man with a watch knows what time it is. A man with two watches is never sure." });

int rowsAffected = command.ExecuteNonQuery();
Console.WriteLine($"ExecuteNonQuery() inserted {rowsAffected} row(s).");
GenericFunctions.Pause();
```

Run it. `ExecuteNonQuery()` is the right tool for `INSERT`, `UPDATE`, and `DELETE` -- statements that don't return rows, only a count of how many rows were affected.

Parameters specify type and length explicitly (`SqlDbType.VarChar, 50`) rather than letting ADO.NET infer them. Explicit types avoid edge cases where inference produces the wrong SQL type or sends data with a different collation than the column expects.

### Mini-Program 5: DataAdapter and DataSet

Clear `Main()` and write:

```csharp
string connectionString = ConfigurationManager.ConnectionStrings["ExternalData"].ConnectionString;

using var connection = new SqlConnection(connectionString);
using var adapter = new SqlDataAdapter(
    "SELECT State, City, ZipCode FROM dbo.ZipCodes ORDER BY State, City", connection);

var dataSet = new DataSet();

// Fill() opens the connection, runs the query, populates the DataSet,
// and closes the connection again -- all in one call.
adapter.Fill(dataSet, "ZipCodes");

DataTable zipCodesTable = dataSet.Tables["ZipCodes"];
Console.WriteLine($"Filled {zipCodesTable?.Rows.Count} rows into the DataTable.");

Console.WriteLine("\nFirst 5 rows:");
foreach (DataRow row in zipCodesTable?.Rows.Cast<DataRow>().Take(5) ?? Enumerable.Empty<DataRow>())
    Console.WriteLine($" - {row["City"]}, {row["State"]} {row["ZipCode"]}");

GenericFunctions.Pause();
```

Run it. The key distinction from a `DataReader`: the `DataTable` you get back is **fully disconnected**. The connection was opened, used, and closed inside `Fill()`. You can keep reading, modifying, or passing around the `DataTable` long after the database connection itself has closed. That's useful for desktop and offline scenarios; it's also why `DataSet`/`DataTable` dominated early .NET web development before LINQ to SQL and EF arrived.

### Mini-Program 6: Stored Procedure via ADO.NET

Clear `Main()` and write:

```csharp
string connectionString = ConfigurationManager.ConnectionStrings["ExternalData"].ConnectionString;

using var connection = new SqlConnection(connectionString);
connection.Open();

using var command = new SqlCommand("dbo.GetZipCodesByState", connection)
{
    CommandType = CommandType.StoredProcedure
};
command.Parameters.Add(new SqlParameter("@State", SqlDbType.VarChar, 20) { Value = "TX" });

try
{
    using var reader = command.ExecuteReader();
    Console.WriteLine("Zip codes in TX via stored procedure:");
    while (reader.Read())
        Console.WriteLine($" - {reader["City"]}, {reader["State"]} {reader["ZipCode"]}");
}
catch (SqlException ex) when (ex.Number == 2812)
{
    Console.WriteLine("Stored procedure not found. See README.md step 4 to create it.");
}

GenericFunctions.Pause();
```

Run it. Two things change from a plain query: `CommandType.StoredProcedure` tells ADO.NET to call the procedure by name rather than execute the string as inline SQL, and parameters map to the procedure's `@State` parameter by name.

The `when (ex.Number == 2812)` exception filter catches specifically "stored procedure not found" (SQL Server error 2812), so a missed setup step produces a clear message rather than an unhandled exception crashing everything else.

---

## Part 2: Entity Framework

EF sits on top of ADO.NET and maps database tables to ordinary C# classes, letting you write LINQ queries instead of hand-written SQL for most everyday operations. This project uses "Code First against an existing database": the context maps to the already-restored tables; EF never tries to create or alter the schema.

### Mini-Program 7: Select Records

Clear `Main()` and write:

```csharp
using var context = new ExternalDataContext();

// EF translates this LINQ query into SQL and runs it when actually enumerated (ToList()).
var laws = context.MurphysLaws
    .Where(law => law.LawName.Contains("Law"))
    .OrderBy(law => law.LawId)
    .ToList();

Console.WriteLine($"Laws with \"Law\" in the name ({laws.Count} found):");
foreach (var law in laws)
    Console.WriteLine($" - [{law.LawId}] {law.LawName}: {law.LawText}");

GenericFunctions.Pause();
```

Run it. LINQ instead of SQL, strongly-typed properties instead of string column names, no manual reader management.

The query is **deferred** -- nothing hits the database until `ToList()` forces evaluation. That's the same deferred execution covered in Chapter 10's LINQ content; here it's expressed as a database query rather than an in-memory operation.

### Mini-Program 8: Insert a Record

Clear `Main()` and write:

```csharp
using var context = new ExternalDataContext();

var newLaw = new MurphysLaw
{
    LawName = "Muphry's Law",
    LawText = "If you write anything criticizing editing or proofreading, there will be a fault in what you have written."
};

// Add() stages the entity in memory. Nothing hits the database yet.
context.MurphysLaws.Add(newLaw);

// SaveChanges() generates and runs the INSERT.
int rowsAffected = context.SaveChanges();
Console.WriteLine($"Inserted {rowsAffected} row(s). New LawID: {newLaw.LawId}");

GenericFunctions.Pause();
```

Run it. Note `newLaw.LawId` is populated after `SaveChanges()` -- `LawID` is an identity column, and EF reads back the database-generated value for you automatically.

### Mini-Program 9: Update a Record

Clear `Main()` and write:

```csharp
using var context = new ExternalDataContext();

var law = context.MurphysLaws.FirstOrDefault(l => l.LawName == "Segal's Law");
if (law == null)
{
    Console.WriteLine("Segal's Law not found -- run Mini-Program 4 first.");
    return;
}

law.LawText = "A man with a watch knows what time it is. A man with two watches is never quite sure.";

// No explicit Update() call. EF tracks changes to loaded entities.
// SaveChanges() generates the UPDATE for anything that changed since it was fetched.
int rowsAffected = context.SaveChanges();
Console.WriteLine($"Updated {rowsAffected} row(s).");
GenericFunctions.Pause();
```

Run it. EF's change tracking is the key feature here -- you modify a property on a loaded entity and call `SaveChanges()`. EF compares the current state against a snapshot taken at load time, generates a targeted `UPDATE` for only the columns that changed, and runs it. No `UPDATE` statement, no column lists, no `WHERE` clause written by hand.

### Mini-Program 10: Delete a Record

Clear `Main()` and write:

```csharp
using var context = new ExternalDataContext();

var law = context.MurphysLaws.FirstOrDefault(l => l.LawName == "Muphry's Law");
if (law == null)
{
    Console.WriteLine("Muphry's Law not found -- run Mini-Program 8 first.");
    return;
}

context.MurphysLaws.Remove(law);
int rowsAffected = context.SaveChanges();
Console.WriteLine($"Deleted {rowsAffected} row(s).");
GenericFunctions.Pause();
```

Run it. `Remove()` stages the deletion; `SaveChanges()` executes the `DELETE`.

### Mini-Program 11: Stored Procedure via EF

Clear `Main()` and write:

```csharp
using var context = new ExternalDataContext();

try
{
    // Database.SqlQuery<T>() runs raw SQL (including stored procedure calls) and maps
    // the results onto T the same way a LINQ query would.
    var results = context.Database
        .SqlQuery<ZipCodeRecord>("EXEC dbo.GetZipCodesByState @State",
            new SqlParameter("@State", "TX"))
        .ToList();

    Console.WriteLine($"Zip codes in TX via EF ({results.Count} found):");
    foreach (var zip in results)
        Console.WriteLine($" - {zip.City}, {zip.State} {zip.ZipCode}");
}
catch (Exception ex) when (ex.InnerException is SqlException { Number: 2812 })
{
    Console.WriteLine("Stored procedure not found. See README.md step 4.");
}

GenericFunctions.Pause();
```

Run it. `Database.SqlQuery<T>()` is EF's escape hatch for raw SQL -- stored procedures, complex joins, or anything LINQ can't express cleanly. Results are materialized as typed objects the same way a LINQ query would be.

---

## Worth Knowing: `Database.SqlQuery<T>()` Does Not Honor `[Column]` Mappings

`ZipCodeRecord.ZipCode` matches the database column name exactly, so no `[Column]` attribute is needed. But if the property were named `Zip` with `[Column("ZipCode")]` on it, EF's normal LINQ pipeline (a `DbSet<T>` query) would still work fine -- it reads the mapping metadata. `Database.SqlQuery<T>()` would throw `EntityCommandExecutionException: ... does not have a corresponding column in the data reader`, because `SqlQuery<T>()` performs simple name-based matching directly against the raw column names in the `DataReader`. It does not consult the same mapping metadata a `DbSet<T>` query uses.

The practical rule: when a class will be used with `Database.SqlQuery<T>()` against a stored procedure or raw SQL, name its properties to match the actual result-set column names directly. Don't rely on `[Column(...)]` to bridge a mismatch -- it won't be consulted.

Also worth noting: `ZipCodeRecord` is named that rather than `ZipCode` because C# does not allow a property to share its enclosing type's exact name (CS0542). A class named `ZipCode` could never have a property also named `ZipCode`. Renaming the class sidesteps the restriction entirely.

---

## Takeaways

- Always use `using` with database connections. A connection not returned to the pool hurts everyone.
- Use `ExecuteReader()` for result sets, `ExecuteScalar()` for single values, `ExecuteNonQuery()` for INSERT/UPDATE/DELETE.
- Always use parameterized queries. See `Supplemental.02.SqlInjection` for exactly what happens when you don't.
- `DataAdapter.Fill()` produces a disconnected `DataTable` -- useful when you need to keep the data around after the connection closes.
- EF's change tracking generates targeted `UPDATE` statements for only the columns that changed.
- `SaveChanges()` is where all staged ADO.NET operations actually hit the database. Nothing happens before it.
- `Database.SqlQuery<T>()` is EF's escape hatch for stored procedures and raw SQL.
- `Database.SqlQuery<T>()` does not consult `[Column]` mapping metadata -- property names must match result-set column names directly.
- EF populates identity-column primary keys on the entity object after `SaveChanges()`.
