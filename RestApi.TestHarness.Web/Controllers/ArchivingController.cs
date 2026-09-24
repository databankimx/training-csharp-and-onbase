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
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestApi._02.AccessingTaxonomy.HelperClasses.OnBase;
using RestApi._03.DocumentRetrieval.HelperClasses.OnBase;
using RestApi._03.DocumentRetrieval.Models.Objects;
using RestApi._04.DocumentArchiving.HelperClasses.OnBase;
using RestApi._04.DocumentArchiving.Models.Enumerations;
using RestApi._04.DocumentArchiving.Models.Objects;
using RestApi.TestHarness.Web.Infrastructure;
using RestApi.TestHarness.Web.Models;
#endregion

#pragma warning disable S1192 // In a training project, keep literals
namespace RestApi.TestHarness.Web.Controllers
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness.Web's own
     * ArchivingController. Same overall shape (SetMode is a plain POST-then-redirect,
     * five modes each get their own full page render rather than client-side show/hide,
     * matching each mode's own substantial, distinct markup), but:
     *
     * - IFormFile (ASP.NET Core's own upload type) replaces HttpPostedFileBase.
     *   SaveUploadedFiles uses CopyToAsync into a temp file (RestApi.04's own
     *   NewDocumentRequest/UpdateDocumentRequest.Files still just take local paths, see
     *   its own Training Notes for why, the REST API's own three-step staging process
     *   happens internally in DocumentStorage), CleanUpTempFiles removes them afterward
     *   regardless of success or failure, same as before.
     *
     * - ParseKeywordsFromForm builds RestApi.03's own KeywordGroup/KeywordInfo (reused
     *   directly by RestApi.04, not duplicated), with GroupId always left null: see
     *   ArchivingModels.cs's own Training Notes for why that's correct, not an omission.
     *
     * - No CanAddRevision/CanAddRendition calls at all (they don't exist), no
     *   IsRevisable/IsRenditionable page-model fields to set from them: a confirmed gap,
     *   Add Revision/Add Rendition are simply always offered once a document is loaded.
     *
     * - Delete has no purgeDocument parameter: a confirmed gap, RestApi.04's own
     *   DeleteRequest has no such field.
     *
     * - DocumentStorage's Create/Modify/DeleteDocumentAsync are genuinely async (real
     *   HTTP calls, including the multi-step upload staging), awaited directly here, not
     *   wrapped in anything special, a classic ASP.NET Core action method already runs
     *   off the UI thread the way RestApi.TestHarness (the WPF version)'s own Task.Run
     *   wrapping was needed to avoid blocking.
     */
    #endregion

    /// <summary>
    /// The Archiving page: Store New / Modify Metadata / Add Revision / Add Rendition / Delete.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the ArchivingController class
    /// </remarks>
    /// <param name="store">The per-session connection store.</param>
    /// <param name="log">The shared, per-session output log.</param>
    public class ArchivingController(SessionManagementStore store, SessionLogService log) : Controller
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
        public IActionResult Index()
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
        public IActionResult SetMode(ArchivingMode mode)
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
        /// <param name="username">The OnBase username to connect with, if not already connected.</param>
        /// <param name="password">The OnBase password to connect with, if not already connected.</param>
        /// <returns>A redirect back to the Archiving page.</returns>
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

                var model = GetModel();
                model.IsTaxonomyLoaded = true;
                model.DocumentTypeGroups = [.. groups.Select(g => new NamedItem { Id = g.Id, Name = g.Name })];
                model.AllDocumentTypes = [.. docTypes.Select(d => new NamedItem { Id = d.Id, Name = d.Name })];
                SaveModel(model);

                log.Success($"Loaded {model.AllDocumentTypes.Count} document type(s), {model.DocumentTypeGroups.Count} group(s).");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Selects a Document Type for Store New, building its keyword editor schema.
        /// </summary>
        /// <param name="documentTypeId">The Document Type's id.</param>
        /// <returns>A redirect back to the Archiving page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectDocumentType(string documentTypeId)
        {
            var model = GetModel();

            try
            {
                var http = store.TryGet(HttpContext.Session.Id)?.GetHttpClient();
                if (http == null)
                {
                    log.Error("Cannot select document type: not connected.");
                    return RedirectToAction("Index");
                }

                var docType = await OnBaseTaxonomy.GetDocumentTypeAsync(documentTypeId, http);
                if (docType == null)
                {
                    log.Error($"Document type [{documentTypeId}] not found.");
                    return RedirectToAction("Index");
                }

                model.SelectedDocumentTypeId = docType.Id;
                model.SelectedDocumentTypeName = docType.Name;
                model.NewEditorSchema = await BuildSchemaForDocTypeAsync(docType.Id, http);
                SaveModel(model);

                log.Success($"Loaded keyword editor for document type [{docType.Name}].");
            }
            catch (Exception ex)
            {
                log.Error(ex);
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
        public async Task<IActionResult> StoreNew(DateTime documentDate, IEnumerable<IFormFile> files)
        {
            var model = GetModel();
            var tempFiles = new List<string>();

            try
            {
                var http = store.TryGet(HttpContext.Session.Id)?.GetHttpClient();
                if (http == null)
                {
                    log.Error("Cannot store: not connected.");
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrEmpty(model.SelectedDocumentTypeId))
                {
                    log.Error("Cannot store: no document type selected.");
                    return RedirectToAction("Index");
                }

                tempFiles = await SaveUploadedFilesAsync(files);
                if (tempFiles.Count == 0)
                {
                    log.Error("Cannot store: no file(s) attached.");
                    return RedirectToAction("Index");
                }

                var (groups, keywords) = ParseKeywordsFromForm(Request.Form, model.NewEditorSchema);

                var request = new NewDocumentRequest
                {
                    DocumentType = model.SelectedDocumentTypeId,
                    DocumentDate = documentDate,
                    Files = tempFiles,
                    KeywordGroups = groups,
                    Keywords = keywords
                };

                var newId = await DocumentStorage.CreateDocumentAsync(request, http);

                log.Success($"Stored new document [{newId}].");
            }
            catch (Exception ex)
            {
                log.Error(ex);
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
        /// <param name="username">The OnBase username to connect with, if not already connected.</param>
        /// <param name="password">The OnBase password to connect with, if not already connected.</param>
        /// <returns>A redirect back to the Archiving page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoadDocument(string documentIdInput, string username, string password)
        {
            var model = GetModel();
            model.DocumentIdInput = documentIdInput;
            model.HasLoadedDocument = false;

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
                    SaveModel(model);
                    return RedirectToAction("Index");
                }
            }

            if (!long.TryParse(documentIdInput, out _))
            {
                log.Error($"[{documentIdInput}] is not a valid Document ID.");
                SaveModel(model);
                return RedirectToAction("Index");
            }

            try
            {
                var http = session.GetHttpClient();

                var metadata = await DocumentRetrieval.GetDocumentInfoAsync(documentIdInput, http);
                if (metadata == null)
                {
                    log.Error($"No document found with ID [{documentIdInput}].");
                    SaveModel(model);
                    return RedirectToAction("Index");
                }

                var docType = await OnBaseTaxonomy.GetDocumentTypeAsync(metadata.Type, http);

                model.HasLoadedDocument = true;
                model.LoadedDocumentId = documentIdInput;
                model.LoadedDocumentTypeName = metadata.Type;
                model.LoadedDocumentDate = metadata.DocumentDate;
                model.ExistingEditorSchema = docType != null
                    ? await BuildSchemaForDocumentAsync(docType.Id, metadata, http)
                    : new KeywordEditorSchema();
                SaveModel(model);

                log.Success($"Loaded document [{documentIdInput}] for editing.");
            }
            catch (Exception ex)
            {
                log.Error(ex);
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
        public async Task<IActionResult> ModifyMetadata(string documentTypeName, DateTime documentDate)
        {
            var model = GetModel();

            if (!model.HasLoadedDocument)
            {
                log.Error("Cannot modify: no document loaded.");
                return RedirectToAction("Index");
            }

            try
            {
                var http = store.TryGet(HttpContext.Session.Id)?.GetHttpClient();
                if (http == null)
                {
                    log.Error("Cannot modify: not connected.");
                    return RedirectToAction("Index");
                }

                var (groups, keywords) = ParseKeywordsFromForm(Request.Form, model.ExistingEditorSchema);

                var request = new UpdateDocumentRequest
                {
                    UpdateType = UpdateType.Metadata,
                    DocumentId = model.LoadedDocumentId,
                    DocumentType = documentTypeName,
                    DocumentDate = documentDate,
                    KeywordGroups = groups,
                    Keywords = keywords
                };

                await DocumentStorage.ModifyDocumentAsync(request, http);

                log.Success($"Updated metadata on document [{model.LoadedDocumentId}].");
            }
            catch (Exception ex)
            {
                log.Error(ex);
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
        public async Task<IActionResult> AddRevision(IEnumerable<IFormFile> files)
        {
            var model = GetModel();
            var tempFiles = new List<string>();

            if (!model.HasLoadedDocument)
            {
                log.Error("Cannot add revision: no document loaded.");
                return RedirectToAction("Index");
            }

            try
            {
                var http = store.TryGet(HttpContext.Session.Id)?.GetHttpClient();
                if (http == null)
                {
                    log.Error("Cannot add revision: not connected.");
                    return RedirectToAction("Index");
                }

                tempFiles = await SaveUploadedFilesAsync(files);
                if (tempFiles.Count == 0)
                {
                    log.Error("Cannot add revision: no file(s) attached.");
                    return RedirectToAction("Index");
                }

                var request = new UpdateDocumentRequest
                {
                    UpdateType = UpdateType.Revision,
                    DocumentId = model.LoadedDocumentId,
                    Files = tempFiles
                };

                await DocumentStorage.ModifyDocumentAsync(request, http);

                log.Success($"Added a new revision to document [{model.LoadedDocumentId}].");
            }
            catch (Exception ex)
            {
                log.Error(ex);
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
        public async Task<IActionResult> AddRendition(IEnumerable<IFormFile> files)
        {
            var model = GetModel();
            var tempFiles = new List<string>();

            if (!model.HasLoadedDocument)
            {
                log.Error("Cannot add rendition: no document loaded.");
                return RedirectToAction("Index");
            }

            try
            {
                var http = store.TryGet(HttpContext.Session.Id)?.GetHttpClient();
                if (http == null)
                {
                    log.Error("Cannot add rendition: not connected.");
                    return RedirectToAction("Index");
                }

                tempFiles = await SaveUploadedFilesAsync(files);
                if (tempFiles.Count == 0)
                {
                    log.Error("Cannot add rendition: no file(s) attached.");
                    return RedirectToAction("Index");
                }

                var request = new UpdateDocumentRequest
                {
                    UpdateType = UpdateType.Rendition,
                    DocumentId = model.LoadedDocumentId,
                    Files = tempFiles
                };

                await DocumentStorage.ModifyDocumentAsync(request, http);

                log.Success($"Added a new rendition to document [{model.LoadedDocumentId}]'s latest revision.");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                CleanUpTempFiles(tempFiles);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Deletes the loaded document. No purge/permanent-delete option: a confirmed gap.
        /// </summary>
        /// <returns>A redirect back to the Archiving page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete()
        {
            var model = GetModel();

            if (!model.HasLoadedDocument)
            {
                log.Error("Cannot delete: no document loaded.");
                return RedirectToAction("Index");
            }

            try
            {
                var http = store.TryGet(HttpContext.Session.Id)?.GetHttpClient();
                if (http == null)
                {
                    log.Error("Cannot delete: not connected.");
                    return RedirectToAction("Index");
                }

                var documentId = model.LoadedDocumentId;
                var request = new DeleteRequest { DocumentId = documentId };

                await DocumentStorage.DeleteDocumentAsync(request, http);

                log.Success($"Document [{documentId}] deleted.");

                model.HasLoadedDocument = false;
                model.DocumentIdInput = null;
                model.ExistingEditorSchema = new KeywordEditorSchema();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            SaveModel(model);
            return RedirectToAction("Index");
        }
        #endregion

        #region Private Methods
        // Read the cached page model from Session (as JSON), or a fresh one
        private ArchivingPageModel GetModel()
        {
            var json = HttpContext.Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json)) return new ArchivingPageModel();

            try
            {
                return JsonSerializer.Deserialize<ArchivingPageModel>(json) ?? new ArchivingPageModel();
            }
            catch (JsonException)
            {
                return new ArchivingPageModel();
            }
        }

        // Save the page model to Session (as JSON)
        private void SaveModel(ArchivingPageModel model)
        {
            HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(model));
        }

        // Build a keyword editor schema from a Document Type id, with no existing values
        // (Store New)
        private static async Task<KeywordEditorSchema> BuildSchemaForDocTypeAsync(string documentTypeId, HttpClient http)
        {
            var allGroups = await OnBaseTaxonomy.GetDocumentTypeKeywordGroupsAsync(documentTypeId, http);
            var (groups, standalone) = OnBaseTaxonomy.SplitKeywordGroups(allGroups);

            var schema = new KeywordEditorSchema();

            foreach (var group in groups)
            {
                schema.Groups.Add(new KeywordGroupSchema
                {
                    Id = group.Id,
                    Name = group.Name,
                    MultiInstance = group.StorageType == "MultiInstance",
                    FieldDefinitions = [.. group.KeywordTypes.Select(k => new KeywordFieldSchema { Id = k.Id, Name = k.Name })]
                });
            }

            foreach (var keywordType in standalone)
            {
                schema.Standalone.Add(new StandaloneKeywordSchema { Id = keywordType.Id, Name = keywordType.Name });
            }

            return schema;
        }

        // Build a keyword editor schema from an existing document, pre-populated with its
        // current values (Modify Metadata)
        private static async Task<KeywordEditorSchema> BuildSchemaForDocumentAsync(string documentTypeId, DocumentInfo metadata, HttpClient http)
        {
            var schema = await BuildSchemaForDocTypeAsync(documentTypeId, http);

            foreach (var standaloneSchema in schema.Standalone)
            {
                var values = metadata.Keywords.Where(k => k.Id == standaloneSchema.Id).Select(k => k.Value).ToList();
                if (values.Count > 0) standaloneSchema.Values = values;
            }

            foreach (var groupSchema in schema.Groups)
            {
                foreach (var group in metadata.KeywordGroups.Where(g => g.TypeGroupId == groupSchema.Id))
                {
                    groupSchema.Instances.Add(group.Keywords.ToDictionary(k => k.Id, k => k.Value));
                }
            }

            return schema;
        }

        // Parse posted kw_group_{typeGroupId}_{instanceKey}_{fieldId}/kw_standalone_{keywordId}_{valueKey}
        // fields back into the KeywordGroup/KeywordInfo lists the library expects
        #pragma warning disable S3776 // Not overly complex
        private static (List<KeywordGroup> Groups, List<KeywordInfo> Keywords) ParseKeywordsFromForm(IFormCollection form, KeywordEditorSchema schema)
        #pragma warning restore S3776
        {
            var groupInstances = new Dictionary<string, Dictionary<string, string>>();
            var standaloneValues = new Dictionary<string, List<string>>();

            foreach (var key in form.Keys)
            {
                var value = form[key].ToString();
                if (string.IsNullOrWhiteSpace(value)) continue;

                if (key.StartsWith("kw_group_", StringComparison.Ordinal))
                {
                    var parts = key["kw_group_".Length..].Split('_');
                    if (parts.Length != 3) continue;
                    var fieldId = parts[2];

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
                    var parts = key["kw_standalone_".Length..].Split('_');
                    if (parts.Length != 2) continue;
                    var keywordId = parts[0];

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
                var typeGroupId = kvp.Key.Split('_')[0];
                var groupSchema = schema.Groups.FirstOrDefault(g => g.Id == typeGroupId);
                if (groupSchema == null) continue;

                // GroupId is deliberately left null here, see ArchivingModels.cs's own
                // Training Notes for why.
                groups.Add(new KeywordGroup
                {
                    TypeGroupId = typeGroupId,
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
        private static async Task<List<string>> SaveUploadedFilesAsync(IEnumerable<IFormFile> files)
        {
            var paths = new List<string>();
            if (files == null) return paths;

            foreach (var file in files)
            {
                if (file == null || file.Length == 0) continue;

                var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "_" + Path.GetFileName(file.FileName));
                await using var stream = new FileStream(tempPath, FileMode.Create);
                await file.CopyToAsync(stream);
                paths.Add(tempPath);
            }

            return paths;
        }

        // Delete temp files created by SaveUploadedFilesAsync
        private void CleanUpTempFiles(List<string> paths)
        {
            foreach (var path in paths)
            {
                try
                {
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }
                catch (Exception ex)
                {
                    log.Error(ex);
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
