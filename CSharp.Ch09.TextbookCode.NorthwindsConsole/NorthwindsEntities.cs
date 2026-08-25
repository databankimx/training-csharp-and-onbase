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
using NorthwindsConsole.Models;
#endregion

namespace NorthwindsConsole
{
    /// <summary>
    /// Entity Framework context for the (simplified) Northwinds database. Code First against
    /// an ALREADY-EXISTING database, matching the pattern established in
    /// CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework, rather than the original
    /// textbook download's EDMX-based, Database First model, see LectureNotes.md for why.
    /// </summary>
    public class NorthwindsEntities : DbContext
    {
        #region Constructors
        /// <summary>
        /// Create a new instance of the NorthwindsEntities class, using the "Northwinds"
        /// connection string entry from App.config
        /// </summary>
        public NorthwindsEntities() : base("name=Northwinds")
        {
            // The database and its tables already exist (see README.md), this context
            //   should never attempt to create or alter their structure.
            Database.SetInitializer<NorthwindsEntities>(null);
        }
        #endregion

        #region DbSets
        /// <summary>
        /// Product categories
        /// </summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>
        /// Products
        /// </summary>
        public DbSet<Product> Products { get; set; }
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
