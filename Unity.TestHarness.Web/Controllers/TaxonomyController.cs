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
using System.Linq;
using System.Web.Mvc;
using Hyland.Unity;
using Unity._02.AccessingTaxonomy.HelperClasses.OnBase;
using Unity.TestHarness.Web.Infrastructure;
using Unity.TestHarness.Web.Models;
#endregion

namespace Unity.TestHarness.Web.Controllers
{
    #region Training Notes
    /*
     * *Migration Note: web equivalent of Unity.TestHarness (the WPF version)'s Taxonomy
     * page (TaxonomyViewModel + TaxonomyView.xaml). Unlike Connect, this page's cascading
     * selection (Group -> Document Types -> Keyword Groups/Standalone -> a group's own
     * Keyword Types) genuinely needs AJAX, not full page reloads, a reload per click
     * through a multi-level hierarchy would feel nothing like the WPF version's
     * instant-feeling cascading combo boxes. Index()/LoadTaxonomy() stay plain
     * POST-then-redirect (LoadTaxonomy connects, a real side effect, and both actions
     * change what the WHOLE page shows), but everything below Document Type Group is
     * fetched via JSON endpoints as the user drills in, matching
     * LoadDocumentTypes/LoadKeywordGroupTypesAndStandalone/LoadGroupKeywordTypes's own
     * on-demand loading in the WPF version (each level is fetched only once its parent is
     * actually selected, not all eagerly upfront).
     *
     * The loaded Document Type Groups/Custom Queries are cached in Session (not
     * re-fetched on every Index() visit), since Load is a deliberate, explicit action
     * here exactly like in the WPF version, not something that happens automatically
     * setting up on page navigation.
     *
     * All lookups (GetDocumentType, GetKeywordGroupType, etc.) use the string name/ID
     * overloads throughout, matching OnBaseTaxonomy's own name-or-ID convention, so the
     * client only ever needs to pass a name back for a subsequent lookup, not track IDs
     * separately.
     */
    #endregion

    /// <summary>
    /// The Taxonomy page: browses Document Type Groups/Types/Keyword Groups/Keyword
    /// Types hierarchically, plus flat Custom Query/File Type/Unity Form lookups.
    /// </summary>
    public class TaxonomyController : Controller
    {
        #region Private Members
        // Session key for the Taxonomy page model, which is cached in Session after LoadTaxonomy().
        private const string SessionKey = "TestHarness.TaxonomyPageModel";
        #endregion

        #region Public Methods
        /// <summary>
        /// Displays the Taxonomy page, from whatever was last loaded this session (or an
        /// empty state if nothing has been loaded yet).
        /// </summary>
        /// <returns>The Taxonomy page.</returns>
        public ActionResult Index()
        {
            var model = Session[SessionKey] as TaxonomyPageModel ?? new TaxonomyPageModel();
            return View(model);
        }

        /// <summary>
        /// Loads Document Type Groups and Custom Queries, connecting first (using
        /// whatever's currently configured) if not already connected.
        /// </summary>
        /// <returns>A redirect back to the Taxonomy page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoadTaxonomy()
        {
            if (!SessionConnectionManager.IsConnected())
            {
                SessionConnectionManager.Connect();
                if (!SessionConnectionManager.IsConnected())
                {
                    SessionLog.Error("Cannot load taxonomy: connect attempt failed, see the error above.");
                    return RedirectToAction("Index");
                }
            }

            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                var taxonomy = new OnBaseTaxonomy(app);

                var groups = taxonomy.GetDocumentTypeGroups(app: app) ?? [];
                var queries = taxonomy.GetCustomQueries(app: app) ?? [];

                var model = new TaxonomyPageModel
                {
                    IsLoaded = true,
                    DocumentTypeGroups = [.. groups.Select(g => new NamedItem { Id = g.ID, Name = g.Name })],
                    CustomQueries = [.. queries.Select(q => new NamedItem { Id = q.ID, Name = q.Name })]
                };

                Session[SessionKey] = model;

                SessionLog.Success($"Loaded {model.DocumentTypeGroups.Count} document type group(s), {model.CustomQueries.Count} custom quer{(model.CustomQueries.Count == 1 ? "y" : "ies")}.");
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// AJAX: the Document Types belonging to the named Document Type Group.
        /// </summary>
        /// <param name="groupName">The Document Type Group's name.</param>
        /// <returns>A JSON list of <see cref="NamedItem"/>.</returns>
        [HttpGet]
        public JsonResult GetDocumentTypes(string groupName)
        {
            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                var taxonomy = new OnBaseTaxonomy(app);

                var docTypes = taxonomy.GetDocumentTypes(groupName, app) ?? [];
                var result = docTypes.Select(d => new NamedItem { Id = d.ID, Name = d.Name }).ToList();

                SessionLog.Success($"Loaded {result.Count} document type(s) in group [{groupName}].");
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
                return Json(new System.Collections.Generic.List<NamedItem>(), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// AJAX: the named Document Type's Keyword Group Types and standalone Keyword Types.
        /// </summary>
        /// <param name="docTypeName">The Document Type's name.</param>
        /// <returns>A JSON <see cref="KeywordGroupsAndStandaloneResult"/>.</returns>
        [HttpGet]
        public JsonResult GetKeywordGroupsAndStandalone(string docTypeName)
        {
            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                var taxonomy = new OnBaseTaxonomy(app);

                var docType = taxonomy.GetDocumentType(docTypeName, app);
                if (docType == null)
                {
                    SessionLog.Error($"Document type [{docTypeName}] not found.");
                    return Json(new KeywordGroupsAndStandaloneResult(), JsonRequestBehavior.AllowGet);
                }

                var (groups, standalone) = taxonomy.SplitKeywordGroups(docType);

                var result = new KeywordGroupsAndStandaloneResult
                {
                    Groups = [.. groups.Select(g => new KeywordGroupItem { Id = g.ID, Name = g.Name, MultiInstance = g.RecordType == RecordType.MultiInstance })],
                    Standalone = [.. standalone.Select(k => new KeywordTypeItem { Id = k.ID, Name = k.Name, DataType = k.DataType.ToString(), Length = k.DataLength })]
                };

                SessionLog.Success($"Loaded {result.Groups.Count} keyword group(s), {result.Standalone.Count} standalone keyword(s) on document type [{docType.Name}].");
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
                return Json(new KeywordGroupsAndStandaloneResult(), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// AJAX: the named Keyword Group Type's own Keyword Types, on the named Document Type.
        /// </summary>
        /// <param name="docTypeName">The Document Type's name.</param>
        /// <param name="groupName">The Keyword Group Type's name.</param>
        /// <returns>A JSON list of <see cref="KeywordTypeItem"/>.</returns>
        [HttpGet]
        public JsonResult GetGroupKeywordTypes(string docTypeName, string groupName)
        {
            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                var taxonomy = new OnBaseTaxonomy(app);

                var docType = taxonomy.GetDocumentType(docTypeName, app);
                var group = docType != null ? taxonomy.GetKeywordGroupType(groupName, docType) : null;

                if (group == null)
                {
                    SessionLog.Error($"Keyword group [{groupName}] not found on document type [{docTypeName}].");
                    return Json(new System.Collections.Generic.List<KeywordTypeItem>(), JsonRequestBehavior.AllowGet);
                }

                var result = group.KeywordTypes.Select(k => new KeywordTypeItem { Id = k.ID, Name = k.Name, DataType = k.DataType.ToString(), Length = k.DataLength }).ToList();

                SessionLog.Success($"Loaded {result.Count} keyword type(s) in group [{group.Name}].");
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
                return Json(new System.Collections.Generic.List<KeywordTypeItem>(), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Looks up a File Type by extension or numeric ID.
        /// </summary>
        /// <param name="input">The extension or numeric ID to search for.</param>
        /// <returns>A JSON <see cref="FileTypeLookupResult"/>.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult FindFileType(string input)
        {
            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                var taxonomy = new OnBaseTaxonomy(app);

                var fileType = long.TryParse(input, out var id) ? taxonomy.GetFileType(id, app) : taxonomy.GetFileType(input, app);

                var result = fileType != null
                    ? new FileTypeLookupResult { Found = true, Id = fileType.ID, Name = fileType.Name }
                    : new FileTypeLookupResult { Found = false };

                SessionLog.Success(result.Found ? $"Found file type [{result.Name}] (ID {result.Id})." : $"No file type found for [{input}].");
                return Json(result);
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
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
        public JsonResult FindUnityForm(string input)
        {
            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                var taxonomy = new OnBaseTaxonomy(app);

                var form = taxonomy.GetUnityForm(input, app);

                var result = form != null
                    ? new UnityFormLookupResult { Found = true, Id = form.ID, Name = form.Name }
                    : new UnityFormLookupResult { Found = false };

                SessionLog.Success(result.Found ? $"Found unity form [{result.Name}] (ID {result.Id})." : $"No unity form found for [{input}].");
                return Json(result);
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
                return Json(new UnityFormLookupResult { Found = false });
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
