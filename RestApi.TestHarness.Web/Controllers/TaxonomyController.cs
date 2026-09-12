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
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestApi._02.AccessingTaxonomy.HelperClasses.OnBase;
using RestApi.TestHarness.Web.Infrastructure;
using RestApi.TestHarness.Web.Models;
#endregion

namespace RestApi.TestHarness.Web.Controllers
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness.Web's own
     * TaxonomyController. Same overall shape (Index/LoadTaxonomy stay plain
     * POST-then-redirect, everything below Document Type Group is AJAX), but:
     *
     * - No GetGroupKeywordTypes endpoint at all: eliminated, see TaxonomyModels.cs's own
     *   Training Notes, RestApi.02's own GetDocumentTypeKeywordGroupsAsync already
     *   resolves every group's KeywordTypes up front, so GetKeywordGroupsAndStandalone
     *   includes them directly, no follow-up round trip needed.
     *
     * - TaxonomyPageModel is cached in ASP.NET Core's own Session as JSON (via
     *   Session.SetString/GetString), not a live object reference the way classic
     *   ASP.NET's Session[key] = model works: this data (a Document Type Group/Custom
     *   Query name/ID list) is plain serializable data, same situation as
     *   SessionLogService's own LogEntry list, no separate server-memory store needed
     *   for it the way SessionManagement itself requires.
     *
     * - Every AJAX/lookup action here uses store.TryGet(...), not GetOrCreate: these all
     *   require an ALREADY-connected session (LoadTaxonomy is what actually connects),
     *   so there's no reason to fabricate a new, disconnected SessionManagement just to
     *   immediately fail a null-HttpClient check, see SessionManagementStore's own
     *   Training Notes for the same reasoning applied to the shared layout's connection
     *   indicator.
     */
    #endregion

    /// <summary>
    /// The Taxonomy page: browses Document Type Groups/Types/Keyword Groups/Keyword
    /// Types hierarchically, plus flat Custom Query/File Type/Unity Form lookups.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the TaxonomyController class
    /// </remarks>
    /// <param name="store">The per-session connection store.</param>
    /// <param name="log">The shared, per-session output log.</param>
    public class TaxonomyController(SessionManagementStore store, SessionLogService log) : Controller
    {
        #region Private Members
        // Session key for the Taxonomy page model, which is cached in Session (as JSON)
        // after LoadTaxonomy()
        private const string SessionKey = "TestHarness.TaxonomyPageModel";
        #endregion

        #region Public Methods
        /// <summary>
        /// Displays the Taxonomy page, from whatever was last loaded this session (or an
        /// empty state if nothing has been loaded yet).
        /// </summary>
        /// <returns>The Taxonomy page.</returns>
        public IActionResult Index()
        {
            var model = LoadCachedModel() ?? new TaxonomyPageModel();
            return View(model);
        }

        /// <summary>
        /// Loads Document Type Groups and Custom Queries, connecting first (using
        /// whatever's currently configured) if not already connected.
        /// </summary>
        /// <param name="username">The OnBase username to connect with, if not already connected.</param>
        /// <param name="password">The OnBase password to connect with, if not already connected.</param>
        /// <returns>A redirect back to the Taxonomy page.</returns>
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
                var queries = await OnBaseTaxonomy.GetCustomQueriesAsync(client: http) ?? [];

                var model = new TaxonomyPageModel
                {
                    IsLoaded = true,
                    DocumentTypeGroups = [.. groups.Select(g => new NamedItem { Id = g.Id, Name = g.Name })],
                    CustomQueries = [.. queries.Select(q => new NamedItem { Id = q.Id, Name = q.Name })]
                };

                SaveCachedModel(model);

                log.Success($"Loaded {model.DocumentTypeGroups.Count} document type group(s), {model.CustomQueries.Count} custom quer{(model.CustomQueries.Count == 1 ? "y" : "ies")}.");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// AJAX: the Document Types belonging to the given Document Type Group.
        /// </summary>
        /// <param name="groupId">The Document Type Group's ID.</param>
        /// <returns>A JSON list of <see cref="NamedItem"/>.</returns>
        [HttpGet]
        public async Task<IActionResult> GetDocumentTypes(string groupId)
        {
            try
            {
                var http = store.TryGet(HttpContext.Session.Id)?.GetHttpClient();
                if (http == null) return Json(new List<NamedItem>());

                var docTypes = await OnBaseTaxonomy.GetDocumentTypesForGroupAsync(groupId, http) ?? [];
                var result = docTypes.Select(d => new NamedItem { Id = d.Id, Name = d.Name }).ToList();

                log.Success($"Loaded {result.Count} document type(s) in the selected group.");
                return Json(result);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return Json(new List<NamedItem>());
            }
        }

        /// <summary>
        /// AJAX: the given Document Type's Keyword Group Types (each with its own
        /// Keyword Types already resolved) and standalone Keyword Types.
        /// </summary>
        /// <param name="docTypeId">The Document Type's ID.</param>
        /// <returns>A JSON <see cref="KeywordGroupsAndStandaloneResult"/>.</returns>
        [HttpGet]
        public async Task<IActionResult> GetKeywordGroupsAndStandalone(string docTypeId)
        {
            try
            {
                var http = store.TryGet(HttpContext.Session.Id)?.GetHttpClient();
                if (http == null) return Json(new KeywordGroupsAndStandaloneResult());

                var allGroups = await OnBaseTaxonomy.GetDocumentTypeKeywordGroupsAsync(docTypeId, http);
                var (groups, standalone) = OnBaseTaxonomy.SplitKeywordGroups(allGroups);

                var result = new KeywordGroupsAndStandaloneResult
                {
                    Groups = [.. groups.Select(g => new KeywordGroupItem
                    {
                        Id = g.Id,
                        Name = g.Name,
                        MultiInstance = g.StorageType == "MultiInstance",
                        KeywordTypes = [.. g.KeywordTypes.Select(k => new KeywordTypeItem { Id = k.Id, Name = k.Name, DataType = k.DataType })]
                    })],
                    Standalone = [.. standalone.Select(k => new KeywordTypeItem { Id = k.Id, Name = k.Name, DataType = k.DataType })]
                };

                log.Success($"Loaded {result.Groups.Count} keyword group(s), {result.Standalone.Count} standalone keyword(s).");
                return Json(result);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return Json(new KeywordGroupsAndStandaloneResult());
            }
        }

        /// <summary>
        /// Looks up a File Type by extension, name, or numeric ID.
        /// </summary>
        /// <param name="input">The extension, name, or numeric ID to search for.</param>
        /// <returns>A JSON <see cref="FileTypeLookupResult"/>.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FindFileType(string input)
        {
            try
            {
                var http = store.TryGet(HttpContext.Session.Id)?.GetHttpClient();
                if (http == null) return Json(new FileTypeLookupResult { Found = false });

                var allFileTypes = await OnBaseTaxonomy.GetFileTypesAsync(http);
                var fileType = long.TryParse(input, out _)
                    ? allFileTypes.Find(f => f.Id == input)
                    : allFileTypes.Find(f => string.Equals(f.Name, input, StringComparison.OrdinalIgnoreCase) || string.Equals(f.SystemName, input, StringComparison.OrdinalIgnoreCase));

                var result = fileType != null
                    ? new FileTypeLookupResult { Found = true, Id = fileType.Id, Name = fileType.Name }
                    : new FileTypeLookupResult { Found = false };

                log.Success(result.Found ? $"Found file type [{result.Name}] (ID {result.Id})." : $"No file type found for [{input}].");
                return Json(result);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return Json(new FileTypeLookupResult { Found = false });
            }
        }

        /// <summary>
        /// Looks up a Unity Form template by name or numeric ID.
        /// </summary>
        /// <param name="input">The name or numeric ID to search for.</param>
        /// <returns>A JSON <see cref="UnityFormLookupResult"/>.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FindUnityForm(string input)
        {
            try
            {
                var session = store.TryGet(HttpContext.Session.Id);
                var formsHttp = session?.GetFormsHttpClient();
                if (formsHttp == null) return Json(new UnityFormLookupResult { Found = false });

                var form = await OnBaseTaxonomy.GetUnityFormTemplateAsync(input, formsHttp);

                var result = form != null
                    ? new UnityFormLookupResult { Found = true, Id = form.Id, Name = form.Name }
                    : new UnityFormLookupResult { Found = false };

                log.Success(result.Found ? $"Found unity form [{result.Name}] (ID {result.Id})." : $"No unity form found for [{input}].");
                return Json(result);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return Json(new UnityFormLookupResult { Found = false });
            }
        }
        #endregion

        #region Private Methods
        // Read the cached TaxonomyPageModel back from Session (as JSON), or null if nothing's cached yet
        private TaxonomyPageModel LoadCachedModel()
        {
            var json = HttpContext.Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json)) return null;

            try
            {
                return JsonSerializer.Deserialize<TaxonomyPageModel>(json);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        // Save the given TaxonomyPageModel to Session (as JSON)
        private void SaveCachedModel(TaxonomyPageModel model)
        {
            HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(model));
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
