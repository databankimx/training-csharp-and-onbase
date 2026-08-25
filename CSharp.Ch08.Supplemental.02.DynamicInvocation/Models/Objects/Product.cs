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
using System;
#endregion

namespace CSharp.Ch08.Supplemental._02.DynamicInvocation.Models.Objects
{
    /// <summary>
    /// A simple product, used to demonstrate Activator.CreateInstance(), PropertyInfo
    /// GetValue()/SetValue(), and invoking a method that takes parameters via reflection
    /// </summary>
    public class Product
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
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the Product class
        /// </summary>
        public Product() { }

        /// <summary>
        /// Create and initialize a new instance of the Product class
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="name">Product Name</param>
        /// <param name="price">Product Price</param>
        public Product(int id, string name, decimal price)
        {
            Id = id;
            Name = name;
            Price = price;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Apply a discount and return the discounted price, without modifying this instance
        /// </summary>
        /// <param name="percentage">Discount percentage, expressed as a value between 0 and 1</param>
        /// <returns>Discounted price</returns>
        public decimal ApplyDiscount(decimal percentage)
        {
            if (percentage < 0 || percentage > 1)
                throw new ArgumentOutOfRangeException(nameof(percentage), percentage, "Discount percentage must be between 0 and 1.");

            return Price * (1 - percentage);
        }
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
