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

namespace CSharp.Ch03.WorkingWithTheTypeSystem.Models.Objects
{
    /// <summary>
    /// Representation of an IP address, one bit per array element
    /// </summary>
    public class IpAddress
    {
        #region Private Members
        // Component parts of IP Address
        private readonly int[] ip;
        #endregion

        #region Indexers
        /// <summary>
        /// Return one bit of the IP address array at the specified index
        /// </summary>
        /// <param name="index">Bit position (0-31)</param>
        public int this[int index]
        {
            get => ip[index];
            set
            {
                if (value == 0 || value == 1) ip[index] = value;
                else throw new ArgumentException("Invalid value, must be 0 or 1", nameof(value));
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the IpAddress class
        /// </summary>
        public IpAddress()
        {
            // Added this constructor to create the array (missing in text)
            ip = new int[32];
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
