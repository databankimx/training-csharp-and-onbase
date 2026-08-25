# SQL Injection and Parameterized Queries

## Introduction

This lesson shows exactly what SQL injection is, exactly why it's dangerous, and exactly what parameterized queries do to stop it, hands-on, against a real (harmless, restorable) database. **Only ever run this against `ExternalData`**, the sandbox database from the ADO.NET lesson, never anything you're not prepared to lose and restore.

---

## The Vulnerability, in One Line

```csharp
string sql = "SELECT RTRIM(LawText) FROM dbo.MurphysLaws WHERE LawName = " + $"'{lawName}'";
```

Whatever the user typed gets glued directly into the SQL text. If someone types a normal law name, this works fine. If someone types something like `Murphy's Law' OR '1' = '1`, the resulting SQL becomes:

```sql
SELECT RTRIM(LawText) FROM dbo.MurphysLaws WHERE LawName = 'Murphy's Law' OR '1' = '1'
```

`'1' = '1'` is always true, so the `WHERE` clause now matches *every row*, not just the one that was searched for. The database has no way to know the person typing wasn't supposed to be able to change the query's logic, as far as SQL Server is concerned, this is just the SQL statement it was given.

---

## The Fix

```csharp
var command = new SqlCommand("SELECT RTRIM(LawText) FROM dbo.MurphysLaws WHERE LawName = @lawName", connection);
command.Parameters.Add(new SqlParameter { ParameterName = "lawName", SqlDbType = SqlDbType.VarChar, Size = 50, Value = lawName });
```

Here, the search value never touches the SQL text at all. `@lawName` is a placeholder in a query that's already complete on its own, and the actual value gets sent to the database separately, as pure data. No matter what characters someone types, quotes, semicolons, entire SQL statements, none of it can change what the query does. It's always treated as one literal value to compare against.

This is why "just remove dangerous characters before building the SQL" isn't a real fix: it's genuinely hard to get escaping right in every case, and parameterized queries sidestep the whole problem instead of trying to patch around it.

---

## What This Actually Lets an Attacker Do

Run the project and work through the walkthrough embedded in `Program.cs`'s comments, step by step. It walks through everything a real attacker could do through nothing but this one search box: confirm the app is vulnerable, figure out which database engine is running underneath, list every table in the database, list the columns in an interesting-looking table, pull out every row of data, and, as a final step, delete data outright.

The unsettling part worth sitting with: **all of that comes from one search field**, no special access needed, no inside knowledge of the database. It's all discoverable purely by watching how the vulnerable query behaves from the outside.

---

## Try It Yourself

Type `Murphy's Law` first, both queries should return the same result, confirming the normal case works. Then try `Murphy's Law' OR '1' = '1'` and compare: the safe query finds nothing (no law is actually named that), while the unsafe one returns every row in the table. That single comparison is the whole lesson, made visible.
