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
using Hyland.Unity;
using Hyland.Unity.UnityForm;
using Unity._00.CommonFunctionality.Models.Enumerations;
using Unity._00.CommonFunctionality.Models.Objects;
using Unity._02.AccessingTaxonomy.HelperClasses.OnBase;
using Unity._03.DocumentRetrieval.HelperClasses.OnBase;
using Unity._03.DocumentRetrieval.Models.Objects;
using Unity._04.DocumentArchiving.Models.Enumerations;
using Unity._04.DocumentArchiving.Models.Objects;
#endregion

#pragma warning disable S125 // Allow commented code in lesson files
namespace Unity._04.DocumentArchiving.HelperClasses.OnBase
{
    #region Training Notes
    /*
     * *Migration Note: this class was originally missing a class-level <summary> and XML
     * doc comments on Config/Metadata/ContinueOnError, added here for consistency with
     * every other class in this training set (every public member gets one).
     *
     * Two areas are DELIBERATELY left incomplete, matching the original exactly, not
     * bugs to fix:
     *
     * 1. Repeater support. StoreNewUnityForm and UpdateUnityFormMetadata both accept
     *    request.Form.Repeaters as part of their input, but neither one actually adds
     *    those repeater rows to OnBase, each has a "// TODO: Add repeater(s)" comment
     *    marking exactly where that logic would go. RepeaterInfo (the model) exists;
     *    wiring it into the Unity API's repeater-row creation methods does not.
     *
     * 2. UpdateEFormRevision, UpdateUnityFormRevision, UpdateEFormRendition, and
     *    UpdateUnityFormRendition all throw NotImplementedException outright. Storing a
     *    new REVISION or RENDITION of a conventional document is fully implemented
     *    (UpdateRevision/UpdateRendition's main paths); the form-specific equivalents are
     *    stubs only, the same class of intentional gap as Unity.01's SAML/ADFS
     *    IdpGrantType stubs.
     *
     * Neither gap was implemented here since doing so correctly needs specific Unity API
     * documentation for OnBase repeater controls and e-form/Unity Form revision/rendition
     * storage that wasn't available while porting this project. See LectureNotes.md.
     */
    #endregion

    /// <summary>
    /// Exposes OnBase document creation, update, and deletion functions, for
    /// conventional documents, e-forms, and Unity Forms.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the DocumentStorage class
    /// </remarks>
    /// <param name="app">Unity API Application Object</param>
    public class DocumentStorage(Application app)
    {
        #region Properties
        /// <summary>
        /// Unity API Application Object
        /// </summary>
        public Application App { get; set; } = app;

        /// <summary>
        /// Access functions for OnBase configuration
        /// </summary>
        public OnBaseTaxonomy Config { get; set; }

        /// <summary>
        /// Keyword and keyword group generator functions
        /// </summary>
        public Metadata Metadata { get; set; }

        /// <summary>
        /// When <see langword="true"/>, a keyword or keyword group that fails to build
        /// (see <see cref="Metadata"/>) is skipped rather than aborting the whole
        /// operation. Set per-call via each public method's own <c>continueOnError</c>
        /// parameter.
        /// </summary>
        public bool ContinueOnError { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Store a new document in the OnBase system
        /// </summary>
        /// <param name="request">New document information<seealso cref="NewDocumentRequest"/></param>
        /// <param name="app">Unity API application object<seealso cref="Application"/></param>
        /// <param name="continueOnError">When true, keyword errors are non-terminal</param>
        /// <returns>OnBase document<seealso cref="Document"/></returns>
        public Document CreateDocument(NewDocumentRequest request, Application app = null, bool continueOnError = true)
        {
            try
            {
                ContinueOnError = continueOnError;
                return request.StorageType switch
                {
                    StorageType.Document => StoreNewDocument(request, app),
                    StorageType.EForm => StoreNewEForm((NewFormRequest)request, app),
                    StorageType.UnityForm => StoreNewUnityForm((NewFormRequest)request, app),
                    //TODO: StorageType.UnindexedDocument:
                    _ or StorageType.Undefined => throw new DatabankException($"Storage type [{request.StorageType}] not supported!"),
                };
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error creating document!", ex);
            }
        }

        /// <summary>
        /// Update existing document in the OnBase system
        /// </summary>
        /// <param name="request">Update document information<seealso cref="UpdateDocumentRequest"/></param>
        /// <param name="app">Unity API application object<seealso cref="Application"/></param>
        /// <param name="continueOnError">When true, keyword errors are non-terminal</param>
        /// <returns>OnBase document<seealso cref="Document"/></returns>
        public bool ModifyDocument(UpdateDocumentRequest request, Application app = null, bool continueOnError = true)
        {
            try
            {
                ContinueOnError = continueOnError;
                return request.UpdateType switch
                {
                    UpdateType.Metadata => UpdateMetadata(request, app),
                    UpdateType.Revision => UpdateRevision(request, app),
                    UpdateType.Rendition => UpdateRendition(request, app),
                    _ or UpdateType.Undefined => throw new DatabankException($"Update type [{request.UpdateType}] not supported!"),
                };
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error modifying document!", ex);
            }
        }

        /// <summary>
        /// Delete existing document from OnBase
        /// </summary>
        /// <param name="request">Delete parameters</param>
        /// <param name="app">Unity API application object</param>
        /// <param name="continueOnError">When true, keyword errors are non-terminal</param>
        /// <returns>True when the document is successfully deleted</returns>
        public bool DeleteDocument(DeleteRequest request, Application app = null, bool continueOnError = true)
        {
            try
            {
                ContinueOnError = continueOnError;
                Initialize(app);

                var doc = App.Core.GetDocumentByID(request.DocumentId)
                    ?? throw new DatabankException($"Document [{request.DocumentId}] not found!");
                var storage = App.Core.Storage;

                if (request.PurgeDocument) storage.PurgeDocument(doc);
                else storage.DeleteDocument(doc);

                return true;
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error deleting document [{request.DocumentId}]!", ex);
            }
        }
        #endregion

        #region Private Create Methods
        // Store a new OnBase document
        #pragma warning disable S3776 // Not overly complex
        private Document StoreNewDocument(NewDocumentRequest request, Application app)
        #pragma warning restore S3776
        {
            try
            {
                Initialize(app);

                var docType = Config.GetDocumentType(request.DocumentType)
                    ?? throw new DatabankException($"Cannot find document type [{request.DocumentType}]!");

                if (!docType.CanI(DocumentTypePrivileges.DocumentCreation))
                    throw new DatabankException($"User [{App.CurrentUser.DisplayName}] cannot create document type [{docType.Name}]!");

                if (request.Files == null || request.Files.Count < 1)
                    throw new DatabankException("No file(s) provided for new document!");

                var format = Config.GetFileType(request.Files)
                    ?? throw new DatabankException($"No file type found matching extension [{Path.GetExtension(request.Files[0])}]!");

                var storage = App.Core.Storage;

                var props = storage.CreateStoreNewDocumentProperties(docType, format);

                props.DocumentDate = request.DocumentDate;

                if (request.Keywords != null && request.Keywords.Count > 0)
                {
                    foreach (var keyItem in request.Keywords)
                    {
                        var keyword = Metadata.MakeKeyword(keyItem);
                        if (keyword == null)
                        {
                            if (ContinueOnError) continue;
                            throw new DatabankException($"Failed to create keyword [{keyItem.Name}] = [{keyItem.Type}]!");
                        }
                        props.AddKeyword(keyword);
                    }
                }

                if (request.KeywordGroups != null && request.KeywordGroups.Count > 0)
                {
                    foreach (var keyGroup in request.KeywordGroups)
                    {
                        var record = Metadata.MakeKeywordGroup(keyGroup);
                        if (record == null)
                        {
                            if (ContinueOnError) continue;
                            throw new DatabankException($"Failed to create keyword record [{keyGroup.Name}]!");
                        }
                        props.AddKeywordRecord(record);
                    }
                }

                return storage.StoreNewDocument(request.Files, props);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error storing document!", ex);
            }
        }

        // Store a new OnBase e-form
        private Document StoreNewEForm(NewFormRequest request, Application app)
        {
            try
            {
                Initialize(app);

                var docType = Config.GetDocumentType(request.DocumentType)
                    ?? throw new DatabankException($"Cannot find document type [{request.DocumentType}]!");

                if (!docType.CanI(DocumentTypePrivileges.DocumentCreation))
                    throw new DatabankException($"User [{App.CurrentUser.DisplayName}] cannot create document type [{docType.Name}]!");

                var storage = App.Core.Storage;

                var format = Config.GetFileType((long)FileFormat.EForm);

                var props = storage.CreateStoreNewEFormProperties(docType, format);

                props.DocumentDate = request.DocumentDate;

                foreach (var keyItem in request.Keywords)
                {
                    var keyword = Metadata.MakeKeyword(keyItem);
                    if (keyword == null)
                    {
                        if (ContinueOnError) continue;
                        throw new DatabankException($"Failed to create keyword [{keyItem.Name}] = [{keyItem.Type}]!");
                    }
                    props.AddKeyword(keyword);
                }

                foreach (var keyGroup in request.KeywordGroups)
                {
                    var record = Metadata.MakeKeywordGroup(keyGroup);
                    if (record == null)
                    {
                        if (ContinueOnError) continue;
                        throw new DatabankException($"Failed to create keyword record [{keyGroup.Name}]!");
                    }
                    props.AddKeywordRecord(record);
                }

                foreach (var field in request.Form.Fields) props.AddField(field.Name, field.Value);

                return storage.StoreNewEForm(props);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error storing e-form!", ex);
            }
        }

        // Store a new OnBase unity form
        #pragma warning disable S3776 // Not overly complex
        private Document StoreNewUnityForm(NewFormRequest request, Application app)
        #pragma warning restore S3776
        {
            try
            {
                Initialize(app);

                var template = (request.Form.Id < 101
                    ? Config.GetUnityForm(request.Form.Name)
                    : Config.GetUnityForm(request.Form.Id.ToString()))
                    ?? throw new DatabankException($"Cannot find unity form template [{request.Form.Name}]!");

                if (!template.DocumentType.CanI(DocumentTypePrivileges.DocumentCreation))
                    throw new DatabankException($"User [{App.CurrentUser.DisplayName}] cannot create document type [{template.DocumentType.Name}]!");

                var storage = App.Core.Storage;

                var props = storage.CreateStoreNewUnityFormProperties(template);

                props.DocumentDate = request.DocumentDate;

                foreach (var keyItem in request.Keywords)
                {
                    var keyword = Metadata.MakeKeyword(keyItem);
                    if (keyword == null)
                    {
                        if (ContinueOnError) continue;
                        throw new DatabankException($"Failed to create keyword [{keyItem.Name}] = [{keyItem.Type}]!");
                    }
                    props.AddKeyword(keyword);
                }

                foreach (var keyGroup in request.KeywordGroups)
                {
                    var record = Metadata.MakeKeywordGroup(keyGroup);
                    if (record == null)
                    {
                        if (ContinueOnError) continue;
                        throw new DatabankException($"Failed to create keyword record [{keyGroup.Name}]!");
                    }
                    props.AddKeywordRecord(record);
                }

                foreach (var field in request.Form.Fields) props.AddField(field.Name, field.Value);

                // TODO: Add repeater Items, see Training Notes above

                return storage.StoreNewUnityForm(props);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error storing unity form!", ex);
            }
        }
        #endregion

        #region Private Update Methods
        #region Metadata Methods
        // Update keywords, document type, and/or document date on document
        private bool UpdateMetadata(UpdateDocumentRequest request, Application app)
        {
            try
            {
                Initialize(app);

                var doc = App.Core.GetDocumentByID(request.DocumentId)
                    ?? throw new DatabankException($"Cannot find document [{request.DocumentId}]!");

                var docLock = doc.LockDocument();
                if (docLock.Status != DocumentLockStatus.LockObtained)
                    throw new DatabankException($"Document already locked by [{docLock.UserHoldingLock.DisplayName}]!");

                var modifier = doc.CreateKeywordModifier();

                UpdateKeywords(doc, modifier, request.Keywords, request.OverwriteKeywords);

                UpdateKeywordGroups(doc, modifier, request.KeywordGroups);

                modifier.ApplyChanges();

                switch (request.StorageType)
                {
                    case StorageType.EForm:
                        if (!UpdateEFormMetadata((UpdateFormRequest)request, doc))
                            throw new DatabankException($"Failed to update e-form doc [{doc.ID}]!");
                        break;
                    case StorageType.UnityForm:
                        if (! UpdateUnityFormMetadata((UpdateFormRequest)request, doc))
                            throw new DatabankException($"Failed to update unity form doc [{doc.ID}]!");
                        break;
                }

                UpdateDocumentType(doc, request.DocumentType, request.DocumentDate);

                docLock.Release();

                return true;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error updating document metadata!", ex);
            }
        }

        // Update keywords, document type, and/or document date on e-form
        private static bool UpdateEFormMetadata(UpdateFormRequest request, Document doc)
        {
            try
            {
                if (doc == null) throw new DatabankException($"Cannot find document [{request.DocumentId}]!");

                var form = GetEForm(doc)
                    ?? throw new DatabankException($"No e-form exists on document [{doc.ID}]!");

                var modifier = form.CreateFieldModifier();

                foreach (var field in request.Form.Fields)
                {
                    if (string.IsNullOrEmpty(field.Value)) continue;
                    var oldField = form.Fields.Find(field.Name);
                    if (oldField == null || string.IsNullOrEmpty(oldField.Value)) modifier.AddField(field.Name, field.Value);
                    else modifier.UpdateField(field.Name, field.Value);
                }

                modifier.ApplyChanges();

                return true;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error updating e-form metadata!", ex);
            }
        }

        // Update keywords, document type, and/or document date on unity form
        private static bool UpdateUnityFormMetadata(UpdateFormRequest request, Document doc)
        {
            try
            {
                if (doc == null) throw new DatabankException($"Cannot find document [{request.DocumentId}]!");

                var form = GetUnityForm(doc)
                    ?? throw new DatabankException($"No e-form exists on document [{doc.ID}]!");

                var modifier = form.CreateUnityFormModifier();

                foreach (var field in request.Form.Fields.Where(field => !string.IsNullOrEmpty(field.Value)))
                {
                    modifier.SetFieldValue(field.Name, field.Value);
                }

                // TODO: Add repeaters, see Training Notes above

                modifier.ApplyChanges();

                return true;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error updating e-form metadata!", ex);
            }
        }
        #endregion

        #region Revision Methods
        // Store a new revision for an existing document
        private bool UpdateRevision(UpdateDocumentRequest request, Application app)
        {
            try
            {
                Initialize(app);

                switch (request.StorageType)
                {
                    case StorageType.EForm:
                        return UpdateEFormRevision((UpdateFormRequest) request, app);
                    case StorageType.UnityForm:
                        return UpdateUnityFormRevision((UpdateFormRequest)request, app);
                }

                var doc = App.Core.GetDocumentByID(request.DocumentId)
                    ?? throw new DatabankException($"Cannot find document [{request.DocumentId}]!");

                if (!doc.DocumentType.Revisable)
                    throw new DatabankException($"Document type [{doc.DocumentType.Name}] is not revisable!");

                if (!doc.DocumentType.CanI(DocumentTypePrivileges.CreateDeleteRevisions))
                    throw new DatabankException($"User [{App.CurrentUser.DisplayName}] cannot make revisions on document type [{doc.DocumentType.Name}]!");

                if (request.Files == null || request.Files.Count < 1)
                    throw new DatabankException("No file(s) provided for new document!");

                var format = Config.GetFileType(request.Files)
                    ?? throw new DatabankException($"No file type found matching extension [{Path.GetExtension(request.Files[0])}]!");

                var storage = App.Core.Storage;

                var props = storage.CreateStoreRevisionProperties(doc, format);
                props.Comment = "Revision generated by API";

                return storage.StoreNewRevision(request.Files, props) != null;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error updating document revision!", ex);
            }
        }

        // Store a new revision for an existing e-form
        // TODO: *Not implemented, see Training Notes above*
        private bool UpdateEFormRevision(UpdateFormRequest request, Application app)
        {
            try
            {
                Initialize(app);

                throw new NotImplementedException();

            }
            catch (Exception ex)
            {
                throw new DatabankException("Error updating e-form revision!", ex);
            }
        }

        // Store a new revision for an existing unity form
        // TODO: *Not implemented, see Training Notes above*
        private bool UpdateUnityFormRevision(UpdateFormRequest request, Application app)
        {
            try
            {
                Initialize(app);

                throw new NotImplementedException();

            }
            catch (Exception ex)
            {
                throw new DatabankException("Error updating unity form revision!", ex);
            }
        }
        #endregion

        #region Rendition Methods
        // Store a new rendition file for an existing document
        private bool UpdateRendition(UpdateDocumentRequest request, Application app)
        {
            try
            {
                Initialize(app);

                switch (request.StorageType)
                {
                    case StorageType.EForm:
                        return UpdateEFormRendition((UpdateFormRequest) request, app);
                    case StorageType.UnityForm:
                        return UpdateUnityFormRendition((UpdateFormRequest)request, app);
                }

                var doc = App.Core.GetDocumentByID(request.DocumentId)
                    ?? throw new DatabankException($"Cannot find document [{request.DocumentId}]!");

                if (!doc.DocumentType.Renditionable)
                    throw new DatabankException($"Document type [{doc.DocumentType.Name}] is not renditionable!");

                // There is no specific CanI privilege for rendition creation
                if (!doc.DocumentType.CanI(DocumentTypePrivileges.DocumentModification))
                    throw new DatabankException($"User [{App.CurrentUser.DisplayName}] cannot modify document type [{doc.DocumentType.Name}]!");

                if (request.Files == null || request.Files.Count < 1)
                    throw new DatabankException("No file(s) provided for new document!");

                var format = Config.GetFileType(request.Files)
                    ?? throw new DatabankException($"No file type found matching extension [{Path.GetExtension(request.Files[0])}]!");

                if (doc.LatestRevision.Renditions.Any(rendition => rendition.FileType == format))
                    throw new DatabankException($"Document [{doc.ID}] already has a [{format}] rendition on the current revision!");

                var storage = App.Core.Storage;

                var props = storage.CreateStoreRenditionProperties(doc.LatestRevision, format);
                props.Comment = "Rendition generated by API";

                return storage.StoreNewRendition(request.Files, props) != null;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error updating document rendition!", ex);
            }
        }

        // Store a new e-form rendition file for an existing document
        // TODO: *Not implemented, see Training Notes above*
        private bool UpdateEFormRendition(UpdateFormRequest request, Application app)
        {
            try
            {
                Initialize(app);

                throw new NotImplementedException();

            }
            catch (Exception ex)
            {
                throw new DatabankException("Error updating e-form rendition!", ex);
            }
        }

        // Store a new unity form rendition file for an existing document
        // TODO: *Not implemented, see Training Notes above*
        private bool UpdateUnityFormRendition(UpdateFormRequest request, Application app)
        {
            try
            {
                Initialize(app);

                throw new NotImplementedException();

            }
            catch (Exception ex)
            {
                throw new DatabankException("Error updating unity form rendition!", ex);
            }
        }
        #endregion
        #endregion

        #region Helper Methods
        // Initialize the Unity API
        private void Initialize(Application app)
        {
            try
            {
                if (app != null) App = app;

                if (App == null) throw new DatabankException("Application cannot be null!");

                Config = new OnBaseTaxonomy(App);
                Metadata = new Metadata(App);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error initializing Application object!", ex);
            }
        }

        // Update stand-alone or SIKG keywords
        private void UpdateKeywords(Document doc, KeywordModifier modifier, List<KeywordInfo> keywords, bool overwriteKeywords)
        {
            try
            {
                if (!doc.DocumentType.CanI(DocumentTypePrivileges.ModifyKeywords))
                    throw new DatabankException($"User [{App.CurrentUser.DisplayName}] cannot modify keywords on document type [{doc.DocumentType.Name}]!");

                foreach (var keyItem in keywords)
                {
                    var newKeyword = Metadata.MakeKeyword(keyItem);

                    if (newKeyword == null)
                    {
                        if (ContinueOnError) continue;
                        throw new DatabankException($"Failed to create keyword [{keyItem.Name}] = [{keyItem.Type}]!");
                    }

                    if (overwriteKeywords)
                    {
                        var oldKeyword = doc.GetKeyword(keyItem.Name);
                        if (oldKeyword != null && !oldKeyword.IsBlank)
                        {
                            modifier.UpdateKeyword(oldKeyword, newKeyword);
                            continue;
                        }
                    }
                    modifier.AddKeyword(newKeyword);
                }
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error updating keywords!", ex);
            }
        }

        // Update MIKG keyword records
        private void UpdateKeywordGroups(Document doc, KeywordModifier modifier, List<KeywordGroup> keywordGroups)
        {
            try
            {
                if (!doc.DocumentType.CanI(DocumentTypePrivileges.ModifyKeywords))
                    throw new DatabankException($"User [{App.CurrentUser.DisplayName}] cannot modify keywords on document type [{doc.DocumentType.Name}]!");

                foreach (var keyGroup in keywordGroups)
                {
                    if (!keyGroup.MultiInstance)
                    {
                        UpdateKeywords(doc, modifier, keyGroup.Keywords, true);
                        continue;
                    }

                    var record = Metadata.MakeKeywordGroup(keyGroup);
                    if (record == null)
                    {
                        if (ContinueOnError) continue;
                        throw new DatabankException($"Failed to create keyword record [{keyGroup.Name}]!");
                    }

                    // We can only add MIKG records, not overwrite
                    modifier.AddKeywordRecord(record);
                }
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error updating keyword groups!", ex);
            }
        }

        // Re-index document type and/or document date
        private void UpdateDocumentType(Document doc, string docType, DateTime docDate)
        {
            try
            {
                if (!doc.DocumentType.CanI(DocumentTypePrivileges.ReindexDocument))
                    throw new DatabankException($"User [{App.CurrentUser.DisplayName}] cannot re-index document type [{doc.DocumentType.Name}]!");

                var documentType = string.Equals(doc.DocumentType.Name, docType, StringComparison.CurrentCultureIgnoreCase)
                    ? doc.DocumentType
                    : Config.GetDocumentType(docType);

                if (documentType == null)
                {
                    if (ContinueOnError) return;
                    throw new DatabankException($"Cannot find document type [{docType}]!");
                }

                if (documentType == doc.DocumentType && DateTime.Equals(doc.DocumentDate.Date, docDate.Date)) return;

                var props = App.Core.Storage.CreateReindexProperties(doc, documentType);

                if (!DateTime.Equals(doc.DocumentDate.Date, docDate.Date)) props.DocumentDate = docDate;

                App.Core.Storage.ReindexDocument(props);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error updating document type!", ex);
            }
        }

        // Obtain electronic form object
        private static EForm GetEForm(Document doc)
        {
            try
            {
                return (from rendition in doc.LatestRevision.Renditions where rendition.FileType.ID == (long)FileFormat.EForm || rendition.FileType.ID == (long)FileFormat.VirtualForm select rendition.EForm).FirstOrDefault();

                // LINQ above equivalent to the following:
                //foreach (var rendition in doc.LatestRevision.Renditions)
                //{
                //    if (rendition.FileType.ID == (long)FileFormat.EForm || rendition.FileType.ID == (long)FileFormat.VirtualForm)
                //        return rendition.EForm;
                //}
                //return null;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error getting e-form!", ex);
            }
        }

        // Obtain unity form object
        private static Form GetUnityForm(Document doc)
        {
            try
            {
                return (from rendition in doc.LatestRevision.Renditions where rendition.FileType.ID == (long)FileFormat.UnityForm select rendition.UnityForm).FirstOrDefault();

                // LINQ above equivalent to the following:
                //foreach (var rendition in doc.LatestRevision.Renditions)
                //{
                //    if (rendition.FileType.ID == (long)FileFormat.UnityForm) return rendition.UnityForm;
                //}
                //return null;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error getting e-form!", ex);
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
