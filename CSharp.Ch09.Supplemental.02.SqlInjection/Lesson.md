# Chapter 9 Supplemental 02: SQL Injection

## What This Is

The other half of Chapter 9's data-access story, and the reason `Supplemental.01` quietly insisted on parameters every single time it touched a value.

This project is one console loop, one search box, and two classes that are deliberately near-identical: `SafeDatabaseUtility` and `UnsafeDatabaseUtility`. Same connection handling, same disposal pattern, same query, same table. Every input you type runs through **both** of them, back to back, so you can watch them agree perfectly on ordinary input and then part company spectacularly on input that is a little more creative.

> **Setup required.** This project needs the same restored `ExternalData` database as `CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework`. See that project's `README.md` if you have not restored it yet, and keep the backup file handy, because Step 7 of the walkthrough deletes a table's worth of rows on purpose.

> **Read the warning at the top of `Program.cs` and mean it.** Run this against the throwaway `ExternalData` sandbox and nothing else. Not production, not staging, not "just to test." The whole point of the lesson is that this code hands an attacker the ability to run arbitrary SQL, and it will not pause to ask whether you were only curious.

From the Chapter Notes:

```
SQL injection happens when user-supplied input is spliced directly into a SQL
  statement's text, rather than passed as a separate parameter. When that happens,
  the database has no way to tell "data the user typed" apart from "more SQL the
  user wrote", because by the time the string reaches the database, they're both
  just... more SQL text.
```

That is the entire vulnerability, stated in four lines. Everything that follows is consequences.

---

## The One Line That Matters

`UnsafeDatabaseUtility` opens with a query constant that is missing something, on purpose:

```csharp
// SQL Query to Execute (missing its WHERE value on purpose, see ExecuteQuery() below)
private const string SqlQuery = "SELECT RTRIM(LawText) FROM dbo.MurphysLaws WHERE LawName = ";
```

Then `ExecuteQuery()` finishes the sentence for you:

```csharp
string sql = SqlQuery + $"'{lawName}'";

#pragma warning disable S2077 // Explicitly demonstrating unsafe practice
command = new SqlCommand(sql, connection);
#pragma warning restore S2077
```

That is the whole bug. One concatenation and a pair of quote characters.

Notice the `#pragma warning disable S2077`. That is a static analysis rule aimed at precisely this pattern, and it had to be explicitly silenced to get this project building cleanly. The tooling knew. It usually does. When that warning surfaces in a real code review, treat it as the fire alarm it is rather than noise to suppress.

The comment sitting directly above the concatenation states the problem plainly:

```csharp
// Here is the entire vulnerability, in one line: the value the user typed is
//   glued directly into the SQL statement's text. SQL Server has no way to
//   distinguish "data the user searched for" from "additional SQL the user
//   wrote", because by the time this string reaches the database, they are
//   the exact same thing: more SQL text.
```

---

## The Fix, Which Is Disappointingly Boring

Now the safe version. Same class, same shape, same everything, except the value never enters the query text at all:

```csharp
// SQL Query to Execute. Note the query text itself never contains the search value,
//   only the parameter placeholder ("@lawName") that gets added below.
private const string SqlQuery = "SELECT RTRIM(LawText) FROM dbo.MurphysLaws WHERE LawName = @lawName";
```

```csharp
command.Parameters.Add(new SqlParameter
{
	ParameterName = "lawName",
	SqlDbType = SqlDbType.VarChar,
	Size = 50,
	Value = lawName
});
```

The query string here is a `const`. It is fixed at compile time, and no user on earth can alter a single character of it. `@lawName` is a placeholder, and SqlClient ships the value to the server separately, tagged as data, with a declared type and a declared size. SQL Server parses the statement once, sees a parameter slot, and fills it with something it has already decided is not code.

So `WHERE LawName = @lawName` compares against exactly what the user typed, in full, as a literal string. Type `Murphy's Law' OR '1' = '1` and the database dutifully hunts for a law named literally that, finds nothing, and says so. The apostrophes are just characters. They are not punctuation in a language the parameter is permitted to speak.

Note what the safe version does **not** do. It does not strip quotes, blacklist the word `DROP`, or run the input through a regular expression somebody wrote in 2009:

```
The fix is not "sanitize" or "escape" the input yourself, string-manipulation
  defenses are notoriously easy to get wrong and easy to bypass. The fix is
  PARAMETERIZED QUERIES: pass the SQL statement's shape (with placeholders) and the
  actual values as two SEPARATE things, and let the database driver handle sending
  them to the database safely.
```

Hand-rolled escaping is a losing game. You would be out-thinking every quoting trick, every encoding, and every edge case in a language whose parser you did not write, forever, without slipping once. Parameterization skips the contest by never letting the value be parsed as SQL at all. It is also less code than the clever alternative, which ought to settle the argument on its own.

---

## The Walkthrough: Thinking Like an Attacker

`Program.cs` carries an embedded seven-step playbook, and it rewards running rather than skimming. The framing matters. You happen to know this schema, but pretend you do not. All an attacker knows is that some query, somewhere, probably looks roughly like this:

```
SELECT [column_a] FROM [table] WHERE [column_b] = '[value]'
```

Everything else gets discovered from the outside, one typed string at a time, through nothing but a search box.

### Step 1: Confirm the normal behavior

```
Murphy's Law
```

which is exactly why "just reject apostrophes" fails as a defense before it starts. Legitimate data contains them.

<!-- END -->
