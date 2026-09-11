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
using System.Windows.Data;
using RestApi._02.AccessingTaxonomy.HelperClasses.OnBase;
using RestApi._02.AccessingTaxonomy.Models.Objects;
using RestApi._03.DocumentRetrieval.HelperClasses.OnBase;
using RestApi._03.DocumentRetrieval.Models.Objects;
using RestApi.TestHarness.Models;
#endregion

namespace RestApi.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness's own
     * RetrievalViewModel. Same three search modes, same multi-select Document Type
     * design (SelectableItem<DocumentType> wrappers, an optional Group filter), same
     * "recompute common keywords whenever the selection changes" behavior, binding
     * directly to RestApi.02/03's own plain DTOs.
     *
     * One real behavioral difference: RecomputeCommonKeywords is now genuinely
     * asynchronous (RestApi.02's own GetCommonKeywordTypesAsync makes real HTTP calls,
     * unlike Unity API's synchronous, purely in-memory GetCommonKeywordTypes, which
     * needed no connection at all to run). DocumentTypeSelectionChanged (a synchronous
     * PropertyChanged handler, can't itself be async) fires it off with the same
     * "_ = SomeAsync();" fire-and-forget pattern SelectedDocumentTypeGroup's own setter
     * already uses elsewhere in this class, rather than blocking the property-changed
     * callback on a network round-trip.
     *
     * SearchMode.DocumentId has no Search() case (matches Unity.TestHarness: that mode
     * bypasses search entirely, via GetByIdCommand -> Detail.LoadDocument directly).
     */
    #endregion

    /// <summary>
    /// The Retrieval page: search (by Document Type(s), Custom Query, or direct Document
    /// ID), a results list, and the selected document's detail pane.
    /// </summary>
    public class RetrievalViewModel : ViewModelBase
    {
        #region Private Members
        // View model for the shared connection state, so this can connect if not already connected
        private readonly ConnectionViewModel connection;

        // View model for the shared output log, so this can write messages to it
        private readonly LogViewModel log;

        // The currently-active search mode, defaulting to Document Type
        private SearchMode searchMode = SearchMode.DocumentType;

        // The Document Type Group currently narrowing AllDocumentTypesView, or null for no filter
        private DocumentTypeGroup selectedGroupFilter;

        // The currently-selected Custom Query, or null if none selected
        private CustomQuery selectedCustomQuery;

        // The Document ID to retrieve directly, used in DocumentId mode
        private string documentIdInput;

        // The search date range's start and end, defaulting to the widest possible range
        private DateTime startDate = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private DateTime endDate = DateTime.Today;

        // The currently-selected search result, or null if none selected
        private DocumentInfo selectedResult;

        // Whether a search/load operation is currently in progress, used to disable commands
        private bool isLoading;
        #endregion

        #region Properties
        /// <summary>
        /// Which search mode is currently active.
        /// </summary>
        public SearchMode SearchMode
        {
            get => searchMode;
            set
            {
                if (!SetField(ref searchMode, value)) return;
                OnPropertyChanged(nameof(IsDocumentTypeMode));
                OnPropertyChanged(nameof(IsCustomQueryMode));
                OnPropertyChanged(nameof(IsDocumentIdMode));
            }
        }

        /// <summary>
        /// Every <see cref="Models.SearchMode"/> value, for a mode selector.
        /// </summary>
        public IEnumerable<SearchMode> SearchModes { get; } = (SearchMode[])Enum.GetValues(typeof(SearchMode));

        /// <summary>
        /// Whether <see cref="SearchMode"/> is <see cref="Models.SearchMode.DocumentType"/>.
        /// </summary>
        public bool IsDocumentTypeMode => SearchMode == SearchMode.DocumentType;

        /// <summary>
        /// Whether <see cref="SearchMode"/> is <see cref="Models.SearchMode.CustomQuery"/>.
        /// </summary>
        public bool IsCustomQueryMode => SearchMode == SearchMode.CustomQuery;

        /// <summary>
        /// Whether <see cref="SearchMode"/> is <see cref="Models.SearchMode.DocumentId"/>.
        /// </summary>
        public bool IsDocumentIdMode => SearchMode == SearchMode.DocumentId;

        /// <summary>
        /// Every Document Type Group, for the optional "narrow the list below" filter.
        /// A <see langword="null"/> entry (rendered as "All Groups") is included first.
        /// </summary>
        public ObservableCollection<DocumentTypeGroup> DocumentTypeGroupFilters { get; } = [];

        /// <summary>
        /// The Document Type Group currently narrowing <see cref="AllDocumentTypesView"/>,
        /// or <see langword="null"/> for no filter (every Document Type shown).
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
        /// Every Document Type in OnBase, each wrapped for multi-select. Always the FULL
        /// list, filtering for display happens via <see cref="AllDocumentTypesView"/>.
        /// </summary>
        public ObservableCollection<SelectableItem<DocumentType>> AllDocumentTypes { get; } = [];

        /// <summary>
        /// A filtered view over <see cref="AllDocumentTypes"/>, narrowed by
        /// <see cref="SelectedGroupFilter"/> when set.
        /// </summary>
        public ICollectionView AllDocumentTypesView { get; }

        /// <summary>
        /// Every Custom Query in OnBase.
        /// </summary>
        public ObservableCollection<CustomQuery> CustomQueries { get; } = [];

        /// <summary>
        /// The currently-selected Custom Query. Setting this recomputes
        /// <see cref="SearchKeywordFields"/> from its own Keyword Types.
        /// </summary>
        public CustomQuery SelectedCustomQuery
        {
            get => selectedCustomQuery;
            set
            {
                if (!SetField(ref selectedCustomQuery, value)) return;
                _ = RecomputeCustomQueryKeywords();
            }
        }

        /// <summary>
        /// The Document ID to retrieve directly, used in <see cref="Models.SearchMode.DocumentId"/> mode.
        /// </summary>
        public string DocumentIdInput
        {
            get => documentIdInput;
            set => SetField(ref documentIdInput, value);
        }

        /// <summary>
        /// The search date range's start.
        /// </summary>
        public DateTime StartDate
        {
            get => startDate;
            set => SetField(ref startDate, value);
        }

        /// <summary>
        /// The search date range's end.
        /// </summary>
        public DateTime EndDate
        {
            get => endDate;
            set => SetField(ref endDate, value);
        }

        /// <summary>
        /// Dynamically-generated keyword search fields: for
        /// <see cref="Models.SearchMode.DocumentType"/> mode, only fields common to every
        /// currently-selected Document Type; for <see cref="Models.SearchMode.CustomQuery"/>
        /// mode, the selected query's own Keyword Types.
        /// </summary>
        public ObservableCollection<SearchKeywordField> SearchKeywordFields { get; } = [];

        /// <summary>
        /// The current search's results.
        /// </summary>
        public ObservableCollection<DocumentInfo> Results { get; } = [];

        /// <summary>
        /// Raised after <see cref="SearchCommand"/> finishes populating <see cref="Results"/>
        /// (whether or not any were found), so the View can scroll the results list into
        /// view without the ViewModel needing to know anything about scrolling itself.
        /// </summary>
        public event EventHandler SearchCompleted;

        /// <summary>
        /// The currently-selected search result. Setting this loads the full document
        /// into <see cref="Detail"/>.
        /// </summary>
        public DocumentInfo SelectedResult
        {
            get => selectedResult;
            set
            {
                if (!SetField(ref selectedResult, value)) return;
                if (value != null) _ = Detail.LoadDocument(value.Handle);
            }
        }

        /// <summary>
        /// The selected document's detail pane.
        /// </summary>
        public DocumentDetailViewModel Detail { get; }

        /// <summary>
        /// Whether a search/load operation is currently in progress.
        /// </summary>
        public bool IsLoading
        {
            get => isLoading;
            private set => SetField(ref isLoading, value);
        }
        #endregion

        #region Commands
        /// <summary>
        /// (Re)loads Document Type Groups, Document Types, and Custom Queries, connecting
        /// first if not already connected.
        /// </summary>
        public AsyncRelayCommand LoadCommand { get; }

        /// <summary>
        /// Executes a search using the current mode's criteria.
        /// </summary>
        public AsyncRelayCommand SearchCommand { get; }

        /// <summary>
        /// Retrieves the document identified by <see cref="DocumentIdInput"/> directly
        /// into <see cref="Detail"/>, bypassing search entirely.
        /// </summary>
        public AsyncRelayCommand GetByIdCommand { get; }

        /// <summary>
        /// Sets <see cref="SearchMode"/> to the <see cref="Models.SearchMode"/> value
        /// passed as the command parameter (used by the mode selector's RadioButtons,
        /// since IsDocumentTypeMode/IsCustomQueryMode/IsDocumentIdMode are read-only
        /// computed properties, a plain OneWay IsChecked binding can't set them back).
        /// </summary>
        public RelayCommand SetModeCommand { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the RetrievalViewModel class
        /// </summary>
        /// <param name="connection">The shared connection state.</param>
        /// <param name="log">The shared output log.</param>
        public RetrievalViewModel(ConnectionViewModel connection, LogViewModel log)
        {
            this.connection = connection;
            this.log = log;

            Detail = new DocumentDetailViewModel(connection, log);

            AllDocumentTypesView = CollectionViewSource.GetDefaultView(AllDocumentTypes);
            AllDocumentTypesView.Filter = FilterDocumentTypeByGroup;

            LoadCommand = new AsyncRelayCommand(_ => Load(), _ => !IsLoading);
            SearchCommand = new AsyncRelayCommand(_ => Search(), _ => !IsLoading && connection.IsConnected);
            GetByIdCommand = new AsyncRelayCommand(_ => GetById(), _ => !IsLoading && connection.IsConnected && !string.IsNullOrEmpty(DocumentIdInput));
            SetModeCommand = new RelayCommand(m => SearchMode = (SearchMode)m);
        }
        #endregion

        #region Private Methods
        // Only show Document Types belonging to SelectedGroupFilter, when set
        private bool FilterDocumentTypeByGroup(object obj)
        {
            if (SelectedGroupFilter == null) return true;
            if (obj is not SelectableItem<DocumentType> item) return false;
            return item.Item.DocumentTypeGroupId == SelectedGroupFilter.Id;
        }

        // Load Document Type Groups, all Document Types, and Custom Queries, connecting
        // first if not already connected
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
                    log.Error("Cannot load: connect attempt failed, see the error above.");
                    return;
                }
            }

            IsLoading = true;

            foreach (var item in AllDocumentTypes) item.PropertyChanged -= DocumentTypeSelectionChanged;
            DocumentTypeGroupFilters.Clear();
            AllDocumentTypes.Clear();
            CustomQueries.Clear();
            SearchKeywordFields.Clear();

            try
            {
                var groups = await OnBaseTaxonomy.GetDocumentTypeGroupsAsync();
                var docTypes = await OnBaseTaxonomy.GetDocumentTypesAsync();
                var queries = await OnBaseTaxonomy.GetCustomQueriesAsync();

                DocumentTypeGroupFilters.Add(null);
                if (groups != null) foreach (var group in groups) DocumentTypeGroupFilters.Add(group);

                if (docTypes != null)
                {
                    foreach (var docType in docTypes)
                    {
                        var item = new SelectableItem<DocumentType>(docType);
                        item.PropertyChanged += DocumentTypeSelectionChanged;
                        AllDocumentTypes.Add(item);
                    }
                }

                if (queries != null) foreach (var query in queries) CustomQueries.Add(query);

                log.Success($"Loaded {AllDocumentTypes.Count} document type(s), {DocumentTypeGroupFilters.Count - 1} group(s), {CustomQueries.Count} custom quer{(CustomQueries.Count == 1 ? "y" : "ies")}.");
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

        // Recompute SearchKeywordFields whenever a Document Type's selection changes.
        // Fire-and-forget: this handler is itself synchronous (PropertyChanged can't be
        // awaited), but the work it kicks off is genuinely async (see this class's own
        // Training Notes for why, unlike Unity.TestHarness's own synchronous version).
        private void DocumentTypeSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectableItem<object>.IsSelected)) _ = RecomputeCommonKeywords();
        }

        // Intersect Keyword Types across every currently-selected Document Type
        private async Task RecomputeCommonKeywords()
        {
            SearchKeywordFields.Clear();

            var selectedIds = AllDocumentTypes.Where(x => x.IsSelected).Select(x => x.Item.Id).ToList();
            if (selectedIds.Count == 0) return;

            try
            {
                var commonKeywords = await OnBaseTaxonomy.GetCommonKeywordTypesAsync(selectedIds);
                foreach (var keywordType in commonKeywords) SearchKeywordFields.Add(new SearchKeywordField(keywordType));
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        // Rebuild SearchKeywordFields from the selected Custom Query's own Keyword Types
        private async Task RecomputeCustomQueryKeywords()
        {
            SearchKeywordFields.Clear();
            if (SelectedCustomQuery == null) return;

            try
            {
                var keywordTypes = await OnBaseTaxonomy.GetCustomQueryKeywordTypesAsync(SelectedCustomQuery.Id);
                foreach (var keywordType in keywordTypes) SearchKeywordFields.Add(new SearchKeywordField(keywordType));
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        // Execute a search using the current mode's criteria
        private async Task Search()
        {
            IsLoading = true;
            Results.Clear();

            try
            {
                var request = new RetrievalRequest
                {
                    DateRanges = [new QueryDateRange { Start = StartDate, End = EndDate }]
                };

                switch (SearchMode)
                {
                    case SearchMode.DocumentType:
                        request.Scope = QueryScope.DocumentType;
                        request.Ids = [.. AllDocumentTypes.Where(x => x.IsSelected).Select(x => x.Item.Id)];
                        if (request.Ids.Count == 0)
                        {
                            log.Error("Select at least one Document Type to search.");
                            return;
                        }
                        break;

                    case SearchMode.CustomQuery:
                        if (SelectedCustomQuery == null)
                        {
                            log.Error("Select a Custom Query to search.");
                            return;
                        }
                        request.Scope = QueryScope.CustomQuery;
                        request.Ids = [SelectedCustomQuery.Id];
                        break;

                    default:
                        log.Error($"Search is not applicable in {SearchMode} mode.");
                        return;
                }

                foreach (var field in SearchKeywordFields.Where(f => !string.IsNullOrWhiteSpace(f.Value)))
                {
                    request.Keywords.Add(new QueryKeyword { TypeId = field.Id, Value = field.Value });
                }

                var results = await DocumentRetrieval.GetDocumentInfoAsync(request);

                if (results != null) foreach (var result in results) Results.Add(result);

                log.Success($"Found {Results.Count} document{(Results.Count == 1 ? "" : "s")}.");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                IsLoading = false;
                SearchCompleted?.Invoke(this, EventArgs.Empty);
            }
        }

        // Retrieve a document directly by ID, bypassing search
        private async Task GetById()
        {
            if (!long.TryParse(DocumentIdInput, out _))
            {
                log.Error($"[{DocumentIdInput}] is not a valid Document ID.");
                return;
            }

            IsLoading = true;
            try
            {
                await Detail.LoadDocument(DocumentIdInput);
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
