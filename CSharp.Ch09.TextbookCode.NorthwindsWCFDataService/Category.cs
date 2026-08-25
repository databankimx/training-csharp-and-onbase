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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace NorthwindsWCFDataService
{
    /// <summary>
    /// Maps to the Categories table in the Northwinds database
    /// </summary>
    [Table("Categories")]
    public class Category
    {
        #region Properties
        /// <summary>
        /// Identity primary key
        /// </summary>
        [Key]
        public int CategoryID { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Category description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Products belonging to this category
        /// </summary>
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
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
