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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace CSharp.Ch09.Supplemental._01.AdoNetAndEntityFramework.Models.Objects
{
    /// <summary>
    /// Maps to the dbo.ZipCodes table in the ExternalData database. Named "ZipCodeRecord"
    /// rather than "ZipCode" specifically so its ZipCode property (see below) can be named
    /// to match the actual database column exactly, C# does not allow a member to share
    /// its enclosing type's exact name (CS0542), so "ZipCode.ZipCode" isn't legal, even
    /// though "ZipCodeRecord.ZipCode" is.
    /// </summary>
    [Table("ZipCodes")]
    public class ZipCodeRecord
    {
        #region Properties
        /// <summary>
        /// Identity primary key
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// State abbreviation or name
        /// </summary>
        [MaxLength(20)]
        public string State { get; set; }

        /// <summary>
        /// County name
        /// </summary>
        [MaxLength(50)]
        public string County { get; set; }

        /// <summary>
        /// City name
        /// </summary>
        [MaxLength(50)]
        public string City { get; set; }

        /// <summary>
        /// ZIP code. Named to match the actual database column exactly rather than relying
        /// on a [Column("ZipCode")] mapping, see the note in LectureNotes.md:
        /// Database.SqlQuery&lt;T&gt;() (used in EfCallStoredProcedure()) matches by column
        /// name directly and does NOT honor [Column] attribute mappings the way an ordinary
        /// DbSet&lt;T&gt; LINQ query does, so a mismatched property name here would work fine
        /// for EfSelectRecords() but throw for EfCallStoredProcedure().
        /// </summary>
        [MaxLength(10)]
        public string ZipCode { get; set; }
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
