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
using System.Linq;
using System.Threading.Tasks;
using Hyland.Unity;
using Hyland.Unity.UnityForm;
using Unity._02.AccessingTaxonomy.HelperClasses.OnBase;
#endregion

namespace Unity.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: this mirrors the original WinForms harness's cascading
     * combo-box behavior (SelDocTypeGroup_SelectedIndexChanged populating SelDocType,
     * etc.), rather than a flat "pick a lookup type, search" design: selecting a
     * Document Type Group loads its Document Types, selecting a Document Type loads its
     * Keyword Group Types (and its Standalone Keywords, in parallel, see below),
     * selecting a Keyword Group Type loads its own Keyword Types. Every level binds
     * directly to the real Hyland.Unity types (DocumentTypeGroup, DocumentType,
     * KeywordRecordType, KeywordType) rather than wrapper DTOs, this is a tool for
     * exercising the Unity API directly, wrapping it would obscure exactly what's being
     * tested.
     *
     * Not every keyword belongs to a named group: DocumentType.KeywordRecordTypes
     * includes a "StandAlone" pseudo-group (RecordType.StandAlone) representing
     * keywords that aren't part of any MultiInstance/SingleInstance group at all, the
     * same distinction DocumentStorage.cs/DocumentRetrieval.cs check for when reading a
     * document's actual KeywordRecords. Selecting a Document Type splits
     * GetKeywordGroupTypes(null, docType) into two: the StandAlone entry's own
     * KeywordTypes populate StandaloneKeywordTypes directly (no extra click needed),
     * while every OTHER (named) group populates KeywordGroupTypes, for drilling further
     * into via SelectedKeywordGroupType.
     *
     * Custom Queries, File Types, and Unity Forms have no equivalent parent/child
     * relationship, they're flat lookups, kept in their own separate sections rather
     * than forced into this hierarchy.
     *
     * LoadCommand is enabled regardless of current connection state: if not already
     * connected, Load() attempts to connect first (using whatever's currently configured
     * on Settings/Connect), then proceeds with the actual taxonomy load only if that
     * succeeds. FindFileTypeCommand/FindUnityFormCommand still require an existing
     * connection, they're reached far less often as a genuine "first thing you click"
     * entry point the way Load is.
     */
    #endregion

    /// <summary>
    /// Browses OnBase's taxonomy hierarchically: Document Type Groups, their Document
    /// Types, and each Document Type's Keyword Group Types/Standalone Keywords/(further)
    /// Keyword Types, plus flat Custom Query/File Type/Unity Form lookups.
    /// </summary>
    public class TaxonomyViewModel : ViewModelBase
    {
        #region Private Members
        private readonly ConnectionViewModel connection;
        private readonly LogViewModel log;
        private readonly OnBaseTaxonomy taxonomy = new OnBaseTaxonomy();

        private DocumentTypeGroup selectedDocumentTypeGroup;
        private DocumentType selectedDocumentType;
        private KeywordRecordType selectedKeywordGroupType;
        private CustomQuery selectedCustomQuery;
        private string fileTypeSearchInput;
        private FileType foundFileType;
        private string unityFormSearchInput;
        private FormTemplate foundUnityForm;
        private bool isLoading;
        #endregion

        #region Properties
        /// <summary>
        /// Every Document Type Group in OnBase.
        /// </summary>
        public ObservableCollection<DocumentTypeGroup> DocumentTypeGroups { get; } = new ObservableCollection<DocumentTypeGroup>();

        /// <summary>
        /// The currently-selected Document Type Group. Setting this loads its
        /// <see cref="DocumentTypes"/>.
        /// </summary>
        public DocumentTypeGroup SelectedDocumentTypeGroup
        {
            get => selectedDocumentTypeGroup;
            set
            {
                if (!SetField(ref selectedDocumentTypeGroup, value)) return;
                _ = LoadDocumentTypes();
            }
        }

        /// <summary>
        /// The Document Types belonging to <see cref="SelectedDocumentTypeGroup"/>.
        /// </summary>
        public ObservableCollection<DocumentType> DocumentTypes { get; } = new ObservableCollection<DocumentType>();

        /// <summary>
        /// The currently-selected Document Type. Setting this loads its
        /// <see cref="KeywordGroupTypes"/> and <see cref="StandaloneKeywordTypes"/>.
        /// </summary>
        public DocumentType SelectedDocumentType
        {
            get => selectedDocumentType;
            set
            {
                if (!SetField(ref selectedDocumentType, value)) return;
                _ = LoadKeywordGroupTypesAndStandalone();
            }
        }

        /// <summary>
        /// The NAMED (MultiInstance/SingleInstance) Keyword Group Types on
        /// <see cref="SelectedDocumentType"/>. Does not include the StandAlone
        /// pseudo-group, see <see cref="StandaloneKeywordTypes"/> for that.
        /// </summary>
        public ObservableCollection<KeywordRecordType> KeywordGroupTypes { get; } = new ObservableCollection<KeywordRecordType>();

        /// <summary>
        /// The Keyword Types on <see cref="SelectedDocumentType"/> that don't belong to
        /// any named group (the StandAlone pseudo-group's own Keyword Types), populated
        /// as soon as a Document Type is selected, no further click needed.
        /// </summary>
        public ObservableCollection<KeywordType> StandaloneKeywordTypes { get; } = new ObservableCollection<KeywordType>();

        /// <summary>
        /// The currently-selected Keyword Group Type. Setting this loads its
        /// <see cref="GroupKeywordTypes"/>.
        /// </summary>
        public KeywordRecordType SelectedKeywordGroupType
        {
            get => selectedKeywordGroupType;
            set
            {
                if (!SetField(ref selectedKeywordGroupType, value)) return;
                _ = LoadGroupKeywordTypes();
            }
        }

        /// <summary>
        /// The Keyword Types belonging to <see cref="SelectedKeywordGroupType"/>.
        /// </summary>
        public ObservableCollection<KeywordType> GroupKeywordTypes { get; } = new ObservableCollection<KeywordType>();

        /// <summary>
        /// Every Custom Query in OnBase (flat, no children).
        /// </summary>
        public ObservableCollection<CustomQuery> CustomQueries { get; } = new ObservableCollection<CustomQuery>();

        /// <summary>
        /// The currently-selected Custom Query, for display only (Custom Queries have no
        /// children in this hierarchy).
        /// </summary>
        public CustomQuery SelectedCustomQuery
        {
            get => selectedCustomQuery;
            set => SetField(ref selectedCustomQuery, value);
        }

        /// <summary>
        /// The extension or numeric ID to look up a File Type by.
        /// </summary>
        public string FileTypeSearchInput
        {
            get => fileTypeSearchInput;
            set => SetField(ref fileTypeSearchInput, value);
        }

        /// <summary>
        /// The File Type found by <see cref="FindFileTypeCommand"/>, or <see langword="null"/>.
        /// </summary>
        public FileType FoundFileType
        {
            get => foundFileType;
            private set => SetField(ref foundFileType, value);
        }

        /// <summary>
        /// The name or numeric ID to look up a Unity Form template by.
        /// </summary>
        public string UnityFormSearchInput
        {
            get => unityFormSearchInput;
            set => SetField(ref unityFormSearchInput, value);
        }

        /// <summary>
        /// The Unity Form template found by <see cref="FindUnityFormCommand"/>, or <see langword="null"/>.
        /// </summary>
        public FormTemplate FoundUnityForm
        {
            get => foundUnityForm;
            private set => SetField(ref foundUnityForm, value);
        }

        /// <summary>
        /// Whether a taxonomy lookup is currently in progress.
        /// </summary>
        public bool IsLoading
        {
            get => isLoading;
            private set => SetField(ref isLoading, value);
        }
        #endregion

        #region Commands
        /// <summary>
        /// (Re)loads <see cref="DocumentTypeGroups"/> and <see cref="CustomQueries"/>
        /// from the current connection.
        /// </summary>
        public AsyncRelayCommand LoadCommand { get; }

        /// <summary>
        /// Looks up a File Type by <see cref="FileTypeSearchInput"/>.
        /// </summary>
        public AsyncRelayCommand FindFileTypeCommand { get; }

        /// <summary>
        /// Looks up a Unity Form template by <see cref="UnityFormSearchInput"/>.
        /// </summary>
        public AsyncRelayCommand FindUnityFormCommand { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the TaxonomyViewModel class
        /// </summary>
        /// <param name="connection">The shared connection state.</param>
        /// <param name="log">The shared output log.</param>
        public TaxonomyViewModel(ConnectionViewModel connection, LogViewModel log)
        {
            this.connection = connection;
            this.log = log;

            LoadCommand = new AsyncRelayCommand(_ => Load(), _ => !IsLoading);
            FindFileTypeCommand = new AsyncRelayCommand(_ => FindFileType(),
                _ => !IsLoading && connection.IsConnected && !string.IsNullOrEmpty(FileTypeSearchInput));
            FindUnityFormCommand = new AsyncRelayCommand(_ => FindUnityForm(),
                _ => !IsLoading && connection.IsConnected && !string.IsNullOrEmpty(UnityFormSearchInput));
        }
        #endregion

        #region Private Methods
        // Load the top-level Document Type Groups and Custom Queries, connecting first
        // (using whatever's currently configured on Settings/Connect) if not already
        // connected
        private async Task Load()
        {
            if (!connection.IsConnected)
            {
                // Connect() itself runs synchronously (a plain, already-established
                // RelayCommand elsewhere in this app), called here before any await, so
                // this still executes on the calling (UI) thread, exactly like clicking
                // the Connect button on the Connect page directly would.
                connection.ConnectCommand.Execute(null);

                if (!connection.IsConnected)
                {
                    log.Error("Cannot load taxonomy: connect attempt failed, see the error above.");
                    return;
                }
            }

            IsLoading = true;
            DocumentTypeGroups.Clear();
            DocumentTypes.Clear();
            KeywordGroupTypes.Clear();
            StandaloneKeywordTypes.Clear();
            GroupKeywordTypes.Clear();
            CustomQueries.Clear();

            try
            {
                var app = connection.CurrentApplication;

                var groups = await Task.Run(() => taxonomy.GetDocumentTypeGroups(app: app));
                var queries = await Task.Run(() => taxonomy.GetCustomQueries(app: app));

                if (groups != null) foreach (var group in groups) DocumentTypeGroups.Add(group);
                if (queries != null) foreach (var query in queries) CustomQueries.Add(query);

                log.Success($"Loaded {DocumentTypeGroups.Count} document type group(s), {CustomQueries.Count} custom quer{(CustomQueries.Count == 1 ? "y" : "ies")}.");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Load the Document Types belonging to SelectedDocumentTypeGroup
        private async Task LoadDocumentTypes()
        {
            DocumentTypes.Clear();
            KeywordGroupTypes.Clear();
            StandaloneKeywordTypes.Clear();
            GroupKeywordTypes.Clear();
            SelectedDocumentType = null;

            if (SelectedDocumentTypeGroup == null) return;

            IsLoading = true;
            try
            {
                var app = connection.CurrentApplication;
                var groupName = SelectedDocumentTypeGroup.Name;

                var docTypes = await Task.Run(() => taxonomy.GetDocumentTypes(groupName, app));
                if (docTypes != null) foreach (var docType in docTypes) DocumentTypes.Add(docType);

                log.Success($"Loaded {DocumentTypes.Count} document type(s) in group [{groupName}].");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Load KeywordGroupTypes (named groups) and StandaloneKeywordTypes for SelectedDocumentType
        private async Task LoadKeywordGroupTypesAndStandalone()
        {
            KeywordGroupTypes.Clear();
            StandaloneKeywordTypes.Clear();
            GroupKeywordTypes.Clear();
            SelectedKeywordGroupType = null;

            if (SelectedDocumentType == null) return;

            IsLoading = true;
            try
            {
                var app = connection.CurrentApplication;
                var docType = SelectedDocumentType;

                var allGroups = await Task.Run(() => taxonomy.GetKeywordGroupTypes(docType: docType, app: app));

                foreach (var group in allGroups ?? new List<KeywordRecordType>())
                {
                    if (group.RecordType == RecordType.StandAlone)
                    {
                        foreach (var keyType in group.KeywordTypes) StandaloneKeywordTypes.Add(keyType);
                    }
                    else
                    {
                        KeywordGroupTypes.Add(group);
                    }
                }

                log.Success($"Loaded {KeywordGroupTypes.Count} keyword group(s), {StandaloneKeywordTypes.Count} standalone keyword(s) on document type [{docType.Name}].");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Load the Keyword Types belonging to SelectedKeywordGroupType
        private async Task LoadGroupKeywordTypes()
        {
            GroupKeywordTypes.Clear();

            if (SelectedKeywordGroupType == null) return;

            IsLoading = true;
            try
            {
                var group = SelectedKeywordGroupType;

                // Materialize off the UI thread first (KeywordTypes may itself trigger a
                // lazy network round-trip depending on the Unity API's own
                // implementation), then populate the ObservableCollection back on the UI
                // thread once this resumes.
                var keywordTypes = await Task.Run(() => group.KeywordTypes.ToList());

                foreach (var keyType in keywordTypes) GroupKeywordTypes.Add(keyType);

                log.Success($"Loaded {GroupKeywordTypes.Count} keyword type(s) in group [{group.Name}].");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Look up a File Type by extension or numeric ID
        private async Task FindFileType()
        {
            IsLoading = true;
            try
            {
                var app = connection.CurrentApplication;
                var input = FileTypeSearchInput;

                FoundFileType = long.TryParse(input, out var id)
                    ? await Task.Run(() => taxonomy.GetFileType(id, app))
                    : await Task.Run(() => taxonomy.GetFileType(input, app));

                log.Success(FoundFileType != null
                    ? $"Found file type [{FoundFileType.Name}] (ID {FoundFileType.ID})."
                    : $"No file type found for [{input}].");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Look up a Unity Form template by name or numeric ID
        private async Task FindUnityForm()
        {
            IsLoading = true;
            try
            {
                var app = connection.CurrentApplication;
                var input = UnityFormSearchInput;

                FoundUnityForm = await Task.Run(() => taxonomy.GetUnityForm(input, app));

                log.Success(FoundUnityForm != null
                    ? $"Found unity form [{FoundUnityForm.Name}] (ID {FoundUnityForm.ID})."
                    : $"No unity form found for [{input}].");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                IsLoading = false;
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
