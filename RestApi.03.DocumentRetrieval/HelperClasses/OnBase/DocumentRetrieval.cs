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
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RestApi._00.CommonFunctionality.Models.Objects;
using RestApi._02.AccessingTaxonomy.HelperClasses.OnBase;
using RestApi._03.DocumentRetrieval.Models.Objects;
#endregion

namespace RestApi._03.DocumentRetrieval.HelperClasses.OnBase
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.03.DocumentRetrieval's own
     * DocumentRetrieval, but the query flow is genuinely two-step, not one synchronous
     * call: POST /documents/queries creates the query (the response body's own "id"
     * field, not the Location header, is used to get the query id, simpler and avoids
     * header parsing), then GET .../results fetches the actual results. GetDocumentInfoAsync
     * does both steps internally, so callers still get one result list back, matching
     * Unity.03's own single ExecuteQueryResults() call.
     *
     * Display columns are requested explicitly (DocumentId/DocumentName/DocumentTypeName/
     * DocumentDate/ArchivalDate, then one Keyword column per resolved common keyword
     * type), the same five-plus-keywords shape Unity.03's own GetKeywordColumns builds,
     * and results are parsed back by POSITION (userDisplayColumns' own order), avoiding a
     * third call to GET .../columns to resolve what each result index means, since the
     * order requested is already known.
     *
     * preferPdf uses the Accept header (Accept: application/pdf) for content negotiation,
     * replacing Unity API's own retrieval.PDF/.Image/.Text/.Native provider dispatch
     * entirely, there's no equivalent of IsPdfConvertible's hardcoded FileFormat
     * whitelist here: the server itself decides what it can convert, GetDocumentFileAsync
     * just asks for PDF and falls back to the rendition's native format if that request
     * fails, rather than pre-checking a whitelist client-side.
     *
     * GetDocumentLinks (DocPop/UnityPop) has NO REST equivalent here, a confirmed gap, see
     * RestApi.00's ServiceLocation Training Notes.
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
    /// Exposes OnBase document retrieval functions via the Document Management (REST) API.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the DocumentRetrieval class.
    /// </remarks>
    /// <param name="client">The connected HttpClient to use, or null to use RestApi.01.ConnectingToOnBase.HelperClasses.OnBase.SessionManagement.GetHttpClient().</param>
    public class DocumentRetrieval(HttpClient client = null)
    {
        #region Private Members
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
        #endregion

        #region Properties
        /// <summary>The connected, authenticated HttpClient used for requests.</summary>
        public HttpClient Client { get; set; } = client;
        #endregion

        #region Public Methods
        /// <summary>
        /// Retrieve hit-list of document metadata (does not include file contents).
        /// </summary>
        /// <param name="request">Request object with filter information for document query.</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>Document hit list, or null if there were no results.</returns>
        public static async Task<List<DocumentInfo>> GetDocumentInfoAsync(RetrievalRequest request, HttpClient client = null)
        {
            try
            {
                var http = Initialize(client);

                if (request.Ids == null || request.Ids.Count == 0)
                {
                    throw new DatabankException("Request must specify one or more ids to search (document types, document type groups, or a single custom query)!");
                }

                var keywordColumns = await GetKeywordColumnsAsync(request, http).ConfigureAwait(false);
                var queryId = await CreateQueryAsync(request, keywordColumns, http).ConfigureAwait(false);

                using var response = await http.GetAsync($"documents/queries/{Uri.EscapeDataString(queryId)}/results").ConfigureAwait(false);
                await EnsureSuccessAsync(response, $"documents/queries/{queryId}/results").ConfigureAwait(false);

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                using var document = JsonDocument.Parse(json);
                #pragma warning disable S1192 // Keep literals in training projects
                var items = document.RootElement.GetProperty("items");
                #pragma warning restore S1192

                var docs = new List<DocumentInfo>();
                foreach (var item in items.EnumerateArray())
                {
                    docs.Add(ParseDocumentResult(item, keywordColumns));
                }

                return docs.Count == 0 ? null : docs;
            }
            catch (DatabankException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error retrieving document hit list!", ex);
            }
        }

        /// <summary>
        /// Obtain metadata (keywords/keyword groups and other fields) for a specified
        /// document id, without fetching its file content.
        /// </summary>
        /// <param name="id">Document id.</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>The document's metadata, or null if not found.</returns>
        #pragma warning disable S3776 // Not overly complex
        public static async Task<DocumentInfo> GetDocumentInfoAsync(string id, HttpClient client = null)
        #pragma warning restore S3776
        {
            try
            {
                var http = Initialize(client);

                using var docResponse = await http.GetAsync($"documents/{Uri.EscapeDataString(id)}").ConfigureAwait(false);
                if (docResponse.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
                await EnsureSuccessAsync(docResponse, $"documents/{id}").ConfigureAwait(false);
                var docJson = await docResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                using var docDoc = JsonDocument.Parse(docJson);
                var docRoot = docDoc.RootElement;

                var typeId = docRoot.TryGetProperty("typeId", out var typeIdEl) ? typeIdEl.GetString() : null;
                var docType = typeId != null ? await OnBaseTaxonomy.GetDocumentTypeAsync(typeId, http).ConfigureAwait(false) : null;

                var info = new DocumentInfo
                {
                    Handle = docRoot.GetProperty("id").GetString(),
                    Name = docRoot.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null,
                    Type = docType?.Name,
                    DocumentDate = ParseDate(docRoot, "documentDate"),
                    DateStored = ParseDate(docRoot, "storedDate")
                };

                using var kwResponse = await http.GetAsync($"documents/{Uri.EscapeDataString(id)}/keywords").ConfigureAwait(false);
                await EnsureSuccessAsync(kwResponse, $"documents/{id}/keywords").ConfigureAwait(false);
                var kwJson = await kwResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                using var kwDoc = JsonDocument.Parse(kwJson);
                var kwItems = kwDoc.RootElement.GetProperty("items");

                foreach (var groupElement in kwItems.EnumerateArray())
                {
                    var hasGroupId = groupElement.TryGetProperty("groupId", out var groupIdEl) && groupIdEl.ValueKind != JsonValueKind.Null;
                    var hasTypeGroupId = groupElement.TryGetProperty("typeGroupId", out var typeGroupIdEl) && typeGroupIdEl.ValueKind != JsonValueKind.Null;

                    var keywords = new List<KeywordInfo>();
                    if (groupElement.TryGetProperty("keywords", out var keywordsElement))
                    {
                        foreach (var kw in keywordsElement.EnumerateArray())
                        {
                            string firstValue = null;
                            if (kw.TryGetProperty("values", out var valuesEl) && valuesEl.ValueKind == JsonValueKind.Array && valuesEl.GetArrayLength() > 0 && valuesEl[0].TryGetProperty("value", out var valEl))
                                firstValue = valEl.GetString();

                            keywords.Add(new KeywordInfo
                            {
                                Id = kw.GetProperty("typeId").GetString(),
                                Value = firstValue
                            });
                        }
                    }

                    if (!hasTypeGroupId)
                    {
                        // Standalone: no group nesting at all, add each keyword directly
                        info.Keywords.AddRange(keywords);
                    }
                    else
                    {
                        info.KeywordGroups.Add(new KeywordGroup
                        {
                            TypeGroupId = typeGroupIdEl.GetString(),
                            GroupId = hasGroupId ? groupIdEl.GetString() : null,
                            MultiInstance = hasGroupId,
                            Keywords = keywords
                        });
                    }
                }

                return info;
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting metadata for document [{id}]!", ex);
            }
        }

        /// <summary>
        /// Retrieve document data (including file contents) for a specified document id.
        /// </summary>
        /// <param name="id">Document id.</param>
        /// <param name="preferPdf">When true, retrieves the file converted to PDF instead of its native format, if the server supports it (falls back to native format otherwise).</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>Document data, or null if not found.</returns>
        public static async Task<DocumentData> GetDocumentAsync(string id, bool preferPdf = false, HttpClient client = null)
        {
            try
            {
                var http = Initialize(client);

                var metadata = await GetDocumentInfoAsync(id, http).ConfigureAwait(false);
                if (metadata == null) return null;

                return new DocumentData
                {
                    Metadata = metadata,
                    File = await GetDocumentFileAsync(id, preferPdf, http).ConfigureAwait(false)
                };
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error retrieving document!", ex);
            }
        }

        /// <summary>
        /// Retrieve a document's revisions, each with its own renditions.
        /// </summary>
        /// <param name="id">Document id.</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>The document's revisions.</returns>
        #pragma warning disable S3776 // Not overly complex
        public static async Task<List<RevisionInfo>> GetDocumentRevisionsAsync(string id, HttpClient client = null)
        #pragma warning restore S3776
        {
            try
            {
                var http = Initialize(client);

                var revisions = await GetRawItemsAsync(http, $"documents/{Uri.EscapeDataString(id)}/revisions").ConfigureAwait(false);
                var result = new List<RevisionInfo>();

                foreach (var revisionElement in revisions)
                {
                    var revisionId = revisionElement.GetProperty("id").GetString();
                    var revisionInfo = new RevisionInfo
                    {
                        Id = revisionId,
                        RevisionNumber = revisionElement.TryGetProperty("revisionNumber", out var rn) ? rn.GetInt32() : 0
                    };

                    var renditions = await GetRawItemsAsync(http, $"documents/{Uri.EscapeDataString(id)}/revisions/{Uri.EscapeDataString(revisionId)}/renditions").ConfigureAwait(false);
                    foreach (var renditionElement in renditions)
                    {
                        DateTime? created = null;
                        if (renditionElement.TryGetProperty("created", out var cr) && cr.ValueKind == JsonValueKind.String
                            && DateTime.TryParse(cr.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var createdDate))
                        {
                            created = createdDate;
                        }

                        revisionInfo.Renditions.Add(new RenditionInfo
                        {
                            FileTypeId = renditionElement.GetProperty("fileTypeId").GetString(),
                            PageCount = renditionElement.TryGetProperty("pageCount", out var pc) ? pc.GetInt32() : 0,
                            CreatedByUserId = renditionElement.TryGetProperty("createdByUserId", out var cb) ? cb.GetString() : null,
                            Comment = renditionElement.TryGetProperty("comment", out var cm) ? cm.GetString() : null,
                            Created = created
                        });
                    }

                    result.Add(revisionInfo);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting revisions for document [{id}]!", ex);
            }
        }

        /// <summary>
        /// Retrieve file contents for a document's default rendition of its latest revision.
        /// </summary>
        /// <param name="id">Document id.</param>
        /// <param name="preferPdf">When true, retrieves the file converted to PDF instead of its native format, if the server supports it (falls back to native format otherwise).</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>Document file.</returns>
        public static Task<DocumentFile> GetDocumentFileAsync(string id, bool preferPdf = false, HttpClient client = null)
        {
            return GetDocumentFileAsync(id, "latest", "default", preferPdf, client);
        }

        /// <summary>
        /// Retrieve file contents for a specific revision/rendition on a document. Use
        /// this instead of the two-argument overload when the caller needs a rendition
        /// OTHER than the document's own default rendition of its latest revision (e.g.,
        /// an older revision, or a non-default rendition on the latest one).
        /// </summary>
        /// <param name="id">Document id.</param>
        /// <param name="revisionId">The target revision's id, or "latest".</param>
        /// <param name="renditionFileTypeId">The target rendition's File Type id, or "default".</param>
        /// <param name="preferPdf">When true, retrieves the file converted to PDF instead of its native format, if the server supports it (falls back to native format otherwise).</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>Document file.</returns>
        public static async Task<DocumentFile> GetDocumentFileAsync(string id, string revisionId, string renditionFileTypeId, bool preferPdf = false, HttpClient client = null)
        {
            try
            {
                var http = Initialize(client);
                var path = $"documents/{Uri.EscapeDataString(id)}/revisions/{Uri.EscapeDataString(revisionId)}/renditions/{Uri.EscapeDataString(renditionFileTypeId)}/content";

                if (preferPdf)
                {
                    var pdfContent = await TryGetContentAsync(http, path, "application/pdf").ConfigureAwait(false);
                    if (pdfContent != null) return new DocumentFile { Content = pdfContent };
                    // Falls through to native format below, the same "silently ignored
                    // for unsupported file types" behavior Unity.03's own preferPdf had.
                }

                using var response = await http.GetAsync(path).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
                await EnsureSuccessAsync(response, path).ConfigureAwait(false);
                var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                return new DocumentFile { Content = bytes };
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error retrieving rendition content for document [{id}]!", ex);
            }
        }
        #endregion

        #region Private Methods
        // Resolve the HttpClient to use for a call: must be explicitly supplied, no
        // static/global fallback exists anymore (see this class's own Training Notes)
        private static HttpClient Initialize(HttpClient client)
        {
            return client ?? throw new DatabankException("HttpClient cannot be null! RestApi.01's SessionManagement is no longer static, an instance's GetHttpClient() must be passed explicitly.");
        }

        // Resolve which Keyword Types should become display columns for a search,
        // matching Unity.03's own GetKeywordColumns: common keyword types across every
        // searched Document Type, or a Custom Query's own keyword types
        private static async Task<List<RestApi._02.AccessingTaxonomy.Models.Objects.KeywordType>> GetKeywordColumnsAsync(RetrievalRequest request, HttpClient http)
        {
            return request.Scope switch
            {
                QueryScope.DocumentType => await OnBaseTaxonomy.GetCommonKeywordTypesAsync(request.Ids, http).ConfigureAwait(false),
                QueryScope.CustomQuery => await OnBaseTaxonomy.GetCustomQueryKeywordTypesAsync(request.Ids[0], http).ConfigureAwait(false),
                // DocumentTypeGroup: resolving every document type across every group and
                // intersecting is possible but not done here, keyword columns are simply
                // omitted for this scope (results still include id/name/type/dates).
                QueryScope.DocumentTypeGroup => [],
                _ => [],
            };
        }

        // POST /documents/queries, returning the new query's id
        private static async Task<string> CreateQueryAsync(RetrievalRequest request, List<RestApi._02.AccessingTaxonomy.Models.Objects.KeywordType> keywordColumns, HttpClient http)
        {
            var displayColumns = new List<object>
            {
                new { displayColumnType = "DocumentId" },
                new { displayColumnType = "DocumentName" },
                new { displayColumnType = "DocumentTypeName" },
                new { displayColumnType = "DocumentDate" },
                new { displayColumnType = "ArchivalDate" }
            };
            foreach (var keywordType in keywordColumns)
            {
                displayColumns.Add(new { displayColumnType = "Keyword", keywordTypeId = keywordType.Id });
            }

            var body = new
            {
                queryType = new[] { new { type = request.Scope.ToString(), ids = request.Ids } },
                maxResults = request.MaxResults,
                queryKeywordCollection = request.Keywords.Select(k => new { typeId = k.TypeId, value = k.Value, @operator = k.Operator, relation = k.Relation }).ToArray(),
                documentDateRangeCollection = request.DateRanges.Select(d => new { start = d.Start?.ToString("yyyy-MM-dd"), end = d.End?.ToString("yyyy-MM-dd") }).ToArray(),
                userDisplayColumns = displayColumns.ToArray()
            };

            using var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            using var response = await http.PostAsync("documents/queries", content).ConfigureAwait(false);
            await EnsureSuccessAsync(response, "documents/queries").ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var document = JsonDocument.Parse(json);
            return document.RootElement.GetProperty("id").GetString();
        }

        // Parse one search result item back into a DocumentInfo, using the SAME
        // positional order display columns were requested in (0=id, 1=name, 2=type,
        // 3=documentDate, 4=archivalDate, 5+=keywordColumns, in order)
        private static DocumentInfo ParseDocumentResult(JsonElement item, List<RestApi._02.AccessingTaxonomy.Models.Objects.KeywordType> keywordColumns)
        {
            var info = new DocumentInfo { Handle = item.GetProperty("id").GetString() };
            var displayColumns = item.GetProperty("displayColumns");

            foreach (var column in displayColumns.EnumerateArray())
            {
                var index = int.Parse(column.GetProperty("index").GetString(), CultureInfo.InvariantCulture);
                var values = column.GetProperty("values");
                var firstValue = values.GetArrayLength() > 0 ? values[0].GetString() : null;
                if (firstValue == null) continue;

                switch (index)
                {
                    case 0: break; // DocumentId, already have it from item.id
                    case 1: info.Name = firstValue; break;
                    case 2: info.Type = firstValue; break;
                    case 3: info.DocumentDate = DateTime.Parse(firstValue, CultureInfo.InvariantCulture); break;
                    case 4: info.DateStored = DateTime.Parse(firstValue, CultureInfo.InvariantCulture); break;
                    default:
                        var keywordIndex = index - 5;
                        if (keywordIndex >= 0 && keywordIndex < keywordColumns.Count)
                        {
                            var keywordType = keywordColumns[keywordIndex];
                            info.Keywords.Add(new KeywordInfo { Id = keywordType.Id, Name = keywordType.Name, Value = firstValue });
                        }
                        break;
                }
            }

            return info;
        }

        // GET a path expecting {"items": [...]}, deserializing into a List<T>
        #pragma warning disable S1144 // Keep as lesson
        private static async Task<List<T>> GetCollectionAsync<T>(HttpClient http, string relativePath)
        #pragma warning restore S1144
        {
            using var response = await http.GetAsync(relativePath).ConfigureAwait(false);
            await EnsureSuccessAsync(response, relativePath).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var document = JsonDocument.Parse(json);
            var itemsJson = document.RootElement.GetProperty("items").GetRawText();
            return JsonSerializer.Deserialize<List<T>>(itemsJson, JsonOptions) ?? [];
        }

        // GET a path expecting {"items": [...]}, returning the raw JsonElement items
        // (used where the shape varies enough, or enough manual null-checking is needed,
        // that a typed POCO deserialization isn't a good fit, e.g. revisions/renditions)
        private static async Task<List<JsonElement>> GetRawItemsAsync(HttpClient http, string relativePath)
        {
            using var response = await http.GetAsync(relativePath).ConfigureAwait(false);
            await EnsureSuccessAsync(response, relativePath).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var document = JsonDocument.Parse(json);
            return [.. document.RootElement.GetProperty("items").EnumerateArray()];
        }

        // Attempt a GET with the given Accept header, returning the content bytes on
        // success, or null if the server rejects the requested format (rather than
        // throwing, since a fallback to the native format is the caller's own next step)
        private static async Task<byte[]> TryGetContentAsync(HttpClient http, string path, string acceptType)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptType));

            using var response = await http.SendAsync(request).ConfigureAwait(false);
            return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false) : null;
        }

        // Parse a date-only or date-time string property, returning DateTime.MinValue if absent
        private static DateTime ParseDate(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var element) || element.ValueKind != JsonValueKind.String) return DateTime.MinValue;
            return DateTime.TryParse(element.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : DateTime.MinValue;
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
