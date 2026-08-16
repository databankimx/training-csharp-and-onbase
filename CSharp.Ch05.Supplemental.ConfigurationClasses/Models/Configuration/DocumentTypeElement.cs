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
    /// Defines a document type (in a collection) with a nested collection of keyword types
    /// </summary>
    public class DocumentTypeElement : ConfigurationElement
    {
        #region Properties
        /// <summary>
        /// OnBase Document Type Name
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get => (string)base["name"];
            set => base["name"] = value;
        }

        /// <summary>
        /// OnBase Document Type ID
        /// </summary>
        [ConfigurationProperty("id", IsRequired = true)]
        public long Id
        {
            get => (long)base["id"];
            set => base["id"] = value;
        }

        /// <summary>
        /// Collection of keyword types (nested within each document type in the collection)
        /// </summary>
        [ConfigurationProperty("keywordTypes", IsRequired = true)]
        [ConfigurationCollection(typeof(KeywordTypeElement),
            // Here, we are specifying a human-readable XML name instead of "add"
            AddItemName = "keywordType",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public KeywordTypeCollection KeywordTypes
        {
            get => (KeywordTypeCollection)base["keywordTypes"];
            set => base["keywordTypes"] = value;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the DocumentTypeElement class
        /// The default constructor is required
        /// </summary>
        public DocumentTypeElement() { }

        /// <summary>
        /// Create a new instance of the DocumentTypeElement class
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="keywordTypes"></param>
        public DocumentTypeElement(string name, long id, KeywordTypeCollection keywordTypes)
        {
            Name = name;
            Id = id;
            KeywordTypes = keywordTypes;
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
