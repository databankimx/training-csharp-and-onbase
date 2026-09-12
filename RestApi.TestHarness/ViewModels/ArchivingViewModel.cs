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
using Microsoft.Win32;
using RestApi._02.AccessingTaxonomy.HelperClasses.OnBase;
using RestApi._02.AccessingTaxonomy.Models.Objects;
using RestApi._03.DocumentRetrieval.HelperClasses.OnBase;
using RestApi._03.DocumentRetrieval.Models.Objects;
using RestApi._04.DocumentArchiving.HelperClasses.OnBase;
using RestApi._04.DocumentArchiving.Models.Enumerations;
using RestApi._04.DocumentArchiving.Models.Objects;
using RestApi.TestHarness.Models;
#endregion

namespace RestApi.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness's own
     * ArchivingViewModel, scoped to conventional documents only, matching RestApi.04's
     * own scoping decision (see its .csproj Training Notes).
     *
     * No LoadedDocument (a live Hyland.Unity.Document): loadedDocumentId/
     * loadedDocumentTypeId/LoadedDocumentTypeName (plain strings) replace it, matching
     * how DocumentDetailViewModel holds currentDocumentId instead of a raw Document too.
     *
     * IsRevisable/IsRenditionable are hardcoded true whenever a document is loaded, not
     * delegated to a CanAddRevision/CanAddRendition check: confirmed there's no REST
     * equivalent of that pre-flight check at all (see RestApi.04's own LectureNotes.md).
     * Add Revision/Add Rendition are always offered once a document is loaded; a
     * disallowed attempt surfaces as an ordinary failed-request error instead.
     *
     * No PurgeDocument: confirmed no REST equivalent exists either (RestApi.04's own
     * DeleteRequest has no such property, "purge" appears nowhere in document-api.json).
     * DeleteCommand still confirms first (still the one irreversible action on this
     * page), just without the purge-specific wording/checkbox.
     *
     * SelectedDocumentType's setter and LoadDocument() both kick off genuinely async work
     * (KeywordEditorSet.CreateForDocumentTypeAsync/CreateForDocumentAsync make real HTTP
     * calls), the same "_ = SomeAsync();" fire-and-forget pattern used elsewhere in this
     * app for property setters that can't themselves be async.
     */
    #endregion

    /// <summary>
    /// The Archiving page: store a new document, or modify/add a revision/add a
    /// rendition to an existing one, or delete one.
    /// </summary>
    public class ArchivingViewModel : ViewModelBase
    {
        #region Private Members
        private readonly ConnectionViewModel connection;
        private readonly LogViewModel log;

        private ArchivingMode mode = ArchivingMode.StoreNew;
        private bool isBusy;

        // Store New
        private DocumentTypeGroup selectedGroupFilter;
        private DocumentType selectedDocumentType;
        private KeywordEditorSet newEditors;
        private DateTime newDocumentDate = DateTime.Today;

        // Existing document (Modify/AddRevision/AddRendition/Delete)
        private string documentIdInput;
        private string loadedDocumentId;

        #pragma warning disable S1450 // Keeping global for future extensibility
        private string loadedDocumentTypeId;
        #pragma warning restore S1450
        private string loadedDocumentTypeName;
        private KeywordEditorSet existingEditors;
        private DateTime existingDocumentDate;
        private string existingDocumentTypeName;
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
                _ = LoadNewEditorsAsync();
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
        /// File(s) to store as the new document, added via <see cref="AddNewFilesCommand"/>.
        /// </summary>
        public ObservableCollection<string> NewFiles { get; } = [];

        // --- Existing document (Modify/AddRevision/AddRendition/Delete) ---

        /// <summary>
        /// The Document ID to load for editing.
        /// </summary>
        public string DocumentIdInput
        {
            get => documentIdInput;
            set => SetField(ref documentIdInput, value);
        }

        /// <summary>
        /// Whether a document is currently loaded.
        /// </summary>
        public bool HasLoadedDocument => loadedDocumentId != null;

        /// <summary>
        /// The loaded document's ID, or <see langword="null"/> if none is loaded.
        /// </summary>
        public string LoadedDocumentId => loadedDocumentId;

        /// <summary>
        /// The loaded document's Document Type name, or <see langword="null"/> if none is loaded.
        /// </summary>
        public string LoadedDocumentTypeName => loadedDocumentTypeName;

        /// <summary>
        /// Whether <see cref="ArchivingMode.AddRevision"/> is offered. Confirmed no REST
        /// equivalent of a pre-flight Revisable check exists, so this is simply "is a
        /// document loaded at all" (see this class's own Training Notes).
        /// </summary>
        public bool IsRevisable => HasLoadedDocument;

        /// <summary>
        /// Whether <see cref="ArchivingMode.AddRendition"/> is offered. Confirmed no REST
        /// equivalent of a pre-flight Renditionable check exists, so this is simply "is a
        /// document loaded at all" (see this class's own Training Notes).
        /// </summary>
        public bool IsRenditionable => HasLoadedDocument;

        /// <summary>
        /// The keyword editors for the loaded document, pre-populated with its current
        /// values. Used by <see cref="ArchivingMode.ModifyMetadata"/>.
        /// </summary>
        public KeywordEditorSet ExistingEditors
        {
            get => existingEditors;
            private set => SetField(ref existingEditors, value);
        }

        /// <summary>
        /// The loaded document's document date, editable for <see cref="ArchivingMode.ModifyMetadata"/>.
        /// </summary>
        public DateTime ExistingDocumentDate
        {
            get => existingDocumentDate;
            set => SetField(ref existingDocumentDate, value);
        }

        /// <summary>
        /// The loaded document's document type name, editable for
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
        /// <see cref="ExistingDocumentTypeName"/> to the loaded document.
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
        /// Stores a new revision on the loaded document.
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
        /// Stores a new rendition on the loaded document's latest revision.
        /// </summary>
        public AsyncRelayCommand AddRenditionCommand { get; }

        /// <summary>
        /// Deletes the loaded document, after a confirmation prompt.
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
        public async Task LoadDocumentForEditing(string id)
        {
            DocumentIdInput = id;
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
            return docType.DocumentTypeGroupId == SelectedGroupFilter.Id;
        }

        // Load Document Type Groups and all Document Types, connecting first if needed
        private async Task LoadTaxonomy()
        {
            if (!connection.IsConnected)
            {
                await connection.ConnectAsync();
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
                var http = connection.GetHttpClient();
                var groups = await OnBaseTaxonomy.GetDocumentTypeGroupsAsync(client: http);
                var docTypes = await OnBaseTaxonomy.GetDocumentTypesAsync(client: http);

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

        // Build NewEditors for the newly-selected SelectedDocumentType
        private async Task LoadNewEditorsAsync()
        {
            if (SelectedDocumentType == null)
            {
                NewEditors = null;
                return;
            }

            try
            {
                NewEditors = await KeywordEditorSet.CreateForDocumentTypeAsync(SelectedDocumentType.Id, connection.GetHttpClient());
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        // Open a file picker and add the chosen file(s) to the given collection
        private static void AddFiles(ObservableCollection<string> target)
        {
            var dialog = new OpenFileDialog { Multiselect = true, Filter = "All Files (*.*)|*.*" };
            if (dialog.ShowDialog() != true) return;
            foreach (var path in from path in dialog.FileNames
                                 where !target.Contains(path)
                                 select path)
            {
                target.Add(path);
            }
        }

        // Remove a file path from the given collection
        private static void RemoveFile(ObservableCollection<string> target, string path)
        {
            if (path != null) target.Remove(path);
        }

        // Convert a KeywordEditorSet's currently-filled-in fields into the
        // KeywordGroup/KeywordInfo lists RestApi.04's own request objects expect. Blank
        // fields/instances are skipped entirely, not sent as empty keyword records. A
        // NEW instance has no GroupId yet (the server assigns one), only TypeGroupId.
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
                        TypeGroupId = groupEditor.GroupType.Id,
                        Name = groupEditor.Name,
                        MultiInstance = groupEditor.IsMultiInstance,
                        Keywords = [.. filled.Select(f => new KeywordInfo { Id = f.Id, Name = f.Name, Value = f.Value })]
                    });
                }
            }

            var keywords = new List<KeywordInfo>();
            foreach (var standaloneEditor in editors.Standalone)
            {
                foreach (var field in standaloneEditor.Values.Where(f => !string.IsNullOrWhiteSpace(f.Value)))
                {
                    keywords.Add(new KeywordInfo { Id = field.Id, Name = field.Name, Value = field.Value });
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
                var (groups, keywords) = BuildKeywordLists(NewEditors);

                var request = new NewDocumentRequest
                {
                    DocumentType = SelectedDocumentType.Id,
                    DocumentDate = NewDocumentDate,
                    Files = [.. NewFiles],
                    KeywordGroups = groups,
                    Keywords = keywords
                };

                var newId = await DocumentStorage.CreateDocumentAsync(request, connection.GetHttpClient());

                log.Success($"Stored new document [{newId}].");
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
                await connection.ConnectAsync();
                if (!connection.IsConnected)
                {
                    log.Error("Cannot load: connect attempt failed, see the error above.");
                    return;
                }
            }

            var id = DocumentIdInput;
            if (!long.TryParse(id, out _))
            {
                log.Error($"[{id}] is not a valid Document ID.");
                return;
            }

            IsBusy = true;
            loadedDocumentId = null;
            loadedDocumentTypeId = null;
            loadedDocumentTypeName = null;
            OnPropertyChanged(nameof(HasLoadedDocument));
            OnPropertyChanged(nameof(LoadedDocumentId));
            OnPropertyChanged(nameof(LoadedDocumentTypeName));
            OnPropertyChanged(nameof(IsRevisable));
            OnPropertyChanged(nameof(IsRenditionable));
            ExistingEditors = null;

            try
            {
                var http = connection.GetHttpClient();
                var docInfo = await DocumentRetrieval.GetDocumentInfoAsync(id, http);

                if (docInfo == null)
                {
                    log.Error($"No document found with ID [{id}].");
                    return;
                }

                var docType = await OnBaseTaxonomy.GetDocumentTypeAsync(docInfo.Type, http);

                loadedDocumentId = id;
                loadedDocumentTypeId = docType?.Id;
                loadedDocumentTypeName = docInfo.Type;
                OnPropertyChanged(nameof(HasLoadedDocument));
                OnPropertyChanged(nameof(LoadedDocumentId));
                OnPropertyChanged(nameof(LoadedDocumentTypeName));
                OnPropertyChanged(nameof(IsRevisable));
                OnPropertyChanged(nameof(IsRenditionable));

                ExistingEditors = await KeywordEditorSet.CreateForDocumentAsync(id, loadedDocumentTypeId, http);
                ExistingDocumentDate = docInfo.DocumentDate;
                ExistingDocumentTypeName = docInfo.Type;

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

        // Apply metadata changes to the loaded document
        private async Task ModifyMetadata()
        {
            IsBusy = true;
            try
            {
                var (groups, keywords) = BuildKeywordLists(ExistingEditors);

                var request = new UpdateDocumentRequest
                {
                    UpdateType = UpdateType.Metadata,
                    DocumentId = loadedDocumentId,
                    DocumentType = ExistingDocumentTypeName,
                    DocumentDate = ExistingDocumentDate,
                    KeywordGroups = groups,
                    Keywords = keywords
                };

                await DocumentStorage.ModifyDocumentAsync(request, connection.GetHttpClient());

                log.Success($"Updated metadata on document [{loadedDocumentId}].");
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

        // Store a new revision on the loaded document
        private async Task AddRevision()
        {
            IsBusy = true;
            try
            {
                var request = new UpdateDocumentRequest
                {
                    UpdateType = UpdateType.Revision,
                    DocumentId = loadedDocumentId,
                    Files = [.. RevisionFiles]
                };

                await DocumentStorage.ModifyDocumentAsync(request, connection.GetHttpClient());

                log.Success($"Added a new revision to document [{loadedDocumentId}].");
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

        // Store a new rendition on the loaded document's latest revision
        private async Task AddRendition()
        {
            IsBusy = true;
            try
            {
                var request = new UpdateDocumentRequest
                {
                    UpdateType = UpdateType.Rendition,
                    DocumentId = loadedDocumentId,
                    Files = [.. RenditionFiles]
                };

                await DocumentStorage.ModifyDocumentAsync(request, connection.GetHttpClient());

                log.Success($"Added a new rendition to document [{loadedDocumentId}]'s latest revision.");
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

        // Delete the loaded document, after a confirmation prompt
        private async Task DeleteDocument()
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete document [{loadedDocumentId}]?\n\nThis can be undone from OnBase Document Maintenance until it's permanently cleared.",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                log.Info("Delete cancelled.");
                return;
            }

            IsBusy = true;
            try
            {
                var documentId = loadedDocumentId;
                var request = new DeleteRequest { DocumentId = documentId };

                await DocumentStorage.DeleteDocumentAsync(request, connection.GetHttpClient());

                log.Success($"Document [{documentId}] deleted.");

                // The document no longer exists, clear everything that referenced it.
                loadedDocumentId = null;
                loadedDocumentTypeId = null;
                loadedDocumentTypeName = null;
                OnPropertyChanged(nameof(HasLoadedDocument));
                OnPropertyChanged(nameof(LoadedDocumentId));
                OnPropertyChanged(nameof(LoadedDocumentTypeName));
                OnPropertyChanged(nameof(IsRevisable));
                OnPropertyChanged(nameof(IsRenditionable));
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
