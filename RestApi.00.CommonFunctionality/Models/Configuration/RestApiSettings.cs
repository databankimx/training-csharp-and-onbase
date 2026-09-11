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
using System.Configuration;
#endregion

namespace RestApi._00.CommonFunctionality.Models.Configuration
{
    #region Training Notes
    /*
     * *Migration Note: the REST API equivalent of Unity.00.CommonFunctionality's own
     * OnBaseSettings, holding this whole config section together. No DocPop element
     * here, deliberately: nothing in the Document Management API documentation reviewed
     * for this training set (Getting Started, Authentication, Document Retrieval,
     * Creating and Executing Document Queries, Upload and Archive Interactions, Working
     * with Keywords) mentions a DocPop or UnityPop equivalent. This is a genuine feature
     * gap worth flagging, same as ServiceLocation's own SessionId omission,
     * RestApi.TestHarness's Retrieval page will need to omit those two link buttons
     * rather than fabricate a URL scheme that isn't documented anywhere.
     */
    #endregion

    /// <summary>
    /// Defines REST API-related settings in XML configuration file
    /// </summary>
    public class RestApiSettings : ConfigurationSection
    {
        #region Properties
        /// <summary>
        /// XML configuration file section name
        /// </summary>
        public const string SectionName = "restApiSettings";

        /// <summary>
        /// OnBase API Server connection settings
        /// </summary>
        [ConfigurationProperty(ServiceLocation.ElementName, IsRequired = true)]
        public ServiceLocation ServiceLocation
        {
            get => (ServiceLocation)base[ServiceLocation.ElementName];
            set => base[ServiceLocation.ElementName] = value;
        }

        /// <summary>
        /// Hyland Identity Provider (IdP) Settings, required for the currently-implemented
        /// AuthenticationMode.OnBaseCredentials ("password" grant). See
        /// <see cref="IdpSettings"/>.
        /// </summary>
        [ConfigurationProperty(Configuration.IdpSettings.ElementName, IsRequired = false)]
        public IdpSettings IdpSettings
        {
            get => (IdpSettings)base[Configuration.IdpSettings.ElementName];
            set => base[Configuration.IdpSettings.ElementName] = value;
        }
        #endregion

        #region Parent Class Overrides
        /// <summary>
        /// In order to allow the element to be modified at runtime, we need IsReadOnly to return false
        /// </summary>
        /// <returns>Always false</returns>
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
