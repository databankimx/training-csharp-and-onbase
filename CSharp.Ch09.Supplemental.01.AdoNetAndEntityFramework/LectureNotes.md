# Chapter 9 Supplemental 01: ADO.NET and Entity Framework

## What This Is

The core data-access half of Chapter 9's "Consuming Data" section: raw ADO.NET (`Connection`, `Command`, `DataReader`, `DataAdapter`/`DataSet`) and Entity Framework 6 (Select/Insert/Update/Delete, plus calling a stored procedure), all running against a real, restorable SQL Server database, `ExternalData`. **See `README.md` in this project's folder before running this**, it walks through installing SQL Server, restoring the database backup, and creating the one stored procedure this lesson needs.

New content, built for this migration using the actual `ExternalData` schema (`MurphysLaws`, `ZipCodes`, `Numbers`, `Phrases`, `TestItems`).

---

## ADO.NET: `Connection`, `Command`, `DataReader`

```csharp
using var connection = new SqlConnection(connectionString);
connection.Open();

using var command = new SqlCommand("SELECT LawID, LawName, LawText FROM dbo.MurphysLaws ORDER BY LawID", connection);
using var reader = command.ExecuteReader();

while (reader.Read())
{
    short lawId = reader.GetInt16(0);
    ...
}
```

This is the lowest-level, most direct way to talk to a database in .NET: open a `Connection`, build a `Command` against it, and read results back one row at a time with a `DataReader`. Fast and forward-only, once you've read past a row, you can't go back, and the connection has to stay open the entire time you're reading.

`ExecuteReader()`, `ExecuteScalar()`, and `ExecuteNonQuery()` are `Command`'s three execution modes, each suited to a different shape of result: `ExecuteReader()` for multi-row results, `ExecuteScalar()` for a single value (a `COUNT(*)`, worth using specifically because it skips the overhead of setting up a full reader for one value), and `ExecuteNonQuery()` for statements that don't return rows at all (`INSERT`/`UPDATE`/`DELETE`), returning only a count of affected rows.

---

## `DataAdapter` and `DataSet`/`DataTable`: Disconnected Data

```csharp
using var adapter = new SqlDataAdapter("SELECT State, City, ZipCode FROM dbo.ZipCodes ORDER BY State, City", connection);
var dataSet = new DataSet();
adapter.Fill(dataSet, "ZipCodes");

DataTable zipCodesTable = dataSet.Tables["ZipCodes"];
```

Where a `DataReader` requires the connection to stay open the whole time you're reading, `DataAdapter.Fill()` opens the connection, runs the query, copies everything into an in-memory `DataTable`, and closes the connection again, all in one call. The `DataTable` you get back is fully disconnected, you can keep reading (or even editing) it long after the database connection has closed. This is the older, `DataSet`-centric approach to data access that predates both LINQ and Entity Framework, still worth knowing since it shows up in plenty of existing code, but generally superseded by EF (or, for read-only scenarios, `DataReader` + your own object mapping) in new development.

---

## Entity Framework: Mapping Tables to Classes

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

```csharp
public class ExternalDataContext : DbContext
{
    public ExternalDataContext() : base("name=ExternalData")
    {
        Database.SetInitializer<ExternalDataContext>(null);
    }

    public DbSet<MurphysLaw> MurphysLaws { get; set; }
    public DbSet<ZipCodeRecord> ZipCodes { get; set; }
}
```

`MurphysLaw` is a POCO (Plain Old CLR Object), an ordinary C# class with no database-specific base class required, decorated with attributes (`[Table]`, `[Key]`, `[Column]`) telling EF how it maps onto the actual `MurphysLaws` table. `ExternalDataContext` is EF's entry point, a `DbContext` exposing one `DbSet<T>` per table you want to work with. Worth noticing `Database.SetInitializer<ExternalDataContext>(null)` specifically: by default, EF assumes it's allowed to create or migrate the database schema to match your C# model. Since `ExternalData`'s tables already exist exactly as restored from the backup, that behavior is explicitly turned off, this context only ever reads and writes data, never touches structure.

One naming detail worth explaining: the class mapped to `ZipCodes` is named `ZipCodeRecord`, not `ZipCode`. That's not arbitrary, its `ZipCode` *property* needs to match the database column name exactly (see the gotcha below), and C# does not allow a member to share its enclosing type's exact name (`CS0542`), so a class literally named `ZipCode` could never have a property also named `ZipCode`. Renaming the class sidesteps that restriction entirely.

---

## CRUD With EF: No SQL, Mostly

```csharp
// Select
var laws = context.MurphysLaws.Where(law => law.LawName.Contains("Law")).ToList();

// Insert
context.MurphysLaws.Add(newLaw);
context.SaveChanges();

// Update (no explicit "Update()" call needed)
law.LawText = "...";
context.SaveChanges();

// Delete
context.MurphysLaws.Remove(law);
context.SaveChanges();
```

The pattern worth internalizing: nothing hits the database until `SaveChanges()` is called. `Add()`, changing a tracked entity's properties, and `Remove()` all just stage changes in memory, `SaveChanges()` is the one moment EF actually generates and runs SQL. Also worth noticing in `EfInsertRecord()`: after `SaveChanges()`, `newLaw.LawId` is already populated, even though `LawID` is a database-generated identity column, EF automatically reads the generated value back and updates your object with it.

---

## Calling a Stored Procedure, Two Ways

```csharp
// Raw ADO.NET
using var command = new SqlCommand("dbo.GetZipCodesByState", connection) { CommandType = CommandType.StoredProcedure };
command.Parameters.Add(new SqlParameter("@State", SqlDbType.VarChar, 20) { Value = "TX" });
using var reader = command.ExecuteReader();
```

```csharp
// Entity Framework
var results = context.Database.SqlQuery<ZipCodeRecord>("EXEC dbo.GetZipCodesByState @State", new SqlParameter("@State", "TX")).ToList();
```

Both call the exact same stored procedure (`GetZipCodesByState`, created as part of the README's setup steps), worth comparing directly: raw ADO.NET requires manually setting `CommandType.StoredProcedure` and reading each column off the `DataReader` yourself, while `Database.SqlQuery<T>()` maps the results straight onto `ZipCodeRecord` objects the same way an ordinary LINQ query would. Both approaches use a parameterized call (`@State`), not string concatenation, see `CSharp.Ch09.Supplemental.02.SqlInjection` for exactly why that distinction is worth taking seriously, not just a style preference.

---

## A Real Gotcha: `SqlQuery<T>()` Doesn't Honor `[Column]` Mappings

This one is worth knowing about specifically because it's easy to hit by accident, and the error message doesn't immediately point at the real cause. `ZipCodeRecord.ZipCode` (the property) maps to the `ZipCode` column, and there is no `[Column(...)]` attribute needed, the property name already matches the column name exactly. If it *didn't* match, say the property were instead named `Zip` with `[Column("ZipCode")]` on it, `EfSelectRecords()`-style `DbSet<T>` LINQ queries would still work fine, EF's normal query pipeline reads that mapping metadata. But `EfCallStoredProcedure()`'s `Database.SqlQuery<T>()` call would throw `EntityCommandExecutionException: ... does not have a corresponding column in the data reader with the same name`, because `SqlQuery<T>()` performs simple name-based matching directly against the raw `DataReader`'s column names, it does not consult the same mapping metadata a `DbSet<T>` query uses.

The practical takeaway: when a POCO is going to be used with `Database.SqlQuery<T>()` (or `SqlQuery<T>()` on a `DbContext` more generally) against hand-written SQL or a stored procedure, the safest approach is naming properties to match the actual result-set column names directly, rather than leaning on `[Column(...)]` to paper over a mismatch. That attribute is real and does something, just not everywhere EF touches a database.
