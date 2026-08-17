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

using System;

namespace CSharp.Ch05.TextbookCode.IComparableCars
{
    class Car : IComparable<Car>
    {
        public string Name { get; set; }
        public int MaxMph { get; set; }
        public int Horsepower { get; set; }
        public decimal Price { get; set; }

        // Non-generic version.
        // Compare Cars alphabetically by Name.
        //public int CompareTo(object obj)
        //{
        //    if (!(obj is Car))
        //        throw new ArgumentException("Object is not a Car");

        //    Car other = obj as Car;
        //    return Name.CompareTo(other.Name);
        //}

        // Generic version.
        // Compare Cars alphabetically by Name.
        public int CompareTo(Car other)
        {
            return this.Name.CompareTo(other.Name);
        }
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
