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
using CSharp.SharedLibrary.Models;
using Samples.WcfService.Models.Enumerations;
#endregion

namespace Samples.WcfService.Models.Configuration
{
    /// <summary>
    /// Defines connection settings for the lookup database
    /// </summary>
    public class DatabaseSettings : ConfigurationElement
    {
        #region Properties
        /// <summary>
        /// Database server architecture
        /// </summary>
        [ConfigurationProperty("architecture", IsRequired = true)]
        public DbArchitecture Architecture
        {
            get => (DbArchitecture)base["architecture"];
            set => base["architecture"] = value;
        }

        /// <summary>
        /// Server hostname (SQL Server and MySQL)
        /// </summary>
        [ConfigurationProperty("server", IsRequired = false)]
        public string Server
        {
            get => (string)base["server"];
            set => base["server"] = value;
        }

        /// <summary>
        /// Port number (SQL Server and MySQL)
        /// </summary>
        [ConfigurationProperty("port", IsRequired = false)]
        public int Port
        {
            get => (int)base["port"];
            set => base["port"] = value;
        }

        /// <summary>
        /// Instance name (SQL Server only)
        /// </summary>
        [ConfigurationProperty("sqlInstance", IsRequired = false)]
        public string SqlInstance
        {
            get => (string)base["sqlInstance"];
            set => base["sqlInstance"] = value;
        }

        /// <summary>
        /// Data source name (ODBC only)
        /// </summary>
        [ConfigurationProperty("odbcDataSource", IsRequired = false)]
        public string OdbcDataSource
        {
            get => (string)base["odbcDataSource"];
            set => base["odbcDataSource"] = value;
        }

        /// <summary>
        /// TNS Name (Oracle only)
        /// </summary>
        [ConfigurationProperty("oracleTnsName", IsRequired = false)]
        public string OracleTnsName
        {
            get => (string)base["oracleTnsName"];
            set => base["oracleTnsName"] = value;
        }

        /// <summary>
        /// Database Name (SQL Server and MySQL)
        /// </summary>
        [ConfigurationProperty("database", IsRequired = false)]
        public string Database
        {
            get => (string)base["database"];
            set => base["database"] = value;
        }

        /// <summary>
        /// When true, use NT authentication
        /// </summary>
        [ConfigurationProperty("ntAuthentication", IsRequired = false)]
        public bool NtAuthentication
        {
            get => (bool)base["ntAuthentication"];
            set => base["ntAuthentication"] = value;
        }

        /// <summary>
        /// Database login user name
        /// </summary>
        [ConfigurationProperty("username", IsRequired = false)]
        public string Username
        {
            get => (string)base["username"];
            set => base["username"] = value;
        }

        /// <summary>
        /// Database login password
        /// </summary>
        [ConfigurationProperty("password", IsRequired = false)]
        public string Password
        {
            get => (string)base["password"];
            set => base["password"] = value;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Generate the connection string for selected database
        /// </summary>
        /// <returns>Connection string</returns>
        public string ConnectionString()
        {
            return Architecture switch
            {
                DbArchitecture.Odbc => OdbcConnectionString(),
                DbArchitecture.MySql => MySqlConnectionString(),
                DbArchitecture.Oracle => OracleConnectionString(),
                DbArchitecture.SqlServer => SqlServerConnectionString(),
                _ => throw new DatabankException($"DB architecture [{Architecture}] not supported!"),
            };
        }
        #endregion

        #region Private Methods
        // Generate SQL Server connection string
        // Server=<hostname>[\<instance>][,<port>];Database=<database>;[User Id=<username>;Password=<password>;][Integrated Security=true;]
        private string SqlServerConnectionString()
        {
            if (string.IsNullOrEmpty(Server))
                throw new DatabankException("Cannot create SQL Server connection sting without a server name!");

            if (string.IsNullOrEmpty(Database))
                throw new DatabankException("Cannot create SQL Server connection sting without a database name!");

            if (!NtAuthentication && (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password)))
                throw new DatabankException("Cannot create SQL Server connection sting without a username and password!");

            string server = $"Server={Server}{(Port > 0 ? $",{Port}" : "")}{(string.IsNullOrEmpty(SqlInstance) ? "" : $@"\{SqlInstance}")};";
            string database = $"Database={Database};";
            string credentials = NtAuthentication
                ? "Integrated Security=true;"
                : $"User Id={Username};Password={Password};";

            return $"{server}{database}{credentials}";
        }

        // Generate ODBC connection string
        // DSN=<data source>;Uid=<username>;Pwd=<password>
        private string OdbcConnectionString()
        {
            if (string.IsNullOrEmpty(OdbcDataSource))
                throw new DatabankException("Cannot create ODBC connection sting without a data source name!");

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
                throw new DatabankException("Cannot create ODBC connection sting without a username and password!");

            return $"DSN={OdbcDataSource};Uid={Username};Pwd={Password};";
        }

        // Generate MySQL connection string
        // Server=<hostname>;[Port=<port>;]Database=<database>;[Uid=<username>;Pwd=<password>;][IntegratedSecurity=true;]
        private string MySqlConnectionString()
        {
            if (string.IsNullOrEmpty(Server))
                throw new DatabankException("Cannot create MySQL connection sting without a server name!");

            if (string.IsNullOrEmpty(Database))
                throw new DatabankException("Cannot create MySQL connection sting without a database name!");

            if (!NtAuthentication && (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password)))
                throw new DatabankException("Cannot create MySQL connection sting without a username and password!");

            string server = $"Server={Server};";
            string port = Port > 0 ? $"Port={Port};" : "";
            string database = $"Database={Database};";
            string credentials = NtAuthentication
                ? "IntegratedSecurity=true;"
                : $"Uid={Username};Pwd={Password};";

            return $"{server}{port}{database}{credentials}";
        }

        // Generate Oracle connection string
        // Data Source=<tns name>;[User Id=<username>;Password=<password>;][Integrated Security=SSPI;]
        private string OracleConnectionString()
        {
            if (string.IsNullOrEmpty(OracleTnsName))
                throw new DatabankException("Cannot create Oracle connection sting without a TNS name!");

            if (!NtAuthentication && (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password)))
                throw new DatabankException("Cannot create Oracle connection sting without a username and password!");

            string tns = $"Data Source={OracleTnsName};";
            string credentials = NtAuthentication
                ? "Integrated Security=SSPI;"
                : $"User Id={Username};Password={Password};";

            return $"{tns}{credentials}";
        }
        #endregion

        #region Parent Class Overrides
        /// <summary>
        /// Allow the class properties to be editable
        /// </summary>
        /// <returns>False (not read-only)</returns>
        public override bool IsReadOnly()
        {
            return false;
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
