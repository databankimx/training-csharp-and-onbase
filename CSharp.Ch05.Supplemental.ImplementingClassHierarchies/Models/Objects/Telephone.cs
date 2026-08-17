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
using System.IO;
using System.Text.RegularExpressions;
#endregion

namespace CSharp.Ch05.Supplemental.ImplementingClassHierarchies.Models.Objects
{
    /// <summary>
    /// Defines a telephone number for a contact
    /// </summary>
    public class Telephone
    {
        #region Private Members
        // Phone number
        private string number;

        // Matches non-digits
        private static readonly Regex NonDigits = new Regex(@"\D");
        #endregion

        #region Properties
        /// <summary>
        /// Encapsulating property for phone number
        /// </summary>
        public string Number
        {
            get => FormatPhoneNumber(number);
            set => number = SetPhoneNumber(value);
        }
        #endregion

        #region Helper Functions
        // Format the phone number for display
        private static string FormatPhoneNumber(string phoneNumber)
        {
            if (NonDigits.Match(phoneNumber).Success)
                throw new InvalidDataException($"Phone number {phoneNumber} contains non-digits!");
            if (phoneNumber.Length != 10)
                throw new InvalidDataException($"Phone number {phoneNumber} does not contain ten digits!");
            return $"({phoneNumber.Substring(0, 3)}) {phoneNumber.Substring(3, 3)}-{phoneNumber.Substring(6, 4)}";
        }

        // Assign a value to the phone number
        private static string SetPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                throw new InvalidDataException("Phone number cannot be blank!");
            
            string temp = NonDigits.Replace(phoneNumber, "");
            if (temp.Length != 10)
                throw new InvalidDataException($"Phone number {phoneNumber} does not contain ten digits!");

            return temp;
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
