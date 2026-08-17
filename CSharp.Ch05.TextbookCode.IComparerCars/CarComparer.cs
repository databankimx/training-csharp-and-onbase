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

using System.Collections.Generic;

namespace CSharp.Ch05.TextbookCode.IComparerCars
{
    class CarComparer : IComparer<Car>
    {
        // The field to compare.
        public enum CompareField
        {
            Name,
            MaxMph,
            Horsepower,
            Price,
        }
        public CompareField SortBy = CompareField.Name;

        public int Compare(Car x, Car y)
        {
            switch (SortBy)
            {
                case CompareField.Name:
                    return x.Name.CompareTo(y.Name);
                case CompareField.MaxMph:
                    return x.MaxMph.CompareTo(y.MaxMph);
                case CompareField.Horsepower:
                    return x.Horsepower.CompareTo(y.Horsepower);
                case CompareField.Price:
                    return x.Price.CompareTo(y.Price);
            }
            return x.Name.CompareTo(y.Name);
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
