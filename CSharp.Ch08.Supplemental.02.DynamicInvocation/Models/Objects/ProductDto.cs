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

namespace CSharp.Ch08.Supplemental._02.DynamicInvocation.Models.Objects
{
    /// <summary>
    /// A data-transfer-shaped class deliberately similar to, but distinct from, Product,
    /// used to demonstrate a reflection-based property mapper. Id/Name/Price match Product
    /// by name and type; Source does not exist on Product at all, and is included specifically
    /// to show that the mapper only copies properties that genuinely match on both ends.
    /// </summary>
    public class ProductDto
    {
        #region Properties
        /// <summary>
        /// Product ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Product Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Product Price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Where this DTO came from, has no counterpart on Product
        /// </summary>
        public string Source { get; set; } = "Unknown";
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
