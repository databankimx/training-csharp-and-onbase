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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace EForms.TrainingNavigator.Data
{
    /// <summary>
    /// Represents a postal code record that associates a ZIP code with a state, county, and city.
    /// </summary>
    /// <remarks>Mapped to the ZipCodes table for data persistence.</remarks>
    [Table("ZipCodes")]
    public class ZipCode
    {
        #region Properties
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the state name.
        /// </summary>
        /// <remarks>Mapped to the "State" column. Maximum length is 50 characters.</remarks>
        [Column("State")]
        [MaxLength(50)]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the county name.
        /// </summary>
        /// <remarks>Maximum length is 50 characters.</remarks>
        [Column("County")]
        [MaxLength(50)]
        public string County { get; set; }

        /// <summary>
        /// Gets or sets the name of the city.
        /// </summary>
        [Column("City")]
        [MaxLength(50)]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the ZIP code.
        /// </summary>
        /// <remarks>Mapped to the ZipCode column and limited to 5 characters.</remarks>
        [Column("ZipCode")]
        [MaxLength(5)]
        public string Zip { get; set; }
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
