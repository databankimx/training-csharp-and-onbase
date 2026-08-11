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

#region Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
#endregion

namespace CSharp.SharedLibrary.HelperClasses
{
    /* NOTES
     * One of the most useful functions of classes is the ability to extend the functionality of
     *     existing classes (either programmer-created or pre-existing .NET classes can be extended).
     * 
     * In order to expose extension methods, the class in which they are implemented must be a static
     *     class (which cannot be instantiated and belongs to the assembly, not to the calling object).
     * 
     * Syntax:
     *   // Extension methods are declared thus:
     *   access_modifier static return_type MethodName(this extended_type variableName [,arguments]) {} // NOSONAR
     */

    /// <summary>
    /// Exposes class extension methods
    /// </summary>
    public static class GenericExtensions
    {
        #region Public Extension Methods
        /// <summary>
        /// Returns the square of a provided integer
        /// </summary>
        /// <param name="number">integer to square</param>
        /// <returns>Square of a provided integer</returns>
        public static int Square(this int number)
        {
            return number * number;
        }

        /* A good example of the use of an extension method is to combine it with a function overload
         *     in order to add functionality to an existing class.
         *     For example, you might wish to be able to replace text in a string without
         *     case sensitivity. The existing Replace() method in .NET does not carry this functionality.
         */
        /// <summary>
        /// Extension method for String.Replace that accepts comparison type to allow case-insensitive replacement
        /// </summary>
        /// <param name="source">Original string</param>
        /// <param name="oldValue">Pattern to find and replace in the string</param>
        /// <param name="newValue">Text to replace found instances of the pattern</param>
        /// <param name="comparisonType">Rules for comparison - default is CurrentCultureIgnoreCase</param>
        /// <returns></returns>
        public static string Replace(this string source, string oldValue, string newValue, StringComparison comparisonType)
        {
            if (string.IsNullOrEmpty(source)) return "";
            int startIndex = 0;
            while (true)
            {
                startIndex = source.IndexOf(oldValue, startIndex, comparisonType);
                if (startIndex == -1) break;
                source = source.Substring(0, startIndex) + newValue +
                         source.Substring(startIndex + oldValue.Length);
                startIndex += newValue.Length;
            }
            return source;
        }

        /*
         * Some other good examples of the use of extension methods are data type conversions. While
         *    Most types have a ToString() method, many other automatic conversions are not natively supported.
         */
        /// <summary>
        /// Convert string to integer
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <returns></returns>
        public static int ToInt(this string value)
        {
            return int.TryParse(value, out int returnValue)
                ? returnValue
                : 0;
        }

        /// <summary>
        /// Convert string to long integer
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <returns></returns>
        public static long ToLong(this string value)
        {
            return long.TryParse(value, out long returnValue)
                ? returnValue
                : 0;
        }

        /// <summary>
        /// Convert string to float
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <returns></returns>
        public static float ToFloat(this string value)
        {
            return float.TryParse(value, out float returnValue)
                ? returnValue
                : 0.0f;
        }

        /// <summary>
        /// Convert string to double
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <returns></returns>
        public static double ToDouble(this string value)
        {
            return double.TryParse(value, out double returnValue)
                ? returnValue
                : 0.0d;
        }

        /// <summary>
        /// Convert string to decimal
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <returns></returns>
        public static decimal ToDecimal(this string value)
        {
            return decimal.TryParse(value, out decimal returnValue)
                ? returnValue
                : 0.0m;
        }

        /// <summary>
        /// Convert string to boolean value
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <returns></returns>
        public static bool ToBoolean(this string value)
        {
            // This handles more scenarios than bool.Parse(), which only accepts "true" for true

            if (string.IsNullOrEmpty(value)) return false;
            if (int.TryParse(value, out int num)) return num > 0;

            string[] trueValues = ["t", "y"]; // looks for values like "true", "yes"
            return Array.IndexOf(trueValues, value.Substring(0, 1).ToLower()) > -1;
        }

        /// <summary>
        /// An improved Parse for Booleans
        /// </summary>
        /// <param name="value">String to evaluate</param>
        /// <returns>True if value parsed to a valid output</returns>
        public static bool Parse(this string value)
        {

            if (!value.TryParse(out bool result)) throw new FormatException($"Cannot parse [{value}] as Boolean!");

            return result;
        }

        /// <summary>
        /// An improved TryParse for Booleans
        /// </summary>
        /// <param name="value">String to evaluate</param>
        /// <param name="result">Parsed boolean value</param>
        /// <returns>True if value parsed to a valid output</returns>
        public static bool TryParse(this string value, out bool result)
        {
            result = false;

            if (string.IsNullOrEmpty(value))
            {
                result = false;
                return true;
            }

            if (int.TryParse(value, out int num))
            {
                result = num > 0;
                return true;
            }

            string[] trueValues = ["t", "y"]; // looks for values like "true", "yes"
            string[] falseValues = ["f", "n"]; // looks for values like "true", "yes"

            if (Array.IndexOf(trueValues, value.Substring(0, 1).ToLower()) > -1)
            {
                result = true;
                return true;
            }

            if (Array.IndexOf(falseValues, value.Substring(0, 1).ToLower()) > -1)
            {
                result = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Split the delimited list
        /// </summary>
        /// <param name="value">String to split</param>
        /// <param name="delimiter">Delimiter character (default is ',')</param>
        /// <returns></returns>
        public static string[] ToArray(this string value, char delimiter = ',')
        {
            return value.Split(delimiter);
        }

        /// <summary>
        /// Split the delimited list
        /// </summary>
        /// <param name="value">String to split</param>
        /// <param name="delimiter">Delimiter string</param>
        /// <returns></returns>
        public static string[] ToArray(this string value, string delimiter = ",")
        {
            var values = new List<string>();
            while (value.Contains(delimiter))
            {
                values.Add(value.Substring(0, value.IndexOf(delimiter, StringComparison.Ordinal)));
                value = value.Substring(value.IndexOf(delimiter, StringComparison.Ordinal) + delimiter.Length);
            }
            values.Add(value);
            return [.. values];
        }

        /*
         * Still another use for extensions methods is type comparisons
         */

        /// <summary>
        /// Check if a string is a number
        /// </summary>
        /// <param name="value">String to evaluate</param>
        /// <param name="integerOnly">If true, only integer values are allowed (default is false)</param>
        /// <returns></returns>
        public static bool IsNumeric(this string value, bool integerOnly = false)
        {
            return integerOnly ? int.TryParse(value, out _) : double.TryParse(value, out _);
        }

        /// <summary>
        /// Check if a string is a positive number
        /// </summary>
        /// <param name="value"></param>
        /// <param name="integerOnly"></param>
        /// <returns></returns>
        public static bool IsPositive(this string value, bool integerOnly = false)
        {
            if (integerOnly)
            {
                return int.TryParse(value, out int tempInt) && tempInt > 0;
            }
            
            return double.TryParse(value, out double tempDouble) && tempDouble > 0;
        }

        /// <summary>
        /// Check if object is a List
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool IsList(this object o)
        {
            if (o == null) return false;

            return o is IList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        /// <summary>
        /// Check if object is a Dictionary
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool IsDictionary(this object o)
        {
            if (o == null) return false;

            return o is IDictionary &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
        }

        // Uses generic type T applying constraint on types to structs that implement IConvertible
        // See: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/where-generic-type-constraint
        /// <summary>
        /// Returns whether the bit at the specified position is set.
        /// </summary>
        /// <typeparam name="T">Any integer data type</typeparam>
        /// <param name="t">Value to check</param>
        /// <param name="pos">The position of the bit to check where 0 is the least significant bit</param>
        /// <returns>true if the specified bit is on (1), otherwise false</returns>
        public static bool IsBitSet<T>(this T t, int pos) where T : struct, IConvertible
        {
            var value = t.ToInt64(CultureInfo.CurrentCulture);
            return (value & (1 << pos)) != 0;
        }

        /*
         * Although not an extension method, we can take advantage of this static class to add a
         *     sample generic type method to swap the values of two objects
         */
        /// <summary>
        /// Exchange the values of two objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueOne"></param>
        /// <param name="valueTwo"></param>
        public static void Swap<T>(ref T valueOne, ref T valueTwo)
        {
            (valueTwo, valueOne) = (valueOne, valueTwo);
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
