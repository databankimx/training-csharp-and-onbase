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

#region Directives
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using Serilog;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using CSharp.SharedLibrary.Models;
using Samples.WcfService.Models.Configuration;
using Samples.WcfService.Models.Enumerations;
using Samples.WcfService.Models.Objects;
#endregion

namespace Samples.WcfService.HelperClasses
{
    /// <summary>
    /// Exposes functionality for querying the database
    /// </summary>
    public static class Database
    {
        #region Properties
        /// <summary>
        /// Database connection settings (from web.config)
        /// </summary>
        public static DatabaseSettings Settings { get; set; }

        /// <summary>
        /// Serilog logging utility
        /// </summary>
        public static ILogger Logger { get; set; }

        /// <summary>
        /// When true, include trace logging
        /// </summary>
        public static bool DebugMode { get; set; }
        #endregion

        #region Constants
        // SQL query to execute
        private const string Sql = "SELECT State, County, City FROM dbo.ZipCodes WHERE ZipCode = @zipCode";
        #endregion

        #region Private Globals
        // Database connection and command objects
        private static SqlConnection sqlConn;
        private static SqlCommand sqlCmd;
        private static OracleConnection oraConn;
        private static OracleCommand oraCmd;
        private static OdbcConnection odbcConn;
        private static OdbcCommand odbcCmd;
        private static MySqlConnection mySqlConn;
        private static MySqlCommand mySqlCmd;

        // Zip Code to Query
        private static string zip;
        #endregion

        #region Public Methods
        /// <summary>
        /// Look up the location information from the provided zip code value
        /// </summary>
        /// <param name="zipCode">Lookup Zip Code</param>
        /// <returns>State, County, and City<see cref="Location"/></returns>
        public static List<Location> LookupLocations(this string zipCode)
        {
            try
            {
                if (DebugMode) Logger?.Debug("Start location lookup...");

                // Store the zip code as a global 
                zip = zipCode;
                if (DebugMode) Logger?.Debug("Lookup ZIP Code [{ZipCode}]...", zip);

                // Establish a database connection
                Connect();
                if (DebugMode) Logger?.Debug("Connected to database...");

                // Parameterize the SQL query (to avoid SQL injection)
                ParameterizeQuery();

                // Run the query and return the results
                if (DebugMode) Logger?.Debug("Executing query...");
                return ExecuteQuery();
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error looking up locations in database!", ex);
            }
            finally
            {
                // Close the database connection
                Disconnect();
                if (DebugMode) Logger?.Debug("Disconnected from database...");
                if (DebugMode) Logger?.Debug("End location lookup...");
            }
        }
        #endregion

        #region Private Methods
        // Establish a database connection
        #pragma warning disable S3776 // Not overly complex
        private static void Connect()
        #pragma warning restore S3776
        {
            try
            {
                switch (Settings.Architecture)
                {
                    case DbArchitecture.Oracle:
                        if (DebugMode) Logger?.Debug("Connecting to Oracle database [{TnsName}]...", Settings.OracleTnsName);
                        oraConn = new OracleConnection(Settings.ConnectionString());
                        oraConn.Open();
                        if (oraConn.State != ConnectionState.Open)
                            throw new DatabankException("Failed to open Oracle database connection!");
                        return;
                    case DbArchitecture.Odbc:
                        if (DebugMode) Logger?.Debug("Connecting to ODBC database [{DataSource}]...", Settings.OdbcDataSource);
                        odbcConn = new OdbcConnection(Settings.ConnectionString());
                        odbcConn.Open();
                        if (odbcConn.State != ConnectionState.Open)
                            throw new DatabankException("Failed to open ODBC database connection!");
                        return;
                    case DbArchitecture.MySql:
                        if (DebugMode) Logger?.Debug("Connecting to MySQL database [{Database}]...", Settings.Database);
                        mySqlConn = new MySqlConnection(Settings.ConnectionString());
                        mySqlConn.Open();
                        if (mySqlConn.State != ConnectionState.Open)
                            throw new DatabankException("Failed to open MySQL database connection!");
                        return;
                    case DbArchitecture.SqlServer:
                        if (DebugMode) Logger?.Debug("Connecting to SQL Server database [{Database}]...", Settings.Database);
                        sqlConn = new SqlConnection(Settings.ConnectionString());
                        sqlConn.Open();
                        if (sqlConn.State != ConnectionState.Open)
                            throw new DatabankException("Failed to open SQL Server database connection!");
                        return;
                    default:
                        throw new DatabankException($"DB architecture [{Settings.Architecture}] not supported!");
                }
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error connecting to database!", ex);
            }
        }

        // Parameterize the SQL query (to avoid SQL injection)
        private static void ParameterizeQuery()
        {
            try
            {
                switch (Settings.Architecture)
                {
                    case DbArchitecture.Oracle:
                        #pragma warning disable S1192 // Keeping literals in lesson
                        oraCmd = new OracleCommand(Sql.Replace("@zipCode", ":1"), oraConn);
                        #pragma warning restore S1192
                        oraCmd.Parameters.Add(new OracleParameter
                        {
                            ParameterName = "zipCode",
                            OracleDbType = OracleDbType.Char,
                            Value = zip
                        });
                        return;
                    case DbArchitecture.Odbc:
                        odbcCmd = new OdbcCommand(Sql.Replace("@zipCode", "?"), odbcConn);
                        odbcCmd.Parameters.AddWithValue("zipCode", zip);
                        return;
                    case DbArchitecture.MySql:
                        mySqlCmd = new MySqlCommand(Sql.Replace("@zipCode", "?zipCode"), mySqlConn);
                        mySqlCmd.Parameters.AddWithValue("?zipCode", zip);
                        return;
                    case DbArchitecture.SqlServer:
                        sqlCmd = new SqlCommand(Sql, sqlConn);
                        sqlCmd.Parameters.AddWithValue("@zipCode", zip);
                        return;
                    default:
                        throw new DatabankException($"DB architecture [{Settings.Architecture}] not supported!");
                }
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error parameterizing SQL query!", ex);
            }
        }

        // Run the query and return the results
        #pragma warning disable S3776 // Not overly complex
        private static List<Location> ExecuteQuery()
        #pragma warning restore S3776
        {
            try
            {
                var locations = new List<Location>();

                switch (Settings.Architecture)
                {
                    case DbArchitecture.Oracle:
                        using (var oraReader = oraCmd.ExecuteReader())
                        {
                            #pragma warning disable S1168 // Intentionally returning null to indicate no results found
                            if (!oraReader.HasRows) return null;
                            #pragma warning restore S1168
                            while (oraReader.Read())
                            {
                                locations.Add(new Location
                                {
                                    State = oraReader.GetString(0),
                                    County = oraReader.GetString(1),
                                    City = oraReader.GetString(2),
                                    ZipCode = zip
                                });
                            }
                        }
                        break;
                    case DbArchitecture.Odbc:
                        using (var odbcReader = odbcCmd.ExecuteReader())
                        {
                            if (!odbcReader.HasRows) return null;
                            while (odbcReader.Read())
                            {
                                locations.Add(new Location
                                {
                                    State = odbcReader.GetString(0),
                                    County = odbcReader.GetString(1),
                                    City = odbcReader.GetString(2),
                                    ZipCode = zip
                                });
                            }
                        }
                        break;
                    case DbArchitecture.MySql:
                        using (var mySqlReader = mySqlCmd.ExecuteReader())
                        {
                            if (!mySqlReader.HasRows) return null;
                            while (mySqlReader.Read())
                            {
                                locations.Add(new Location
                                {
                                    State = mySqlReader.GetString(0),
                                    County = mySqlReader.GetString(1),
                                    City = mySqlReader.GetString(2),
                                    ZipCode = zip
                                });
                            }
                        }
                        break;
                    case DbArchitecture.SqlServer:
                        using (var sqlReader = sqlCmd.ExecuteReader())
                        {
                            if (!sqlReader.HasRows) return null;
                            while (sqlReader.Read())
                            {
                                locations.Add(new Location
                                {
                                    State = sqlReader.GetString(0),
                                    County = sqlReader.GetString(1),
                                    City = sqlReader.GetString(2),
                                    ZipCode = zip
                                });
                            }
                        }
                        break;
                    default:
                        throw new DatabankException($"DB architecture [{Settings.Architecture}] not supported!");
                }

                if (DebugMode && locations.Count > 0)
                {
                    foreach (var location in locations)
                        Logger?.Debug("Found Location: [{Location}]", location.Info);
                }
                return locations.Count > 0 ? locations : null;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error executing database query!", ex);
            }
        }

        // Close the database connection
        private static void Disconnect()
        {
            sqlCmd?.Dispose();
            if (sqlConn != null && sqlConn.State == ConnectionState.Open) sqlConn.Close();
            sqlConn?.Dispose();
            oraCmd?.Dispose();
            if (oraConn != null && oraConn.State == ConnectionState.Open) oraConn.Close();
            oraConn?.Dispose();
            mySqlCmd?.Dispose();
            if (mySqlConn != null && mySqlConn.State == ConnectionState.Open) mySqlConn.Close();
            mySqlConn?.Dispose();
            odbcCmd?.Dispose();
            if (odbcConn != null && odbcConn.State == ConnectionState.Open) odbcConn.Close();
            odbcConn?.Dispose();
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
