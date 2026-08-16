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
    /// Defines a keyword type for configuration
    /// </summary>
    public class KeywordTypeElement : ConfigurationElement
    {
        #region Properties
        /// <summary>
        /// OnBase keyword type Name
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get => (string)base["name"];
            set => this["name"] = value;
        }

        /// <summary>
        /// OnBase keyword type ID
        /// </summary>
        [ConfigurationProperty("id", IsRequired = true)]
        public long Id
        {
            get => (long)base["id"];
            set => this["id"] = value;
        }

        /// <summary>
        /// Data type (alphanumeric, floating-point, date, etc.)
        /// </summary>
        [ConfigurationProperty("dataType", IsRequired = true)]
        public string DataType
        {
            get => (string)base["dataType"];
            set => this["dataType"] = value;
        }

        /// <summary>
        /// Maximum length permitted (if data type is alphanumeric)
        /// </summary>
        [ConfigurationProperty("dataLength", IsRequired = true)]
        public int DataLength
        {
            get => (int)base["dataLength"];
            set => this["dataLength"] = value;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the KeywordTypeElement
        /// The default constructor is required in order to support serialization
        /// </summary>
        public KeywordTypeElement() { }

        /// <summary>
        /// Create a new instance of the KeywordTypeElement
        /// </summary>
        /// <param name="name">OnBase keyword type Name</param>
        /// <param name="id">OnBase keyword type ID</param>
        /// <param name="dataType">Data type (alphanumeric, floating-point, date, etc.)</param>
        /// <param name="dataLength">Maximum length permitted (if data type is alphanumeric)</param>
        public KeywordTypeElement(string name, long id, string dataType, int dataLength)
        {
            Name = name;
            Id = id;
            DataType = dataType;
            DataLength = dataLength;
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
