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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Hyland.Unity;
using Microsoft.Win32;
using Unity._02.AccessingTaxonomy.HelperClasses.OnBase;
using Unity._03.DocumentRetrieval.HelperClasses.OnBase;
using Unity._03.DocumentRetrieval.Models.Objects;
using Unity._04.DocumentArchiving.HelperClasses.OnBase;
using Unity._04.DocumentArchiving.Models.Enumerations;
using Unity._04.DocumentArchiving.Models.Objects;
using Unity.TestHarness.Models;
#endregion

namespace Unity.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: five modes, matching Unity.04.DocumentArchiving's own split:
     * Store New goes through DocumentStorage.CreateDocument, three go through
     * ModifyDocument with a different UpdateType (Metadata/Revision/Rendition), and
     * Delete goes through DocumentStorage.DeleteDocument directly. Scoped to
     * StorageType.Document specifically (plain file-based documents), not
     * EForm/UnityForm, matching what was actually asked for.
     *
     * The four "existing document" modes share ONE document-loading step
     * (DocumentIdInput/LoadDocumentCommand/LoadedDocument), rather than each mode having
     * its own separate loader, there's only ever one document being acted on regardless
     * of which of the four you're doing to it. LoadDocument goes through
     * DocumentRetrieval.GetDocumentById (not App.Core.GetDocumentByID directly), the same
     * library method DocumentDetailViewModel uses, rather than each independently calling
     * the raw Unity API. IsRevisable/IsRenditionable delegate to
     * DocumentStorage.CanAddRevision/CanAddRendition, the same checks
     * ModifyDocument(UpdateType.Revision/.Rendition) perform internally before throwing,
     * surfaced here so the UI can decide whether to even offer the option beforehand.
     *
     * KeywordEditorSet -> KeywordGroup/KeywordInfo conversion (BuildKeywordLists) skips
     * any field/instance left entirely blank, an empty MultiInstance GroupInstance (Add
     * Instance clicked, nothing filled in) or an empty standalone value slot doesn't
     * become a keyword record with no actual data.
     *
     * Delete is the one genuinely irreversible action here (permanently so with
     * PurgeDocument checked), so it's the one action gated by a confirmation prompt
     * (a plain MessageBox.Show call, a pragmatic choice consistent with this ViewModel
     * already calling OpenFileDialog/SaveFileDialog directly elsewhere, rather than a
     * separate confirmation-service abstraction). PurgeDocument defaults to false,
     * matching "unchecked by default" exactly as specified. A successful delete clears
     * LoadedDocument/ExistingEditors/DocumentIdInput, since none of them refer to
     * anything that still exists afterward.
     */
    #endregion

    /// <summary>
    /// The Archiving page: store a new document, or modify/add a revision/add a
    /// rendition to an existing one.
    /// </summary>
    public class ArchivingViewModel : ViewModelBase
    {
        #region Private Members
        private readonly ConnectionViewModel connection;
        private readonly LogViewModel log;
        private DocumentStorage storage;

        private ArchivingMode mode = ArchivingMode.StoreNew;
        private bool isBusy;

        // Store New
        private DocumentTypeGroup selectedGroupFilter;
        private DocumentType selectedDocumentType;
        private KeywordEditorSet newEditors;
        private DateTime newDocumentDate = DateTime.Today;

        // Existing document (Modify/AddRevision/AddRendition/Delete)
        private string documentIdInput;
        private Document loadedDocument;
        private KeywordEditorSet existingEditors;
        private DateTime existingDocumentDate;
        private string existingDocumentTypeName;
        private bool purgeDocument;
        #endregion

        #region Properties
        /// <summary>
        /// Which archiving operation is currently active.
        /// </summary>
        public ArchivingMode Mode
        {
            get => mode;
            set
            {
                if (!SetField(ref mode, value)) return;
                OnPropertyChanged(nameof(IsStoreNewMode));
                OnPropertyChanged(nameof(IsModifyMetadataMode));
                OnPropertyChanged(nameof(IsAddRevisionMode));
                OnPropertyChanged(nameof(IsAddRenditionMode));
                OnPropertyChanged(nameof(IsDeleteMode));
            }
        }

        /// <summary>
        /// Whether <see cref="Mode"/> is <see cref="ArchivingMode.StoreNew"/>.
        /// </summary>
        public bool IsStoreNewMode => Mode == ArchivingMode.StoreNew;

        /// <summary>
        /// Whether <see cref="Mode"/> is <see cref="ArchivingMode.ModifyMetadata"/>.
        /// </summary>
        public bool IsModifyMetadataMode => Mode == ArchivingMode.ModifyMetadata;

        /// <summary>
        /// Whether <see cref="Mode"/> is <see cref="ArchivingMode.AddRevision"/>.
        /// </summary>
        public bool IsAddRevisionMode => Mode == ArchivingMode.AddRevision;

        /// <summary>
        /// Whether <see cref="Mode"/> is <see cref="ArchivingMode.AddRendition"/>.
        /// </summary>
        public bool IsAddRenditionMode => Mode == ArchivingMode.AddRendition;

        /// <summary>
        /// Whether <see cref="Mode"/> is <see cref="ArchivingMode.Delete"/>.
        /// </summary>
        public bool IsDeleteMode => Mode == ArchivingMode.Delete;

        /// <summary>
        /// Whether an operation is currently in progress.
        /// </summary>
        public bool IsBusy
        {
            get => isBusy;
            private set => SetField(ref isBusy, value);
        }

        // --- Store New ---

        /// <summary>
        /// Every Document Type Group, for the optional "narrow the list below" filter.
        /// A <see langword="null"/> entry (rendered as "All Groups") is included first.
        /// </summary>
        public ObservableCollection<DocumentTypeGroup> DocumentTypeGroupFilters { get; } = [];

        /// <summary>
        /// The Document Type Group currently narrowing <see cref="AllDocumentTypesView"/>.
        /// </summary>
        public DocumentTypeGroup SelectedGroupFilter
        {
            get => selectedGroupFilter;
            set
            {
                if (!SetField(ref selectedGroupFilter, value)) return;
                AllDocumentTypesView?.Refresh();
            }
        }

        /// <summary>
        /// Every Document Type in OnBase.
        /// </summary>
        public ObservableCollection<DocumentType> AllDocumentTypes { get; } = [];

        /// <summary>
        /// A filtered view over <see cref="AllDocumentTypes"/>, narrowed by
        /// <see cref="SelectedGroupFilter"/> when set.
        /// </summary>
        public ICollectionView AllDocumentTypesView { get; }

        /// <summary>
        /// The Document Type to store the new document as. Setting this rebuilds
        /// <see cref="NewEditors"/>.
        /// </summary>
        public DocumentType SelectedDocumentType
        {
            get => selectedDocumentType;
            set
            {
                if (!SetField(ref selectedDocumentType, value)) return;
                NewEditors = value == null ? null : new KeywordEditorSet(value);
            }
        }

        /// <summary>
        /// The keyword editors for <see cref="SelectedDocumentType"/>.
        /// </summary>
        public KeywordEditorSet NewEditors
        {
            get => newEditors;
            private set => SetField(ref newEditors, value);
        }

        /// <summary>
        /// The new document's document date.
        /// </summary>
        public DateTime NewDocumentDate
        {
            get => newDocumentDate;
            set => SetField(ref newDocumentDate, value);
        }

        /// <summary>
        /// File(s) to store as the new document, added via drag/drop or
        /// <see cref="AddNewFilesCommand"/>.
        /// </summary>
        public ObservableCollection<string> NewFiles { get; } = [];

        // --- Existing document (Modify/AddRevision/AddRendition) ---

        /// <summary>
        /// The Document ID to load for editing.
        /// </summary>
        public string DocumentIdInput
        {
            get => documentIdInput;
            set => SetField(ref documentIdInput, value);
        }

        /// <summary>
        /// The currently-loaded document, or <see langword="null"/> if none is loaded.
        /// </summary>
        public Document LoadedDocument
        {
            get => loadedDocument;
            private set
            {
                if (!SetField(ref loadedDocument, value)) return;
                OnPropertyChanged(nameof(HasLoadedDocument));
                OnPropertyChanged(nameof(IsRevisable));
                OnPropertyChanged(nameof(IsRenditionable));
            }
        }

        /// <summary>
        /// Whether a document is currently loaded.
        /// </summary>
        public bool HasLoadedDocument => LoadedDocument != null;

        /// <summary>
        /// Whether <see cref="LoadedDocument"/>'s Document Type allows new revisions AND
        /// the current user has permission to create them. Delegates to
        /// DocumentStorage.CanAddRevision, the same check ModifyDocument(UpdateType.Revision)
        /// performs internally before throwing.
        /// </summary>
        public bool IsRevisable => LoadedDocument != null && storage != null && storage.CanAddRevision(LoadedDocument);

        /// <summary>
        /// Whether <see cref="LoadedDocument"/>'s Document Type allows new renditions AND
        /// the current user has permission to create them. Delegates to
        /// DocumentStorage.CanAddRendition, the same check ModifyDocument(UpdateType.Rendition)
        /// performs internally before throwing.
        /// </summary>
        public bool IsRenditionable => LoadedDocument != null && storage != null && storage.CanAddRendition(LoadedDocument);

        /// <summary>
        /// The keyword editors for <see cref="LoadedDocument"/>, pre-populated with its
        /// current values. Used by <see cref="ArchivingMode.ModifyMetadata"/>.
        /// </summary>
        public KeywordEditorSet ExistingEditors
        {
            get => existingEditors;
            private set => SetField(ref existingEditors, value);
        }

        /// <summary>
        /// <see cref="LoadedDocument"/>'s document date, editable for
        /// <see cref="ArchivingMode.ModifyMetadata"/>.
        /// </summary>
        public DateTime ExistingDocumentDate
        {
            get => existingDocumentDate;
            set => SetField(ref existingDocumentDate, value);
        }

        /// <summary>
        /// <see cref="LoadedDocument"/>'s document type name, editable for
        /// <see cref="ArchivingMode.ModifyMetadata"/> (re-indexing to a different type).
        /// </summary>
        public string ExistingDocumentTypeName
        {
            get => existingDocumentTypeName;
            set => SetField(ref existingDocumentTypeName, value);
        }

        /// <summary>
        /// File(s) to store as the new revision. Used by <see cref="ArchivingMode.AddRevision"/>.
        /// </summary>
        public ObservableCollection<string> RevisionFiles { get; } = [];

        /// <summary>
        /// File(s) to store as the new rendition. Used by <see cref="ArchivingMode.AddRendition"/>.
        /// </summary>
        public ObservableCollection<string> RenditionFiles { get; } = [];

        /// <summary>
        /// Whether <see cref="DeleteCommand"/> permanently purges the document rather than
        /// a regular (recoverable, until cleared from OnBase Document Maintenance) delete.
        /// Tied directly to <c>DeleteRequest.PurgeDocument</c>. Off by default, this is
        /// the one genuinely irreversible action on this page.
        /// </summary>
        public bool PurgeDocument
        {
            get => purgeDocument;
            set => SetField(ref purgeDocument, value);
        }
        #endregion

        #region Commands
        /// <summary>
        /// (Re)loads Document Type Groups and Document Types for <see cref="ArchivingMode.StoreNew"/>,
        /// connecting first if not already connected.
        /// </summary>
        public AsyncRelayCommand LoadTaxonomyCommand { get; }

        /// <summary>
        /// Sets <see cref="Mode"/> to the value passed as the command parameter.
        /// </summary>
        public RelayCommand SetModeCommand { get; }

        /// <summary>
        /// Opens a file picker and adds the chosen file(s) to <see cref="NewFiles"/>.
        /// </summary>
        public RelayCommand AddNewFilesCommand { get; }

        /// <summary>
        /// Removes the file path passed as the command parameter from <see cref="NewFiles"/>.
        /// </summary>
        public RelayCommand RemoveNewFileCommand { get; }

        /// <summary>
        /// Stores the new document.
        /// </summary>
        public AsyncRelayCommand StoreNewCommand { get; }

        /// <summary>
        /// Loads <see cref="DocumentIdInput"/> for editing, connecting first if not
        /// already connected.
        /// </summary>
        public AsyncRelayCommand LoadDocumentCommand { get; }

        /// <summary>
        /// Applies <see cref="ExistingEditors"/>/<see cref="ExistingDocumentDate"/>/
        /// <see cref="ExistingDocumentTypeName"/> to <see cref="LoadedDocument"/>.
        /// </summary>
        public AsyncRelayCommand ModifyMetadataCommand { get; }

        /// <summary>
        /// Opens a file picker and adds the chosen file(s) to <see cref="RevisionFiles"/>.
        /// </summary>
        public RelayCommand AddRevisionFilesCommand { get; }

        /// <summary>
        /// Removes the file path passed as the command parameter from <see cref="RevisionFiles"/>.
        /// </summary>
        public RelayCommand RemoveRevisionFileCommand { get; }

        /// <summary>
        /// Stores a new revision on <see cref="LoadedDocument"/>.
        /// </summary>
        public AsyncRelayCommand AddRevisionCommand { get; }

        /// <summary>
        /// Opens a file picker and adds the chosen file(s) to <see cref="RenditionFiles"/>.
        /// </summary>
        public RelayCommand AddRenditionFilesCommand { get; }

        /// <summary>
        /// Removes the file path passed as the command parameter from <see cref="RenditionFiles"/>.
        /// </summary>
        public RelayCommand RemoveRenditionFileCommand { get; }

        /// <summary>
        /// Stores a new rendition on <see cref="LoadedDocument"/>'s latest revision.
        /// </summary>
        public AsyncRelayCommand AddRenditionCommand { get; }

        /// <summary>
        /// Deletes (or, if <see cref="PurgeDocument"/> is set, permanently purges)
        /// <see cref="LoadedDocument"/>, after a confirmation prompt.
        /// </summary>
        public AsyncRelayCommand DeleteCommand { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the ArchivingViewModel class
        /// </summary>
        /// <param name="connection">The shared connection state.</param>
        /// <param name="log">The shared output log.</param>
        public ArchivingViewModel(ConnectionViewModel connection, LogViewModel log)
        {
            this.connection = connection;
            this.log = log;

            AllDocumentTypesView = CollectionViewSource.GetDefaultView(AllDocumentTypes);
            AllDocumentTypesView.Filter = FilterDocumentTypeByGroup;

            LoadTaxonomyCommand = new AsyncRelayCommand(_ => LoadTaxonomy(), _ => !IsBusy);
            SetModeCommand = new RelayCommand(m => Mode = (ArchivingMode)m);

            AddNewFilesCommand = new RelayCommand(_ => AddFiles(NewFiles));
            RemoveNewFileCommand = new RelayCommand(p => RemoveFile(NewFiles, p as string));
            StoreNewCommand = new AsyncRelayCommand(_ => StoreNew(), _ => !IsBusy && connection.IsConnected && SelectedDocumentType != null && NewFiles.Count > 0);

            LoadDocumentCommand = new AsyncRelayCommand(_ => LoadDocument(), _ => !IsBusy && !string.IsNullOrEmpty(DocumentIdInput));
            ModifyMetadataCommand = new AsyncRelayCommand(_ => ModifyMetadata(), _ => !IsBusy && connection.IsConnected && HasLoadedDocument);

            AddRevisionFilesCommand = new RelayCommand(_ => AddFiles(RevisionFiles));
            RemoveRevisionFileCommand = new RelayCommand(p => RemoveFile(RevisionFiles, p as string));
            AddRevisionCommand = new AsyncRelayCommand(_ => AddRevision(), _ => !IsBusy && connection.IsConnected && HasLoadedDocument && IsRevisable && RevisionFiles.Count > 0);

            AddRenditionFilesCommand = new RelayCommand(_ => AddFiles(RenditionFiles));
            RemoveRenditionFileCommand = new RelayCommand(p => RemoveFile(RenditionFiles, p as string));
            AddRenditionCommand = new AsyncRelayCommand(_ => AddRendition(), _ => !IsBusy && connection.IsConnected && HasLoadedDocument && IsRenditionable && RenditionFiles.Count > 0);

            DeleteCommand = new AsyncRelayCommand(_ => DeleteDocument(), _ => !IsBusy && connection.IsConnected && HasLoadedDocument);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Loads the given Document ID directly, switching to <see cref="ArchivingMode.ModifyMetadata"/>.
        /// Used by Retrieval's "Edit in Archiving" action.
        /// </summary>
        /// <param name="id">The Document ID to load.</param>
        public async Task LoadDocumentForEditing(long id)
        {
            DocumentIdInput = id.ToString();
            Mode = ArchivingMode.ModifyMetadata;
            await LoadDocument();
        }
        #endregion

        #region Private Methods
        // Only show Document Types belonging to SelectedGroupFilter, when set
        private bool FilterDocumentTypeByGroup(object obj)
        {
            if (SelectedGroupFilter == null) return true;
            if (obj is not DocumentType docType) return false;
            return SelectedGroupFilter.DocumentTypes.Any(dt => dt.ID == docType.ID);
        }

        // Load Document Type Groups and all Document Types, connecting first if needed
        private async Task LoadTaxonomy()
        {
            if (!connection.IsConnected)
            {
                connection.ConnectCommand.Execute(null);
                if (!connection.IsConnected)
                {
                    log.Error("Cannot load: connect attempt failed, see the error above.");
                    return;
                }
            }

            IsBusy = true;
            DocumentTypeGroupFilters.Clear();
            AllDocumentTypes.Clear();

            try
            {
                var app = connection.CurrentApplication;
                var taxonomy = new OnBaseTaxonomy(app);

                var groups = await Task.Run(() => taxonomy.GetDocumentTypeGroups(app: app));
                var docTypes = await Task.Run(() => taxonomy.GetDocumentTypes((string[])null, app));

                DocumentTypeGroupFilters.Add(null);
                if (groups != null) foreach (var group in groups) DocumentTypeGroupFilters.Add(group);
                if (docTypes != null) foreach (var docType in docTypes) AllDocumentTypes.Add(docType);

                log.Success($"Loaded {AllDocumentTypes.Count} document type(s), {DocumentTypeGroupFilters.Count - 1} group(s).");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Open a file picker and add the chosen file(s) to the given collection
        private void AddFiles(ObservableCollection<string> target)
        {
            var dialog = new OpenFileDialog { Multiselect = true, Filter = "All Files (*.*)|*.*" };
            if (dialog.ShowDialog() != true) return;
            #pragma warning disable S3267 // LINQ unnecessary here
            foreach (var path in dialog.FileNames) if (!target.Contains(path)) target.Add(path);
            #pragma warning restore S3267
        }

        // Remove a file path from the given collection
        private static void RemoveFile(ObservableCollection<string> target, string path)
        {
            if (path != null) target.Remove(path);
        }

        // Convert a KeywordEditorSet's currently-filled-in fields into the
        // KeywordGroup/KeywordInfo lists StorageRequest expects. Blank fields/instances
        // are skipped entirely, not sent as empty keyword records.
        private static (List<KeywordGroup> Groups, List<KeywordInfo> Keywords) BuildKeywordLists(KeywordEditorSet editors)
        {
            var groups = new List<KeywordGroup>();
            foreach (var groupEditor in editors.Groups)
            {
                foreach (var instance in groupEditor.Instances)
                {
                    var filled = instance.Fields.Where(f => !string.IsNullOrWhiteSpace(f.Value)).ToList();
                    if (filled.Count == 0) continue;

                    groups.Add(new KeywordGroup
                    {
                        Id = groupEditor.GroupType.ID,
                        Name = groupEditor.Name,
                        MultiInstance = groupEditor.IsMultiInstance,
                        Keywords = [.. filled.Select(f => new KeywordInfo { Id = f.Id, Name = f.Name, Value = f.Value, Type = f.DataType, Length = f.Length })]
                    });
                }
            }

            var keywords = new List<KeywordInfo>();
            foreach (var standaloneEditor in editors.Standalone)
            {
                foreach (var field in standaloneEditor.Values.Where(f => !string.IsNullOrWhiteSpace(f.Value)))
                {
                    keywords.Add(new KeywordInfo { Id = field.Id, Name = field.Name, Value = field.Value, Type = field.DataType, Length = field.Length });
                }
            }

            return (groups, keywords);
        }

        // Store the new document
        private async Task StoreNew()
        {
            IsBusy = true;
            try
            {
                var app = connection.CurrentApplication;
                storage = new DocumentStorage(app);

                var (groups, keywords) = BuildKeywordLists(NewEditors);

                var request = new NewDocumentRequest(StorageType.Document)
                {
                    DocumentType = SelectedDocumentType.Name,
                    DocumentDate = NewDocumentDate,
                    Files = [.. NewFiles],
                    KeywordGroups = groups,
                    Keywords = keywords
                };

                var doc = await Task.Run(() => storage.CreateDocument(request, app));

                log.Success($"Stored new document [{doc.ID}].");
                NewFiles.Clear();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Load a document for editing, connecting first if needed
        private async Task LoadDocument()
        {
            if (!connection.IsConnected)
            {
                connection.ConnectCommand.Execute(null);
                if (!connection.IsConnected)
                {
                    log.Error("Cannot load: connect attempt failed, see the error above.");
                    return;
                }
            }

            if (!long.TryParse(DocumentIdInput, out var id))
            {
                log.Error($"[{DocumentIdInput}] is not a valid Document ID.");
                return;
            }

            IsBusy = true;
            LoadedDocument = null;
            ExistingEditors = null;

            try
            {
                var app = connection.CurrentApplication;
                storage = new DocumentStorage(app);
                var retrieval = new DocumentRetrieval(app, new Metadata(app));

                var doc = await Task.Run(() => retrieval.GetDocumentById(id, app));

                if (doc == null)
                {
                    log.Error($"No document found with ID [{id}].");
                    return;
                }

                LoadedDocument = doc;
                ExistingEditors = new KeywordEditorSet(doc);
                ExistingDocumentDate = doc.DocumentDate;
                ExistingDocumentTypeName = doc.DocumentType.Name;

                log.Success($"Loaded document [{id}] for editing. Revisable: {IsRevisable}, Renditionable: {IsRenditionable}.");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Apply metadata changes to LoadedDocument
        private async Task ModifyMetadata()
        {
            IsBusy = true;
            try
            {
                var app = connection.CurrentApplication;
                storage = new DocumentStorage(app);

                var (groups, keywords) = BuildKeywordLists(ExistingEditors);

                var request = new UpdateDocumentRequest(UpdateType.Metadata, StorageType.Document)
                {
                    DocumentId = LoadedDocument.ID,
                    DocumentType = ExistingDocumentTypeName,
                    DocumentDate = ExistingDocumentDate,
                    OverwriteKeywords = true,
                    KeywordGroups = groups,
                    Keywords = keywords
                };

                await Task.Run(() => storage.ModifyDocument(request, app));

                log.Success($"Updated metadata on document [{LoadedDocument.ID}].");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Store a new revision on LoadedDocument
        private async Task AddRevision()
        {
            IsBusy = true;
            try
            {
                var app = connection.CurrentApplication;
                storage = new DocumentStorage(app);

                var request = new UpdateDocumentRequest(UpdateType.Revision, StorageType.Document)
                {
                    DocumentId = LoadedDocument.ID,
                    DocumentType = LoadedDocument.DocumentType.Name,
                    Files = [.. RevisionFiles]
                };

                await Task.Run(() => storage.ModifyDocument(request, app));

                log.Success($"Added a new revision to document [{LoadedDocument.ID}].");
                RevisionFiles.Clear();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Store a new rendition on LoadedDocument's latest revision
        private async Task AddRendition()
        {
            IsBusy = true;
            try
            {
                var app = connection.CurrentApplication;
                storage = new DocumentStorage(app);

                var request = new UpdateDocumentRequest(UpdateType.Rendition, StorageType.Document)
                {
                    DocumentId = LoadedDocument.ID,
                    DocumentType = LoadedDocument.DocumentType.Name,
                    Files = [.. RenditionFiles]
                };

                await Task.Run(() => storage.ModifyDocument(request, app));

                log.Success($"Added a new rendition to document [{LoadedDocument.ID}]'s latest revision.");
                RenditionFiles.Clear();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Delete (or purge) LoadedDocument, after a confirmation prompt
        private async Task DeleteDocument()
        {
            var action = PurgeDocument ? "permanently purge" : "delete";
            var result = MessageBox.Show(
                $"Are you sure you want to {action} document [{LoadedDocument.ID}]?" +
                (PurgeDocument ? "\n\nThis cannot be undone." : "\n\nThis can be undone from OnBase Document Maintenance until it's permanently cleared."),
                "Confirm Delete", MessageBoxButton.YesNo,
                PurgeDocument ? MessageBoxImage.Warning : MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                log.Info("Delete cancelled.");
                return;
            }

            IsBusy = true;
            try
            {
                var app = connection.CurrentApplication;
                storage = new DocumentStorage(app);

                var documentId = LoadedDocument.ID;

                var request = new DeleteRequest
                {
                    DocumentId = documentId,
                    PurgeDocument = PurgeDocument
                };

                await Task.Run(() => storage.DeleteDocument(request, app));

                log.Success($"Document [{documentId}] {(PurgeDocument ? "purged" : "deleted")}.");

                // The document no longer exists, clear everything that referenced it.
                LoadedDocument = null;
                ExistingEditors = null;
                DocumentIdInput = null;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                IsBusy = false;
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
