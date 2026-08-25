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
    /// Runs the same query as UnsafeDatabaseUtility, but with the search value passed as a
    /// genuine SQL parameter rather than spliced directly into the SQL text. Compare the two
    /// classes side by side, the only real difference is a few lines in ExecuteQuery().
    /// </summary>
    public class SafeDatabaseUtility : IDisposable
    {
        #region Private Members
        // Database connection object
        private SqlConnection connection;

        // Database command object
        private SqlCommand command;
        #endregion

        #region Constants
        // SQL Query to Execute. Note the query text itself never contains the search value,
        //   only the parameter placeholder ("@lawName") that gets added below.
        private const string SqlQuery = "SELECT RTRIM(LawText) FROM dbo.MurphysLaws WHERE LawName = @lawName";
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
        #endregion

        #region IDisposable
        /// <summary>
        /// Safely finalize the database utility
        /// </summary>
        ~SafeDatabaseUtility()
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
