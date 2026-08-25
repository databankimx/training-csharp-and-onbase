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
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CSharp.Ch09.Supplemental._01.AdoNetAndEntityFramework.Models.Data;
using CSharp.Ch09.Supplemental._01.AdoNetAndEntityFramework.Models.Objects;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

#pragma warning disable S1192 // "String literals should not be duplicated" - this lesson intentionally has some repeated strings for clarity
namespace CSharp.Ch09.Supplemental._01.AdoNetAndEntityFramework
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * This project needs a real SQL Server database restored first, see README.md in this
         *   project's folder for setup instructions (installing SQL Server, restoring the
         *   ExternalData.bak backup, and creating one stored procedure).
         *
         * ADO.NET is the foundational .NET data access API everything else in this section
         *   (Entity Framework included) is ultimately built on top of. The core pieces:
         * - Connection      Represents an open link to the database (SqlConnection)
         * - Command         Represents a SQL statement or stored procedure to run (SqlCommand)
         * - DataReader      A fast, forward-only, read-only stream of results (SqlDataReader)
         * - DataAdapter     Bridges a Command's results into a disconnected, in-memory
         *                     DataSet/DataTable you can work with after the connection closes
         *
         * Entity Framework (EF6, the classic .NET Framework version) sits on top of ADO.NET and
         *   maps database tables to ordinary C# classes (POCOs), letting you write LINQ queries
         *   against your data instead of hand-written SQL for most everyday operations. This
         *   project uses EF "Code First against an existing database": the ExternalDataContext
         *   maps to the ALREADY-RESTORED ExternalData tables, rather than having EF generate a
         *   new database from the C# model.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                #region ADO.NET
                // Demonstrate opening and closing a Connection directly
                UsingConnection();
                GenericFunctions.Pause();

                // Demonstrate a Command with ExecuteReader()
                UsingCommandExecuteReader();
                GenericFunctions.Pause();

                // Demonstrate a Command with ExecuteScalar()
                UsingCommandExecuteScalar();
                GenericFunctions.Pause();

                // Demonstrate a parameterized INSERT with ExecuteNonQuery()
                UsingParameterizedInsert();
                GenericFunctions.Pause();

                // Demonstrate DataAdapter and DataSet/DataTable
                UsingDataAdapterAndDataSet();
                GenericFunctions.Pause();

                // Demonstrate calling a stored procedure via raw ADO.NET
                UsingStoredProcedureViaAdoNet();
                GenericFunctions.Pause();
                #endregion

                #region Entity Framework
                // Demonstrate selecting records with EF
                EfSelectRecords();
                GenericFunctions.Pause();

                // Demonstrate inserting a record with EF
                EfInsertRecord();
                GenericFunctions.Pause();

                // Demonstrate updating a record with EF
                EfUpdateRecord();
                GenericFunctions.Pause();

                // Demonstrate deleting a record with EF
                EfDeleteRecord();
                GenericFunctions.Pause();

                // Demonstrate calling a stored procedure with EF
                EfCallStoredProcedure();
                GenericFunctions.Pause();
                #endregion
                #endregion
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

        #region ADO.NET Lesson Methods
        // Demonstrate opening and closing a Connection directly
        private static void UsingConnection()
        {
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

            // The "using" statement above ensures Dispose() runs even if an exception
            //   happens in between, releasing the underlying connection resources.
        }

        // Demonstrate a Command with ExecuteReader()
        private static void UsingCommandExecuteReader()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ExternalData"].ConnectionString;

            using var connection = new SqlConnection(connectionString);
            connection.Open();

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
        }

        // Demonstrate a Command with ExecuteScalar()
        private static void UsingCommandExecuteScalar()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ExternalData"].ConnectionString;

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var command = new SqlCommand("SELECT COUNT(*) FROM dbo.ZipCodes", connection);

            // ExecuteScalar() is the right tool specifically when a query returns exactly one
            //   value (a single row, single column), like a COUNT(*), a MAX(), or checking for
            //   existence. It's more efficient than ExecuteReader() for that narrow case,
            //   since it doesn't set up the full reader machinery for one value.
            object result = command.ExecuteScalar();
            int zipCodeCount = Convert.ToInt32(result);

            Console.WriteLine($"Total ZipCodes rows: {zipCodeCount}");
        }

        // Demonstrate a parameterized INSERT with ExecuteNonQuery()
        private static void UsingParameterizedInsert()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ExternalData"].ConnectionString;

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            // Note the parameter placeholder (@LawName, @LawText) rather than concatenating
            //   the values directly into the SQL string. See
            //   CSharp.Ch09.Supplemental.02.SqlInjection for a full, hands-on demonstration of
            //   exactly why this distinction matters.
            const string sql = "INSERT INTO dbo.MurphysLaws (LawName, LawText) VALUES (@LawName, @LawText)";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("@LawName", SqlDbType.VarChar, 50) { Value = "Segal's Law" });
            command.Parameters.Add(new SqlParameter("@LawText", SqlDbType.VarChar, 250)
            {
                Value = "A man with a watch knows what time it is. A man with two watches is never sure."
            });

            // ExecuteNonQuery() is the right tool for INSERT/UPDATE/DELETE, statements that
            //   don't return rows, only a count of how many rows were affected.
            int rowsAffected = command.ExecuteNonQuery();
            Console.WriteLine($"ExecuteNonQuery() inserted {rowsAffected} row(s).");
        }

        // Demonstrate DataAdapter and DataSet/DataTable
        private static void UsingDataAdapterAndDataSet()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ExternalData"].ConnectionString;

            using var connection = new SqlConnection(connectionString);
            using var adapter = new SqlDataAdapter("SELECT State, City, ZipCode FROM dbo.ZipCodes ORDER BY State, City", connection);

            var dataSet = new DataSet();

            // Fill() opens the connection, runs the query, populates the DataSet, and closes
            //   the connection again, all in this one call. Unlike a DataReader, the DataTable
            //   you get back is fully disconnected, you can keep reading (or even modifying)
            //   it long after the database connection itself has closed.
            adapter.Fill(dataSet, "ZipCodes");

            DataTable zipCodesTable = dataSet.Tables["ZipCodes"];
            Console.WriteLine($"DataAdapter.Fill() populated {zipCodesTable?.Rows.Count} row(s) into the DataTable.");

            Console.WriteLine($"{Environment.NewLine}First 5 rows:");
            #pragma warning disable IDE0301 // Not simplifying collection initialization here, the explicit cast to DataRow is clearer for this lesson
            foreach (DataRow row in zipCodesTable?.Rows.Cast<DataRow>().Take(5) ?? Enumerable.Empty<DataRow>())
            {
                Console.WriteLine($" - {row["City"]}, {row["State"]} {row["ZipCode"]}");
            }
            #pragma warning restore IDE0301
        }

        // Demonstrate calling a stored procedure via raw ADO.NET
        private static void UsingStoredProcedureViaAdoNet()
        {
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

                Console.WriteLine("Zip codes in TX, via dbo.GetZipCodesByState:");
                while (reader.Read())
                {
                    Console.WriteLine($" - {reader["City"]}, {reader["State"]} {reader["ZipCode"]}");
                }
            }
            catch (Exception ex) when (IsMissingStoredProcedureError(ex))
            {
                PrintMissingStoredProcedureMessage();
            }
        }
        #endregion

        #region Entity Framework Lesson Methods
        // Demonstrate selecting records with EF
        private static void EfSelectRecords()
        {
            using var context = new ExternalDataContext();

            // A LINQ query against the DbSet, EF translates this into SQL and runs it when
            //   the query is actually enumerated (here, by the foreach loop).
            var laws = context.MurphysLaws
                .Where(law => law.LawName.Contains("Law"))
                .OrderBy(law => law.LawId)
                .ToList();

            Console.WriteLine($"Laws with \"Law\" in the name ({laws.Count} found):");
            foreach (var law in laws)
            {
                Console.WriteLine($" - [{law.LawId}] {law.LawName}: {law.LawText}");
            }
        }

        // Demonstrate inserting a record with EF
        private static void EfInsertRecord()
        {
            using var context = new ExternalDataContext();

            var newLaw = new MurphysLaw
            {
                LawName = "Muphry's Law",
                LawText = "If you write anything criticizing editing or proofreading, there will be a fault in what you have written."
            };

            // Add() stages the new entity in memory, nothing hits the database yet.
            context.MurphysLaws.Add(newLaw);

            // SaveChanges() is what actually generates and runs the INSERT statement.
            int rowsAffected = context.SaveChanges();
            Console.WriteLine($"SaveChanges() inserted {rowsAffected} row(s). New LawID: {newLaw.LawId}");

            // Note: LawId is populated automatically after SaveChanges(), since LawID is an
            //   identity column, EF reads back the database-generated value for you.
        }

        // Demonstrate updating a record with EF
        private static void EfUpdateRecord()
        {
            using var context = new ExternalDataContext();

            var law = context.MurphysLaws.FirstOrDefault(l => l.LawName == "Segal's Law");
            if (law == null)
            {
                Console.WriteLine("Segal's Law not found, run EfInsertRecord()/UsingParameterizedInsert() first.");
                return;
            }

            law.LawText = "A man with a watch knows what time it is. A man with two watches is never quite sure.";

            // No explicit "Update()" call needed, EF tracks changes to entities it has
            //   already loaded, and SaveChanges() generates an UPDATE for anything that
            //   changed since it was fetched.
            int rowsAffected = context.SaveChanges();
            Console.WriteLine($"SaveChanges() updated {rowsAffected} row(s).");
        }

        // Demonstrate deleting a record with EF
        private static void EfDeleteRecord()
        {
            using var context = new ExternalDataContext();

            var law = context.MurphysLaws.FirstOrDefault(l => l.LawName == "Muphry's Law");
            if (law == null)
            {
                Console.WriteLine("Muphry's Law not found, run EfInsertRecord() first.");
                return;
            }

            context.MurphysLaws.Remove(law);
            int rowsAffected = context.SaveChanges();
            Console.WriteLine($"SaveChanges() deleted {rowsAffected} row(s).");
        }

        // Demonstrate calling a stored procedure with EF
        private static void EfCallStoredProcedure()
        {
            using var context = new ExternalDataContext();

            try
            {
                // Database.SqlQuery<T>() runs raw SQL (including a stored procedure call) and
                //   maps the results onto the given type, T, the same way a LINQ query would.
                var results = context.Database.SqlQuery<ZipCodeRecord>("EXEC dbo.GetZipCodesByState @State", new SqlParameter("@State", "TX")).ToList();

                Console.WriteLine($"Zip codes in TX, via EF calling dbo.GetZipCodesByState ({results.Count} found):");
                foreach (var zip in results)
                {
                    Console.WriteLine($" - {zip.City}, {zip.State} {zip.ZipCode}");
                }
            }
            catch (Exception ex) when (IsMissingStoredProcedureError(ex))
            {
                PrintMissingStoredProcedureMessage();
            }
        }
        #endregion

        #region Helper Functions
        // SQL Server error 2812: "Could not find stored procedure '...'". Both stored-procedure
        //   demos above can hit this if step 4 in README.md (creating dbo.GetZipCodesByState)
        //   was skipped. Walking the InnerException chain, rather than checking only the
        //   outermost exception, matters here specifically because EF can wrap the original
        //   SqlException inside its own exception type before it reaches this catch block.
        private static bool IsMissingStoredProcedureError(Exception ex)
        {
            while (ex != null)
            {
                if (ex is SqlException sqlEx && sqlEx.Number == 2812) return true;
                ex = ex.InnerException;
            }
            return false;
        }

        // Prints a clear pointer back to README.md instead of letting a raw SqlException
        //   stack trace (and, without the try/catch above, everything else queued to run
        //   after it in Main()) be the only thing a missed setup step produces.
        private static void PrintMissingStoredProcedureMessage()
        {
            Console.WriteLine("Could not find dbo.GetZipCodesByState.");
            Console.WriteLine("See \"4. Create the Stored Procedure Used by This Lesson\" in this project's");
            Console.WriteLine("README.md, run that CREATE PROCEDURE script once against ExternalData, then");
            Console.WriteLine("re-run this lesson.");
        }
        #endregion
    }
}
#pragma warning restore S1192

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
