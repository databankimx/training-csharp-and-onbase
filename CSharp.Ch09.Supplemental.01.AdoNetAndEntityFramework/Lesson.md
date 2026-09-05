# Chapter 9 Supplemental 01: ADO.NET and Entity Framework

## What This Is

The core data-access half of Chapter 9's "Consuming Data" section: **raw ADO.NET** (`Connection`, `Command`, `DataReader`, `DataAdapter`/`DataSet`) and **Entity Framework 6** (Select/Insert/Update/Delete, plus calling a stored procedure), all running against a real, restorable SQL Server database named `ExternalData`.

> **Setup required.** See `README.md` in this project's folder before running this. It walks through installing SQL Server, restoring the `ExternalData.bak` backup, and creating the one stored procedure this lesson needs. The demos read from real tables (`MurphysLaws`, `ZipCodes`, `Numbers`, `Phrases`, `TestItems`) and will fail without them.

From the Chapter Notes:

```
ADO.NET is the foundational .NET data access API everything else in this section
  (Entity Framework included) is ultimately built on top of. The core pieces:
- Connection      Represents an open link to the database (SqlConnection)
- Command         Represents a SQL statement or stored procedure to run (SqlCommand)
- DataReader      A fast, forward-only, read-only stream of results (SqlDataReader)
- DataAdapter     Bridges a Command's results into a disconnected, in-memory
					DataSet/DataTable you can work with after the connection closes
```

That last clause of the first sentence matters: **Entity Framework is built on ADO.NET.** EF isn't an alternative to it — it's a layer above it. Understanding the lower layer explains a great deal of the upper one's behavior.

---

## Part 1: ADO.NET

### The `Connection`

`UsingConnection()` exists purely to make the connection lifecycle visible:

```csharp
string connectionString = ConfigurationManager.ConnectionStrings["ExternalData"].ConnectionString;

using var connection = new SqlConnection(connectionString);
Console.WriteLine($"Connection.State before Open(): {connection.State}");

connection.Open();
Console.WriteLine($"Connection.State after Open(): {connection.State}");
Console.WriteLine($"Connection.Database: {connection.Database}");
Console.WriteLine($"Connection.DataSource: {connection.DataSource}");
Console.WriteLine($"Connection.ServerVersion: {connection.ServerVersion}");

connection.Close();
Console.WriteLine($"Connection.State after Close(): {connection.State}");
```

Note two things.

**The connection string comes from `App.config`, not from a literal.** `ConfigurationManager.ConnectionStrings["ExternalData"]` reads the named entry. Hardcoding a connection string means recompiling to point at a different server, and it puts credentials in source control.

**`ServerVersion` is only readable while open.** Several `SqlConnection` members throw if the connection is closed. `State` progresses `Closed` → `Open` → `Closed` across the demo.

The closing comment explains why `using` is there despite the explicit `Close()`:

```csharp
// The "using" statement above ensures Dispose() runs even if an exception
//   happens in between, releasing the underlying connection resources.
```

That's the real point. The explicit `Close()` is for demonstration; `using` is what makes it correct. An exception between `Open()` and `Close()` would otherwise leak the connection.

> **On connection pooling:** `Close()` doesn't usually tear down the TCP connection to SQL Server. ADO.NET pools connections, so `Close()`/`Dispose()` returns it to the pool for reuse. This is why the correct pattern is "open as late as possible, close as early as possible" rather than "hold one connection for the life of the app" — the pool makes opening cheap, and holding connections open exhausts it.

### `ExecuteReader()` — Multi-Row Results

```csharp
using var command = new SqlCommand("SELECT LawID, LawName, LawText FROM dbo.MurphysLaws ORDER BY LawID", connection);
using var reader = command.ExecuteReader();

Console.WriteLine("Murphy's Laws:");
while (reader.Read())
{
	short lawId = reader.GetInt16(0);
	string lawName = reader.GetString(1);
	string lawText = reader.GetString(2);
	Console.WriteLine($" - [{lawId}] {lawName}: {lawText}");
}
```

`Read()` advances one row and returns `false` when exhausted — hence the `while` loop.

**`DataReader` is fast and forward-only.** Once you've read past a row you can't go back, and the connection must stay open the entire time. It streams rows from the server rather than buffering them, which is exactly what you want for a large result set and exactly what makes it unusable after the connection closes.

Note the typed getters: `GetInt16(0)`, `GetString(1)`. These take **ordinal positions**, and `GetInt16` specifically reflects that `LawID` is a SQL `smallint`. Two hazards here:

- **Ordinals depend on the `SELECT` column order.** Reorder the columns in the SQL and the indices silently read the wrong fields. The `reader["City"]` string-indexer form used later in the stored-procedure demo is slower but resilient to that.
- **Typed getters throw on type mismatch.** `GetInt32(0)` on a `smallint` column throws `InvalidCastException`, not a silent widening conversion.

Also worth knowing: `NULL` columns throw on typed getters. Real code needs `reader.IsDBNull(ordinal)` checks for any nullable column. These particular columns aren't nullable, which is why the demo can skip it.

### `ExecuteScalar()` — A Single Value

```csharp
using var command = new SqlCommand("SELECT COUNT(*) FROM dbo.ZipCodes", connection);

// ExecuteScalar() is the right tool specifically when a query returns exactly one
//   value (a single row, single column), like a COUNT(*), a MAX(), or checking for
//   existence. It's more efficient than ExecuteReader() for that narrow case,
//   since it doesn't set up the full reader machinery for one value.
object result = command.ExecuteScalar();
int zipCodeCount = Convert.ToInt32(result);
```

Note the return type is `object`, requiring a conversion. `Convert.ToInt32()` is used rather than a cast because it handles the `DBNull` case gracefully — a direct `(int)` cast would throw if the query returned no rows.

### `ExecuteNonQuery()` — Statements That Don't Return Rows

```csharp
// Note the parameter placeholder (@LawName, @LawText) rather than concatenating
//   the values directly into the SQL string. See
//   CSharp.Ch09.Supplemental.02.SqlInjection for a full, hands-on demonstration of
//   exactly why this distinction matters.
const string sql = "INSERT INTO dbo.MurphysLaws (LawName, LawText) VALUES (@LawName, @LawText)";

using var command = new SqlCommand(sql, connection);
command.Parameters.Add(new SqlParameter("@LawName", SqlDbType.VarChar, 50) { Value = "Segal's Law" });
command.Parameters.Add(new SqlParameter("@LawText", SqlDbType.VarChar, 250) { Value = "..." });

int rowsAffected = command.ExecuteNonQuery();
```

`ExecuteNonQuery()` returns the **affected row count**, not data. Right for `INSERT`/`UPDATE`/`DELETE`.

**The parameterization is the important part here.** Note that `"Segal's Law"` contains an apostrophe — the exact character that breaks naive string-concatenated SQL. Passed as a parameter, it's simply data and needs no escaping. `Supplemental.02` covers the security consequences in full.

Note also that `SqlParameter` specifies both `SqlDbType.VarChar` and a length. Being explicit helps SQL Server reuse cached execution plans rather than compiling a new one per distinct inferred length.

Three execution modes, summarized:

| Method | Returns | Use for |
|---|---|---|
| `ExecuteReader()` | `SqlDataReader` | Multi-row results |
| `ExecuteScalar()` | `object` (one value) | `COUNT(*)`, `MAX()`, existence checks |
| `ExecuteNonQuery()` | `int` (rows affected) | `INSERT`, `UPDATE`, `DELETE` |

### `DataAdapter` and `DataSet`/`DataTable` — Disconnected Data

```csharp
using var connection = new SqlConnection(connectionString);
using var adapter = new SqlDataAdapter("SELECT State, City, ZipCode FROM dbo.ZipCodes ORDER BY State, City", connection);

var dataSet = new DataSet();

// Fill() opens the connection, runs the query, populates the DataSet, and closes
//   the connection again, all in this one call.
adapter.Fill(dataSet, "ZipCodes");

DataTable zipCodesTable = dataSet.Tables["ZipCodes"];
```

**Note there is no explicit `connection.Open()` in this method.** `Fill()` handles the entire connection lifecycle itself — opening, querying, populating, and closing. (If the connection were already open, `Fill()` politely leaves it open.)

The result is **fully disconnected**. The `DataTable` is a complete in-memory copy you can read and modify long after the connection is gone. That's the fundamental trade against `DataReader`:

| | `DataReader` | `DataSet`/`DataTable` |
|---|---|---|
| Connection | Must stay open | Closed after `Fill()` |
| Memory | One row at a time | Entire result set |
| Direction | Forward-only | Random access, re-readable |
| Mutability | Read-only | Editable, tracks changes |

The demo's row access uses string indexers on untyped `DataRow` objects:

```csharp
foreach (DataRow row in zipCodesTable?.Rows.Cast<DataRow>().Take(5) ?? Enumerable.Empty<DataRow>())
{
	Console.WriteLine($" - {row["City"]}, {row["State"]} {row["ZipCode"]}");
}
```

Note `row["City"]` returns `object`, and a misspelled column name throws at runtime. This is the same lack of type safety `ArrayList` had in the main lesson — `DataSet` is from the same pre-generics era and carries the same costs.

The `?? Enumerable.Empty<DataRow>()` guards against `Tables["ZipCodes"]` returning `null`, which happens if the table name doesn't match. The `.Cast<DataRow>()` is needed because `DataRowCollection` implements only the non-generic `IEnumerable`, so LINQ's `Take()` isn't otherwise available.

> **When to use which today:** `DataSet` is largely superseded — by EF for general work, or by `DataReader` plus your own mapping for read-only performance-sensitive paths. It's worth knowing because it appears in a great deal of existing code, and it genuinely shines in one niche: `DataTable` tracks row state (added/modified/deleted), which `SqlDataAdapter.Update()` can push back to the database as a batch.

### Calling a Stored Procedure via ADO.NET

```csharp
using var command = new SqlCommand("dbo.GetZipCodesByState", connection)
{
	CommandType = CommandType.StoredProcedure
};
command.Parameters.Add(new SqlParameter("@State", SqlDbType.VarChar, 20) { Value = "TX" });

using var reader = command.ExecuteReader();
while (reader.Read())
{
	Console.WriteLine($" - {reader["City"]}, {reader["State"]} {reader["ZipCode"]}");
}
```

Two required changes from an ordinary query: **`CommandType = CommandType.StoredProcedure`**, and the command text becomes the procedure *name* rather than a SQL statement. Omit the `CommandType` and SQL Server receives `"dbo.GetZipCodesByState"` as a literal statement and rejects it.

Note the shift to `reader["City"]` string indexing here rather than the ordinal getters used earlier — a deliberate contrast showing both styles.

---

## Part 2: Entity Framework

### Mapping a Table to a Class

```csharp
[Table("MurphysLaws")]
public class MurphysLaw
{
	[Key]
	[Column("LawID")]
	public short LawId { get; set; }

	public string LawName { get; set; }
	public string LawText { get; set; }
}
```

`MurphysLaw` is a **POCO** — a Plain Old CLR Object. No database-specific base class, no interface to implement. Just a class with attributes describing how it maps:

- **`[Table("MurphysLaws")]`** — the class name is singular, the table plural
- **`[Key]`** — marks the primary key, which EF requires
- **`[Column("LawID")]`** — the property is `LawId`, the column is `LawID`

`LawName` and `LawText` carry no attributes because their names already match their columns exactly. **Remember that detail** — it becomes the subject of the gotcha below.

### The `DbContext`

```csharp
public class ExternalDataContext : DbContext
{
	public ExternalDataContext() : base("name=ExternalData")
	{
		// Tell EF not to check/create/migrate the schema at all.
		Database.SetInitializer<ExternalDataContext>(null);
	}

	public DbSet<MurphysLaw> MurphysLaws { get; set; }
	public DbSet<ZipCodeRecord> ZipCodes { get; set; }
}
```

The `DbContext` is EF's entry point, exposing one `DbSet<T>` per table you want to work with. `base("name=ExternalData")` points it at the same `App.config` connection string the ADO.NET demos used.

**`Database.SetInitializer<ExternalDataContext>(null)` is the line worth understanding.** By default, EF assumes it's allowed to create or migrate the database schema to match your C# model. Since `ExternalData`'s tables already exist exactly as restored from the backup, that behavior is explicitly disabled. This context only ever reads and writes data — it never touches structure.

This is the "**Code First against an existing database**" pattern. Confusingly named, since nothing is created first: it means you hand-write the classes (rather than generating them from a designer file) but point them at a schema that already exists.

Without that line, EF would compare the model to the database, decide they disagree, and either throw or attempt to alter tables it has no business altering.

### CRUD: Nothing Happens Until `SaveChanges()`

**Select:**

```csharp
// A LINQ query against the DbSet, EF translates this into SQL and runs it when
//   the query is actually enumerated (here, by the foreach loop).
var laws = context.MurphysLaws
	.Where(law => law.LawName.Contains("Law"))
	.OrderBy(law => law.LawId)
	.ToList();
```

EF translates this into `SELECT ... WHERE LawName LIKE '%Law%' ORDER BY LawID` and runs it against the server. Note that the filtering happens **in the database**, not in C# — this is `IQueryable<T>`, not `IEnumerable<T>`, a distinction Chapter 10's `Supplemental.04.IQueryableVsIEnumerable` covers in depth. Getting it wrong means accidentally pulling an entire table into memory to filter it locally.

The `.ToList()` forces execution. Without it, the query object is just a description.

**Insert:**

```csharp
// Add() stages the new entity in memory, nothing hits the database yet.
context.MurphysLaws.Add(newLaw);

// SaveChanges() is what actually generates and runs the INSERT statement.
int rowsAffected = context.SaveChanges();
Console.WriteLine($"SaveChanges() inserted {rowsAffected} row(s). New LawID: {newLaw.LawId}");

// Note: LawId is populated automatically after SaveChanges(), since LawID is an
//   identity column, EF reads back the database-generated value for you.
```

That last note is a genuinely useful behavior. `LawID` is a database-generated identity column, so its value doesn't exist until the `INSERT` runs — yet `newLaw.LawId` is populated immediately afterward. EF reads the generated key back and updates your in-memory object. No second query needed.

**Update — note the absence of any `Update()` call:**

```csharp
law.LawText = "A man with a watch knows what time it is...";

// No explicit "Update()" call needed, EF tracks changes to entities it has
//   already loaded, and SaveChanges() generates an UPDATE for anything that
//   changed since it was fetched.
int rowsAffected = context.SaveChanges();
```

This is **change tracking**, and it surprises people coming from raw SQL. When EF loads an entity it keeps a snapshot of the original values. At `SaveChanges()`, it compares current against original and generates an `UPDATE` containing only the changed columns.

The corollary is worth internalizing: **modifying a tracked entity is enough.** There's no way to "forget to save" a property — but equally, an accidental assignment to a tracked entity *will* be persisted.

**Delete:**

```csharp
context.MurphysLaws.Remove(law);
int rowsAffected = context.SaveChanges();
```

Note both `EfUpdateRecord()` and `EfDeleteRecord()` guard against a missing row:

```csharp
var law = context.MurphysLaws.FirstOrDefault(l => l.LawName == "Segal's Law");
if (law == null)
{
	Console.WriteLine("Segal's Law not found, run EfInsertRecord()/UsingParameterizedInsert() first.");
	return;
}
```

`FirstOrDefault()` returns `null` rather than throwing (unlike `First()`). The message points at the dependency — these demos are ordered, and running them out of sequence leaves nothing to update or delete.

**The unifying pattern:** `Add()`, property changes, and `Remove()` all just stage changes in memory. `SaveChanges()` is the single moment EF generates and runs SQL — and it does so inside a transaction, so either all staged changes commit or none do.

### Calling a Stored Procedure With EF

```csharp
// Database.SqlQuery<T>() runs raw SQL (including a stored procedure call) and
//   maps the results onto the given type, T, the same way a LINQ query would.
var results = context.Database
	.SqlQuery<ZipCodeRecord>("EXEC dbo.GetZipCodesByState @State", new SqlParameter("@State", "TX"))
	.ToList();
```

Compare directly against the ADO.NET version of the same call. Raw ADO.NET requires setting `CommandType.StoredProcedure` and reading each column off the reader by hand. `SqlQuery<T>()` maps results straight onto `ZipCodeRecord` objects.

Note that **both** approaches parameterize `@State` rather than concatenating. That's not a style preference — see `Supplemental.02`.

---

## The Gotcha: `SqlQuery<T>()` Doesn't Honor `[Column]` Mappings

This one is worth knowing specifically because it's easy to hit by accident and the error message doesn't point at the real cause.

Recall that `MurphysLaw.LawId` uses `[Column("LawID")]` to bridge a name mismatch. Now look at `ZipCodeRecord`:

```csharp
[MaxLength(10)]
public string ZipCode { get; set; }
```

**No `[Column]` attribute** — the property name matches the column name exactly, deliberately.

If it *didn't* match — say the property were named `Zip` with `[Column("ZipCode")]` — then:

- `EfSelectRecords()`-style `DbSet<T>` LINQ queries would still work fine. EF's normal query pipeline reads that mapping metadata.
- `EfCallStoredProcedure()`'s `Database.SqlQuery<T>()` call would throw:

```
EntityCommandExecutionException: ... does not have a corresponding column
in the data reader with the same name
```

**Why:** `SqlQuery<T>()` performs simple **name-based matching directly against the raw `DataReader`'s column names**. It does not consult the mapping metadata a `DbSet<T>` query uses. It's much closer to the ADO.NET layer than to EF's ORM layer — which makes sense given it accepts raw SQL.

The practical takeaway: **when a POCO will be used with `Database.SqlQuery<T>()` against hand-written SQL or a stored procedure, name its properties to match the actual result-set column names directly.** Don't lean on `[Column(...)]` to paper over a mismatch. That attribute is real and does something — just not everywhere EF touches a database.

### The Class Naming Consequence

This explains a detail that otherwise looks arbitrary — the class mapped to `ZipCodes` is named `ZipCodeRecord`, not `ZipCode`:

```csharp
/// Named "ZipCodeRecord" rather than "ZipCode" specifically so its ZipCode property
/// can be named to match the actual database column exactly, C# does not allow a member
/// to share its enclosing type's exact name (CS0542), so "ZipCode.ZipCode" isn't legal,
/// even though "ZipCodeRecord.ZipCode" is.
```

The chain of reasoning: the property *must* be named `ZipCode` (because of the `SqlQuery<T>()` limitation) → C# forbids a member sharing its type's exact name (`CS0542`) → therefore the class must be named something else.

A real design constraint from one framework quirk, propagating into a naming decision. `[Table("ZipCodes")]` then reconnects the renamed class to the actual table.

---

## What to Take Away

**EF is built on ADO.NET, not instead of it.** Every EF operation eventually becomes a `SqlCommand` on a `SqlConnection`.

**Match the execution method to the result shape.** `ExecuteReader()` for rows, `ExecuteScalar()` for one value, `ExecuteNonQuery()` for row counts.

**`DataReader` streams with the connection open; `DataSet` buffers and disconnects.** Choose based on whether you need the data after the connection closes, and on how much of it there is.

**Always parameterize.** Both stored-procedure demos and the `INSERT` do. `Supplemental.02` shows what happens when you don't.

**Disable the initializer when mapping to an existing database.** `Database.SetInitializer<T>(null)` stops EF from trying to own a schema it didn't create.

**Nothing hits the database until `SaveChanges()`.** `Add()`, `Remove()`, and property edits all stage in memory, then commit together in one transaction.

**`SqlQuery<T>()` matches by raw column name and ignores `[Column]`.** Name properties to match result-set columns whenever raw SQL is involved, and rename the class if C# won't allow the property name you need.
