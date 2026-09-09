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
using System.Web.Mvc;
using Hyland.Unity;
using Unity._02.AccessingTaxonomy.HelperClasses.OnBase;
using Unity._03.DocumentRetrieval.HelperClasses.OnBase;
using Unity._03.DocumentRetrieval.Models.Objects;
using Unity.TestHarness.Web.Infrastructure;
using Unity.TestHarness.Web.Models;
#endregion

#pragma warning disable S1192 // In a training project, keep literals
namespace Unity.TestHarness.Web.Controllers
{
    #region Training Notes
    /*
     * *Migration Note: web equivalent of Unity.TestHarness (the WPF version)'s Retrieval
     * page (RetrievalViewModel + DocumentDetailViewModel + their Views). Search
     * (Document Type(s)/Custom Query/Document ID) is a plain POST-then-redirect
     * (Post-Redirect-Get, avoids a "resubmit form?" browser prompt on refresh), storing
     * results in Session. Once results exist, clicking one loads its detail via AJAX
     * (LoadDetail, returning a rendered partial view rather than raw JSON, the detail
     * pane's markup is complex enough that reconstructing it in JS would just be a worse,
     * duplicate copy of _DetailPane.cshtml), and revision/rendition selection within an
     * already-loaded detail is ALSO AJAX (SelectRevision/SelectRendition), since that
     * data is already cached in Session, no new Unity API call is needed for those, just
     * updating which cached item is "selected" and re-rendering.
     *
     * RetrieveFile is a plain form POST, not AJAX: triggering a real browser "Save As"
     * download from a fetch()'d blob is real extra JS ceremony a native form
     * post-to-a-File-result avoids entirely, the browser handles the download natively
     * via the response's Content-Disposition header.
     *
     * GetDocumentInfo(RetrievalRequest, app) and GetDocumentById/GetDocumentInfo(Document)/
     * GetDocumentRevisions/GetDocumentLink/GetDocumentFile(doc, revisionId, fileTypeId, ...)
     * are ALL Unity.03.DocumentRetrieval methods used AS-IS, no web-specific
     * reimplementation, this whole page is really just a UI over that library surface.
     */
    #endregion

    /// <summary>
    /// The Retrieval page: search by Document Type(s)/Custom Query/Document ID, browse
    /// results, and view a selected document's full detail (metadata, keyword groups,
    /// revisions/renditions, DocPop/UnityPop links, file retrieval).
    /// </summary>
    public class RetrievalController : Controller
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
        public ActionResult Index()
        {
            return View(GetModel());
        }

        /// <summary>
        /// Loads Document Type Groups, Document Types, and Custom Queries for the search
        /// mode selectors, connecting first (using whatever's currently configured) if
        /// not already connected.
        /// </summary>
        /// <returns>A redirect back to the Retrieval page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoadTaxonomy()
        {
            if (!SessionConnectionManager.IsConnected())
            {
                SessionConnectionManager.Connect();
                if (!SessionConnectionManager.IsConnected())
                {
                    SessionLog.Error("Cannot load: connect attempt failed, see the error above.");
                    return RedirectToAction("Index");
                }
            }

            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                var taxonomy = new OnBaseTaxonomy(app);

                var groups = taxonomy.GetDocumentTypeGroups(app: app) ?? [];
                var docTypes = taxonomy.GetDocumentTypes((string[])null, app) ?? [];
                var queries = taxonomy.GetCustomQueries(app: app) ?? [];

                var model = GetModel();
                model.IsTaxonomyLoaded = true;
                model.DocumentTypeGroups = [.. groups.Select(g => new NamedItem { Id = g.ID, Name = g.Name })];
                model.AllDocumentTypes = [.. docTypes.Select(d => new NamedItem { Id = d.ID, Name = d.Name })];
                model.CustomQueries = [.. queries.Select(q => new NamedItem { Id = q.ID, Name = q.Name })];
                SaveModel(model);

                SessionLog.Success($"Loaded {model.AllDocumentTypes.Count} document type(s), {model.DocumentTypeGroups.Count} group(s), {model.CustomQueries.Count} custom quer{(model.CustomQueries.Count == 1 ? "y" : "ies")}.");
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// AJAX: Keyword Types common to every given Document Type, for the Document
        /// Type(s) search mode's dynamic keyword fields.
        /// </summary>
        /// <param name="docTypeNames">The currently-selected Document Type names.</param>
        /// <returns>A JSON list of <see cref="KeywordTypeItem"/>.</returns>
        [HttpGet]
        public JsonResult GetCommonKeywords(string[] docTypeNames)
        {
            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                var taxonomy = new OnBaseTaxonomy(app);

                var docTypes = (docTypeNames ?? [])
                    .Select(name => taxonomy.GetDocumentType(name, app))
                    .Where(d => d != null)
                    .ToList();

                var common = taxonomy.GetCommonKeywordTypes(docTypes);
                var result = common.Select(k => new KeywordTypeItem { Id = k.ID, Name = k.Name, DataType = k.DataType.ToString(), Length = k.DataLength }).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
                return Json(new List<KeywordTypeItem>(), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// AJAX: the named Custom Query's own Keyword Types, for the Custom Query search
        /// mode's dynamic keyword fields.
        /// </summary>
        /// <param name="queryName">The Custom Query's name.</param>
        /// <returns>A JSON list of <see cref="KeywordTypeItem"/>.</returns>
        [HttpGet]
        public JsonResult GetCustomQueryKeywords(string queryName)
        {
            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                var taxonomy = new OnBaseTaxonomy(app);

                var query = taxonomy.GetCustomQuery(queryName, app);
                var result = query?.KeywordTypes.Select(k => new KeywordTypeItem { Id = k.ID, Name = k.Name, DataType = k.DataType.ToString(), Length = k.DataLength }).ToList()
                    ?? [];

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
                return Json(new List<KeywordTypeItem>(), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Performs a search (Document Type(s) or Custom Query mode) and stores the
        /// results, or retrieves a single document directly (Document ID mode) and loads
        /// its detail immediately, bypassing the results list entirely.
        /// </summary>
        /// <param name="mode">Which search mode was used.</param>
        /// <param name="documentTypeNames">Selected Document Type names (Document Type mode).</param>
        /// <param name="customQueryName">The selected Custom Query's name (Custom Query mode).</param>
        /// <param name="startDate">Search start date.</param>
        /// <param name="endDate">Search end date.</param>
        /// <param name="keywordNames">Parallel array of keyword names for the dynamic keyword fields.</param>
        /// <param name="keywordValues">Parallel array of keyword values for the dynamic keyword fields.</param>
        /// <param name="documentIdInput">The Document ID to retrieve directly (Document ID mode).</param>
        /// <returns>A redirect back to the Retrieval page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        #pragma warning disable S107 // Method has many parameters due to the nature of the search form, acceptable in this context
        public ActionResult Search(RetrievalSearchMode mode, string[] documentTypeNames, string customQueryName,
            DateTime? startDate, DateTime? endDate, string[] keywordNames, string[] keywordValues, string documentIdInput)
        #pragma warning restore S107
        {
            var model = GetModel();
            model.SearchResults = [];
            model.Detail = new RetrievalDetailModel();
            model.LastSearchMode = mode;

            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                if (app == null)
                {
                    SessionLog.Error("Cannot search: not connected.");
                    SaveModel(model);
                    return RedirectToAction("Index");
                }

                if (mode == RetrievalSearchMode.DocumentId)
                {
                    if (!long.TryParse(documentIdInput, out var id))
                    {
                        SessionLog.Error($"[{documentIdInput}] is not a valid Document ID.");
                        SaveModel(model);
                        return RedirectToAction("Index");
                    }

                    SaveModel(model);
                    return LoadDetailAndRedirect(id);
                }

                var retrieval = new DocumentRetrieval(app, new Metadata(app));

                var request = new RetrievalRequest();
                request.DateRange.StartDate = startDate ?? DateTime.MinValue;
                request.DateRange.EndDate = endDate ?? DateTime.Now;

                if (mode == RetrievalSearchMode.CustomQuery) request.CustomQuery = customQueryName;
                else request.DocumentTypes = [.. (documentTypeNames ?? [])];

                if (keywordNames != null)
                {
                    for (int i = 0; i < keywordNames.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(keywordValues?[i])) continue;
                        request.Keywords.Add(new KeywordInfo { Name = keywordNames[i], Value = keywordValues[i] });
                    }
                }

                var results = retrieval.GetDocumentInfo(request, app) ?? [];
                model.SearchResults = results;
                SaveModel(model);

                SessionLog.Success($"Found {results.Count} document(s).");
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
                SaveModel(model);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// AJAX: loads a document's full detail (metadata, links, and its revision/rendition
        /// tree), defaulting to its first revision's first rendition.
        /// </summary>
        /// <param name="handle">The Document ID to load.</param>
        /// <returns>The rendered detail pane partial view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoadDetail(long handle)
        {
            LoadDetailInternal(handle);
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
        public ActionResult SelectRevision(long revisionId)
        {
            var model = GetModel();
            if (model.Detail != null)
            {
                model.Detail.SelectedRevisionId = revisionId;
                model.Detail.SelectedRenditionFileTypeId = model.Detail.SelectedRevision?.Renditions.FirstOrDefault()?.FileTypeId ?? 0;
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
        public ActionResult SelectRendition(long fileTypeId)
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
        /// <param name="preferPdf">Whether to retrieve converted to PDF, where supported.</param>
        /// <returns>The file content, or a redirect back if nothing is selected.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RetrieveFile(bool preferPdf)
        {
            var model = GetModel();
            var detail = model.Detail;

            if (detail?.SelectedRevision == null || detail.SelectedRendition == null)
            {
                SessionLog.Error("Cannot retrieve file: no revision/rendition selected.");
                return RedirectToAction("Index");
            }

            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                var retrieval = new DocumentRetrieval(app, new Metadata(app));

                var doc = retrieval.GetDocumentById(detail.Metadata.Handle, app);
                var file = retrieval.GetDocumentFile(doc, detail.SelectedRevisionId, detail.SelectedRenditionFileTypeId, preferPdf, app);

                var extension = preferPdf && DocumentRetrieval.IsPdfConvertible(detail.SelectedRenditionFileTypeId) ? "pdf" : detail.SelectedRendition.FileExtension;
                var fileName = $"{detail.Metadata.Name}.{extension}";

                SessionLog.Success($"Retrieved file [{fileName}].");
                return File(file.Content, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
                return RedirectToAction("Index");
            }
        }
        #endregion

        #region Private Methods
        // Read the cached page model from Session, or a fresh one
        private RetrievalPageModel GetModel()
        {
            return Session[SessionKey] as RetrievalPageModel ?? new RetrievalPageModel();
        }

        // Save the page model to Session
        private void SaveModel(RetrievalPageModel model)
        {
            Session[SessionKey] = model;
        }

        // Load a document's detail into the cached model (metadata, links, revisions,
        // defaulting to the first revision's first rendition)
        private void LoadDetailInternal(long handle)
        {
            var model = GetModel();
            model.Detail = new RetrievalDetailModel();

            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                if (app == null)
                {
                    SessionLog.Error("Cannot load document: not connected.");
                    SaveModel(model);
                    return;
                }

                var retrieval = new DocumentRetrieval(app, new Metadata(app));
                var doc = retrieval.GetDocumentById(handle, app);

                if (doc == null)
                {
                    SessionLog.Error($"No document found with ID [{handle}].");
                    SaveModel(model);
                    return;
                }

                var detail = new RetrievalDetailModel
                {
                    Metadata = retrieval.GetDocumentInfo(doc),
                    Links = retrieval.GetDocumentLink(doc),
                    Revisions = retrieval.GetDocumentRevisions(doc)
                };

                var firstRevision = detail.Revisions.FirstOrDefault();
                detail.SelectedRevisionId = firstRevision?.Id ?? 0;
                detail.SelectedRenditionFileTypeId = firstRevision?.Renditions.FirstOrDefault()?.FileTypeId ?? 0;

                model.Detail = detail;
                SaveModel(model);

                SessionLog.Success($"Loaded document [{handle}]: {detail.Revisions.Count} revision(s).");
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
                SaveModel(model);
            }
        }

        // Load a document's detail (Document ID search mode) then redirect to Index
        private ActionResult LoadDetailAndRedirect(long handle)
        {
            LoadDetailInternal(handle);
            return RedirectToAction("Index");
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
