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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hyland.Unity;
using Hyland.Unity.UnityForm;
using Unity._00.CommonFunctionality.HelperClasses.Extensions;
using Unity._00.CommonFunctionality.Models.Objects;
#endregion

#pragma warning disable S125 // Allow commented code in lesson files
namespace Unity._02.AccessingTaxonomy.HelperClasses.OnBase
{
    /// <summary>
    /// Exposes methods to access OnBase taxonomy elements
    /// </summary>
    public class OnBaseTaxonomy
    {
        #region Properties
        /// <summary>
        /// Unity API Application Object
        /// </summary>
        public Application App { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the OnBaseTaxonomy class
        /// </summary>
        /// <param name="app">Unity API Application Object</param>
        public OnBaseTaxonomy(Application app = null)
        {
            if (app != null) App = app;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Obtain document type groups
        /// </summary>
        /// <param name="names">List of document type group names</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>List of document type groups</returns>
        public List<DocumentTypeGroup> GetDocumentTypeGroups(string[] names = null, Application app = null)
        {
            try
            {
                Initialize(app);

                // If no list is specified, return all document type groups
                if (names == null || names.Length == 0) return [.. App.Core.DocumentTypeGroups];

                // Obtain all document type groups that match the provided list of names
                return [.. names.Select(name => GetDocumentTypeGroup(name)).Where(docTypeGroup => docTypeGroup != null)];

                // The above LINQ expression is equivalent to the following:
                //
                // var docTypeGroups = new List<DocumentTypeGroup>();
                // foreach (string name in names)
                // {
                //     var docTypeGroup = GetDocumentTypeGroup(name);
                //     if (docTypeGroup != null) docTypeGroups.Add(docTypeGroup);
                // }
                // return docTypeGroups;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error getting document type groups!", ex);
            }
        }

        /// <summary>
        /// Obtain document type group
        /// </summary>
        /// <param name="name">Document type group name</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Document type group</returns>
        public DocumentTypeGroup GetDocumentTypeGroup(string name, Application app = null)
        {
            try
            {
                Initialize(app);

                // We want to retrieve by either ID or name, so we check to see if the name is numeric
                //    and treat it as an ID if it is
                return long.TryParse(name, out long id)
                    ? App.Core.DocumentTypeGroups.Find(id)
                    : App.Core.DocumentTypeGroups.Find(name);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting document type group [{name}]!", ex);
            }
        }

        /// <summary>
        /// Obtain document types
        /// </summary>
        /// <param name="names">List of document type names</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>List of document types</returns>
        public List<DocumentType> GetDocumentTypes(string[] names, Application app = null)
        {
            try
            {
                Initialize(app);

                // If no list is specified, return all document type groups
                if (names == null || names.Length == 0) return [.. App.Core.DocumentTypes];

                // Obtain all document types that match the provided list of names
                return [.. names.Select(name => GetDocumentType(name)).Where(docType => docType != null)];

                // The above LINQ expression is equivalent to the following:
                //
                // var docTypes = new List<DocumentType>();
                // foreach (string name in names)
                // {
                //     var docType = GetDocumentType(name);
                //     if (docType != null) docTypes.Add(docType);
                // }
                // return docTypes;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error getting document types!", ex);
            }
        }

        /// <summary>
        /// Obtain document types for a specified document type group
        /// </summary>
        /// <param name="groupName">Document type group name</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>List of document types in group</returns>
        public List<DocumentType> GetDocumentTypes(string groupName, Application app = null)
        {
            try
            {
                Initialize(app);

                // Get the named document type group
                var docTypeGroup = GetDocumentTypeGroup(groupName);

                // Return all document types in the group (or null if the group was not found)
                return docTypeGroup?.DocumentTypes.ToList();
            }
            catch (Exception ex)
            {
                // *Fixed*: this catch block originally just logged to the console and
                // re-threw the raw exception, inconsistent with every other method in
                // this class (and every other "good" project in this training set), which
                // wraps failures in DatabankException. See LectureNotes.md.
                throw new DatabankException($"Error getting document types for group [{groupName}]!", ex);
            }
        }

        /// <summary>
        /// Obtain document type
        /// </summary>
        /// <param name="name">Document type name</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Document type</returns>
        public DocumentType GetDocumentType(string name, Application app = null)
        {
            try
            {
                Initialize(app);

                // We want to retrieve by either ID or name, so we check to see if the name is numeric
                //    and treat it as an ID if it is
                return long.TryParse(name, out long id)
                    ? App.Core.DocumentTypes.Find(id)
                    : App.Core.DocumentTypes.Find(name);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting document type [{name}]!", ex);
            }
        }
        
        /// <summary>
        /// Obtain keyword group (record) types
        /// </summary>
        /// <param name="names">List of keyword group names</param>
        /// <param name="doc">OnBase document</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>List of keyword group types</returns>
        public List<KeywordRecordType> GetKeywordGroupTypes(string[] names, Document doc, Application app = null)
        {
            try
            {
                Initialize(app);

                return GetKeywordGroupTypes(names, doc.DocumentType);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error getting keyword group types!", ex);
            }
        }

        /// <summary>
        /// Obtain keyword group (record) types
        /// </summary>
        /// <param name="names">List of keyword group names</param>
        /// <param name="docType">OnBase document type</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>List of keyword group types</returns>
        public List<KeywordRecordType> GetKeywordGroupTypes(string[] names = null, DocumentType docType = null, Application app = null)
        {
            try
            {
                Initialize(app);

                // If we have a list of KGs, cycle through the names and return the ones that are found
                if (names != null && names.Length > 0)
                {
                    return [.. names.Select(name => GetKeywordGroupType(name, docType)).Where(recordType => recordType != null)];
                }

                return docType == null
                    // If we don't have a document type, just return all the KGs in the system
                    ? [.. App.Core.KeywordRecordTypes]
                    // Otherwise, return the ones associated with this specific document type
                    : [.. docType.KeywordRecordTypes];
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error getting keyword group types!", ex);
            }
        }

        /// <summary>
        /// Obtain keyword group (record) type
        /// </summary>
        /// <param name="name">Keyword group name</param>
        /// <param name="doc">OnBase document</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Keyword group type</returns>
        public KeywordRecordType GetKeywordGroupType(string name, Document doc, Application app = null)
        {
            try
            {
                Initialize(app);

                return GetKeywordGroupType(name, doc.DocumentType);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keyword group type [{name}]!", ex);
            }
        }

        /// <summary>
        /// Obtain keyword group (record) type
        /// </summary>
        /// <param name="name">Keyword group name</param>
        /// <param name="docType">OnBase document type</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Keyword group type</returns>
        public KeywordRecordType GetKeywordGroupType(string name, DocumentType docType = null, Application app = null)
        {
            try
            {
                Initialize(app);

                long id;

                // If we have a doc type, return a KG only if it exists on the document type
                if (docType != null)
                {
                    // We want to retrieve by either ID or name, so we check to see if the name is numeric
                    //    and treat it as an ID if it is
                    return long.TryParse(name, out id)
                        ? docType.KeywordRecordTypes.Find(id)
                        : docType.KeywordRecordTypes.Find(name);
                }

                // Otherwise return the KG if it exists in OnBase
                return long.TryParse(name, out id)
                    // We want to retrieve by either ID or name, so we check to see if the name is numeric
                    //    and treat it as an ID if it is
                    ? App.Core.KeywordRecordTypes.Find(id)
                    : App.Core.KeywordRecordTypes.Find(name);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keyword group type [{name}]!", ex);
            }
        }

        /// <summary>
        /// Obtain keyword types
        /// </summary>
        /// <param name="names">List of keyword type names</param>
        /// <param name="doc">OnBase document</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>List of keyword types</returns>
        public List<KeywordType> GetKeywordTypes(string[] names, Document doc, Application app = null)
        {
            try
            {
                Initialize(app);

                return GetKeywordTypes(names, doc.DocumentType);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error getting keyword types!", ex);
            }
        }

        /// <summary>
        /// Obtain keyword types
        /// </summary>
        /// <param name="names">List of keyword type names</param>
        /// <param name="docType">OnBase document type</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>List of keyword types</returns>
        public List<KeywordType> GetKeywordTypes(string[] names = null, DocumentType docType = null, Application app = null)
        {
            try
            {
                Initialize(app);

                // If we have a list of names, just retrieve those keyword types
                if (names != null && names.Length > 0)
                {
                    return [.. names.Select(name => GetKeywordType(name, docType)).Where(keyType => keyType != null)];
                }

                // If we don't have a document type, get all keyword types
                if (docType == null) return [.. App.Core.KeywordTypes];

                // Get all the keyword types associated with the specified document type
                var keyTypes = new List<KeywordType>();
                foreach (var recordType in docType.KeywordRecordTypes) keyTypes.AddRange(recordType.KeywordTypes);
                return keyTypes;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error getting keyword types!", ex);
            }
        }

        /// <summary>
        /// Obtain keyword type
        /// </summary>
        /// <param name="name">Keyword type name</param>
        /// <param name="doc">OnBase document</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Keyword type</returns>
        public KeywordType GetKeywordType(string name, Document doc, Application app = null)
        {
            try
            {
                Initialize(app);

                return GetKeywordType(name, doc.DocumentType);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keyword type [{name}] from document [{doc.ID}]!", ex);
            }
        }

        /// <summary>
        /// Obtain keyword type
        /// </summary>
        /// <param name="name">Keyword type name</param>
        /// <param name="docType">OnBase document type</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Keyword type</returns>
        public KeywordType GetKeywordType(string name, DocumentType docType = null, Application app = null)
        {
            try
            {
                Initialize(app);

                long id;

                // If we have a document type, only return the keyword type if it is assigned there
                if (docType != null)
                {
                    // We want to retrieve by either ID or name, so we check to see if the name is numeric
                    //    and treat it as an ID if it is
                    return long.TryParse(name, out id)
                        ? docType.KeywordRecordTypes.FindKeywordType(id)
                        : docType.KeywordRecordTypes.FindKeywordType(name);
                }

                // Otherwise, return the keyword type if it exists in OnBase
                return long.TryParse(name, out id)
                    // We want to retrieve by either ID or name, so we check to see if the name is numeric
                    //    and treat it as an ID if it is
                    ? App.Core.KeywordTypes.Find(id)
                    : App.Core.KeywordTypes.Find(name);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keyword type [{name}]!", ex);
            }
        }

        /// <summary>
        /// Obtain custom queries
        /// </summary>
        /// <param name="names">List of custom query names</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>List of custom queries</returns>
        public List<CustomQuery> GetCustomQueries(string[] names = null, Application app = null)
        {
            try
            {
                Initialize(app);

                // If we don't have a list of names, return all custom queries
                if (names == null || names.Length == 0) return [.. App.Core.CustomQueries];

                // Otherwise, only return custom queries in the list
                return [.. names.Select(name => GetCustomQuery(name)).Where(customQuery => customQuery != null)];
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error getting custom queries!", ex);
            }
        }

        /// <summary>
        /// Obtain custom query
        /// </summary>
        /// <param name="name">Custom query name</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Custom query</returns>
        public CustomQuery GetCustomQuery(string name, Application app = null)
        {
            try
            {
                Initialize(app);

                // We want to retrieve by either ID or name, so we check to see if the name is numeric
                //    and treat it as an ID if it is
                return long.TryParse(name, out long id)
                    ? App.Core.CustomQueries.Find(id)
                    : App.Core.CustomQueries.Find(name);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting custom query [{name}]!", ex);
            }
        }

        /// <summary>
        /// Obtain file format based on file extension
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <param name="app">Unity API application object</param>
        /// <returns>OnBase file format</returns>
        public FileType GetFileType(string extension, Application app = null)
        {
            try
            {
                Initialize(app);

                return GetFileType(extension.ToFileTypeId());
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting file format for extension [{extension}]!", ex);
            }
        }

        /// <summary>
        /// Obtain file format based on file extension
        /// </summary>
        /// <param name="id">File format ID</param>
        /// <param name="app">Unity API application object</param>
        /// <returns>OnBase file format</returns>
        public FileType GetFileType(long id, Application app = null)
        {
            try
            {
                Initialize(app);

                return App.Core.FileTypes.Find(id);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting file format for ID [{id}]!", ex);
            }
        }

        /// <summary>
        /// Obtain file format based on file list
        /// </summary>
        /// <param name="files">File list</param>
        /// <param name="app">Unity API application object</param>
        /// <returns>OnBase file format</returns>
        public FileType GetFileType(List<string> files, Application app = null)
        {
            try
            {
                Initialize(app);

                string extension = Path.GetExtension(files[0]).Substring(1).ToUpper();
                for (int i = 1; i < files.Count; i++)
                {
                    string ext = Path.GetExtension(files[i]).Substring(1).ToUpper();
                    if (!string.Equals(extension, ext, StringComparison.CurrentCultureIgnoreCase))
                        // *Fixed*: was ApplicationException, this training set's own
                        // exception standard is DatabankException throughout.
                        throw new DatabankException("All files for document must be of the same type!");
                }

                return GetFileType(extension);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error getting file format for file list!", ex);
            }
        }

        /// <summary>
        /// Obtain unity form template
        /// </summary>
        /// <param name="name">Unity form name or ID</param>
        /// <param name="app">Unity API application object</param>
        /// <returns>Unity form template</returns>
        public FormTemplate GetUnityForm(string name, Application app = null)
        {
            try
            {
                Initialize(app);

                // We want to retrieve by either ID or name, so we check to see if the name is numeric
                //    and treat it as an ID if it is
                return long.TryParse(name, out long id)
                    ? App.Core.UnityFormTemplates.Find(id)
                    : App.Core.UnityFormTemplates.Find(name);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting unity form template [{name}]!", ex);
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
