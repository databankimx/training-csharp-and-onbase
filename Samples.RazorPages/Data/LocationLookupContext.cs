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

using Microsoft.EntityFrameworkCore;
using Samples.RazorPages.Models;

namespace Samples.RazorPages.Data
{
    /// <summary>
    /// EF Core DbContext, Code-First, registered via dependency injection in Program.cs.
    /// </summary>
    public class LocationLookupContext(DbContextOptions<LocationLookupContext> options) : DbContext(options)
    {
        #region Properties
        /// <summary>
        /// ZipCodes table, mapped to ZipCode model class.
        /// </summary>
        public DbSet<ZipCode> ZipCodes => Set<ZipCode>();
        #endregion

        #region Parent Overrides
        /// <summary>
        /// Builds the model for the context. This method is called when the model for a derived context has been initialized, but before it has been locked down and used to initialize the context. The default implementation of this method does nothing, but it can be overridden in a derived class such that the model can be further configured before it is locked down.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ZipCode>().Property(z => z.ZipCode1).HasColumnName("ZipCode");
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
