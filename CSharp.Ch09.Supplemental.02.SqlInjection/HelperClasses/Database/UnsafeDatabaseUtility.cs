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
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using CSharp.SharedLibrary.Models;
using GC = System.GC;
#endregion

namespace CSharp.Ch09.Supplemental._02.SqlInjection.HelperClasses.Database
{
    /// <summary>
    /// Runs the same query as SafeDatabaseUtility, but builds the SQL by splicing the search
    /// value directly into the query text. This is the vulnerable version, deliberately, for
    /// this lesson only. NEVER write code that constructs SQL this way. See Program.cs's
    /// Chapter Notes and the embedded walkthrough for exactly what this allows an attacker
    /// to do with nothing more than what they can type into an ordinary search box.
    /// </summary>
    public class UnsafeDatabaseUtility : IDisposable
    {
        #region Private Members
        // Database connection object
        private SqlConnection connection;

        // Database command object
        private SqlCommand command;
        #endregion

        #region Constants
        // SQL Query to Execute (missing its WHERE value on purpose, see ExecuteQuery() below)
        private const string SqlQuery = "SELECT RTRIM(LawText) FROM dbo.MurphysLaws WHERE LawName = ";
        #endregion

        #region Public Methods
        /// <summary>
        /// Open a new connection to the database
        /// </summary>
        public void Connect()
        {
            try
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
                Console.WriteLine("Opened database connection!");
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error connecting to database!", ex);
            }
        }

        /// <summary>
        /// Close an existing connection to the database
        /// </summary>
        public void Disconnect()
        {
            try
            {
                command?.Dispose();
                command = null;
                if (connection == null) return;
                if (connection.State == ConnectionState.Open) connection.Close();
                connection.Dispose();
                connection = null;
                Console.WriteLine("Closed database connection!");
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error disconnecting from database!", ex);
            }
        }

        /// <summary>
        /// Run the SQL query using the provided law name value
        /// </summary>
        /// <param name="lawName">Law name to filter query</param>
        /// <returns>Matching law text(s)</returns>
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
        #endregion

        #region IDisposable
        /// <summary>
        /// Safely finalize the database utility
        /// </summary>
        ~UnsafeDatabaseUtility()
        {
            Dispose(false);
        }

        /// <summary>
        /// Explicitly dispose the database utility
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Explicitly dispose the database utility
        /// </summary>
        /// <param name="releaseManagedObjects">When true, dispose managed objects</param>
        protected virtual void Dispose(bool releaseManagedObjects)
        {
            if (!releaseManagedObjects) return;
            Disconnect();
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
