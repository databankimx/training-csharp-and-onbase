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
    /// Defines a response from the web service
    /// </summary>
    [DataContract]
    public abstract class ServiceResponseBase
    {
        #region Properties
        /// <summary>
        /// Used to match request to response
        /// </summary>
        [DataMember]
        public string RequestId { get; set; }

        /// <summary>
        /// List of errors (if any) occurring during web service processing
        /// </summary>
        [DataMember]
        public List<string> Errors { get; set; }
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
