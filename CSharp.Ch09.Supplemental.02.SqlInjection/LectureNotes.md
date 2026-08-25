# Chapter 9 Supplemental 02: SQL Injection and Parameterized Queries

## What This Is

Adapted and modernized from a genuinely excellent piece of existing training content, `SafeCoding.AvoidingSqlInjection`, a hands-on, step-by-step demonstration of exactly how SQL injection works and exactly what parameterized queries do to prevent it. The original targeted a specific OnBase database (`hsi.useraccount`, with hardcoded server credentials), adapted here to use the training set's own `ExternalData.dbo.MurphysLaws` table instead, same query shape, same attack technique, harmless data. A companion PDF, `Parameterizing SQL Queries.pdf`, existed alongside the original code but wasn't reviewed as part of this migration, worth checking if it's still relevant material to bring forward separately.

**Only ever run this against `ExternalData`**, the throwaway sandbox database restored from its own backup (see `CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework/README.md`). The walkthrough embedded in `Program.cs` includes a step that deletes every row in a table, that's the point, but it means this project should never be pointed at anything you're not fully prepared to lose and restore from backup.

---

## The Core Idea: Data vs. Code, Collapsed Into One String

```csharp
// UnsafeDatabaseUtility.cs
string sql = SqlQuery + $"'{lawName}'";
command = new SqlCommand(sql, connection);
```

```csharp
// SafeDatabaseUtility.cs
command = new SqlCommand(SqlQuery, connection);   // SqlQuery already ends in "WHERE LawName = @lawName"
command.Parameters.Add(new SqlParameter { ParameterName = "lawName", SqlDbType = SqlDbType.VarChar, Size = 50, Value = lawName });
```

Both methods run structurally the same query, one connection, one command, one call to `ExecuteReader()`. The entire vulnerability is contained in that one line in `UnsafeDatabaseUtility`: `SqlQuery + $"'{lawName}'"` glues whatever the user typed directly into the SQL statement's own text. By the time that string reaches SQL Server, there is no way to tell "the value someone searched for" apart from "additional SQL someone wrote", they're both just characters in the same statement.

`SafeDatabaseUtility` never does this. The query text is fixed and complete before `lawName` ever enters the picture, `@lawName` is a placeholder, and the actual value is handed to `SqlCommand.Parameters` as data, sent to SQL Server through a separate channel entirely. No matter what characters `lawName` contains, quotes, semicolons, entire additional SQL statements, SQL Server receives it as a single, literal value to compare against `LawName`, never as executable SQL.

---

## Why "Sanitizing" Input Yourself Isn't the Fix

It might seem like the fix for `UnsafeDatabaseUtility` is to strip or escape dangerous characters (quotes, semicolons, `--`) before building the SQL string. This is a real, well-documented trap: escaping is easy to get subtly wrong (different databases, different contexts, different encodings all have different escaping rules), and attackers have decades of accumulated technique for finding the gaps. Parameterized queries don't try to make dangerous input "safe to embed", they avoid embedding user input into SQL text at all. That's a categorically stronger guarantee than any amount of manual escaping.

---

## The Walkthrough: What This Actually Lets Someone Do

`Program.cs` includes a full, step-by-step attacker's playbook as inline comments, worth reading start to finish even before running the project. In order: confirm the normal case works, test for the vulnerability, fingerprint the database engine (the `WAITFOR DELAY`/`SLEEP()`/`DBMS_SESSION.sleep()` timing trick works because *some* engine-specific syntax will execute without error and pause, telling an attacker which engine they're up against, purely from the outside), enumerate every table in the database via `INFORMATION_SCHEMA.TABLES`, enumerate the columns in a table of interest, extract every row through what was supposed to be a single-value lookup, and, finally, cause real damage.

Worth sitting with the fact that every one of these steps runs through the exact same input box, a single text field asking for a law name. Nothing about the attack requires special access, a compromised account, or inside knowledge of the schema, all of it is discoverable purely through the vulnerable query's own error messages and timing behavior. That's the real lesson: a single unparameterized query anywhere in an application is enough to expose the entire database behind it, not just the one table the vulnerable query was written against.

---

## Try It Yourself

Work through the walkthrough's steps in order, comparing the safe and unsafe query's output at each one. Step 2 (`Murphy's Law' OR '1' = '1`) is the clearest single moment to pay attention to: the safe query finds nothing (there's no law with that literal, bizarre name), while the unsafe query returns every row in the table, proof that the injected `OR '1'='1'` altered the query's actual logic. If you do try Step 7 (the `DELETE`), restore `ExternalData` from `ExternalData.bak` afterward per the setup README, this project intentionally makes that easy to need.
