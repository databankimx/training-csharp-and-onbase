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

namespace Unity._00.CommonFunctionality.Models.Configuration
{
    /// <summary>
    /// Defines OnBase-related settings in XML configuration file
    /// </summary>
    public class OnBaseSettings : ConfigurationSection
    {
        #region Properties
        /// <summary>
        /// XML configuration file section name
        /// </summary>
        public const string SectionName = "onBaseSettings";

        /// <summary>
        /// OnBase Connection Settings
        /// </summary>
        [ConfigurationProperty(ServiceLocation.ElementName, IsRequired = true)]
        public ServiceLocation ServiceLocation
        {
            get => (ServiceLocation)base[ServiceLocation.ElementName];
            set => base[ServiceLocation.ElementName] = value;
        }
        
        /// <summary>
        /// OnBase DocPop Settings
        /// </summary>
        [ConfigurationProperty(DocPopSettings.ElementName, IsRequired = true)]
        public DocPopSettings DocPop
        {
            get => (DocPopSettings)base[DocPopSettings.ElementName];
            set => base[DocPopSettings.ElementName] = value;
        }

        /// <summary>
        /// Hyland Identity Provider (IdP) Settings. Optional: only needed when
        /// <see cref="ServiceLocation"/>'s AuthenticationMode is AccessToken and a token
        /// is being obtained from the IdP rather than supplied directly. See
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
