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
using System.Data.Odbc;
using System.Data.SQLite;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

#pragma warning disable S2068 // Connection strings don't contain any real secrets
namespace CSharp.Ch09.Supplemental._03.ConnectingToOtherDatabases
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * Everything else in this chapter's ADO.NET content used System.Data.SqlClient,
         *   SqlConnection, SqlCommand, SqlDataReader, specifically for SQL Server. Every
         *   other relational database has its own equivalent provider library, with its own
         *   "Sql"-prefixed class names swapped for a provider-specific prefix, but the exact
         *   SAME shape: a Connection, a Command, a DataReader. Once you've learned this
         *   pattern once, applying it to a new provider is almost entirely a matter of
         *   swapping the NuGet package and the connection string.
         *
         * Provider          NuGet Package                  Connection Class    Command Class
         * ------------      ---------------------------    -----------------   ----------------
         * SQL Server        (built into .NET Framework)     SqlConnection       SqlCommand
         * SQLite            System.Data.SQLite              SQLiteConnection    SQLiteCommand
         * MySQL             MySql.Data                      MySqlConnection     MySqlCommand
         * PostgreSQL        Npgsql                          NpgsqlConnection    NpgsqlCommand
         * Oracle             Oracle.ManagedDataAccess       OracleConnection    OracleCommand
         * ODBC (generic)    (built into .NET Framework)     OdbcConnection      OdbcCommand
         *
         * MongoDB is deliberately different, and included specifically to show that
         *   difference: it's a NoSQL, document-oriented database. There's no Connection/
         *   Command/DataReader shape at all, no SQL text, no rows and columns. You work with
         *   a MongoClient, an IMongoDatabase, and an IMongoCollection<T> holding documents
         *   (BSON, a binary JSON-like format) directly.
         *
         * Only SQLite is genuinely runnable here without any setup, it's a file-based,
         *   serverless database, so this project creates, uses, and deletes a temporary one
         *   automatically. Every other method in this file needs a real server to actually
         *   connect to, see README.md for how to set each one up if you want to try them for
         *   real. Left as they are, each one will print a clear "could not connect" message
         *   rather than crashing the rest of the demo, that's expected, the CODE is what
         *   matters here, not a live connection.
         */
        #endregion

        #region Constants
        // SQL query to execute
        const string _sql = "SELECT LawName, LawText FROM MurphysLaws";
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                // SQLite: genuinely runnable, no external server required
                UsingSqlite();
                GenericFunctions.Pause();

                // The rest need a real server, see README.md, and will print a
                //   "could not connect" message rather than crashing if you haven't set one up
                UsingMySql();
                GenericFunctions.Pause();

                UsingPostgreSql();
                GenericFunctions.Pause();

                UsingOracle();
                GenericFunctions.Pause();

                UsingOdbc();
                GenericFunctions.Pause();

                // MongoDB: a genuinely different paradigm, no SQL at all
                UsingMongoDb();
                GenericFunctions.Pause();
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

        #region Lesson Methods
        // SQLite: file-based, serverless, genuinely runnable without any setup
        private static void UsingSqlite()
        {
            string dbPath = Path.Combine(Path.GetTempPath(), $"ch09-sqlite-demo-{Guid.NewGuid():N}.db");
            string connectionString = $"Data Source={dbPath};Version=3;";

            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                using (var createCommand = new SQLiteCommand("CREATE TABLE MurphysLaws (LawName TEXT, LawText TEXT)", connection))
                {
                    createCommand.ExecuteNonQuery();
                }

                using (var insertCommand = new SQLiteCommand("INSERT INTO MurphysLaws (LawName, LawText) VALUES (@LawName, @LawText)", connection))
                {
                    insertCommand.Parameters.AddWithValue("@LawName", "Murphy's Law");
                    insertCommand.Parameters.AddWithValue("@LawText", "Anything that can go wrong will go wrong.");
                    insertCommand.ExecuteNonQuery();
                }

                using var selectCommand = new SQLiteCommand(_sql, connection);
                using var reader = selectCommand.ExecuteReader();

                Console.WriteLine($"Rows in a temporary SQLite database ({dbPath}):");
                while (reader.Read())
                {
                    Console.WriteLine($" - {reader.GetString(0)}: {reader.GetString(1)}");
                }
            }
            finally
            {
                // Unlike every other provider here, there's no server to leave running,
                //   just a file, clean it up now that the demo is done with it.
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        // MySQL: reference only, needs a real MySQL server, see README.md
        private static void UsingMySql()
        {
            const string connectionString = "Server=localhost;Database=ExternalData;Uid=your_username;Pwd=your_password;";

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                using var command = new MySqlCommand(_sql, connection);
                using var reader = command.ExecuteReader();

                Console.WriteLine("Rows from MySQL:");
                while (reader.Read())
                {
                    Console.WriteLine($" - {reader.GetString(0)}: {reader.GetString(1)}");
                }
            }
            catch (Exception ex)
            {
                PrintReferenceOnlyMessage("MySQL", ex);
            }
        }

        // PostgreSQL: reference only, needs a real PostgreSQL server, see README.md
        private static void UsingPostgreSql()
        {
            const string connectionString = "Host=localhost;Database=ExternalData;Username=your_username;Password=your_password;";

            try
            {
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                // Note the lowercase table/column names: PostgreSQL folds unquoted
                //   identifiers to lowercase, so "MurphysLaws" as created elsewhere in this
                //   chapter would need to be referenced as murphyslaws here.
                using var command = new NpgsqlCommand(_sql, connection);
                using var reader = command.ExecuteReader();

                Console.WriteLine("Rows from PostgreSQL:");
                while (reader.Read())
                {
                    Console.WriteLine($" - {reader.GetString(0)}: {reader.GetString(1)}");
                }
            }
            catch (Exception ex)
            {
                PrintReferenceOnlyMessage("PostgreSQL", ex);
            }
        }

        // Oracle: reference only, needs a real Oracle database, see README.md
        private static void UsingOracle()
        {
            const string connectionString = "User Id=your_username;Password=your_password;Data Source=localhost:1521/XEPDB1;";

            try
            {
                using var connection = new OracleConnection(connectionString);
                connection.Open();

                using var command = new OracleCommand(_sql, connection);
                using var reader = command.ExecuteReader();

                Console.WriteLine("Rows from Oracle:");
                while (reader.Read())
                {
                    Console.WriteLine($" - {reader.GetString(0)}: {reader.GetString(1)}");
                }
            }
            catch (Exception ex)
            {
                PrintReferenceOnlyMessage("Oracle", ex);
            }
        }

        // ODBC: reference only, generic bridge to any ODBC-compliant source (this example
        //   targets a DSN, but ODBC works equally against MS Access, Excel, or other legacy
        //   systems, only the connection string changes), see README.md
        private static void UsingOdbc()
        {
            const string connectionString = "DSN=YourOdbcDataSourceName;Uid=your_username;Pwd=your_password;";

            try
            {
                using var connection = new OdbcConnection(connectionString);
                connection.Open();

                using var command = new OdbcCommand(_sql, connection);
                using var reader = command.ExecuteReader();

                Console.WriteLine("Rows via ODBC:");
                while (reader.Read())
                {
                    Console.WriteLine($" - {reader.GetString(0)}: {reader.GetString(1)}");
                }
            }
            catch (Exception ex)
            {
                PrintReferenceOnlyMessage("ODBC", ex);
            }
        }

        // MongoDB: reference only, needs a real MongoDB server, see README.md. Deliberately
        //   different shape from everything above: no SQL, no Connection/Command/DataReader,
        //   documents in a collection instead of rows in a table.
        private static void UsingMongoDb()
        {
            const string connectionString = "mongodb://localhost:27017";

            try
            {
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase("ExternalData");
                var collection = database.GetCollection<BsonDocument>("MurphysLaws");

                var newLaw = new BsonDocument
                {
                    { "LawName", "Murphy's Law" },
                    { "LawText", "Anything that can go wrong will go wrong." }
                };
                collection.InsertOne(newLaw);

                var documents = collection.Find(new BsonDocument()).ToList();

                Console.WriteLine("Documents in the MurphysLaws collection:");
                foreach (var document in documents)
                {
                    Console.WriteLine($" - {document["LawName"]}: {document["LawText"]}");
                }
            }
            catch (Exception ex)
            {
                PrintReferenceOnlyMessage("MongoDB", ex);
            }
        }
        #endregion

        #region Helper Functions
        // Prints a clear, expected-failure message rather than letting an unhandled
        //   connection exception (from a provider with no server actually set up) crash
        //   the rest of this demo.
        private static void PrintReferenceOnlyMessage(string providerName, Exception ex)
        {
            Console.WriteLine($"Could not connect to {providerName} (expected unless you've set one up, see README.md): {ex.Message}");
        }
        #endregion
    }
}
#pragma warning restore S2068

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
