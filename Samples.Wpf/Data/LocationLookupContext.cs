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
using Microsoft.EntityFrameworkCore;
using Samples.Wpf.Models;
#endregion

namespace Samples.Wpf.Data
{
    /// <summary>
    /// EF Core DbContext, Code-First. Unlike the ASP.NET Core samples, there's no DI
    /// container automatically providing this, MainViewModel constructs it directly (see
    /// LectureNotes.md for the fuller discussion of DI in WPF).
    /// </summary>
    public class LocationLookupContext(DbContextOptions<LocationLookupContext> options) : DbContext(options)
    {
        #region Properties
        /// <summary>
        /// Sets the ZipCodes table in the database. This is the only table in this sample database.
        /// </summary>
        public DbSet<ZipCode> ZipCodes => Set<ZipCode>();
        #endregion

        #region Parent Overrides
        /// <summary>
        /// Configures the context model by mapping `ZipCode.ZipCode1` to the `ZipCode` column.
        /// </summary>
        /// <remarks>Called when the model for a derived context has been initialized and can be further
        /// configured.</remarks>
        /// <param name="modelBuilder">Builder used to configure the model for this context.</param>
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
