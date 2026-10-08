#region Copyright
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
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

#nullable disable

#region Using Directives
using Microsoft.EntityFrameworkCore;
#endregion

namespace EForms.TrainingNavigator.Data
{
    /// <summary>
    /// Represents the Entity Framework Core database context for the application and provides access to ZIP code data.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    /// <remarks>Configures the ZipCode entity schema, including the primary key and maximum lengths for
    /// State, County, City, and Zip properties.</remarks>
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        #region Properties
        /// <summary>
        /// Gets or sets the zip code entities.
        /// </summary>
        public DbSet<ZipCode> ZipCodes { get; set; }
        #endregion

        #region Overrides
        /// <summary>
        /// Configures the entity model for the context, including key and property length constraints for the ZipCode
        /// entity.
        /// </summary>
        /// <remarks>Calls the base implementation before applying ZipCode configuration. Sets Id as the
        /// primary key and limits State, County, and City to 50 characters, and Zip to 5 characters.</remarks>
        /// <param name="modelBuilder">Provides the builder used to define the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the ZipCodes table
            modelBuilder.Entity<ZipCode>()
                .HasKey(z => z.Id);

            modelBuilder.Entity<ZipCode>()
                .Property(z => z.State)
                .HasMaxLength(50);

            modelBuilder.Entity<ZipCode>()
                .Property(z => z.County)
                .HasMaxLength(50);

            modelBuilder.Entity<ZipCode>()
                .Property(z => z.City)
                .HasMaxLength(50);

            modelBuilder.Entity<ZipCode>()
                .Property(z => z.Zip)
                .HasMaxLength(5);
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
