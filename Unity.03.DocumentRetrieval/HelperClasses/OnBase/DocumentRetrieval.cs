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
using Unity._00.CommonFunctionality.Models.Enumerations;
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

        // File formats confirmed convertible to PDF via the PDF data provider (per
        // Hyland's own PDFDataProvider/ImageDataProvider support table). AFP and DJDE are
        // ALSO listed as supported in that table, but have no corresponding FileFormat
        // enum member, so they're deliberately excluded here rather than guessed at.
        private static readonly HashSet<long> PdfConvertibleFileFormats = new HashSet<long>
        {
            (long)FileFormat.Text,
            (long)FileFormat.Image,
            (long)FileFormat.Pcl,
            (long)FileFormat.Word,
            (long)FileFormat.Excel,
            (long)FileFormat.Pdf
        };
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
        /// <param name="preferPdf">When true and the file supports it (see <see cref="IsPdfConvertible"/>), retrieves it converted to PDF instead of its native format.</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Document data</returns>
        public DocumentData GetDocument(long id, bool preferPdf = false, Application app = null)
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
                    File = GetDocumentFile(doc, preferPdf)
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
        /// <param name="preferPdf">When true and the file supports it (see <see cref="IsPdfConvertible"/>), retrieves it converted to PDF instead of its native format.</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Document file</returns>
        public DocumentFile GetDocumentFile(long id, bool preferPdf = false, Application app = null)
        {
            try
            {
                Initialize(app);
                var doc = App.Core.GetDocumentByID(id);
                return doc == null ? null : GetDocumentFile(doc, preferPdf);
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
        /// <param name="preferPdf">When true and the rendition's file type supports it (see <see cref="IsPdfConvertible"/>), retrieves it converted to PDF instead of its native format.</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Document file</returns>
        public DocumentFile GetDocumentFile(Document doc, bool preferPdf = false, Application app = null)
        {
            try
            {
                Initialize(app);

                // Delegates to the Rendition overload below, using the document's own
                // default rendition of its latest revision, the vast majority of callers
                // want exactly this and never need to think about revisions/renditions at
                // all. Callers that DO need a specific revision/rendition (e.g., a document
                // viewer letting a user browse a document's full revision/rendition
                // history) should call the Rendition overload directly instead.
                return GetDocumentFile(doc.DefaultRenditionOfLatestRevision, preferPdf, App);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error retrieving document content!", ex);
            }
        }

        /// <summary>
        /// Retrieve file contents for a specific rendition, use this instead of the
        /// <see cref="GetDocumentFile(Document, bool, Application)"/> overload when the caller
        /// needs a rendition OTHER than a document's own default rendition of its latest
        /// revision (e.g., an older revision, or a non-default rendition on the latest one).
        /// </summary>
        /// <param name="rendition">The rendition to retrieve</param>
        /// <param name="preferPdf">When true and this rendition's file type supports it (see <see cref="IsPdfConvertible"/>), retrieves it converted to PDF instead of its native format. Silently ignored for unsupported file types, retrieval proceeds in the native format instead.</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Document file</returns>
        public DocumentFile GetDocumentFile(Rendition rendition, bool preferPdf = false, Application app = null)
        {
            try
            {
                Initialize(app);

                var retrieval = App.Core.Retrieval;

                PageData data = preferPdf && IsPdfConvertible(rendition.FileType.ID)
                    ? retrieval.PDF.GetDocument(rendition)
                    : rendition.FileType.ID switch
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
                throw new DatabankException("Error retrieving rendition content!", ex);
            }
        }

        /// <summary>
        /// Retrieve file contents for a specific revision/rendition on a document,
        /// identified by ID rather than requiring a live <see cref="Rendition"/> object be
        /// held as state. Use this (not the Rendition overload) when the caller only has
        /// <see cref="RevisionInfo"/>/<see cref="RenditionInfo"/> DTO data (e.g., after a
        /// fresh per-request <see cref="GetDocumentById"/> + <see cref="GetDocumentRevisions"/>),
        /// the pattern a caller without a persistently-held Unity connection (e.g., a web
        /// request) actually needs.
        /// </summary>
        /// <param name="doc">The document to retrieve from.</param>
        /// <param name="revisionId">The target revision's ID.</param>
        /// <param name="renditionFileTypeId">The target rendition's file type ID.</param>
        /// <param name="preferPdf">When true and this rendition's file type supports it (see <see cref="IsPdfConvertible"/>), retrieves it converted to PDF instead of its native format.</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>Document file</returns>
        public DocumentFile GetDocumentFile(Document doc, long revisionId, long renditionFileTypeId, bool preferPdf = false, Application app = null)
        {
            try
            {
                var revision = doc.Revisions.FirstOrDefault(r => r.ID == revisionId)
                    ?? throw new DatabankException($"Revision [{revisionId}] not found on document [{doc.ID}]!");
                var rendition = revision.Renditions.FirstOrDefault(r => r.FileType.ID == renditionFileTypeId)
                    ?? throw new DatabankException($"Rendition with file type [{renditionFileTypeId}] not found on revision [{revisionId}]!");

                return GetDocumentFile(rendition, preferPdf, app);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error retrieving rendition content for document [{doc.ID}]!", ex);
            }
        }

        /// <summary>
        /// Whether the given file type ID can be retrieved converted to PDF via the PDF
        /// data provider. Based on Hyland's own PDFDataProvider/ImageDataProvider support
        /// table; AFP and DJDE are ALSO listed there as supported, but have no
        /// corresponding <see cref="FileFormat"/> enum member, so they're deliberately
        /// excluded here rather than guessed at.
        /// </summary>
        /// <param name="fileTypeId">The file type ID to check.</param>
        /// <returns><see langword="true"/> if convertible to PDF; otherwise, <see langword="false"/>.</returns>
        public static bool IsPdfConvertible(long fileTypeId) => PdfConvertibleFileFormats.Contains(fileTypeId);

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

        /// <summary>
        /// Retrieve the raw document object for a specified document ID, without fetching
        /// its metadata, links, or file content. Use this instead of
        /// <see cref="GetDocument(long, bool, Application)"/> when a caller wants the
        /// document itself (e.g., to walk its Revisions/Renditions directly, or to pass
        /// to <see cref="GetDocumentInfo(Document)"/>/<see cref="GetDocumentRevisions"/>
        /// individually) without eagerly paying for the full metadata+links+file fetch
        /// GetDocument always performs.
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <param name="app">Unity API Application Object</param>
        /// <returns>The document, or <see langword="null"/> if none exists with that ID.</returns>
        public Document GetDocumentById(long id, Application app = null)
        {
            try
            {
                Initialize(app);
                return App.Core.GetDocumentByID(id);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error retrieving document [{id}]!", ex);
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

        // Obtain display column data for keywords on document type. Deliberately
        // SKIPS MultiInstance keyword groups: a document with N instances of a
        // multi-instance group would otherwise come back as N separate result rows (one
        // per instance, each repeating the document's own ID/Name/Type/Date), rather than
        // one row per document, since a flat display-column query has no way to represent
        // "more than one value" for a single row. Full multi-instance data is still
        // available via GetDocumentInfo(Document) once a document is actually selected,
        // this only affects what a hit-list SEARCH shows.
        private static void GetKeywordColumns(DocumentType docType, DocumentQuery query)
        {
            foreach (var keywordRecordType in docType.KeywordRecordTypes)
            {
                if (keywordRecordType.RecordType == RecordType.MultiInstance) continue;

                foreach (var keywordType in keywordRecordType.KeywordTypes) query.AddDisplayColumn(keywordType);
            }
        }

        // Generate a DocPop link for a specified document. Checksum calculation is
        // skipped entirely (not just its use in the URL) when no DocPopChecksumSeed is
        // configured, ChecksumCreator itself throws ("either 'ChecksumKey' or
        // 'ChecksumValue' unavailable") if asked to compute a checksum with a blank seed,
        // a checksum is optional, calculating one shouldn't be mandatory.
        private string CreateDocPopLink(Document doc)
        {
            try
            {
                string queryString = $"clientType=html&docId={doc.ID}";
                string checksum = string.IsNullOrEmpty(docPop.DocPopChecksumSeed)
                    ? null
                    : new ChecksumCreator(queryString, docPop.DocPopChecksumSeed).CreateChecksum();
                return $"{docPop.DocPopBaseUrl}?{queryString}{(checksum == null ? "" : $"&chksum={checksum}")}";
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error creating DocPop URL for document [{doc.ID}]!", ex);
            }
        }

        // Generate a document query based on the request filters. DocumentTypes
        // (plural) takes precedence over DocumentType (singular) when both are populated,
        // see RetrievalRequest's own Training Notes.
        #pragma warning disable S3776 // Not overly complex
        private DocumentQuery MakeDocumentQuery(RetrievalRequest request, bool useDisplayColumns = true)
        #pragma warning restore S3776
        {
            try
            {
                var query = App.Core.CreateDocumentQuery();

                bool hasDocumentTypes = request.DocumentTypes is { Count: > 0 };

                if (string.IsNullOrEmpty(request.CustomQuery) && !hasDocumentTypes && string.IsNullOrEmpty(request.DocumentType))
                    throw new DatabankException("Request must specify one or more document types or a custom query!");

                var config = new OnBaseTaxonomy(App);

                if (!string.IsNullOrEmpty(request.CustomQuery))
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
                else
                {
                    var documentTypeNames = hasDocumentTypes ? request.DocumentTypes : [request.DocumentType];

                    foreach (var documentTypeName in documentTypeNames)
                    {
                        var docType = config.GetDocumentType(documentTypeName) ?? throw new DatabankException($"Cannot find document type [{documentTypeName}]!");
                        if (!docType.CanI(DocumentTypePrivileges.DocumentViewing))
                            throw new DatabankException($"User [{App.CurrentUser.DisplayName}] cannot view document type [{documentTypeName}]!");
                        query.AddDocumentType(docType);
                        if (useDisplayColumns) GetKeywordColumns(docType, query);
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

        #pragma warning disable S3776 // Not overly complex
        /// <summary>
        /// Obtain metadata (keywords/keyword groups and other display fields) for an
        /// already-retrieved <see cref="Document"/>, without fetching its file content.
        /// Exposed publicly (originally used only internally by <see cref="GetDocument(long, bool, Application)"/>,
        /// which also fetches the file) for callers that want a document's metadata
        /// without the potentially expensive file retrieval, e.g., a document viewer
        /// populating a detail pane, where file retrieval is a separate, explicit action.
        /// </summary>
        /// <param name="doc">The document to read metadata from.</param>
        /// <returns>The document's metadata.</returns>
        public DocumentInfo GetDocumentInfo(Document doc)
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

        /// <summary>
        /// Obtain a serializable <see cref="RevisionInfo"/>/<see cref="RenditionInfo"/>
        /// tree for an already-retrieved <see cref="Document"/>'s full revision/rendition
        /// history. Prefer this over walking <c>doc.Revisions</c>/<c>revision.Renditions</c>
        /// directly and holding the live objects as state, a serializable DTO is what a
        /// caller without a persistently-held Unity connection (e.g., a web request)
        /// actually needs.
        /// </summary>
        /// <param name="doc">The document to read revisions/renditions from.</param>
        /// <returns>The document's revisions, each with its own renditions.</returns>
        public List<RevisionInfo> GetDocumentRevisions(Document doc)
        {
            try
            {
                var revisions = new List<RevisionInfo>();

                foreach (var revision in doc.Revisions)
                {
                    var revisionInfo = new RevisionInfo
                    {
                        Id = revision.ID,
                        Date = revision.Date,
                        Comment = revision.Comment,
                        CreatedBy = revision.CreatedBy?.ToString()
                    };

                    foreach (var rendition in revision.Renditions)
                    {
                        revisionInfo.Renditions.Add(new RenditionInfo
                        {
                            FileTypeName = rendition.FileType.Name,
                            FileTypeId = rendition.FileType.ID,
                            FileExtension = rendition.FileExtension,
                            NumberOfPages = rendition.NumberOfPages,
                            Comment = rendition.Comment,
                            CreatedBy = rendition.CreatedBy?.ToString(),
                            CreationDate = rendition.CreationDate
                        });
                    }

                    revisions.Add(revisionInfo);
                }

                return revisions;
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting revisions from document [{doc.ID}]!", ex);
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
