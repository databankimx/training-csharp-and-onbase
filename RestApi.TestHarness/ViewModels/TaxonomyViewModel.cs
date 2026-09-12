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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RestApi._02.AccessingTaxonomy.HelperClasses.OnBase;
using RestApi._02.AccessingTaxonomy.Models.Objects;
#endregion

namespace RestApi.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness's own
     * TaxonomyViewModel. Same cascading-selection design (Document Type Group ->
     * Document Types -> Keyword Group Types/Standalone Keywords), plus flat Custom
     * Query/File Type/Unity Form lookups, binding directly to RestApi.02's own plain DTOs
     * (DocumentTypeGroup, DocumentType, etc.) rather than Hyland.Unity's SOAP-backed
     * types, but the same "bind to the real thing, don't wrap it further" philosophy.
     *
     * One genuine simplification versus Unity.02: RestApi.02's own
     * GetDocumentTypeKeywordGroupsAsync already fully resolves every group's KeywordTypes
     * up front (see its own Training Notes for why: the REST API's raw keyword-type-group
     * structure only gives bare ids, that resolution work already happens once, inside
     * that one call). So there's no LoadGroupKeywordTypes step here the way Unity's
     * version has one, selecting a KeywordGroupTypes entry just displays its own
     * already-populated KeywordTypes directly (bound in the View to
     * SelectedKeywordGroupType.KeywordTypes), no further async call needed.
     *
     * FindUnityFormCommand now genuinely works (Unity Forms DO have a REST equivalent,
     * see RestApi.02's own corrected LectureNotes.md), it's not a stub. No changes needed
     * to FindFileTypeCommand's own shape.
     */
    #endregion

    /// <summary>
    /// Browses OnBase's taxonomy hierarchically: Document Type Groups, their Document
    /// Types, and each Document Type's Keyword Group Types/Standalone Keywords, plus flat
    /// Custom Query/File Type/Unity Form lookups.
    /// </summary>
    public class TaxonomyViewModel : ViewModelBase
    {
        #region Private Members
        // View model for connecting to OnBase and holding the current connection state, shared across the app
        private readonly ConnectionViewModel connection;

        // View model for logging output messages, shared across the app
        private readonly LogViewModel log;

        // Selected document type group (filters document types)
        private DocumentTypeGroup selectedDocumentTypeGroup;

        // Selected document type (filters keyword group types and standalone keywords)
        private DocumentType selectedDocumentType;

        // Selected keyword group type (its own KeywordTypes are already resolved, see this class's own Training Notes)
        private DocumentTypeKeywordGroup selectedKeywordGroupType;

        // Selected custom query (for display only, no children)
        private CustomQuery selectedCustomQuery;

        // Input for searching a file type by extension or numeric ID
        private string fileTypeSearchInput;

        // The file type found by the search, or null if not found
        private FileType foundFileType;

        // Input for searching a Unity Form template by name or numeric ID
        private string unityFormSearchInput;

        // The Unity Form template found by the search, or null if not found
        private UnityFormTemplate foundUnityForm;

        // Whether a taxonomy lookup is currently in progress (used to disable commands during async operations)
        private bool isLoading;
        #endregion

        #region Properties
        /// <summary>
        /// Every Document Type Group in OnBase.
        /// </summary>
        public ObservableCollection<DocumentTypeGroup> DocumentTypeGroups { get; } = [];

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
        public ObservableCollection<DocumentType> DocumentTypes { get; } = [];

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
        /// The NAMED (Single-/Multi-Instance) Keyword Group Types on
        /// <see cref="SelectedDocumentType"/>, each with its own KeywordTypes already
        /// resolved. Does not include the Standalone group, see
        /// <see cref="StandaloneKeywordTypes"/> for that.
        /// </summary>
        public ObservableCollection<DocumentTypeKeywordGroup> KeywordGroupTypes { get; } = [];

        /// <summary>
        /// The Keyword Types on <see cref="SelectedDocumentType"/> that don't belong to
        /// any named group (the Standalone group's own Keyword Types), populated as soon
        /// as a Document Type is selected, no further click needed.
        /// </summary>
        public ObservableCollection<KeywordType> StandaloneKeywordTypes { get; } = [];

        /// <summary>
        /// The currently-selected Keyword Group Type. Its own KeywordTypes are already
        /// resolved (see this class's own Training Notes), no loading needed on
        /// selection.
        /// </summary>
        public DocumentTypeKeywordGroup SelectedKeywordGroupType
        {
            get => selectedKeywordGroupType;
            set => SetField(ref selectedKeywordGroupType, value);
        }

        /// <summary>
        /// Every Custom Query in OnBase (flat, no children).
        /// </summary>
        public ObservableCollection<CustomQuery> CustomQueries { get; } = [];

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
        public UnityFormTemplate FoundUnityForm
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
                // Awaited directly (not via ConnectCommand.Execute()), see
                // ConnectionViewModel's own Training Notes for why that indirection
                // doesn't reliably finish before the next line here would run.
                await connection.ConnectAsync();

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
            CustomQueries.Clear();

            try
            {
                var http = connection.GetHttpClient();
                var groups = await OnBaseTaxonomy.GetDocumentTypeGroupsAsync(client: http);
                var queries = await OnBaseTaxonomy.GetCustomQueriesAsync(client: http);

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
            SelectedDocumentType = null;

            if (SelectedDocumentTypeGroup == null) return;

            IsLoading = true;
            try
            {
                var groupName = SelectedDocumentTypeGroup.Name;

                var docTypes = await OnBaseTaxonomy.GetDocumentTypesForGroupAsync(SelectedDocumentTypeGroup.Id, connection.GetHttpClient());
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

        // Load KeywordGroupTypes (named groups, each with its own KeywordTypes already
        // resolved) and StandaloneKeywordTypes for SelectedDocumentType
        private async Task LoadKeywordGroupTypesAndStandalone()
        {
            KeywordGroupTypes.Clear();
            StandaloneKeywordTypes.Clear();
            SelectedKeywordGroupType = null;

            if (SelectedDocumentType == null) return;

            IsLoading = true;
            try
            {
                var docType = SelectedDocumentType;

                var allGroups = await OnBaseTaxonomy.GetDocumentTypeKeywordGroupsAsync(docType.Id, connection.GetHttpClient());
                var (groups, standalone) = OnBaseTaxonomy.SplitKeywordGroups(allGroups);

                foreach (var group in groups) KeywordGroupTypes.Add(group);
                foreach (var keyType in standalone) StandaloneKeywordTypes.Add(keyType);

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

        // Look up a File Type by extension or numeric ID
        private async Task FindFileType()
        {
            IsLoading = true;
            try
            {
                var input = FileTypeSearchInput;

                var allFileTypes = await OnBaseTaxonomy.GetFileTypesAsync(connection.GetHttpClient());
                FoundFileType = long.TryParse(input, out _)
                    ? allFileTypes.Find(f => f.Id == input)
                    : allFileTypes.Find(f => string.Equals(f.Name, input, StringComparison.OrdinalIgnoreCase) || string.Equals(f.SystemName, input, StringComparison.OrdinalIgnoreCase));

                log.Success(FoundFileType != null
                    ? $"Found file type [{FoundFileType.Name}] (ID {FoundFileType.Id})."
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
                var input = UnityFormSearchInput;

                FoundUnityForm = await OnBaseTaxonomy.GetUnityFormTemplateAsync(input, connection.Session.GetFormsHttpClient());

                log.Success(FoundUnityForm != null
                    ? $"Found unity form [{FoundUnityForm.Name}] (ID {FoundUnityForm.Id})."
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
