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
using Samples.MvcWebPortal.Core.Models;

namespace Samples.MvcWebPortal.Core.Data
{
    /// <summary>
    /// EF Core DbContext, Code-First, registered via dependency injection in Program.cs.
    /// Unlike Samples.MvcWebPortal's LocationLookupController (which constructs
    /// "new ExternalDataEntities()" directly, this context is injected and its lifetime is
    /// managed entirely by the DI container, no manual disposal needed anywhere. See
    /// LectureNotes.md.
    /// </summary>
    public class LocationLookupContext(DbContextOptions<LocationLookupContext> options) : DbContext(options)
    {
        #region Properties
        /// <summary>
        /// Sets the ZipCodes DbSet for EF Core to use. This is the only DbSet in this context, as we only need to query zip codes.
        /// </summary>
        public DbSet<ZipCode> ZipCodes => Set<ZipCode>();
        #endregion

        #region Parent Overrides
        /// <summary>
        /// Configures the model for the context during initialization.
        /// </summary>
        /// <remarks>Maps the ZipCode.ZipCode1 property to the database column named "ZipCode".</remarks>
        /// <param name="modelBuilder">Provides a builder used to configure the context's entity mappings.</param>
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
