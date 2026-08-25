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
#endregion

namespace NorthwindsWCFDataService
{
    /// <summary>
    /// Entity Framework context for the (simplified) Northwinds database. Code First against
    /// an ALREADY-EXISTING database, matching CSharp.Ch09.TextbookCode.NorthwindsConsole's
    /// approach exactly (both point at the same Northwinds database, see that project's
    /// README.md for setup), rather than the original download's EDMX-based, Database First
    /// model, see this project's LectureNotes.md for the full reasoning.
    ///
    /// DataService&lt;T&gt; (see NorthwindsService.svc.cs) works against this Code First
    /// DbContext exactly as it would against an EDMX-based ObjectContext, WCF Data Services
    /// has supported both since version 5.x.
    /// </summary>
    public class NorthwindsEntities : DbContext
    {
        #region Constructors
        /// <summary>
        /// Create a new instance of the NorthwindsEntities class, using the "Northwinds"
        /// connection string entry from Web.config
        /// </summary>
        public NorthwindsEntities() : base("name=Northwinds")
        {
            // The database and its tables already exist (see NorthwindsConsole's
            //   README.md), this context should never attempt to create or alter their
            //   structure.
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
