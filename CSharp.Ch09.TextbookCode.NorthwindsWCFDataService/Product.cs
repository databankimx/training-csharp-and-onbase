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

namespace NorthwindsWCFDataService
{
    /// <summary>
    /// Maps to the Products table in the Northwinds database
    /// </summary>
    [Table("Products")]
    public class Product
    {
        #region Properties
        /// <summary>
        /// Identity primary key
        /// </summary>
        [Key]
        public int ProductID { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Foreign key to the owning Category
        /// </summary>
        public int? CategoryID { get; set; }

        /// <summary>
        /// Unit price
        /// </summary>
        public decimal? UnitPrice { get; set; }

        /// <summary>
        /// Units currently in stock
        /// </summary>
        public short? UnitsInStock { get; set; }

        /// <summary>
        /// Owning category
        /// </summary>
        public virtual Category Category { get; set; }
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
