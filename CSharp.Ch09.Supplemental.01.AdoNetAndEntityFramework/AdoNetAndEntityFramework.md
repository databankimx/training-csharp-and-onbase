# ADO.NET and Entity Framework

## Introduction

This lesson covers two ways to talk to a real database from C#: ADO.NET, the low-level foundation everything else is built on, and Entity Framework, a higher-level layer that maps database tables to ordinary C# classes so you can query them with LINQ instead of hand-written SQL. **Before running this, see `README.md` in this project's folder**, it walks through installing SQL Server and restoring the database this lesson uses.

---

## ADO.NET: Connection, Command, and Reading Results

```csharp
using var connection = new SqlConnection(connectionString);
connection.Open();

using var command = new SqlCommand("SELECT LawID, LawName, LawText FROM dbo.MurphysLaws", connection);
using var reader = command.ExecuteReader();

while (reader.Read())
{
    short lawId = reader.GetInt16(0);
    string lawName = reader.GetString(1);
}
```

Three pieces: a `Connection` (the open link to the database), a `Command` (the SQL statement to run), and a `DataReader` (a fast, forward-only stream of the results). This is the most direct, lowest-level way to talk to a database in .NET.

`Command` has three different execution methods, pick based on what your query returns:

- **`ExecuteReader()`**: for queries returning rows (a `SELECT`).
- **`ExecuteScalar()`**: for a query returning exactly one value, like `COUNT(*)`.
- **`ExecuteNonQuery()`**: for `INSERT`/`UPDATE`/`DELETE`, statements that don't return rows, just a count of how many rows changed.

---

## `DataAdapter` and `DataTable`: Data You Can Keep After the Connection Closes

```csharp
using var adapter = new SqlDataAdapter("SELECT State, City, ZipCode FROM dbo.ZipCodes", connection);
var dataSet = new DataSet();
adapter.Fill(dataSet, "ZipCodes");

DataTable table = dataSet.Tables["ZipCodes"];
foreach (DataRow row in table.Rows)
{
    Console.WriteLine(row["City"]);
}
```

Unlike a `DataReader`, which needs the connection to stay open the whole time, `DataAdapter.Fill()` grabs everything into a `DataTable` in memory and then closes the connection. You can keep working with that `DataTable` afterward, even though the database connection is long gone.

---

## Entity Framework: Tables as Classes

```csharp
public class MurphysLaw
{
    public short LawId { get; set; }
    public string LawName { get; set; }
    public string LawText { get; set; }
}

public class ExternalDataContext : DbContext
{
    public DbSet<MurphysLaw> MurphysLaws { get; set; }
}
```

Entity Framework maps a table (`MurphysLaws`) onto an ordinary C# class (`MurphysLaw`). A `DbContext` (here, `ExternalDataContext`) is your entry point, exposing one `DbSet<T>` per table you want to work with.

### Select, Insert, Update, Delete

```csharp
// Select
var laws = context.MurphysLaws.Where(l => l.LawName.Contains("Law")).ToList();

// Insert
context.MurphysLaws.Add(newLaw);
context.SaveChanges();

// Update
existingLaw.LawText = "New text";
context.SaveChanges();

// Delete
context.MurphysLaws.Remove(lawToDelete);
context.SaveChanges();
```

The key thing to notice: **nothing actually happens to the database until `SaveChanges()` is called**. Adding, changing a property on something you already fetched, and removing all just update EF's in-memory tracking, `SaveChanges()` is the one moment real SQL gets generated and run.

---

## Calling a Stored Procedure

```csharp
// Raw ADO.NET
var command = new SqlCommand("dbo.GetZipCodesByState", connection) { CommandType = CommandType.StoredProcedure };
command.Parameters.Add(new SqlParameter("@State", "TX"));

// Entity Framework
var results = context.Database.SqlQuery<ZipCodeRecord>("EXEC dbo.GetZipCodesByState @State", new SqlParameter("@State", "TX")).ToList();
```

Both call the same stored procedure. Notice both use a *parameter* (`@State`) rather than building the SQL text by hand with the value spliced in, that's not a style choice, it's a safety one. See the SQL Injection lesson for exactly why.

One gotcha worth knowing: `Database.SqlQuery<T>()` matches result columns to your class's properties **by name only**, it does not use `[Column(...)]` mappings the way a normal `context.MurphysLaws` query does. That's exactly why the class mapped to `ZipCodes` is named `ZipCodeRecord` rather than `ZipCode`, its `ZipCode` property needs to match the database column exactly, and C# won't allow a member to share its enclosing type's own name, so the class itself had to be named something else. A mismatch here (say, a property named `Zip` with a `[Column("ZipCode")]` attribute on it) works fine everywhere else in EF, but throws specifically when you call `SqlQuery<T>()` against raw SQL or a stored procedure.

---

## Try It Yourself

After running through the lesson once, open SQL Server Management Studio and look at the `MurphysLaws` table directly, you should see the row the `EfInsertRecord()` step added ("Muphry's Law"), unless the later `EfDeleteRecord()` step already removed it, run the project again and watch each step's effect show up in the actual database.
