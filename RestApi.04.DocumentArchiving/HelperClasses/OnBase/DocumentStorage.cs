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
 * ******************************************************************** */
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RestApi._00.CommonFunctionality.Models.Objects;
using RestApi._02.AccessingTaxonomy.HelperClasses.OnBase;
using RestApi._03.DocumentRetrieval.Models.Objects;
using RestApi._04.DocumentArchiving.Models.Enumerations;
using RestApi._04.DocumentArchiving.Models.Objects;
#endregion

namespace RestApi._04.DocumentArchiving.HelperClasses.OnBase
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.04.DocumentArchiving's own
     * DocumentStorage, scoped to conventional documents only (see this project's own
     * .csproj Training Notes). No CanAddRevision/CanAddRendition: confirmed there's no
     * REST equivalent of Unity API's pre-flight Revisable/Renditionable + CanI privilege
     * check, document-api.json's own DocumentType schema exposes neither flag, the only
     * place that information surfaces at all is AFTER an upload attempt, in a 300
     * Multiple Choices response's per-match canAddAsRevision/canAddAsRendition flags.
     * Confirmed decision: omit the pre-flight check entirely rather than approximate it,
     * RestApi.TestHarness's Add Revision/Add Rendition controls will always be enabled
     * (when a document is loaded) rather than conditionally, and a disallowed attempt
     * surfaces as an ordinary failed-request error instead.
     *
     * File uploads go through the REST API's own three-step staging process
     * (StageUploadsAsync: POST /documents/uploads to initiate each file, PUT
     * .../uploads/{id}?filePart=N for each part, referencing the resulting upload ids in
     * the final archive/revision/rendition request), entirely different from Unity API's
     * own NewDocumentRequest.Files/UpdateDocumentRequest.Files (which the SDK reads
     * directly off local disk). This project's own request models still just take local
     * file paths, though, the staging mechanics are handled internally, not leaked to
     * callers.
     *
     * Keyword collections need a keywordGuid (a concurrency token), obtained via GET
     * /document-types/{id}/default-keywords for a NEW document, or GET
     * /documents/{id}/keywords for an EXISTING one (see BuildKeywordCollectionAsync). This
     * has no Unity API equivalent at all, Unity's own KeywordModifier/
     * CreateStoreNewDocumentProperties have no analogous "did the schema change under you"
     * concurrency guard.
     *
     * 300 Multiple Choices (the REST API's own disambiguation flow for
     * Revisable/Renditionable document types, described in the Upload and Archive
     * Interactions guide) is surfaced as an explicit, readable DatabankException here
     * rather than silently handled or ignored: CreateDocumentAsync does not attempt to
     * pick a match and add a revision/rendition on the caller's behalf, StoreAsNew=true
     * is the deliberate, explicit way to skip the disambiguation, matching how the
     * request model itself already exposes that flag.
     *
     * *Correction*: every method here is static, and always has been, so the instance
     * Client property/constructor parameter were never actually read by any of them,
     * only the per-call client parameter mattered. That was harmless while RestApi.01's
     * SessionManagement was itself static (Initialize() silently fell back to it when
     * client was omitted), but SessionManagement is no longer static (see its own
     * Training Notes: every user of RestApi.TestHarness.Web needs their own IdP token/
     * session, not a shared process-wide one), so that fallback is gone entirely now.
     * Initialize() now simply requires the caller to supply a client, there is no longer
     * any global default to fall back to.
     */
    #endregion

    /// <summary>
    /// Exposes OnBase conventional-document creation, update, and deletion functions via
    /// the Document Management (REST) API.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the DocumentStorage class.
    /// </remarks>
    /// <param name="client">The connected HttpClient to use, or null to use RestApi.01.ConnectingToOnBase.HelperClasses.OnBase.SessionManagement.GetHttpClient().</param>
    public class DocumentStorage(HttpClient client = null)
    {
        #region Properties
        /// <summary>The connected, authenticated HttpClient used for requests.</summary>
        public HttpClient Client { get; set; } = client;

        #endregion
        #region Constructors
        #endregion

        #region Public Methods
        /// <summary>
        /// Store a new document in the OnBase system.
        /// </summary>
        /// <param name="request">New document information.</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>The new document's id.</returns>
        public static async Task<string> CreateDocumentAsync(NewDocumentRequest request, HttpClient client = null)
        {
            var http = Initialize(client);

            try
            {
                if (request.Files == null || request.Files.Count == 0)
                    throw new DatabankException("No file(s) provided for new document!");

                var docType = await OnBaseTaxonomy.GetDocumentTypeAsync(request.DocumentType, http).ConfigureAwait(false)
                    ?? throw new DatabankException($"Cannot find document type [{request.DocumentType}]!");

                var fileTypeId = request.FileTypeId;
                if (string.IsNullOrEmpty(fileTypeId))
                {
                    var extension = Path.GetExtension(request.Files[0]).TrimStart('.');
                    var defaultFileType = await OnBaseTaxonomy.GetDefaultUploadFileTypeAsync(extension, http).ConfigureAwait(false)
                        ?? throw new DatabankException($"No file type found matching extension [{extension}]!");
                    fileTypeId = defaultFileType.Id;
                }

                var keywordCollection = await BuildKeywordCollectionAsync(http, newDocumentTypeId: docType.Id, existingDocumentId: null, request.Keywords, request.KeywordGroups).ConfigureAwait(false);
                var uploads = await StageUploadsAsync(request.Files, http).ConfigureAwait(false);

                var body = new
                {
                    documentTypeId = docType.Id,
                    fileTypeId,
                    storeAsNew = request.StoreAsNew,
                    comment = request.Comment,
                    documentDate = request.DocumentDate.ToString("yyyy-MM-dd"),
                    uploads,
                    keywordCollection
                };

                #pragma warning disable S1192 // Keeping literal for clarity
                using var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                #pragma warning restore S1192
                using var response = await http.PostAsync("documents", content).ConfigureAwait(false);

                if ((int)response.StatusCode == 300)
                {
                    var choicesJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    throw new DatabankException($"Document Type [{docType.Name}] is Revisable/Renditionable and one or more existing documents match; set NewDocumentRequest.StoreAsNew = true to store as a new document regardless, or handle the match explicitly (not implemented in this training set). Server response: {choicesJson}");
                }

                await EnsureSuccessAsync(response, "documents").ConfigureAwait(false);

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                using var document = JsonDocument.Parse(json);
                return document.RootElement.GetProperty("id").GetString();
            }
            catch (DatabankException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error storing document!", ex);
            }
        }

        /// <summary>
        /// Update an existing document in the OnBase system: its metadata, a new
        /// revision, or a new rendition.
        /// </summary>
        /// <param name="request">Update document information.</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>True on success.</returns>
        public static Task<bool> ModifyDocumentAsync(UpdateDocumentRequest request, HttpClient client = null)
        {
            var http = Initialize(client);

            return request.UpdateType switch
            {
                UpdateType.Metadata => UpdateMetadataAsync(request, http),
                UpdateType.Revision => UpdateRevisionAsync(request, http),
                UpdateType.Rendition => UpdateRenditionAsync(request, http),
                _ => throw new DatabankException($"Update type [{request.UpdateType}] not supported!"),
            };
        }

        /// <summary>
        /// Delete an existing document from OnBase. There is no purge/permanent-delete
        /// option: confirmed there is no REST API equivalent of Unity API's
        /// PurgeDocument, "purge" does not appear anywhere in document-api.json.
        /// </summary>
        /// <param name="request">Delete parameters.</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>True on success.</returns>
        public static async Task<bool> DeleteDocumentAsync(DeleteRequest request, HttpClient client = null)
        {
            try
            {
                var http = Initialize(client);

                using var response = await http.DeleteAsync($"documents/{Uri.EscapeDataString(request.DocumentId)}").ConfigureAwait(false);
                await EnsureSuccessAsync(response, $"documents/{request.DocumentId}").ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error deleting document [{request.DocumentId}]!", ex);
            }
        }
        #endregion

        #region Private Update Methods
        // Update keywords, document type, and/or document date (PUT /documents/{id}, a "reindex")
        private static async Task<bool> UpdateMetadataAsync(UpdateDocumentRequest request, HttpClient http)
        {
            try
            {
                var targetDocType = await OnBaseTaxonomy.GetDocumentTypeAsync(request.DocumentType, http).ConfigureAwait(false)
                    ?? throw new DatabankException($"Cannot find document type [{request.DocumentType}]!");

                var keywordCollection = await BuildKeywordCollectionAsync(http, newDocumentTypeId: null, existingDocumentId: request.DocumentId, request.Keywords, request.KeywordGroups).ConfigureAwait(false);

                var body = new
                {
                    targetDocumentTypeId = targetDocType.Id,
                    targetFileTypeId = request.FileTypeId,
                    storeAsNew = false,
                    comment = request.Comment,
                    documentDate = request.DocumentDate.ToString("yyyy-MM-dd"),
                    keywordCollection
                };

                using var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                using var putRequest = new HttpRequestMessage(HttpMethod.Put, $"documents/{Uri.EscapeDataString(request.DocumentId)}") { Content = content };
                using var response = await http.SendAsync(putRequest).ConfigureAwait(false);
                await EnsureSuccessAsync(response, $"documents/{request.DocumentId}").ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error updating document metadata!", ex);
            }
        }

        // Store a new revision (POST /documents/{id}/revisions, objectType: RevisionArchive)
        private static async Task<bool> UpdateRevisionAsync(UpdateDocumentRequest request, HttpClient http)
        {
            try
            {
                if (request.Files == null || request.Files.Count == 0)
                    throw new DatabankException("No file(s) provided for new revision!");

                var uploads = await StageUploadsAsync(request.Files, http).ConfigureAwait(false);
                var keywordCollection = await BuildKeywordCollectionAsync(http, newDocumentTypeId: null, existingDocumentId: request.DocumentId, request.Keywords, request.KeywordGroups).ConfigureAwait(false);

                var body = new
                {
                    objectType = "RevisionArchive",
                    comment = request.Comment,
                    fileTypeId = request.FileTypeId,
                    uploads,
                    keywordCollection
                };

                using var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                using var response = await http.PostAsync($"documents/{Uri.EscapeDataString(request.DocumentId)}/revisions", content).ConfigureAwait(false);
                await EnsureSuccessAsync(response, $"documents/{request.DocumentId}/revisions").ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error updating document revision!", ex);
            }
        }

        // Store a new rendition on the latest revision (POST /documents/{id}/revisions/latest/renditions, objectType: RenditionArchive)
        private static async Task<bool> UpdateRenditionAsync(UpdateDocumentRequest request, HttpClient http)
        {
            try
            {
                if (request.Files == null || request.Files.Count == 0)
                    throw new DatabankException("No file(s) provided for new rendition!");

                var uploads = await StageUploadsAsync(request.Files, http).ConfigureAwait(false);

                var body = new
                {
                    objectType = "RenditionArchive",
                    comment = request.Comment,
                    fileTypeId = request.FileTypeId,
                    uploads
                };

                using var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                using var response = await http.PostAsync($"documents/{Uri.EscapeDataString(request.DocumentId)}/revisions/latest/renditions", content).ConfigureAwait(false);
                await EnsureSuccessAsync(response, $"documents/{request.DocumentId}/revisions/latest/renditions").ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error updating document rendition!", ex);
            }
        }
        #endregion

        #region Private Helper Methods
        // Resolve the HttpClient to use for a call: must be explicitly supplied, no
        // static/global fallback exists anymore (see this class's own Training Notes)
        private static HttpClient Initialize(HttpClient client)
        {
            return client ?? throw new DatabankException("HttpClient cannot be null! RestApi.01's SessionManagement is no longer static, an instance's GetHttpClient() must be passed explicitly.");
        }

        // Build a keywordCollection payload (keywordGuid + grouped items), obtaining the
        // keywordGuid from GET /document-types/{id}/default-keywords for a new document,
        // or GET /documents/{id}/keywords for an existing one (see this class's own
        // Training Notes on why a concurrency token is needed at all here)
        private static async Task<object> BuildKeywordCollectionAsync(HttpClient http, string newDocumentTypeId, string existingDocumentId, List<KeywordInfo> keywords, List<KeywordGroup> keywordGroups)
        {
            var path = newDocumentTypeId != null
                ? $"document-types/{Uri.EscapeDataString(newDocumentTypeId)}/default-keywords"
                : $"documents/{Uri.EscapeDataString(existingDocumentId)}/keywords";

            using var response = await http.GetAsync(path).ConfigureAwait(false);
            await EnsureSuccessAsync(response, path).ConfigureAwait(false);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var document = JsonDocument.Parse(json);
            var keywordGuid = document.RootElement.GetProperty("keywordGuid").GetString();

            var items = new List<object>();

            if (keywords is { Count: > 0 })
            {
                items.Add(new
                {
                    keywords = keywords.Select(k => new { typeId = k.Id, values = new[] { new { value = k.Value } } }).ToArray()
                });
            }

            if (keywordGroups != null)
            {
                foreach (var group in keywordGroups)
                {
                    items.Add(new
                    {
                        typeGroupId = group.TypeGroupId,
                        groupId = group.GroupId,
                        keywords = group.Keywords.Select(k => new { typeId = k.Id, values = new[] { new { value = k.Value } } }).ToArray()
                    });
                }
            }

            return new { keywordGuid, items };
        }

        // Stage one or more local files for upload via the REST API's three-step process
        // (initiate -> upload part(s) -> the resulting upload ids are referenced by the
        // caller's own archive/revision/rendition request), returning the resulting
        // upload references in the same order the files were given (page order)
        private static async Task<List<object>> StageUploadsAsync(List<string> filePaths, HttpClient http)
        {
            var uploads = new List<object>();

            foreach (var filePath in filePaths)
            {
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists) throw new DatabankException($"File not found: [{filePath}]!");

                var extension = fileInfo.Extension.TrimStart('.');
                var metaBody = new { fileExtension = extension, fileSize = fileInfo.Length };

                using var initContent = new StringContent(JsonSerializer.Serialize(metaBody), Encoding.UTF8, "application/json");
                using var initResponse = await http.PostAsync("documents/uploads", initContent).ConfigureAwait(false);
                await EnsureSuccessAsync(initResponse, "documents/uploads").ConfigureAwait(false);

                var initJson = await initResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                using var initDoc = JsonDocument.Parse(initJson);
                var uploadId = initDoc.RootElement.GetProperty("id").GetString();
                var filePartSize = initDoc.RootElement.GetProperty("filePartSize").GetInt32();
                var numberOfParts = initDoc.RootElement.GetProperty("numberOfParts").GetInt32();

                var bytes = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);

                for (var part = 1; part <= numberOfParts; part++)
                {
                    var offset = (part - 1) * filePartSize;
                    var length = Math.Min(filePartSize, bytes.Length - offset);
                    var partBytes = new byte[length];
                    Array.Copy(bytes, offset, partBytes, 0, length);

                    using var partContent = new ByteArrayContent(partBytes);
                    partContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    using var putResponse = await http.PutAsync($"documents/uploads/{Uri.EscapeDataString(uploadId)}?filePart={part}", partContent).ConfigureAwait(false);
                    await EnsureSuccessAsync(putResponse, $"documents/uploads/{uploadId}?filePart={part}").ConfigureAwait(false);
                }

                uploads.Add(new { id = uploadId });
            }

            return uploads;
        }

        // Throw a DatabankException with request context if the response wasn't successful
        private static async Task EnsureSuccessAsync(HttpResponseMessage response, string relativePath)
        {
            if (response.IsSuccessStatusCode) return;

            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new DatabankException($"Request to [{relativePath}] failed with status {(int)response.StatusCode} ({response.ReasonPhrase}): {body}");
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
