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
using System.Data.Entity;
using CSharp.Ch09.Supplemental._01.AdoNetAndEntityFramework.Models.Objects;
#endregion

namespace CSharp.Ch09.Supplemental._01.AdoNetAndEntityFramework.Models.Data
{
    /// <summary>
    /// Entity Framework context for the ExternalData database. Uses Code First mapped
    /// against an ALREADY-EXISTING database (see README.md for how to restore it), so
    /// EF's usual "create/migrate the schema for me" behavior is explicitly disabled,
    /// this context only ever reads and writes the tables exactly as restored from the
    /// backup, it never tries to create or alter them.
    /// </summary>
    public class ExternalDataContext : DbContext
    {
        #region Constructors
        /// <summary>
        /// Create a new instance of the ExternalDataContext class, using the "ExternalData"
        /// connection string entry from App.config
        /// </summary>
        public ExternalDataContext() : base("name=ExternalData")
        {
            // Tell EF not to check/create/migrate the schema at all. The database and
            //   its tables already exist (restored from ExternalData.bak per README.md),
            //   and this context should never attempt to modify their structure.
            Database.SetInitializer<ExternalDataContext>(null);
        }
        #endregion

        #region DbSets
        /// <summary>
        /// Murphy's Laws
        /// </summary>
        public DbSet<MurphysLaw> MurphysLaws { get; set; }

        /// <summary>
        /// Zip codes
        /// </summary>
        public DbSet<ZipCodeRecord> ZipCodes { get; set; }
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
