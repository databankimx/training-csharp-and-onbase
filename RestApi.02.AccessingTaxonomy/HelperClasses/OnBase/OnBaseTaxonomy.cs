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
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using RestApi._00.CommonFunctionality.Models.Objects;
using RestApi._02.AccessingTaxonomy.Models.Objects;
#endregion

namespace RestApi._02.AccessingTaxonomy.HelperClasses.OnBase
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.02.AccessingTaxonomy's own
     * OnBaseTaxonomy. Takes an HttpClient (from RestApi.01's SessionManagement.GetHttpClient(),
     * defaulting to it when none is supplied, same optional-parameter convenience pattern
     * Unity.02's own Application app parameters use) rather than a Hyland.Unity.Application.
     *
     * Method count is deliberately smaller than Unity.02's: several of that class's
     * overloads exist purely to accept either a Document/DocumentType parameter (Unity
     * API objects that carry their own document type reference), which doesn't apply
     * here, every method here takes plain string ids/names instead. The functional
     * surface is the same; the overload sprawl isn't reproduced.
     *
     * GetDocumentTypeKeywordGroupsAsync does real resolution work Unity.02 never needed
     * to: GET .../keyword-type-groups only returns group ids and each keyword type's bare
     * id (see TaxonomyModels.cs's own Training Notes), so this method makes the
     * additional /keyword-type-groups and /keyword-types calls needed to fully resolve
     * names/data types, giving callers one populated result instead of three partial
     * ones, the same convenience Unity API's own docType.KeywordRecordTypes property
     * gives for free via the SDK.
     *
     * No GetUnityForm equivalent, *correction*: an earlier version of this file's own
     * Training Notes claimed Unity Forms had no REST API counterpart at all. That was
     * wrong, just incomplete research at the time: Unity Form Templates/instances DO
     * have a REST equivalent, GetUnityFormTemplatesAsync/GetUnityFormTemplateAsync
     * below, they just live on a genuinely SEPARATE API (Forms, not Document
     * Management, confirmed against forms-api.json's own servers block: a different
     * {product} path segment, onbase/forms vs onbase/core, on the same {server}).
     * That's why these two methods take their own formsClient parameter and route
     * through InitializeForms()/an explicit Forms API HttpClient, rather than this
     * class's usual Initialize(). See LectureNotes.md.
     *
     * *Correction*: every method here is static, and always has been, so the instance
     * Client property/constructor parameter were never actually read by any of them,
     * only the per-call client parameter mattered. That was harmless while RestApi.01's
     * SessionManagement was itself static (Initialize() silently fell back to it when
     * client was omitted), but SessionManagement is no longer static (see its own
     * Training Notes: every user of RestApi.TestHarness.Web needs their own IdP token/
     * session, not a shared process-wide one), so that fallback is gone entirely now.
     * Initialize()/InitializeForms() now simply require the caller to supply a client,
     * either directly to the method call or (for the internal calls this class makes to
     * its own other methods) threaded through explicitly, there is no longer any global
     * default to fall back to. The unused Client property/constructor parameter are left
     * in place rather than removed mid-refactor, worth knowing they're vestigial if you
     * see them, not a bug.
     *
     * Every id here is a string (matching document-api.json's own schemas), not Unity
     * API's long. GetDocumentTypeGroupAsync/GetDocumentTypeAsync/etc. still use the same
     * "is this numeric-looking, so query by id, else query by systemName" heuristic
     * Unity.02 uses (long.TryParse), even though the id itself is ultimately used as a
     * string either way, for the same "let a caller pass either an id or a name and get
     * the right thing back" convenience.
     *
     * Query string values are percent-encoded via Uri.EscapeDataString, not
     * System.Web.HttpUtility.UrlEncode, System.Web is a .NET-Framework/ASP.NET-classic
     * namespace not part of modern .NET's default surface, unlike net48 projects
     * elsewhere in this training set.
     */
    #endregion

    /// <summary>
    /// Exposes methods to access OnBase taxonomy elements via the Document Management (REST) API.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the OnBaseTaxonomy class.
    /// </remarks>
    /// <param name="client">The connected HttpClient to use, or null to use RestApi.01.ConnectingToOnBase.HelperClasses.OnBase.SessionManagement.GetHttpClient().</param>
    public class OnBaseTaxonomy(HttpClient client = null)
    {
        #region Private Members
        // Shared across calls that need case-insensitive property matching ("id" -> Id, etc.)
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
        #endregion

        #region Properties
        /// <summary>The connected, authenticated HttpClient used for requests.</summary>
        public HttpClient Client { get; set; } = client;
        #endregion

        #region Public Methods
        /// <summary>Obtain Document Type Groups.</summary>
        /// <param name="systemNames">List of Document Type Group system names.</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>List of Document Type Groups.</returns>
        public static async Task<List<DocumentTypeGroup>> GetDocumentTypeGroupsAsync(string[] systemNames = null, HttpClient client = null)
        {
            try
            {
                var http = Initialize(client);

                var query = systemNames is { Length: > 0 } ? "?" + string.Join("&", systemNames.Select(n => $"systemName={Uri.EscapeDataString(n)}")) : "";
                return await GetCollectionAsync<DocumentTypeGroup>(http, $"document-type-groups{query}").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error getting document type groups!", ex);
            }
        }

        /// <summary>Obtain a single Document Type Group, by id or system name.</summary>
        /// <param name="idOrSystemName">Document Type Group id or system name.</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>The Document Type Group, or null if not found.</returns>
        public static async Task<DocumentTypeGroup> GetDocumentTypeGroupAsync(string idOrSystemName, HttpClient client = null)
        {
            try
            {
                var http = Initialize(client);
                var query = long.TryParse(idOrSystemName, out _) ? $"id={Uri.EscapeDataString(idOrSystemName)}" : $"systemName={Uri.EscapeDataString(idOrSystemName)}";
                var results = await GetCollectionAsync<DocumentTypeGroup>(http, $"document-type-groups?{query}").ConfigureAwait(false);
                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting document type group [{idOrSystemName}]!", ex);
            }
        }

        /// <summary>Obtain Document Types.</summary>
        /// <param name="systemNames">List of Document Type system names.</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>List of Document Types.</returns>
        public static async Task<List<DocumentType>> GetDocumentTypesAsync(string[] systemNames = null, HttpClient client = null)
        {
            try
            {
                var http = Initialize(client);
                var query = systemNames is { Length: > 0 } ? "?" + string.Join("&", systemNames.Select(n => $"systemName={Uri.EscapeDataString(n)}")) : "";
                return await GetCollectionAsync<DocumentType>(http, $"document-types{query}").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error getting document types!", ex);
            }
        }

        /// <summary>Obtain Document Types for a specified Document Type Group.</summary>
        /// <param name="groupIdOrSystemName">Document Type Group id or system name.</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>List of Document Types in the group, or null if the group was not found.</returns>
        public static async Task<List<DocumentType>> GetDocumentTypesForGroupAsync(string groupIdOrSystemName, HttpClient client = null)
        {
            try
            {
                var http = Initialize(client);
                var group = await GetDocumentTypeGroupAsync(groupIdOrSystemName, http).ConfigureAwait(false);
                if (group == null) return null;

                return await GetCollectionAsync<DocumentType>(http, $"document-type-groups/{Uri.EscapeDataString(group.Id)}/document-types").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting document types for group [{groupIdOrSystemName}]!", ex);
            }
        }

        /// <summary>Obtain a single Document Type, by id or system name.</summary>
        /// <param name="idOrSystemName">Document Type id or system name.</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>The Document Type, or null if not found.</returns>
        public static async Task<DocumentType> GetDocumentTypeAsync(string idOrSystemName, HttpClient client = null)
        {
            try
            {
                var http = Initialize(client);
                var query = long.TryParse(idOrSystemName, out _) ? $"id={Uri.EscapeDataString(idOrSystemName)}" : $"systemName={Uri.EscapeDataString(idOrSystemName)}";
                var results = await GetCollectionAsync<DocumentType>(http, $"document-types?{query}").ConfigureAwait(false);
                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting document type [{idOrSystemName}]!", ex);
            }
        }

        /// <summary>
        /// Obtain a Document Type's Keyword Type Groups, fully resolved (group
        /// name/storage type, and each Keyword Type's own full metadata), by combining
        /// GET .../keyword-type-groups, GET /keyword-type-groups, and GET /keyword-types
        /// (see this class's own Training Notes for why more than one call is needed).
        /// </summary>
        /// <param name="documentTypeId">Document Type id.</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>Fully-resolved Keyword Type Groups (including the Standalone group, with a null Id, if the Document Type has standalone keywords).</returns>
        #pragma warning disable S3776 // Not overly complex
        public static async Task<List<DocumentTypeKeywordGroup>> GetDocumentTypeKeywordGroupsAsync(string documentTypeId, HttpClient client = null)
        #pragma warning restore S3776
        {
            try
            {
                var http = Initialize(client);

                using var structureResponse = await http.GetAsync($"document-types/{Uri.EscapeDataString(documentTypeId)}/keyword-type-groups").ConfigureAwait(false);
                await EnsureSuccessAsync(structureResponse, $"document-types/{documentTypeId}/keyword-type-groups").ConfigureAwait(false);
                var structureJson = await structureResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                using var structureDoc = JsonDocument.Parse(structureJson);
                var items = structureDoc.RootElement.GetProperty("items");

                // Collect every keyword type id referenced, across every group, to resolve in one batch
                var allKeywordTypeIds = new HashSet<string>();
                foreach (var groupElement in items.EnumerateArray())
                {
                    if (groupElement.TryGetProperty("keywordTypes", out var keywordTypesElement))
                    {
                        foreach (var kt in keywordTypesElement.EnumerateArray())
                        {
                            allKeywordTypeIds.Add(kt.GetProperty("id").GetString());
                        }
                    }
                }

                var resolvedKeywordTypes = await GetKeywordTypesByIdAsync(allKeywordTypeIds, http).ConfigureAwait(false);
                var keywordTypesById = resolvedKeywordTypes.ToDictionary(k => k.Id);

                // Resolve named (non-Standalone) group ids to their names/storage types
                var namedGroupIds = new List<string>();
                foreach (var groupElement in items.EnumerateArray())
                {
                    if (groupElement.TryGetProperty("id", out var idElement) && idElement.ValueKind != JsonValueKind.Null)
                    {
                        namedGroupIds.Add(idElement.GetString());
                    }
                }
                var groupMetadataById = (await GetKeywordTypeGroupsByIdAsync(namedGroupIds, http).ConfigureAwait(false)).ToDictionary(g => g.Id);

                var result = new List<DocumentTypeKeywordGroup>();
                foreach (var groupElement in items.EnumerateArray())
                {
                    var hasId = groupElement.TryGetProperty("id", out var idElement) && idElement.ValueKind != JsonValueKind.Null;
                    var groupId = hasId ? idElement.GetString() : null;
                    groupMetadataById.TryGetValue(groupId ?? "", out var metadata);

                    var keywordTypes = new List<KeywordType>();
                    if (groupElement.TryGetProperty("keywordTypes", out var keywordTypesElement))
                    {
                        foreach (var kt in keywordTypesElement.EnumerateArray())
                        {
                            var id = kt.GetProperty("id").GetString();
                            if (keywordTypesById.TryGetValue(id, out var resolved)) keywordTypes.Add(resolved);
                        }
                    }

                    result.Add(new DocumentTypeKeywordGroup
                    {
                        Id = groupId,
                        Name = metadata?.Name,
                        StorageType = metadata?.StorageType,
                        KeywordTypes = keywordTypes
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keyword type groups for document type [{documentTypeId}]!", ex);
            }
        }

        /// <summary>
        /// Splits a Document Type's resolved Keyword Type Groups into named
        /// (non-Standalone) groups and standalone Keyword Types, the same distinction
        /// Unity.02.AccessingTaxonomy's own SplitKeywordGroups provides.
        /// </summary>
        /// <param name="groups">The Document Type's Keyword Type Groups (from <see cref="GetDocumentTypeKeywordGroupsAsync"/>).</param>
        /// <returns>Named Keyword Type Groups, and standalone Keyword Types.</returns>
        public static (List<DocumentTypeKeywordGroup> Groups, List<KeywordType> Standalone) SplitKeywordGroups(List<DocumentTypeKeywordGroup> groups)
        {
            var named = new List<DocumentTypeKeywordGroup>();
            var standalone = new List<KeywordType>();

            foreach (var group in groups)
            {
                if (group.Id == null) standalone.AddRange(group.KeywordTypes);
                else named.Add(group);
            }

            return (named, standalone);
        }

        /// <summary>
        /// Intersects Keyword Types (by id) across multiple Document Types, only fields
        /// common to EVERY given type.
        /// </summary>
        /// <param name="documentTypeIds">The Document Type ids to intersect.</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>Keyword Types common to every given Document Type.</returns>
        public static async Task<List<KeywordType>> GetCommonKeywordTypesAsync(IEnumerable<string> documentTypeIds, HttpClient client = null)
        {
            try
            {
                var http = Initialize(client);
                var idList = documentTypeIds.ToList();
                if (idList.Count == 0) return [];

                var perTypeKeywords = new List<List<KeywordType>>();
                foreach (var id in idList)
                {
                    var groups = await GetDocumentTypeKeywordGroupsAsync(id, http).ConfigureAwait(false);
                    perTypeKeywords.Add([.. groups.SelectMany(g => g.KeywordTypes)]);
                }

                IEnumerable<KeywordType> common = perTypeKeywords[0];
                for (int i = 1; i < perTypeKeywords.Count; i++)
                {
                    var thisTypesKeywords = new HashSet<string>(perTypeKeywords[i].Select(k => k.Id));
                    common = common.Where(k => thisTypesKeywords.Contains(k.Id));
                }

                return [.. common.GroupBy(k => k.Id).Select(g => g.First())];
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error getting common keyword types!", ex);
            }
        }

        /// <summary>Obtain Custom Queries.</summary>
        /// <param name="systemNames">List of Custom Query system names.</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>List of Custom Queries.</returns>
        public static async Task<List<CustomQuery>> GetCustomQueriesAsync(string[] systemNames = null, HttpClient client = null)
        {
            try
            {
                var http = Initialize(client);
                var query = systemNames is { Length: > 0 } ? "?" + string.Join("&", systemNames.Select(n => $"systemName={Uri.EscapeDataString(n)}")) : "";
                return await GetCollectionAsync<CustomQuery>(http, $"custom-queries{query}").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error getting custom queries!", ex);
            }
        }

        /// <summary>Obtain a single Custom Query, by id or system name.</summary>
        /// <param name="idOrSystemName">Custom Query id or system name.</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>The Custom Query, or null if not found.</returns>
        public static async Task<CustomQuery> GetCustomQueryAsync(string idOrSystemName, HttpClient client = null)
        {
            try
            {
                var http = Initialize(client);
                var query = long.TryParse(idOrSystemName, out _) ? $"id={Uri.EscapeDataString(idOrSystemName)}" : $"systemName={Uri.EscapeDataString(idOrSystemName)}";
                var results = await GetCollectionAsync<CustomQuery>(http, $"custom-queries?{query}").ConfigureAwait(false);
                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting custom query [{idOrSystemName}]!", ex);
            }
        }

        /// <summary>Obtain a Custom Query's Keyword Types.</summary>
        /// <param name="customQueryId">Custom Query id.</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>The Custom Query's Keyword Types.</returns>
        public static async Task<List<KeywordType>> GetCustomQueryKeywordTypesAsync(string customQueryId, HttpClient client = null)
        {
            try
            {
                var http = Initialize(client);
                return await GetCollectionAsync<KeywordType>(http, $"custom-queries/{Uri.EscapeDataString(customQueryId)}/keyword-types").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting keyword types for custom query [{customQueryId}]!", ex);
            }
        }

        /// <summary>Obtain File Types.</summary>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>List of File Types.</returns>
        public static async Task<List<FileType>> GetFileTypesAsync(HttpClient client = null)
        {
            try
            {
                var http = Initialize(client);
                return await GetCollectionAsync<FileType>(http, "file-types").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error getting file types!", ex);
            }
        }

        /// <summary>
        /// Obtain the "best guess" File Type for a given file extension, for use when
        /// uploading a new document (see the Upload and Archive Interactions guide's own
        /// description of GET /default-upload-file-types).
        /// </summary>
        /// <param name="extension">File extension (without the leading period).</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>The default File Type for the extension, or null if none is configured.</returns>
        public static async Task<FileType> GetDefaultUploadFileTypeAsync(string extension, HttpClient client = null)
        {
            try
            {
                var http = Initialize(client);
                var results = await GetCollectionAsync<FileType>(http, $"default-upload-file-types?extension={Uri.EscapeDataString(extension)}").ConfigureAwait(false);
                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting default upload file type for extension [{extension}]!", ex);
            }
        }

        /// <summary>
        /// Obtain Unity Form Templates, via the separate Forms API (see this class's own
        /// Training Notes for why this method takes its own, separately-connected
        /// HttpClient rather than defaulting to <see cref="Initialize"/>'s Document
        /// Management API one).
        /// </summary>
        /// <param name="creatable">When true, only templates the logged-in user can create a new document with are returned.</param>
        /// <param name="formsClient">The connected Forms API HttpClient to use, or null to use RestApi.01.ConnectingToOnBase.HelperClasses.OnBase.SessionManagement.GetFormsHttpClient().</param>
        /// <returns>List of Unity Form Templates.</returns>
        public static async Task<List<UnityFormTemplate>> GetUnityFormTemplatesAsync(bool? creatable = null, HttpClient formsClient = null)
        {
            try
            {
                var http = InitializeForms(formsClient);
                var query = creatable.HasValue ? $"?creatable={creatable.Value.ToString().ToLowerInvariant()}" : "";
                return await GetCollectionAsync<UnityFormTemplate>(http, $"unity-form-templates{query}").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error getting unity form templates!", ex);
            }
        }

        /// <summary>
        /// Obtain a single Unity Form Template, by id or name. Unlike this class's other
        /// single-item lookups, the Forms API's own GET /unity-form-templates/{id}
        /// endpoint only accepts an id directly (no systemName query filter), so a
        /// non-numeric-looking value here lists every template and matches by
        /// name/system name client-side instead.
        /// </summary>
        /// <param name="idOrName">Unity Form Template id, name, or system name.</param>
        /// <param name="formsClient">The connected Forms API HttpClient to use.</param>
        /// <returns>The Unity Form Template, or null if not found.</returns>
        public static async Task<UnityFormTemplate> GetUnityFormTemplateAsync(string idOrName, HttpClient formsClient = null)
        {
            try
            {
                var http = InitializeForms(formsClient);

                if (long.TryParse(idOrName, out _))
                {
                    using var response = await http.GetAsync($"unity-form-templates/{Uri.EscapeDataString(idOrName)}").ConfigureAwait(false);
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
                    await EnsureSuccessAsync(response, $"unity-form-templates/{idOrName}").ConfigureAwait(false);
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return JsonSerializer.Deserialize<UnityFormTemplate>(json, JsonOptions);
                }

                var all = await GetUnityFormTemplatesAsync(formsClient: http).ConfigureAwait(false);
                return all.FirstOrDefault(t => string.Equals(t.Name, idOrName, StringComparison.OrdinalIgnoreCase) || string.Equals(t.SystemName, idOrName, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error getting unity form template [{idOrName}]!", ex);
            }
        }
        #endregion

        #region Private Methods
        // Resolve the HttpClient to use for a Forms API call: must be explicitly
        // supplied, no static/global fallback exists anymore (see this class's own
        // Training Notes)
        private static HttpClient InitializeForms(HttpClient client)
        {
            return client ?? throw new DatabankException("Forms API HttpClient cannot be null! RestApi.01's SessionManagement is no longer static, an instance's GetFormsHttpClient() must be passed explicitly.");
        }

        // Resolve the HttpClient to use for a call: must be explicitly supplied, no
        // static/global fallback exists anymore (see this class's own Training Notes)
        private static HttpClient Initialize(HttpClient client)
        {
            return client ?? throw new DatabankException("HttpClient cannot be null! RestApi.01's SessionManagement is no longer static, an instance's GetHttpClient() must be passed explicitly.");
        }

        // GET a path expecting {"items": [...]}, deserializing into a List<T>
        private static async Task<List<T>> GetCollectionAsync<T>(HttpClient http, string relativePath)
        {
            using var response = await http.GetAsync(relativePath).ConfigureAwait(false);
            await EnsureSuccessAsync(response, relativePath).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var document = JsonDocument.Parse(json);
            var itemsJson = document.RootElement.GetProperty("items").GetRawText();
            return JsonSerializer.Deserialize<List<T>>(itemsJson, JsonOptions) ?? [];
        }

        // Resolve a set of Keyword Type ids to their full metadata, one call per id (the
        // API's own GET /keyword-types?id=X&id=Y filter supports multiple ids at once,
        // used here to batch the whole set in a single request)
        private static async Task<List<KeywordType>> GetKeywordTypesByIdAsync(IEnumerable<string> ids, HttpClient http)
        {
            var idList = ids.ToList();
            if (idList.Count == 0) return [];

            var query = "?" + string.Join("&", idList.Select(id => $"id={Uri.EscapeDataString(id)}"));
            return await GetCollectionAsync<KeywordType>(http, $"keyword-types{query}").ConfigureAwait(false);
        }

        // Resolve a set of Keyword Type Group ids to their name/storage type, batched
        // the same way GetKeywordTypesByIdAsync batches Keyword Type ids
        private static async Task<List<KeywordTypeGroup>> GetKeywordTypeGroupsByIdAsync(IEnumerable<string> ids, HttpClient http)
        {
            var idList = ids.ToList();
            if (idList.Count == 0) return [];

            var query = "?" + string.Join("&", idList.Select(id => $"id={Uri.EscapeDataString(id)}"));
            return await GetCollectionAsync<KeywordTypeGroup>(http, $"keyword-type-groups{query}").ConfigureAwait(false);
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
