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
#endregion

namespace CSharp.Ch05.ImplementingClassHierarchies.Models.Objects
{
    /// <summary>
    /// Defines a separate comparer class for objects of type Car
    /// </summary>
    public class CarComparer : IComparer<Car>
    {
        #region Properties
        /// <summary>
        /// Defines fields to compare for equivalency
        /// </summary>
        public enum CompareField
        {
            /// <summary>
            /// Model name of the car (default)
            /// </summary>
            Name,

            /// <summary>
            /// Maximum speed in miles per hour.
            /// </summary>
            MaxMph,

            /// <summary>
            /// Represents power output in horsepower.
            /// </summary>
            Horsepower,

            /// <summary>
            /// Price of the car in USD.
            /// </summary>
            Price
        }

        /// <summary>
        /// Defines sorting property for objects
        /// </summary>
        public CompareField SortBy = CompareField.Name;
        #endregion

        /* IComparer Notes:
         * Implementing IComparer provides a generic version of IComparable, where we can specify the
         * data type when calling the method rather than calling it directly from the class instance
         *
         * In addition, because it is type-checked (remember the IComparable method just takes an object),
         * IComparer can be more reliable and is the preferred method for implementing sort comparisons
         */

        #region IComparer
        /// <summary>
        /// Defines a comparer mechanism for Car objects (implements IComparer)
        /// </summary>
        /// <param name="x">Car object to compare</param>
        /// <param name="y">Car object to compare</param>
        /// <returns>Comparison value</returns>
        public int Compare(Car x, Car y)
        {
            #pragma warning disable IDE0066 // Not using a switch expression for the lesson
            #pragma warning disable S125    // Commented code intentionally left for lesson purposes
            // return SortBy switch
            // {
            //     CompareField.MaxMph => x.MaxMph.CompareTo(y.MaxMph),
            //     CompareField.Horsepower => x.Horsepower.CompareTo(y.Horsepower),
            //     CompareField.Price => x.Price.CompareTo(y.Price),
            //     _ => x.Name.CompareTo(y.Name),
            // };
            #pragma warning restore S125
            switch (SortBy)
            {
                case CompareField.MaxMph:
                    return x.MaxMph.CompareTo(y.MaxMph);
                case CompareField.Horsepower:
                    return x.Horsepower.CompareTo(y.Horsepower);
                case CompareField.Price:
                    return x.Price.CompareTo(y.Price);
                case CompareField.Name:
                default:
                    return x.Name.CompareTo(y.Name);
            }
            #pragma warning restore IDE0066
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
