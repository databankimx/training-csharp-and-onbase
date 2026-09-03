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
using Hyland.Applications.Web.Security;
using Hyland.Unity;
using Hyland.Unity.Extensions;
using Unity._00.CommonFunctionality.Models.Configuration;
using Unity._00.CommonFunctionality.Models.Objects;
using Unity._02.AccessingTaxonomy.HelperClasses.OnBase;
using Unity._03.DocumentRetrieval.Models.Objects;
using SysConfig = System.Configuration;
#endregion

#pragma warning disable S1168 // Null returns are intentional
namespace Unity._03.DocumentRetrieval.HelperClasses.OnBase
{
    /// <summary>
    /// Exposes OnBase document retrieval functions
    /// </summary>
    public class DocumentRetrieval
    {
        #region Properties
        /// <summary>
        /// Unity API Application Object
        /// </summary>
        public Application App { get; set; }

        /// <summary>
        /// Keyword and keyword group generator functions
        /// </summary>
        public Metadata Metadata { get; set; }
        #endregion

        #region Private Members
        // Settings for OnBase DocPop URls
        private readonly DocPopSettings docPop;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the DocumentRetrieval class
        /// </summary>
        /// <param name="app">Unity API Application Object</param>
        /// <param name="metadata">Keyword generator class</param>
        public DocumentRetrieval(Application app, Metadata metadata)
        {
            App = app;
            Metadata = metadata;
            var onbase = (OnBaseSettings)SysConfig.ConfigurationManager.GetSection(OnBaseSettings.SectionName);
            docPop = onbase.DocPop;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Retrieve hit-list of document metadata (does not include file contents)
        /// </summary>
        /// <param name="request">Request object with filter information for document query</param>
        /// <param name="app">Unity API Application Object</param>
        /// <param name="maxDocuments">Maximum documents to retrieve</param>
        /// <returns>Document hit list</returns>
        public List<DocumentInfo> GetDocumentInfo(RetrievalRequest request, Application app = null, long maxDocuments = long.MaxValue)
        {
            try
            {
                Initialize(app);

                var docs = new List<DocumentInfo>();

                var query = MakeDocumentQuery(request);

                var results = query.ExecuteQueryResults(maxDocuments);

                foreach (var resultItem in results.QueryResultItems)
                {
                    var doc = new DocumentInfo();

                    foreach (var column in resultItem.DisplayColumns)
                    {
                        if (column.IsBlank) continue;
                        string tempVal = column.Value.ToString();
                        switch (column.Configuration.Type)
                        {
                            case DisplayColumnType.DocumentID:
                                if (!long.TryParse(tempVal, out long handle))
                                    throw new DatabankException($"Can't parse value [{tempVal}] as doc handle!");
                                doc.Handle = handle;
                                break;
                            case DisplayColumnType.DocumentName:
                                doc.Name = column.Value.ToString();
                                break;
                            case DisplayColumnType.DocumentTypeName:
                                doc.Type = column.Value.ToString();
                                break;
                            case DisplayColumnType.DocumentDate:
                                doc.DocumentDate = column.DateTimeValue;
                                break;
                            case DisplayColumnType.ArchivalDate:
                                doc.DateStored = column.DateTimeValue;
                                break;
                            case DisplayColumnType.Keyword:
                                doc.Keywords.Add(new KeywordInfo
                                {
                                    Id = column.Configuration.KeywordType.ID,
                                    Name = column.Configuration.KeywordType.Name,
                                    Value = column.Value.ToString()
                                });
                                break;
                            default:
                                throw new DatabankException($"Column type [{column.Configuration.Type}] not supported!");
                        }
                    }

                    docs.Add(doc);
                }

                return docs.Count == 0 ? null : docs;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error retrieving document hit list!", ex);
            }
        }

        /// <summary>
        /// Retrieve hit-list of document POP links
        /// </summary>
        /// <param name="request">Request object with filter information for document query</param>
        /// <param name="app">Unity API Application Object</param>
        /// <param name="maxDocuments">Maximum documents to retrieve</param>
        /// <returns>Document link hit list</returns>
        public List<DocumentLink> GetDocumentLinks(RetrievalRequest request, Application app = null, long maxDocuments = long.MaxValue)
        {
            try
            {
                Initialize(app);

                var query = MakeDocumentQuery(request, false);

                var results = query.Execute(maxDocuments);

                var docs = results.Select(document => new DocumentLink {DocPop = CreateDocPopLink(document), UnityPop = document.Upop().ShowDocumentGenerator().CreateUpopLink()}).ToList();

                return docs.Count == 0 ? null : docs;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error retrieving document link list!", ex);
            }
        }

        /// <summary>
        /// Retrieve document data (including file contents) for specified document ID
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Document data</returns>
        public DocumentData GetDocument(long id, Application app = null)
        {
            try
            {
                Initialize(app);

                var doc = App.Core.GetDocumentByID(id);
                if (doc == null) return null;

                return new DocumentData
                {
                    Metadata = GetDocumentInfo(doc),
                    Links = GetDocumentLink(doc),
                    File = GetDocumentFile(doc)
                };
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error retrieving document!", ex);
            }
        }

        /// <summary>
        /// Retrieve document file contents for specified document ID
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Document file</returns>
        public DocumentFile GetDocumentFile(long id, Application app = null)
        {
            try
            {
                Initialize(app);
                var doc = App.Core.GetDocumentByID(id);
                return doc == null ? null : GetDocumentFile(doc);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error retrieving document content!", ex);
            }
        }

        /// <summary>
        /// Retrieve document file contents for specified document
        /// </summary>
        /// <param name="doc">Document</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Document file</returns>
        public DocumentFile GetDocumentFile(Document doc, Application app = null)
        {
            try
            {
                Initialize(app);

                var rendition = doc.DefaultRenditionOfLatestRevision;
                var retrieval = App.Core.Retrieval;
                PageData data = rendition.FileType.ID switch
                {
                    17 or 24 or 27 or 43 => retrieval.Default.GetDocument(rendition),
                    16 or 59 => retrieval.PDF.GetDocument(rendition),
                    2 => retrieval.Image.GetDocument(rendition),
                    1 => retrieval.Text.GetDocument(rendition),
                    _ => retrieval.Native.GetDocument(rendition),
                };
                var stream = new MemoryStream();
                data.Stream.CopyTo(stream);
                return new DocumentFile
                {
                    Content = stream.ToArray()
                };
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error retrieving document content!", ex);
            }
        }

        /// <summary>
        /// Retrieve document POP links for specified document ID
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Document file</returns>
        public DocumentLink GetDocumentLink(long id, Application app = null)
        {
            try
            {
                Initialize(app);
                var doc = App.Core.GetDocumentByID(id);
                return doc == null ? null : GetDocumentLink(doc);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error retrieving document link!", ex);
            }
        }

        /// <summary>
        /// Retrieve document POP links for specified document
        /// </summary>
        /// <param name="doc">Document</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Document file</returns>
        public DocumentLink GetDocumentLink(Document doc, Application app = null)
        {
            try
            {
                Initialize(app);

                return new DocumentLink
                {
                    UnityPop = doc.Upop().ShowDocumentGenerator().CreateUpopLink(),
                    DocPop = CreateDocPopLink(doc)
                };
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error retrieving document link!", ex);
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

                Metadata = new Metadata(App);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error initializing Application object!", ex);
            }
        }

        // Obtain display column data for keywords on document type
        private static void GetKeywordColumns(DocumentType docType, DocumentQuery query)
        {
            foreach (var keywordRecordType in docType.KeywordRecordTypes)
            {
                foreach (var keywordType in keywordRecordType.KeywordTypes) query.AddDisplayColumn(keywordType);
            }
        }

        // Generate a DocPop link for a specified document
        private string CreateDocPopLink(Document doc)
        {
            try
            {
                string queryString = $"clientType=html&docId={doc.ID}";
                string checksum = new ChecksumCreator(queryString, docPop.DocPopChecksumSeed).CreateChecksum();
                return $"{docPop.DocPopBaseUrl}?{queryString}{(string.IsNullOrEmpty(docPop.DocPopChecksumSeed) ? "" : $"&chksum={checksum}")}";
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error creating DocPop URL for document [{doc.ID}]!", ex);
            }
        }

        // Generate a document query based on the request filters
        #pragma warning disable S3776 // Not overly complex
        private DocumentQuery MakeDocumentQuery(RetrievalRequest request, bool useDisplayColumns = true)
        #pragma warning restore S3776
        {
            try
            {
                var query = App.Core.CreateDocumentQuery();

                if (string.IsNullOrEmpty(request.CustomQuery) && string.IsNullOrEmpty(request.DocumentType))
                    throw new DatabankException("Request must specify a document type or custom query!");

                var config = new OnBaseTaxonomy(App);

                if (string.IsNullOrEmpty(request.CustomQuery))
                {
                    var docType = config.GetDocumentType(request.DocumentType) ?? throw new DatabankException($"Cannot find document type [{request.DocumentType}]!");
                    if (!docType.CanI(DocumentTypePrivileges.DocumentViewing))
                        throw new DatabankException($"User [{App.CurrentUser.DisplayName}] cannot view document type [{request.DocumentType}]!");
                    query.AddDocumentType(docType);
                    if (useDisplayColumns) GetKeywordColumns(docType, query);
                }
                else
                {
                    var customQuery = config.GetCustomQuery(request.CustomQuery) ?? throw new DatabankException($"Cannot find custom query [{request.CustomQuery}]!");
                    query.AddCustomQuery(customQuery);
                    if (useDisplayColumns)
                    {
                        foreach (var cqDocType in customQuery.DocumentTypes)
                        {
                            GetKeywordColumns(cqDocType, query);
                        }
                    }
                }

                query.AddDateRange(request.DateRange.StartDate, request.DateRange.EndDate);

                if (request.KeywordGroups != null)
                {
                    foreach (var record in from keyGroup in request.KeywordGroups where keyGroup.MultiInstance select Metadata.MakeQueryKeywordGroup(keyGroup) into record where record != null select record)
                        query.AddQueryKeywordRecord(record);

                    foreach (var keyword in request.KeywordGroups.Where(group => !group.MultiInstance).SelectMany(group => from keyItem in @group.Keywords let keyword = Metadata.MakeKeyword(keyItem) where keyItem != null select keyword)) query.AddKeyword(keyword);
                }

                if (request.Keywords != null)
                {
                    foreach (var keyword in request.Keywords.Select(keyItem => Metadata.MakeKeyword(keyItem)).Where(keyword => keyword != null)) query.AddKeyword(keyword);
                }

                if (!useDisplayColumns) return query;

                query.AddDisplayColumn(DisplayColumnType.DocumentID);
                query.AddDisplayColumn(DisplayColumnType.DocumentName);
                query.AddDisplayColumn(DisplayColumnType.DocumentTypeName);
                query.AddDisplayColumn(DisplayColumnType.DocumentDate);
                query.AddDisplayColumn(DisplayColumnType.ArchivalDate);

                return query;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error creating document query!", ex);
            }
        }

        // Obtain document metadata for a specified document
        #pragma warning disable S3776 // Not overly complex
        private DocumentInfo GetDocumentInfo(Document doc)
        #pragma warning restore S3776
        {
            try
            {
                var metadata = new DocumentInfo
                {
                    Handle = doc.ID,
                    Name = doc.Name,
                    Type = doc.DocumentType.Name,
                    DocumentDate = doc.DocumentDate,
                    DateStored = doc.DateStored
                };

                if (!doc.DocumentType.CanI(DocumentTypePrivileges.ViewKeywords))
                    throw new DatabankException($"User [{App.CurrentUser.DisplayName}] cannot view document type [{doc.DocumentType}]!");
                foreach (var record in doc.KeywordRecords)
                {
                    if (record.KeywordRecordType.RecordType == RecordType.StandAlone)
                    {
                        foreach (var keyword in record.Keywords)
                        {
                            if (keyword == null || keyword.IsBlank) continue;
                            metadata.Keywords.Add(new KeywordInfo
                            {
                                Name = keyword.KeywordType.Name,
                                Id = keyword.KeywordType.ID,
                                Value = keyword.Value.ToString()
                            });
                        }
                    }
                    else
                    {
                        var keyGroup = new KeywordGroup
                        {
                            Id = record.KeywordRecordType.ID,
                            Name = record.KeywordRecordType.Name,
                            MultiInstance = record.KeywordRecordType.RecordType == RecordType.MultiInstance
                        };
                        foreach (var keyword in record.Keywords)
                        {
                            keyGroup.Keywords.Add(new KeywordInfo
                            {
                                Id = keyword.KeywordType.ID,
                                Name = keyword.KeywordType.Name,
                                Value = keyword.Value.ToString()
                            });
                        }
                        metadata.KeywordGroups.Add(keyGroup);
                    }
                }

                return metadata;
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting metadata from document [{doc.ID}]!", ex);
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
