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

namespace CSharp.Ch05.Supplemental.ConfigurationClasses.Models.Configuration
{
    /// <summary>
    /// Example configuration section showing nested configuration collections
    /// </summary>
    public class OnBaseSettings : ConfigurationSection
    {
        #region Properties
        /// <summary>
        /// Section name as it appears in the app.config XML
        /// </summary>
        public const string SectionName = "onBaseSettings";

        /// <summary>
        /// OnBase connection location
        /// </summary>
        [ConfigurationProperty("serviceLocation", IsRequired = true)]
        public ServiceLocation ServiceLocation
        {
            get => (ServiceLocation)base["serviceLocation"];
            set => base["serviceLocation"] = value;
        }

        /// <summary>
        /// Collection of document types, each of which contains a nested collection of keyword types
        /// </summary>
        [ConfigurationProperty("documentTypes", IsRequired = true)]
        [ConfigurationCollection(typeof(DocumentTypeElement),
            // Here, we are specifying a human-readable XML name instead of "add"
            AddItemName = "documentType",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public DocumentTypeCollection DocumentTypes
        {
            get => (DocumentTypeCollection)base["documentTypes"];
            set => this["documentTypes"] = value;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the OnBaseSettings class
        /// The default constructor is required
        /// </summary>
        public OnBaseSettings() { }

        /// <summary>
        /// Create and initialize a new instance of the OnBaseSettings class
        /// </summary>
        /// <param name="documentTypes">Document type collection</param>
        public OnBaseSettings(DocumentTypeCollection documentTypes)
        {
            DocumentTypes = documentTypes;
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
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
