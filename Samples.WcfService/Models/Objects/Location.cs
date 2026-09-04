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
using System.Runtime.Serialization;
#endregion

namespace Samples.WcfService.Models.Objects
{
    /// <summary>
    /// Models a lookup row from the zip code database
    /// </summary>
    [DataContract]
    public class Location
    {
        #region Properties
        /// <summary>
        /// State where zip code is located
        /// </summary>
        [DataMember]
        public string State { get; set; }

        /// <summary>
        /// County where zip code is located
        /// </summary>
        [DataMember]
        public string County { get; set; }

        /// <summary>
        /// City where zip code is located
        /// </summary>
        [DataMember]
        public string City { get; set; }

        /// <summary>
        /// Zip Code
        /// </summary>
        [DataMember]
        public string ZipCode { get; set; }

        /// <summary>
        /// Report all properties for logging
        /// </summary>
        /// <returns></returns>
        [DataMember]
        public string Info
        {
            get => $"State: [{State}] | County: [{County}] | City: [{City}] | Zip: [{ZipCode}]";

            // This setter is meaningless except to prevent an error reading the data contract with the DataContractSerializer
            #pragma warning disable S3237 // Retain for serialization purposes
            protected set { /* Intentionally left blank */ }
            #pragma warning restore S3237
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
