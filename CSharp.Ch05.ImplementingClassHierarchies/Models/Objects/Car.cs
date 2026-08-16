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

namespace CSharp.Ch05.ImplementingClassHierarchies.Models.Objects
{
    /// <summary>
    /// Defines a car and implements IComparable
    /// </summary>
    public class Car : IComparable
    {
        #region Properties
        /// <summary>
        /// Car make
        /// </summary>
        public string Make { get; set; }

        /// <summary>
        /// Car model
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Car model year
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Car horsepower
        /// </summary>
        public int Horsepower { get; set; }

        /// <summary>
        /// Car max speed
        /// </summary>
        public int MaxMph { get; set; }

        /// <summary>
        /// Car price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Car name
        /// </summary>
        public string Name => $"{Year} {Make} {Model}";
        #endregion

        /*
         * Implementing IComparable provides a method to compare two instances of the class base on one or more
         * attribute values to determine which comes first when sorting an array or list
         */

        #region IComparable
        /// <summary>
        /// Implements a comparison method for Car objects
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>Comparison value [lt0 | 0 | gt0]</returns>
        public int CompareTo(object obj)
        {
            // BUG FIX: the original textbook code checked "!(obj is Car)" without first checking
            //     for null. Since "null is Car" is always false, that condition was already true
            //     for a null obj, and the throw branch below then dereferenced obj.GetType().Name
            //     on that same null obj, producing a NullReferenceException instead of a sensible
            //     result. By documented .NET convention, IComparable.CompareTo should never throw
            //     for a null argument, null is treated as sorting before any non-null instance, so
            //     this.CompareTo(null) should return a positive number (this instance comes after
            //     null), not throw. A genuine type mismatch (a non-null object that isn't a Car)
            //     is still correctly an error and still throws ArgumentException, exactly as the
            //     original code intended, only the missing null case has been added.
            if (obj is null) return 1;

            #pragma warning disable IDE0083 // Although pattern matching is preferred, we're leaving `is` here for the lesson
            if (!(obj is Car))
                throw new ArgumentException($"Object type [{obj.GetType().Name}] cannot be compared to type [Car]");
            #pragma warning restore IDE0083

            var other = (Car)obj;

            return string.Compare(Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
        }
        #endregion

        #region Bonus Methods
        // When implementing IComparable, it is standard to implement `Equals()`, ==, !=, <, <=, >, and >= as well.
        // These are not required for the lesson, but are included here for reference.

        /// <summary>
        /// Determines whether the specified object is equal to the current Car
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True if obj is a Car with an equivalent Name</returns>
        public override bool Equals(object obj)
        {
            return obj is Car other && CompareCars(this, other) == 0;
        }

        /// <summary>
        /// Returns a hash code consistent with Equals(). Whenever Equals() is overridden,
        /// GetHashCode() must be too, so equal objects always report the same hash code,
        /// otherwise the type breaks when used as a Dictionary key or stored in a HashSet.
        /// </summary>
        /// <returns>Hash code for the current Car</returns>
        public override int GetHashCode()
        {
            return Name?.ToUpperInvariant().GetHashCode() ?? 0;
        }
        #endregion

        #region Bonus Operator Overloads
        /// <summary>
        /// Equality operator, true when both Car instances are equivalent by Name (or both null)
        /// </summary>
        /// <param name="left">Left-hand side Car</param>
        /// <param name="right">Right-hand side Car</param>
        /// <returns>True if both Car instances are equivalent by Name (or both null)</returns>
        public static bool operator ==(Car left, Car right)
        {
            return CompareCars(left, right) == 0;
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        /// <param name="left">Left-hand side Car</param>
        /// <param name="right">Right-hand side Car</param>
        /// <returns>True if both Car instances are not equivalent by Name (or one is null and the other is not)</returns>
        public static bool operator !=(Car left, Car right)
        {
            return CompareCars(left, right) != 0;
        }

        /// <summary>
        /// Less-than operator, based on CompareTo()
        /// </summary>
        /// <param name="left">Left-hand side Car</param>
        /// <param name="right">Right-hand side Car</param>
        /// <returns>True if the left-hand side Car is less than the right-hand side Car</returns>
        public static bool operator <(Car left, Car right)
        {
            return CompareCars(left, right) < 0;
        }

        /// <summary>
        /// Less-than-or-equal operator, based on CompareTo()
        /// </summary>
        /// <param name="left">Left-hand side Car</param>
        /// <param name="right">Right-hand side Car</param>
        /// <returns>True if the left-hand side Car is less than or equal to the right-hand side Car</returns>
        public static bool operator <=(Car left, Car right)
        {
            return CompareCars(left, right) <= 0;
        }

        /// <summary>
        /// Greater-than operator, based on CompareTo()
        /// </summary>
        /// <param name="left">Left-hand side Car</param>
        /// <param name="right">Right-hand side Car</param>
        /// <returns>True if the left-hand side Car is greater than the right-hand side Car</returns>
        public static bool operator >(Car left, Car right)
        {
            return CompareCars(left, right) > 0;
        }

        /// <summary>
        /// Greater-than-or-equal operator, based on CompareTo()
        /// </summary>
        /// <param name="left">Left-hand side Car</param>
        /// <param name="right">Right-hand side Car</param>
        /// <returns>True if the left-hand side Car is greater than or equal to the right-hand side Car</returns>
        public static bool operator >=(Car left, Car right)
        {
            return CompareCars(left, right) >= 0;
        }
        #endregion

        #region Bosnus Helper Functions
        // Null-safe comparison helper. Two nulls are equal, null sorts before any non-null Car.
        // Only the null-vs-null and null-left cases need special handling here, CompareTo(object)
        //     above already handles a null right-hand side correctly on its own (see the bug fix
        //     note there), but you still can't call an instance method on a null "left" reference,
        //     that's a language-level constraint no amount of null-checking inside CompareTo can help with.
        private static int CompareCars(Car left, Car right)
        {
            if (ReferenceEquals(left, right)) return 0;
            if (left is null) return -1;
            return left.CompareTo(right);
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
