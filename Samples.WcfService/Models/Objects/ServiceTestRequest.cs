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
using System.Runtime.Serialization;
#endregion

namespace Samples.WcfService.Models.Objects
{
    /// <summary>
    /// Defines a request with a data string for testing the web service
    /// </summary>
    [DataContract]
    public class ServiceTestRequest : ServiceRequestBase
    {
        #region Properties
        /// <summary>
        /// Test data
        /// </summary>
        [DataMember]
        public string Data { get; set; }
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
