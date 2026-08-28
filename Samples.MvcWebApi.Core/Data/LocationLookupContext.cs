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
using Samples.MvcWebApi.Core.Models;

namespace Samples.MvcWebApi.Core.Data
{
    /// <summary>
    /// EF Core DbContext, Code-First. Registered via dependency injection in Program.cs
    /// (builder.Services.AddDbContext), not constructed directly the way
    /// Samples.MvcWebApi's EF6 LocationLookupDatabase class was, see LectureNotes.md.
    /// </summary>
    public class LocationLookupContext(DbContextOptions<LocationLookupContext> options) : DbContext(options)
    {
        public DbSet<ZipCode> ZipCodes => Set<ZipCode>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mirrors the actual column name ("ZipCode") used by the same ZipCodes table the
            // classic Samples.MvcWebApi project queries, ZipCode1 exists only because "ZipCode"
            // collides with the table's own name in the EF6 Database-First tooling's naming
            // convention, EF Core Code-First has no such restriction, but the column mapping
            // below keeps this project pointed at the SAME real table either way.
            modelBuilder.Entity<ZipCode>().Property(z => z.ZipCode1).HasColumnName("ZipCode");
        }
    }
}
