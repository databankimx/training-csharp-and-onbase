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
using System.Linq;
using Hyland.Unity;
using Unity._00.CommonFunctionality.Models.Objects;
#endregion

#pragma warning disable S1168 // Null returns are intentional
namespace Unity._03.DocumentRetrieval.HelperClasses.OnBase
{
    #region Training Notes
    /*
     * *Migration Note: this file originally had no Copyright header, no Using Directives
     * region, and no Source Code Information footer at all, the only file in this whole
     * training set missing them. Added here for consistency with every other file, the
     * C# itself is otherwise a faithful port.
     */
    #endregion

    /// <summary>
    /// Extension methods for looking up keywords, keyword groups, and their types on a
    /// <see cref="Document"/>, <see cref="DocumentType"/>, or <see cref="KeywordRecord"/>.
    /// </summary>
    public static class DocumentExtensions
    {
        #region Public Methods
        /// <summary>
        /// Obtain all instances of a named keyword group (record) on a document.
        /// </summary>
        /// <param name="doc">The document to search.</param>
        /// <param name="name">The keyword group name or ID.</param>
        /// <returns>The matching keyword group instances.</returns>
        public static List<KeywordRecord> GetKeywordGroups(this Document doc, string name)
        {
            try
            {
                if (doc == null) return null;
                var groupType = doc.GetKeywordGroupType(name);
                return groupType == null ? null : [.. doc.KeywordRecords.FindAll(groupType)];
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keyword groups for [{name}] on document [{doc?.ID}]!", ex);
            }
        }

        /// <summary>
        /// Obtain a named keyword group (record) on a document.
        /// </summary>
        /// <param name="doc">The document to search.</param>
        /// <param name="name">The keyword group name or ID.</param>
        /// <returns>The matching keyword group instance.</returns>
        public static KeywordRecord GetKeywordGroup(this Document doc, string name)
        {
            try
            {
                if (doc == null) return null;
                var groupType = doc.GetKeywordGroupType(name);
                return groupType == null ? null : doc.KeywordRecords.Find(groupType);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keyword group [{name}] on document [{doc?.ID}]!", ex);
            }
        }

        /// <summary>
        /// Obtain a named keyword group (record) type on a document's document type.
        /// </summary>
        /// <param name="doc">The document whose document type to search.</param>
        /// <param name="name">The keyword group type name or ID.</param>
        /// <returns>The matching keyword group type.</returns>
        public static KeywordRecordType GetKeywordGroupType(this Document doc, string name)
        {
            try
            {
                return doc?.DocumentType.GetKeywordGroupType(name);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keyword record type [{name}] on document [{doc?.ID}]!", ex);
            }
        }

        /// <summary>
        /// Obtain a named keyword group (record) type on a document type.
        /// </summary>
        /// <param name="docType">The document type to search.</param>
        /// <param name="name">The keyword group type name or ID.</param>
        /// <returns>The matching keyword group type.</returns>
        public static KeywordRecordType GetKeywordGroupType(this DocumentType docType, string name)
        {
            try
            {
                if (docType == null) return null;
                return long.TryParse(name, out long id)
                    ? docType.KeywordRecordTypes.Find(id)
                    : docType.KeywordRecordTypes.Find(name);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keyword record type [{name}] on document type [{docType?.Name}]!", ex);
            }
        }

        /// <summary>
        /// Obtain all instances of a named keyword on a document.
        /// </summary>
        /// <param name="doc">The document to search.</param>
        /// <param name="name">The keyword type name or ID.</param>
        /// <returns>The matching keyword instances.</returns>
        public static List<Keyword> GetKeywords(this Document doc, string name)
        {
            try
            {
                var keyType = doc?.GetKeywordType(name);
                if (keyType == null) return null;
                var record = doc.KeywordRecords.Find(keyType);
                return record?.GetKeywords(keyType);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keywords for [{name}] on document [{doc?.ID}]!", ex);
            }
        }

        /// <summary>
        /// Obtain all instances of a named keyword on a keyword record.
        /// </summary>
        /// <param name="record">The keyword record to search.</param>
        /// <param name="name">The keyword type name or ID.</param>
        /// <returns>The matching keyword instances.</returns>
        public static List<Keyword> GetKeywords(this KeywordRecord record, string name)
        {
            try
            {
                var keyType = record?.GetKeywordType(name);
                return keyType == null ? null : record.GetKeywords(keyType);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keywords for [{name}] in record [{record?.KeywordRecordType.Name}]!", ex);
            }
        }

        /// <summary>
        /// Obtain all instances of a keyword type on a keyword record.
        /// </summary>
        /// <param name="record">The keyword record to search.</param>
        /// <param name="keyType">The keyword type.</param>
        /// <returns>The matching keyword instances.</returns>
        public static List<Keyword> GetKeywords(this KeywordRecord record, KeywordType keyType)
        {
            try
            {
                return record?.Keywords.FindAll(keyType).ToList();
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keywords for [{keyType.Name}] in record [{record?.KeywordRecordType.Name}]!", ex);
            }
        }

        /// <summary>
        /// Obtain a named keyword on a document.
        /// </summary>
        /// <param name="doc">The document to search.</param>
        /// <param name="name">The keyword type name or ID.</param>
        /// <returns>The matching keyword.</returns>
        public static Keyword GetKeyword(this Document doc, string name)
        {
            try
            {
                var keyType = doc?.GetKeywordType(name);
                if (keyType == null) return null;
                var record = doc.KeywordRecords.Find(keyType);
                return record?.GetKeyword(keyType);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keyword [{name}] on document [{doc?.ID}]!", ex);
            }
        }

        /// <summary>
        /// Obtain a named keyword on a keyword record.
        /// </summary>
        /// <param name="record">The keyword record to search.</param>
        /// <param name="name">The keyword type name or ID.</param>
        /// <returns>The matching keyword.</returns>
        public static Keyword GetKeyword(this KeywordRecord record, string name)
        {
            try
            {
                var keyType = record?.GetKeywordType(name);
                return keyType == null ? null : record.Keywords.Find(keyType);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keyword [{name}] in record [{record?.KeywordRecordType.Name}]!", ex);
            }
        }

        /// <summary>
        /// Obtain a keyword of a given type on a keyword record.
        /// </summary>
        /// <param name="record">The keyword record to search.</param>
        /// <param name="keyType">The keyword type.</param>
        /// <returns>The matching keyword.</returns>
        public static Keyword GetKeyword(this KeywordRecord record, KeywordType keyType)
        {
            try
            {
                return record?.Keywords.Find(keyType);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keyword [{keyType.Name}] in record [{record?.KeywordRecordType.Name}]!", ex);
            }
        }

        /// <summary>
        /// Obtain a named keyword type on a document's document type.
        /// </summary>
        /// <param name="doc">The document whose document type to search.</param>
        /// <param name="name">The keyword type name or ID.</param>
        /// <returns>The matching keyword type.</returns>
        public static KeywordType GetKeywordType(this Document doc, string name)
        {
            try
            {
                return doc?.DocumentType.GetKeywordType(name);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keyword type [{name}] on document [{doc?.ID}]!", ex);
            }
        }

        /// <summary>
        /// Obtain a named keyword type on a document type.
        /// </summary>
        /// <param name="docType">The document type to search.</param>
        /// <param name="name">The keyword type name or ID.</param>
        /// <returns>The matching keyword type.</returns>
        public static KeywordType GetKeywordType(this DocumentType docType, string name)
        {
            try
            {
                if (docType == null) return null;
                return long.TryParse(name, out long id)
                    ? docType.KeywordRecordTypes.FindKeywordType(id)
                    : docType.KeywordRecordTypes.FindKeywordType(name);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keyword type [{name}] on document type [{docType?.Name}]!", ex);
            }
        }

        /// <summary>
        /// Obtain a named keyword type on a keyword record.
        /// </summary>
        /// <param name="record">The keyword record whose keyword group type to search.</param>
        /// <param name="name">The keyword type name or ID.</param>
        /// <returns>The matching keyword type.</returns>
        public static KeywordType GetKeywordType(this KeywordRecord record, string name)
        {
            try
            {
                return record?.KeywordRecordType.GetKeywordType(name);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keyword type [{name}] on record type [{record?.KeywordRecordType.Name}]!", ex);
            }
        }

        /// <summary>
        /// Obtain a named keyword type on a keyword group (record) type.
        /// </summary>
        /// <param name="recordType">The keyword group type to search.</param>
        /// <param name="name">The keyword type name or ID.</param>
        /// <returns>The matching keyword type.</returns>
        public static KeywordType GetKeywordType(this KeywordRecordType recordType, string name)
        {
            try
            {
                if (recordType == null) return null;
                return long.TryParse(name, out long id)
                    ? recordType.KeywordTypes.Find(id)
                    : recordType.KeywordTypes.Find(name);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keyword type [{name}] on record type [{recordType?.Name}]!", ex);
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
