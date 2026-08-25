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

namespace CSharp.Ch10.Supplemental._04.IQueryableVsIEnumerable.Models
{
    /// <summary>
    /// Maps to the dbo.MurphysLaws table in the ExternalData database, same table used in
    /// CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework
    /// </summary>
    [Table("MurphysLaws")]
    public class MurphysLaw
    {
        #region Properties
        /// <summary>
        /// Identity primary key
        /// </summary>
        [Key]
        [Column("LawID")]
        public short LawId { get; set; }

        /// <summary>
        /// Short name of the law
        /// </summary>
        public string LawName { get; set; }

        /// <summary>
        /// Full text of the law
        /// </summary>
        public string LawText { get; set; }
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
