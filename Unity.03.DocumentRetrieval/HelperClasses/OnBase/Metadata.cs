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
using System.Linq;
using Hyland.Unity;
using Hyland.Unity.Extensions;
using Unity._00.CommonFunctionality.Models.Objects;
using Unity._02.AccessingTaxonomy.HelperClasses.OnBase;
using Unity._03.DocumentRetrieval.Models.Objects;
#endregion

namespace Unity._03.DocumentRetrieval.HelperClasses.OnBase
{
    /// <summary>
    /// Exposes methods to generate keywords and keyword groups
    /// </summary>
    public class Metadata
    {
        #region Properties
        /// <summary>
        /// Unity API Application Object
        /// </summary>
        public Application App { get; set; }

        /// <summary>
        /// Access functions for OnBase configuration
        /// </summary>
        public OnBaseTaxonomy Config { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the Metadata class
        /// </summary>
        /// <param name="app">Unity API Application Object</param>
        public Metadata(Application app = null)
        {
            Initialize(app);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Create an OnBase keyword from supplied metadata
        /// </summary>
        /// <param name="keyItem">Keyword metadata</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>OnBase keyword</returns>
        public Keyword MakeKeyword(KeywordInfo keyItem, Application app = null)
        {
            try
            {
                Initialize(app);

                string name = keyItem.Id < 1 ? keyItem.Name : keyItem.Id.ToString();

                var keyType = Config.GetKeywordType(name);
                if (keyType == null) return null;

                return !keyType.TryCreateKeyword(keyItem.Value, out var keyword)
                    ? null
                    : keyword;
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error creating keyword [{keyItem.Name}] = [{keyItem.Value}]!", ex);
            }
        }

        /// <summary>
        /// Create a keyword group for a document query from supplied metadata
        /// </summary>
        /// <param name="keyGroup">Keyword group metadata</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Keyword group for query filtering</returns>
        public QueryKeywordRecord MakeQueryKeywordGroup(KeywordGroup keyGroup, Application app = null)
        {
            try
            {
                Initialize(app);

                string name = keyGroup.Id < 1 ? keyGroup.Name : keyGroup.Id.ToString();

                var keyGroupType = Config.GetKeywordGroupType(name);
                if (keyGroupType == null) return null;

                var record = keyGroupType.CreateQueryKeywordRecord();
                foreach (var keyword in keyGroup.Keywords.Select(keyItem => MakeKeyword(keyItem)).Where(keyword => keyword != null)) record.AddKeyword(keyword);

                return record;
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error creating keyword group [{keyGroup.Name}]!", ex);
            }
        }

        /// <summary>
        /// Create a keyword group for a document from supplied metadata
        /// </summary>
        /// <param name="keyGroup">Keyword group metadata</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Keyword group</returns>
        public EditableKeywordRecord MakeKeywordGroup(KeywordGroup keyGroup, Application app = null)
        {
            try
            {
                Initialize(app);

                string name = keyGroup.Id < 1 ? keyGroup.Name : keyGroup.Id.ToString();

                var keyGroupType = Config.GetKeywordGroupType(name);
                if (keyGroupType == null) return null;

                var record = keyGroupType.CreateEditableKeywordRecord();
                foreach (var keyword in keyGroup.Keywords.Select(keyItem => MakeKeyword(keyItem)).Where(keyword => keyword != null)) record.AddKeyword(keyword);

                return record;
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error creating keyword group [{keyGroup.Name}]!", ex);
            }
        }
        #endregion

        #region Private Methods
        // Initialize the Unity API
        private void Initialize(Application app)
        {
            try
            {
                if (app != null) App = app;

                if (App == null) throw new DatabankException("Application cannot be null!");

                Config = new OnBaseTaxonomy(App);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error initializing Application object!", ex);
            }
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
