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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestApi._02.AccessingTaxonomy.HelperClasses.OnBase;
using RestApi._03.DocumentRetrieval.HelperClasses.OnBase;
using RestApi._03.DocumentRetrieval.Models.Objects;
using RestApi.TestHarness.Web.Infrastructure;
using RestApi.TestHarness.Web.Models;
#endregion

#pragma warning disable S1192 // In a training project, keep literals
namespace RestApi.TestHarness.Web.Controllers
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness.Web's own
     * RetrievalController. Same overall shape (Search is Post-Redirect-Get, storing
     * results in Session; LoadDetail/SelectRevision/SelectRendition are AJAX returning a
     * rendered partial view, not raw JSON, matching _DetailPane's own markup complexity;
     * RetrieveFile is a plain form POST, letting the browser's own native download
     * handling take over via the response headers), but:
     *
     * - Search builds RestApi.03's own RetrievalRequest (Scope/Ids/Keywords with their
     *   own Operator/Relation/DateRanges as a list), not Unity's flatter
     *   DocumentTypes/CustomQuery/Keywords(name+value only)/DateRange(one). Every
     *   keyword submitted here uses Operator "Equal"/Relation "And" (the same implicit
     *   default Unity's own flat KeywordInfo always meant), the richer per-keyword
     *   operator/relation capability RetrievalRequest exposes isn't surfaced in this
     *   app's own UI, matching the same "identical application" scoping decision made
     *   for RestApi.TestHarness (the WPF version)'s own Retrieval page.
     *
     * - No DocPop/UnityPop links in the detail pane at all: a confirmed gap.
     *
     * - No IsPdfConvertible whitelist check: RestApi.03's own preferPdf uses Accept-header
     *   content negotiation, letting the server decide, with a built-in graceful fallback,
     *   see RestApi.03's own LectureNotes.md. The "Prefer PDF" checkbox is always
     *   enabled here, not conditionally disabled.
     *
     * - RenditionInfo has no FileExtension at all (RestApi.03's own Rendition schema
     *   doesn't have one, confirmed against document-api.json directly), so the
     *   downloaded file's own extension is resolved from the rendition's File Type's own
     *   SystemName instead (a reasonable approximation, not a guaranteed exact match,
     *   same approach RestApi.TestHarness's own DocumentDetailViewModel uses).
     *
     * - RevisionInfo is genuinely sparser than Unity API's own (just Id/RevisionNumber,
     *   no Date at the revision level), and RenditionInfo carries Comment/CreatedByUserId
     *   instead (a real structural difference between the two APIs' own data models, not
     *   a bug), see RestApi.03's own LectureNotes.md.
     */
    #endregion

    /// <summary>
    /// The Retrieval page: search by Document Type(s)/Custom Query/Document ID, browse
    /// results, and view a selected document's full detail (metadata, keyword groups,
    /// revisions/renditions, file retrieval).
    /// </summary>
    /// <remarks>
    /// Create a new instance of the RetrievalController class
    /// </remarks>
    /// <param name="store">The per-session connection store.</param>
    /// <param name="log">The shared, per-session output log.</param>
    public class RetrievalController(SessionManagementStore store, SessionLogService log) : Controller
    {
        #region Private Members
        // Session key for the cached page model
        private const string SessionKey = "TestHarness.RetrievalPageModel";
        #endregion

        #region Public Methods
        /// <summary>
        /// Displays the Retrieval page, from whatever taxonomy/search results/detail was
        /// last loaded this session.
        /// </summary>
        /// <returns>The Retrieval page.</returns>
        public IActionResult Index()
        {
            return View(GetModel());
        }

        /// <summary>
        /// Loads Document Type Groups, Document Types, and Custom Queries for the search
        /// mode selectors, connecting first (using whatever's currently configured) if
        /// not already connected.
        /// </summary>
        /// <param name="username">The OnBase username to connect with, if not already connected.</param>
        /// <param name="password">The OnBase password to connect with, if not already connected.</param>
        /// <returns>A redirect back to the Retrieval page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoadTaxonomy(string username, string password)
        {
            var session = store.GetOrCreate(HttpContext.Session.Id);

            if (!session.IsConnected)
            {
                try
                {
                    if (!string.IsNullOrEmpty(username)) session.ServiceLocation.Username = username;
                    if (!string.IsNullOrEmpty(password)) session.ServiceLocation.Password = password;
                    await session.ConnectAsync();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    return RedirectToAction("Index");
                }
            }

            try
            {
                var http = session.GetHttpClient();

                var groups = await OnBaseTaxonomy.GetDocumentTypeGroupsAsync(client: http) ?? [];
                var docTypes = await OnBaseTaxonomy.GetDocumentTypesAsync(client: http) ?? [];
                var queries = await OnBaseTaxonomy.GetCustomQueriesAsync(client: http) ?? [];

                var model = GetModel();
                model.IsTaxonomyLoaded = true;
                model.DocumentTypeGroups = [.. groups.Select(g => new NamedItem { Id = g.Id, Name = g.Name })];
                model.AllDocumentTypes = [.. docTypes.Select(d => new NamedItem { Id = d.Id, Name = d.Name })];
                model.CustomQueries = [.. queries.Select(q => new NamedItem { Id = q.Id, Name = q.Name })];
                SaveModel(model);

                log.Success($"Loaded {model.AllDocumentTypes.Count} document type(s), {model.DocumentTypeGroups.Count} group(s), {model.CustomQueries.Count} custom quer{(model.CustomQueries.Count == 1 ? "y" : "ies")}.");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// AJAX: Keyword Types common to every given Document Type, for the Document
        /// Type(s) search mode's dynamic keyword fields.
        /// </summary>
        /// <param name="docTypeIds">The currently-selected Document Type IDs.</param>
        /// <returns>A JSON list of <see cref="KeywordTypeItem"/>.</returns>
        [HttpGet]
        public async Task<IActionResult> GetCommonKeywords(string[] docTypeIds)
        {
            try
            {
                var http = store.TryGet(HttpContext.Session.Id)?.GetHttpClient();
                if (http == null || docTypeIds == null || docTypeIds.Length == 0) return Json(new List<KeywordTypeItem>());

                var common = await OnBaseTaxonomy.GetCommonKeywordTypesAsync([.. docTypeIds], http);
                var result = common.Select(k => new KeywordTypeItem { Id = k.Id, Name = k.Name, DataType = k.DataType }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return Json(new List<KeywordTypeItem>());
            }
        }

        /// <summary>
        /// AJAX: the given Custom Query's own Keyword Types, for the Custom Query search
        /// mode's dynamic keyword fields.
        /// </summary>
        /// <param name="queryId">The Custom Query's ID.</param>
        /// <returns>A JSON list of <see cref="KeywordTypeItem"/>.</returns>
        [HttpGet]
        public async Task<IActionResult> GetCustomQueryKeywords(string queryId)
        {
            try
            {
                var http = store.TryGet(HttpContext.Session.Id)?.GetHttpClient();
                if (http == null || string.IsNullOrEmpty(queryId)) return Json(new List<KeywordTypeItem>());

                var keywordTypes = await OnBaseTaxonomy.GetCustomQueryKeywordTypesAsync(queryId, http) ?? [];
                var result = keywordTypes.Select(k => new KeywordTypeItem { Id = k.Id, Name = k.Name, DataType = k.DataType }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return Json(new List<KeywordTypeItem>());
            }
        }

        /// <summary>
        /// Performs a search (Document Type(s) or Custom Query mode) and stores the
        /// results, or retrieves a single document directly (Document ID mode) and loads
        /// its detail immediately, bypassing the results list entirely.
        /// </summary>
        /// <param name="mode">Which search mode was used.</param>
        /// <param name="documentTypeIds">Selected Document Type IDs (Document Type mode).</param>
        /// <param name="customQueryId">The selected Custom Query's ID (Custom Query mode).</param>
        /// <param name="startDate">Search start date.</param>
        /// <param name="endDate">Search end date.</param>
        /// <param name="keywordIds">Parallel array of Keyword Type IDs for the dynamic keyword fields.</param>
        /// <param name="keywordValues">Parallel array of keyword values for the dynamic keyword fields.</param>
        /// <param name="documentIdInput">The Document ID to retrieve directly (Document ID mode).</param>
        /// <returns>A redirect back to the Retrieval page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        #pragma warning disable S107 // Method has many parameters due to the nature of the search form, acceptable in this context
        public async Task<IActionResult> Search(RetrievalSearchMode mode, string[] documentTypeIds, string customQueryId,
            DateTime? startDate, DateTime? endDate, string[] keywordIds, string[] keywordValues, string documentIdInput)
        #pragma warning restore S107
        {
            var model = GetModel();
            model.SearchResults = [];
            model.Detail = new RetrievalDetailModel();
            model.LastSearchMode = mode;

            var session = store.TryGet(HttpContext.Session.Id);
            if (session == null || !session.IsConnected)
            {
                log.Error("Cannot search: not connected.");
                SaveModel(model);
                return RedirectToAction("Index");
            }

            try
            {
                var http = session.GetHttpClient();

                if (mode == RetrievalSearchMode.DocumentId)
                {
                    if (!long.TryParse(documentIdInput, out _))
                    {
                        log.Error($"[{documentIdInput}] is not a valid Document ID.");
                        SaveModel(model);
                        return RedirectToAction("Index");
                    }

                    SaveModel(model);
                    return await LoadDetailAndRedirect(documentIdInput, http);
                }

                var request = new RetrievalRequest
                {
                    DateRanges = [new QueryDateRange { Start = startDate ?? DateTime.MinValue, End = endDate ?? DateTime.Now }]
                };

                if (mode == RetrievalSearchMode.CustomQuery)
                {
                    request.Scope = QueryScope.CustomQuery;
                    request.Ids = string.IsNullOrEmpty(customQueryId) ? [] : [customQueryId];
                }
                else
                {
                    request.Scope = QueryScope.DocumentType;
                    request.Ids = [.. (documentTypeIds ?? [])];
                }

                if (request.Ids.Count == 0)
                {
                    log.Error("Select at least one Document Type or a Custom Query to search.");
                    SaveModel(model);
                    return RedirectToAction("Index");
                }

                if (keywordIds != null)
                {
                    for (var i = 0; i < keywordIds.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(keywordValues?[i])) continue;
                        request.Keywords.Add(new QueryKeyword { TypeId = keywordIds[i], Value = keywordValues[i] });
                    }
                }

                var results = await DocumentRetrieval.GetDocumentInfoAsync(request, http) ?? [];
                model.SearchResults = results;
                SaveModel(model);

                log.Success($"Found {results.Count} document(s).");
            }
            catch (Exception ex)
            {
                log.Error(ex);
                SaveModel(model);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// AJAX: loads a document's full detail (metadata and its revision/rendition
        /// tree), defaulting to its first revision's first rendition.
        /// </summary>
        /// <param name="handle">The Document ID to load.</param>
        /// <returns>The rendered detail pane partial view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoadDetail(string handle)
        {
            var http = store.TryGet(HttpContext.Session.Id)?.GetHttpClient();
            await LoadDetailInternal(handle, http);
            return PartialView("_DetailPane", GetModel().Detail);
        }

        /// <summary>
        /// AJAX: changes the selected revision on the currently-loaded document, defaulting
        /// to that revision's first rendition.
        /// </summary>
        /// <param name="revisionId">The revision ID to select.</param>
        /// <returns>The rendered detail pane partial view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectRevision(string revisionId)
        {
            var model = GetModel();
            if (model.Detail != null)
            {
                model.Detail.SelectedRevisionId = revisionId;
                model.Detail.SelectedRenditionFileTypeId = model.Detail.SelectedRevision?.Renditions.FirstOrDefault()?.FileTypeId;
                SaveModel(model);
            }
            return PartialView("_DetailPane", model.Detail);
        }

        /// <summary>
        /// AJAX: changes the selected rendition on the currently-loaded document's
        /// currently-selected revision.
        /// </summary>
        /// <param name="fileTypeId">The rendition's file type ID to select.</param>
        /// <returns>The rendered detail pane partial view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectRendition(string fileTypeId)
        {
            var model = GetModel();
            if (model.Detail != null)
            {
                model.Detail.SelectedRenditionFileTypeId = fileTypeId;
                SaveModel(model);
            }
            return PartialView("_DetailPane", model.Detail);
        }

        /// <summary>
        /// Retrieves the currently-selected revision/rendition's file content and
        /// triggers a browser download. A plain form POST, not AJAX: a native
        /// post-to-a-file-result is how a browser's own "Save As" download is triggered
        /// without extra JS blob handling.
        /// </summary>
        /// <param name="preferPdf">Whether to retrieve converted to PDF, where the server supports it.</param>
        /// <returns>The file content, or a redirect back if nothing is selected.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RetrieveFile(bool preferPdf)
        {
            var model = GetModel();
            var detail = model.Detail;

            if (detail?.SelectedRevision == null || detail.SelectedRendition == null)
            {
                log.Error("Cannot retrieve file: no revision/rendition selected.");
                return RedirectToAction("Index");
            }

            try
            {
                var http = store.TryGet(HttpContext.Session.Id)?.GetHttpClient();
                if (http == null)
                {
                    log.Error("Cannot retrieve file: not connected.");
                    return RedirectToAction("Index");
                }

                var file = await DocumentRetrieval.GetDocumentFileAsync(detail.Metadata.Handle, detail.SelectedRevisionId, detail.SelectedRenditionFileTypeId, preferPdf, http);
                if (file?.Content == null)
                {
                    log.Error("No file content returned.");
                    return RedirectToAction("Index");
                }

                // See this class's own Training Notes on why SystemName, not a true
                // "FileExtension" field (RestApi.03's own RenditionInfo has none).
                var extension = preferPdf ? "pdf" : await ResolveFileExtension(detail.SelectedRendition.FileTypeId, http);
                var fileName = $"{detail.Metadata.Name}.{extension}";

                log.Success($"Retrieved file [{fileName}].");
                return File(file.Content, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return RedirectToAction("Index");
            }
        }
        #endregion

        #region Private Methods
        // Read the cached page model from Session (as JSON), or a fresh one
        private RetrievalPageModel GetModel()
        {
            var json = HttpContext.Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json)) return new RetrievalPageModel();

            try
            {
                return JsonSerializer.Deserialize<RetrievalPageModel>(json) ?? new RetrievalPageModel();
            }
            catch (JsonException)
            {
                return new RetrievalPageModel();
            }
        }

        // Save the page model to Session (as JSON)
        private void SaveModel(RetrievalPageModel model)
        {
            HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(model));
        }

        // Load a document's detail into the cached model (metadata, revisions,
        // defaulting to the first revision's first rendition), resolving each
        // rendition's own FileTypeName along the way
        private async Task LoadDetailInternal(string handle, HttpClient http)
        {
            var model = GetModel();
            model.Detail = new RetrievalDetailModel();

            if (http == null)
            {
                log.Error("Cannot load document: not connected.");
                SaveModel(model);
                return;
            }

            try
            {
                var metadata = await DocumentRetrieval.GetDocumentInfoAsync(handle, http);

                if (metadata == null)
                {
                    log.Error($"No document found with ID [{handle}].");
                    SaveModel(model);
                    return;
                }

                var revisions = await DocumentRetrieval.GetDocumentRevisionsAsync(handle, http) ?? [];
                var fileTypes = await OnBaseTaxonomy.GetFileTypesAsync(http);
                foreach (var revision in revisions)
                {
                    foreach (var rendition in revision.Renditions)
                    {
                        rendition.FileTypeName = fileTypes?.Find(f => f.Id == rendition.FileTypeId)?.Name;
                    }
                }

                var detail = new RetrievalDetailModel
                {
                    Metadata = metadata,
                    Revisions = revisions
                };

                var firstRevision = detail.Revisions.FirstOrDefault();
                detail.SelectedRevisionId = firstRevision?.Id;
                detail.SelectedRenditionFileTypeId = firstRevision?.Renditions.FirstOrDefault()?.FileTypeId;

                model.Detail = detail;
                SaveModel(model);

                log.Success($"Loaded document [{handle}]: {detail.Revisions.Count} revision(s).");
            }
            catch (Exception ex)
            {
                log.Error(ex);
                SaveModel(model);
            }
        }

        // Load a document's detail (Document ID search mode) then redirect to Index
        private async Task<IActionResult> LoadDetailAndRedirect(string handle, HttpClient http)
        {
            await LoadDetailInternal(handle, http);
            return RedirectToAction("Index");
        }

        // Resolve a File Type id to a best-guess extension (its SystemName), for the
        // downloaded file's own name
        private static async Task<string> ResolveFileExtension(string fileTypeId, HttpClient http)
        {
            try
            {
                var fileTypes = await OnBaseTaxonomy.GetFileTypesAsync(http);
                var fileType = fileTypes?.Find(f => f.Id == fileTypeId);
                return !string.IsNullOrEmpty(fileType?.SystemName) ? fileType.SystemName.ToLowerInvariant() : "dat";
            }
            catch (Exception)
            {
                return "dat";
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
