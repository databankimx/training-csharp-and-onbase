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
using Samples.WinForms.Models;
#endregion

namespace Samples.WinForms.Data
{
    /// <summary>
    /// EF Core DbContext, Code-First. Constructed directly in MainForm's search event
    /// handler, there's no DI container in this project, matching Samples.Wpf's own
    /// approach (see LectureNotes.md for the fuller discussion).
    /// </summary>
    public class LocationLookupContext(DbContextOptions<LocationLookupContext> options) : DbContext(options)
    {
        #region Properties
        /// <summary>
        /// Gets the ZipCodes table in the database. This is the only table in this sample database.
        /// </summary>
        public DbSet<ZipCode> ZipCodes => Set<ZipCode>();
        #endregion

        #region Parent Overrides
        /// <summary>
        /// Configures the context model by mapping <c>ZipCode.ZipCode1</c> to the <c>ZipCode</c> column.
        /// </summary>
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
