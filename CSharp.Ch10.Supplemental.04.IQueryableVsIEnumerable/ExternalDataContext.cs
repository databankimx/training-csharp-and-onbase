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
using CSharp.Ch10.Supplemental._04.IQueryableVsIEnumerable.Models;
#endregion

namespace CSharp.Ch10.Supplemental._04.IQueryableVsIEnumerable
{
    /// <summary>
    /// Entity Framework context for the ExternalData database, same database and pattern as
    /// CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework
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
            Database.SetInitializer<ExternalDataContext>(null);
        }
        #endregion

        #region DbSets
        /// <summary>
        /// Murphy's Laws
        /// </summary>
        public DbSet<MurphysLaw> MurphysLaws { get; set; }
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
