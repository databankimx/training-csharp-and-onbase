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
using System.Runtime.Serialization;
using System.ServiceModel.Configuration;
using CSharp.SharedLibrary.Models;

#endregion

namespace Samples.WcfService.Models.Objects
{
    /// <summary>
    /// Defines a request to the web service lookup method
    /// </summary>
    [DataContract]
    public class LocationLookupRequest : ServiceRequestBase
    {
        #region Encapsulated globals
        // Zip code for lookup
        private string zipCode;
        #endregion

        #region Properties
        /// <summary>
        /// Zip code for lookup
        /// </summary>
        [DataMember]
        public string ZipCode
        {
            get => zipCode;
            set
            {
                // Remove dash if any
                value = value.Replace("-", "");
                // Validate string is either 5 or 9 digits
                if (value.Length < 5 || value.Length > 9 || !int.TryParse(value, out _))
                    throw new DatabankException($"Value [{value}] is not a valid Zip code!");
                // Store 5-digit zip code only
                zipCode = value.Substring(0, 5);
            }
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
