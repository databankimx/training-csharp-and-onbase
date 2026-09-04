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
using System.Runtime.Serialization;

#endregion

namespace Samples.WcfService.Models.Objects
{
    /// <summary>
    /// Defines a response from the web service lookup method
    /// </summary>
    [DataContract]
    public class LocationLookupResponse : ServiceResponseBase
    {
        #region Properties
        /// <summary>
        /// Web service lookup result data
        /// </summary>
        [DataMember]
        public List<Location> Data { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the LocationLookupResponse class
        /// </summary>
        public LocationLookupResponse()
        {
            Data = new List<Location>();
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
