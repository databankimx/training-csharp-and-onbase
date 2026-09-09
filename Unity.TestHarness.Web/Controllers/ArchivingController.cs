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
using System.Web;
using System.Web.Mvc;
using Hyland.Unity;
using Unity._02.AccessingTaxonomy.HelperClasses.OnBase;
using Unity._03.DocumentRetrieval.HelperClasses.OnBase;
using Unity._03.DocumentRetrieval.Models.Objects;
using Unity._04.DocumentArchiving.HelperClasses.OnBase;
using Unity._04.DocumentArchiving.Models.Enumerations;
using Unity._04.DocumentArchiving.Models.Objects;
using Unity.TestHarness.Web.Infrastructure;
using Unity.TestHarness.Web.Models;
#endregion

#pragma warning disable S1192 // In a training project, keep literals
namespace Unity.TestHarness.Web.Controllers
{
    #region Training Notes
    /*
     * *Migration Note: web equivalent of Unity.TestHarness (the WPF version)'s Archiving
     * page (ArchivingViewModel + its Views). Mode switching is a plain POST-then-redirect
     * (SetMode), unlike Retrieval's client-side mode toggle, each of Archiving's five
     * modes has substantially more distinct markup (its own keyword editor, its own file
     * upload, its own "load an existing document" flow) than Retrieval's three simple
     * search-criteria panels, keeping all five in the DOM at once with JS show/hide would
     * be a lot of duplicated complexity for little benefit; a full page reload per mode
     * switch is a perfectly normal pattern here.
     *
     * File uploads (StoreNew/AddRevision/AddRendition) are genuinely different from the
     * WPF version: WPF has direct filesystem access, so NewDocumentRequest.Files/
     * UpdateDocumentRequest.Files just take local paths a file dialog already provided.
     * A web request instead receives HttpPostedFileBase uploads, which SaveUploadedFiles
     * writes to a temp directory first (the library methods still expect local paths, not
     * upload streams), then those temp paths are passed to Files, and CleanUpTempFiles
     * removes them afterward regardless of success or failure.
     *
     * The dynamic keyword editor (Add/Remove Instance/Value) submits flat form fields
     * using a naming convention (kw_group_{groupId}_{instanceKey}_{fieldId},
     * kw_standalone_{keywordId}_{valueKey}), generated client-side as JS adds/removes
     * rows, and ParseKeywordsFromForm reconstructs the KeywordGroup/KeywordInfo lists the
     * library expects from those flat fields, since the shape (how many instances, how
     * many values) is only known at submit time, not something a strongly-typed model
     * binder can express.
     *
     * DocumentStorage's Create/Modify/DeleteDocument are called directly (not wrapped in
     * Task.Run, unlike the WPF version), a classic ASP.NET MVC action method has no UI
     * thread to keep responsive the way the WPF version's Task.Run avoided blocking one,
     * each web request already runs on its own thread pool thread.
     */
    #endregion

    /// <summary>
    /// The Archiving page: Store New / Modify Metadata / Add Revision / Add Rendition / Delete.
    /// </summary>
    public class ArchivingController : Controller
    {
        #region Private Members
        // Session key for the cached page model
        private const string SessionKey = "TestHarness.ArchivingPageModel";
        #endregion

        #region Public Methods
        /// <summary>
        /// Displays the Archiving page, from whatever was last loaded this session.
        /// </summary>
        /// <returns>The Archiving page.</returns>
        public ActionResult Index()
        {
            return View(GetModel());
        }

        /// <summary>
        /// Switches the active mode.
        /// </summary>
        /// <param name="mode">The mode to switch to.</param>
        /// <returns>A redirect back to the Archiving page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetMode(ArchivingMode mode)
        {
            var model = GetModel();
            model.Mode = mode;
            SaveModel(model);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Loads Document Type Groups and Document Types for Store New, connecting first
        /// (using whatever's currently configured) if not already connected.
        /// </summary>
        /// <returns>A redirect back to the Archiving page.</returns>
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

                var model = GetModel();
                model.IsTaxonomyLoaded = true;
                model.DocumentTypeGroups = [.. groups.Select(g => new NamedItem { Id = g.ID, Name = g.Name })];
                model.AllDocumentTypes = [.. docTypes.Select(d => new NamedItem { Id = d.ID, Name = d.Name })];
                SaveModel(model);

                SessionLog.Success($"Loaded {model.AllDocumentTypes.Count} document type(s), {model.DocumentTypeGroups.Count} group(s).");
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Selects a Document Type for Store New, building its keyword editor schema.
        /// </summary>
        /// <param name="documentTypeName">The Document Type's name.</param>
        /// <returns>A redirect back to the Archiving page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelectDocumentType(string documentTypeName)
        {
            var model = GetModel();

            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                var taxonomy = new OnBaseTaxonomy(app);

                var docType = taxonomy.GetDocumentType(documentTypeName, app);
                if (docType == null)
                {
                    SessionLog.Error($"Document type [{documentTypeName}] not found.");
                    return RedirectToAction("Index");
                }

                model.SelectedDocumentTypeName = docType.Name;
                model.NewEditorSchema = BuildSchemaForDocType(docType, taxonomy);
                SaveModel(model);

                SessionLog.Success($"Loaded keyword editor for document type [{docType.Name}].");
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Stores a new document.
        /// </summary>
        /// <param name="documentDate">The new document's document date.</param>
        /// <param name="files">The file(s) to store.</param>
        /// <returns>A redirect back to the Archiving page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StoreNew(DateTime documentDate, IEnumerable<HttpPostedFileBase> files)
        {
            var model = GetModel();
            var tempFiles = new List<string>();

            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                if (app == null)
                {
                    SessionLog.Error("Cannot store: not connected.");
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrEmpty(model.SelectedDocumentTypeName))
                {
                    SessionLog.Error("Cannot store: no document type selected.");
                    return RedirectToAction("Index");
                }

                tempFiles = SaveUploadedFiles(files);
                if (tempFiles.Count == 0)
                {
                    SessionLog.Error("Cannot store: no file(s) attached.");
                    return RedirectToAction("Index");
                }

                var (groups, keywords) = ParseKeywordsFromForm(Request.Form, model.NewEditorSchema);

                var storage = new DocumentStorage(app);
                var request = new NewDocumentRequest(StorageType.Document)
                {
                    DocumentType = model.SelectedDocumentTypeName,
                    DocumentDate = documentDate,
                    Files = tempFiles,
                    KeywordGroups = groups,
                    Keywords = keywords
                };

                var doc = storage.CreateDocument(request, app);

                SessionLog.Success($"Stored new document [{doc.ID}].");
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
            }
            finally
            {
                CleanUpTempFiles(tempFiles);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Loads an existing document for Modify Metadata/Add Revision/Add Rendition/Delete.
        /// </summary>
        /// <param name="documentIdInput">The Document ID to load.</param>
        /// <returns>A redirect back to the Archiving page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoadDocument(string documentIdInput)
        {
            var model = GetModel();
            model.DocumentIdInput = documentIdInput;
            model.HasLoadedDocument = false;

            if (!SessionConnectionManager.IsConnected())
            {
                SessionConnectionManager.Connect();
                if (!SessionConnectionManager.IsConnected())
                {
                    SessionLog.Error("Cannot load: connect attempt failed, see the error above.");
                    SaveModel(model);
                    return RedirectToAction("Index");
                }
            }

            if (!long.TryParse(documentIdInput, out var id))
            {
                SessionLog.Error($"[{documentIdInput}] is not a valid Document ID.");
                SaveModel(model);
                return RedirectToAction("Index");
            }

            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                var retrieval = new DocumentRetrieval(app, new Metadata(app));
                var storage = new DocumentStorage(app);
                var taxonomy = new OnBaseTaxonomy(app);

                var doc = retrieval.GetDocumentById(id, app);
                if (doc == null)
                {
                    SessionLog.Error($"No document found with ID [{id}].");
                    SaveModel(model);
                    return RedirectToAction("Index");
                }

                var metadata = retrieval.GetDocumentInfo(doc);

                model.HasLoadedDocument = true;
                model.LoadedDocumentId = doc.ID;
                model.LoadedDocumentTypeName = doc.DocumentType.Name;
                model.LoadedDocumentDate = doc.DocumentDate;
                model.IsRevisable = storage.CanAddRevision(doc);
                model.IsRenditionable = storage.CanAddRendition(doc);
                model.ExistingEditorSchema = BuildSchemaForDocument(doc, metadata, taxonomy);
                SaveModel(model);

                SessionLog.Success($"Loaded document [{id}] for editing. Revisable: {model.IsRevisable}, Renditionable: {model.IsRenditionable}.");
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
                SaveModel(model);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Applies metadata changes to the loaded document.
        /// </summary>
        /// <param name="documentTypeName">The (possibly changed) document type name.</param>
        /// <param name="documentDate">The (possibly changed) document date.</param>
        /// <returns>A redirect back to the Archiving page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ModifyMetadata(string documentTypeName, DateTime documentDate)
        {
            var model = GetModel();

            if (!model.HasLoadedDocument)
            {
                SessionLog.Error("Cannot modify: no document loaded.");
                return RedirectToAction("Index");
            }

            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                var storage = new DocumentStorage(app);

                var (groups, keywords) = ParseKeywordsFromForm(Request.Form, model.ExistingEditorSchema);

                var request = new UpdateDocumentRequest(UpdateType.Metadata, StorageType.Document)
                {
                    DocumentId = model.LoadedDocumentId,
                    DocumentType = documentTypeName,
                    DocumentDate = documentDate,
                    OverwriteKeywords = true,
                    KeywordGroups = groups,
                    Keywords = keywords
                };

                storage.ModifyDocument(request, app);

                SessionLog.Success($"Updated metadata on document [{model.LoadedDocumentId}].");
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Adds a new revision to the loaded document.
        /// </summary>
        /// <param name="files">The file(s) to store as the new revision.</param>
        /// <returns>A redirect back to the Archiving page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRevision(IEnumerable<HttpPostedFileBase> files)
        {
            var model = GetModel();
            var tempFiles = new List<string>();

            if (!model.HasLoadedDocument || !model.IsRevisable)
            {
                SessionLog.Error("Cannot add revision: no document loaded, or this document type does not allow revisions.");
                return RedirectToAction("Index");
            }

            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                var storage = new DocumentStorage(app);

                tempFiles = SaveUploadedFiles(files);
                if (tempFiles.Count == 0)
                {
                    SessionLog.Error("Cannot add revision: no file(s) attached.");
                    return RedirectToAction("Index");
                }

                var request = new UpdateDocumentRequest(UpdateType.Revision, StorageType.Document)
                {
                    DocumentId = model.LoadedDocumentId,
                    DocumentType = model.LoadedDocumentTypeName,
                    Files = tempFiles
                };

                storage.ModifyDocument(request, app);

                SessionLog.Success($"Added a new revision to document [{model.LoadedDocumentId}].");
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
            }
            finally
            {
                CleanUpTempFiles(tempFiles);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Adds a new rendition to the loaded document's latest revision.
        /// </summary>
        /// <param name="files">The file(s) to store as the new rendition.</param>
        /// <returns>A redirect back to the Archiving page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRendition(IEnumerable<HttpPostedFileBase> files)
        {
            var model = GetModel();
            var tempFiles = new List<string>();

            if (!model.HasLoadedDocument || !model.IsRenditionable)
            {
                SessionLog.Error("Cannot add rendition: no document loaded, or this document type does not allow renditions.");
                return RedirectToAction("Index");
            }

            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                var storage = new DocumentStorage(app);

                tempFiles = SaveUploadedFiles(files);
                if (tempFiles.Count == 0)
                {
                    SessionLog.Error("Cannot add rendition: no file(s) attached.");
                    return RedirectToAction("Index");
                }

                var request = new UpdateDocumentRequest(UpdateType.Rendition, StorageType.Document)
                {
                    DocumentId = model.LoadedDocumentId,
                    DocumentType = model.LoadedDocumentTypeName,
                    Files = tempFiles
                };

                storage.ModifyDocument(request, app);

                SessionLog.Success($"Added a new rendition to document [{model.LoadedDocumentId}]'s latest revision.");
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
            }
            finally
            {
                CleanUpTempFiles(tempFiles);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Deletes (or, if requested, permanently purges) the loaded document.
        /// </summary>
        /// <param name="purgeDocument">Whether to permanently purge rather than a regular, recoverable delete.</param>
        /// <returns>A redirect back to the Archiving page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(bool purgeDocument)
        {
            var model = GetModel();

            if (!model.HasLoadedDocument)
            {
                SessionLog.Error("Cannot delete: no document loaded.");
                return RedirectToAction("Index");
            }

            try
            {
                var app = SessionConnectionManager.GetCurrentApplication();
                var storage = new DocumentStorage(app);

                var documentId = model.LoadedDocumentId;
                var request = new DeleteRequest { DocumentId = documentId, PurgeDocument = purgeDocument };

                storage.DeleteDocument(request, app);

                SessionLog.Success($"Document [{documentId}] {(purgeDocument ? "purged" : "deleted")}.");

                model.HasLoadedDocument = false;
                model.DocumentIdInput = null;
                model.ExistingEditorSchema = new KeywordEditorSchema();
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
            }

            SaveModel(model);
            return RedirectToAction("Index");
        }
        #endregion

        #region Private Methods
        // Read the cached page model from Session, or a fresh one
        private ArchivingPageModel GetModel()
        {
            return Session[SessionKey] as ArchivingPageModel ?? new ArchivingPageModel();
        }

        // Save the page model to Session
        private void SaveModel(ArchivingPageModel model)
        {
            Session[SessionKey] = model;
        }

        // Build a keyword editor schema from a Document Type, with no existing values
        // (Store New)
        private static KeywordEditorSchema BuildSchemaForDocType(DocumentType docType, OnBaseTaxonomy taxonomy)
        {
            var (groups, standalone) = taxonomy.SplitKeywordGroups(docType);
            var schema = new KeywordEditorSchema();

            foreach (var group in groups)
            {
                schema.Groups.Add(new KeywordGroupSchema
                {
                    Id = group.ID,
                    Name = group.Name,
                    MultiInstance = group.RecordType == RecordType.MultiInstance,
                    FieldDefinitions = [.. group.KeywordTypes.Select(k => new KeywordFieldSchema { Id = k.ID, Name = k.Name })]
                });
            }

            foreach (var keywordType in standalone)
            {
                schema.Standalone.Add(new StandaloneKeywordSchema { Id = keywordType.ID, Name = keywordType.Name });
            }

            return schema;
        }

        // Build a keyword editor schema from an existing document, pre-populated with its
        // current values (Modify Metadata)
        private static KeywordEditorSchema BuildSchemaForDocument(Document doc, DocumentInfo metadata, OnBaseTaxonomy taxonomy)
        {
            var schema = BuildSchemaForDocType(doc.DocumentType, taxonomy);

            foreach (var standaloneSchema in schema.Standalone)
            {
                var values = metadata.Keywords.Where(k => k.Id == standaloneSchema.Id).Select(k => k.Value).ToList();
                if (values.Count > 0) standaloneSchema.Values = values;
            }

            foreach (var groupSchema in schema.Groups)
            {
                foreach (var group in metadata.KeywordGroups.Where(g => g.Id == groupSchema.Id))
                {
                    groupSchema.Instances.Add(group.Keywords.ToDictionary(k => k.Id, k => k.Value));
                }
            }

            return schema;
        }

        // Parse posted kw_group_{groupId}_{instanceKey}_{fieldId}/kw_standalone_{keywordId}_{valueKey}
        // fields back into the KeywordGroup/KeywordInfo lists the library expects
        #pragma warning disable S3776 // Not overly complex
        private static (List<KeywordGroup> Groups, List<KeywordInfo> Keywords) ParseKeywordsFromForm(System.Collections.Specialized.NameValueCollection form, KeywordEditorSchema schema)
        #pragma warning restore S3776
        {
            var groupInstances = new Dictionary<string, Dictionary<long, string>>();
            var standaloneValues = new Dictionary<long, List<string>>();

            foreach (var key in form.AllKeys)
            {
                if (key == null) continue;
                var value = form[key];
                if (string.IsNullOrWhiteSpace(value)) continue;

                if (key.StartsWith("kw_group_", StringComparison.Ordinal))
                {
                    var parts = key.Substring("kw_group_".Length).Split('_');
                    if (parts.Length != 3) continue;
                    if (!long.TryParse(parts[2], out var fieldId)) continue;

                    var instanceMapKey = parts[0] + "_" + parts[1];
                    if (!groupInstances.TryGetValue(instanceMapKey, out var instanceDict))
                    {
                        instanceDict = [];
                        groupInstances[instanceMapKey] = instanceDict;
                    }
                    instanceDict[fieldId] = value;
                }
                else if (key.StartsWith("kw_standalone_", StringComparison.Ordinal))
                {
                    var parts = key.Substring("kw_standalone_".Length).Split('_');
                    if (parts.Length != 2) continue;
                    if (!long.TryParse(parts[0], out var keywordId)) continue;

                    if (!standaloneValues.TryGetValue(keywordId, out var values))
                    {
                        values = [];
                        standaloneValues[keywordId] = values;
                    }
                    values.Add(value);
                }
            }

            var groups = new List<KeywordGroup>();
            foreach (var kvp in groupInstances)
            {
                var groupId = long.Parse(kvp.Key.Split('_')[0]);
                var groupSchema = schema.Groups.FirstOrDefault(g => g.Id == groupId);
                if (groupSchema == null) continue;

                groups.Add(new KeywordGroup
                {
                    Id = groupId,
                    Name = groupSchema.Name,
                    MultiInstance = groupSchema.MultiInstance,
                    Keywords = [.. kvp.Value.Select(f => new KeywordInfo
                    {
                        Id = f.Key,
                        Name = groupSchema.FieldDefinitions.FirstOrDefault(fd => fd.Id == f.Key)?.Name,
                        Value = f.Value
                    })]
                });
            }

            var keywords = new List<KeywordInfo>();
            foreach (var kvp in standaloneValues)
            {
                var standaloneSchema = schema.Standalone.FirstOrDefault(s => s.Id == kvp.Key);
                if (standaloneSchema == null) continue;

                keywords.AddRange(kvp.Value.Select(value => new KeywordInfo { Id = kvp.Key, Name = standaloneSchema.Name, Value = value }));
            }

            return (groups, keywords);
        }

        // Save uploaded files to a temp directory, returning their paths (the library's
        // Files property expects local paths, not upload streams)
        private static List<string> SaveUploadedFiles(IEnumerable<HttpPostedFileBase> files)
        {
            var paths = new List<string>();
            if (files == null) return paths;

            foreach (var file in files)
            {
                if (file == null || file.ContentLength == 0) continue;

                var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "_" + Path.GetFileName(file.FileName));
                file.SaveAs(tempPath);
                paths.Add(tempPath);
            }

            return paths;
        }

        // Delete temp files created by SaveUploadedFiles
        private static void CleanUpTempFiles(List<string> paths)
        {
            foreach (var path in paths)
            {
                try
                {
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }
                catch (Exception ex)
                {
                    SessionLog.Error(ex);
                }
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
