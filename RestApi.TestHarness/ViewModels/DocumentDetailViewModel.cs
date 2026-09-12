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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using RestApi._02.AccessingTaxonomy.HelperClasses.OnBase;
using RestApi._03.DocumentRetrieval.HelperClasses.OnBase;
using RestApi._03.DocumentRetrieval.Models.Objects;
#endregion

namespace RestApi.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness's own
     * DocumentDetailViewModel. currentDocumentId (a string) replaces the raw
     * Hyland.Unity.Document currentDocument held there: the REST API has no equivalent
     * "live connected object" to hold onto, RetrieveFile() below just needs the id
     * string to build its own request, same as everything else in this app now does.
     *
     * No Links/DocPop/UnityPop at all: a confirmed gap, see RestApi.00's own
     * ServiceLocation Training Notes. OpenDocPopCommand/OpenUnityPopCommand are simply
     * not present here.
     *
     * CanPreferPdf no longer checks a client-side whitelist (Unity.03's own
     * DocumentRetrieval.IsPdfConvertible, a hardcoded FileFormat list from Hyland's own
     * support table): RestApi.03's own preferPdf uses Accept-header content negotiation
     * instead, letting the SERVER decide what it can convert, with a built-in graceful
     * fallback to the rendition's native format if PDF isn't supported (see
     * RestApi.03.DocumentRetrieval's own LectureNotes.md). So CanPreferPdf here is just
     * "is a rendition even selected", always offering the checkbox rather than
     * pre-filtering by file type client-side.
     *
     * SelectedRendition.FileExtension doesn't exist on RestApi.03's own RenditionInfo
     * (the REST API's Rendition schema has no extension field, and neither does its
     * FileType schema, confirmed against document-api.json directly). The save dialog's
     * filename extension here uses the rendition's own FileTypeId resolved to a File
     * Type's SystemName instead (OnBase File Type system names are conventionally the
     * extension itself, e.g. "PDF", "TIF"), a reasonable approximation, not a guaranteed
     * exact match, worth knowing about if a saved file's extension ever looks off.
     */
    #endregion

    /// <summary>
    /// The selected document's detail pane: metadata, keyword groups, its
    /// revision/rendition tree (defaulting to the first revision's first rendition), and
    /// file retrieval/viewing.
    /// </summary>
    public class DocumentDetailViewModel : ViewModelBase
    {
        #region Private Members
        private readonly ConnectionViewModel connection;
        private readonly LogViewModel log;

        // Held only so RetrieveFile() can ask for a specific revision/rendition's file by
        // ID, never bound to or read from directly for display.
        private string currentDocumentId;

        private DocumentInfo metadata;
        private RevisionInfo selectedRevision;
        private RenditionInfo selectedRendition;
        private bool preferPdf;
        private string savedFilePath;
        private bool isBusy;
        #endregion

        #region Properties
        /// <summary>
        /// Whether a document is currently loaded.
        /// </summary>
        public bool HasDocument => currentDocumentId != null;

        /// <summary>
        /// The loaded document's ID, or <see langword="null"/> if none is loaded. Used by
        /// <see cref="EditInArchivingCommand"/>.
        /// </summary>
        public string DocumentId => currentDocumentId;

        /// <summary>
        /// The loaded document's metadata (keywords, keyword groups, and other display fields).
        /// </summary>
        public DocumentInfo Metadata
        {
            get => metadata;
            private set => SetField(ref metadata, value);
        }

        /// <summary>
        /// The loaded document's revisions.
        /// </summary>
        public ObservableCollection<RevisionInfo> Revisions { get; } = [];

        /// <summary>
        /// The currently-selected revision (defaults to the first one loaded). Setting
        /// this loads its <see cref="Renditions"/>.
        /// </summary>
        public RevisionInfo SelectedRevision
        {
            get => selectedRevision;
            set
            {
                if (!SetField(ref selectedRevision, value)) return;
                LoadRenditions();
            }
        }

        /// <summary>
        /// <see cref="SelectedRevision"/>'s renditions.
        /// </summary>
        public ObservableCollection<RenditionInfo> Renditions { get; } = [];

        /// <summary>
        /// The currently-selected rendition (defaults to the first one on
        /// <see cref="SelectedRevision"/>). Retrieving/viewing a file always acts on this
        /// rendition specifically.
        /// </summary>
        public RenditionInfo SelectedRendition
        {
            get => selectedRendition;
            set
            {
                if (!SetField(ref selectedRendition, value)) return;
                SavedFilePath = null;
                OnPropertyChanged(nameof(CanPreferPdf));
            }
        }

        /// <summary>
        /// When true, retrieval requests the file converted to PDF instead of its native
        /// format (the server decides whether that's actually possible, see this class's
        /// own Training Notes).
        /// </summary>
        public bool PreferPdf
        {
            get => preferPdf;
            set => SetField(ref preferPdf, value);
        }

        /// <summary>
        /// Whether <see cref="PreferPdf"/> is applicable at all (a rendition is selected).
        /// </summary>
        public bool CanPreferPdf => SelectedRendition != null;

        /// <summary>
        /// Where <see cref="RetrieveFileCommand"/> last saved a file, or <see langword="null"/>
        /// if nothing has been retrieved yet for the current <see cref="SelectedRendition"/>.
        /// </summary>
        public string SavedFilePath
        {
            get => savedFilePath;
            private set
            {
                if (!SetField(ref savedFilePath, value)) return;
                OnPropertyChanged(nameof(HasSavedFile));
            }
        }

        /// <summary>
        /// Whether a file has been saved for the current <see cref="SelectedRendition"/>
        /// (enables <see cref="ViewFileCommand"/>).
        /// </summary>
        public bool HasSavedFile => !string.IsNullOrEmpty(SavedFilePath);

        /// <summary>
        /// Whether a retrieval/load operation is currently in progress.
        /// </summary>
        public bool IsBusy
        {
            get => isBusy;
            private set => SetField(ref isBusy, value);
        }
        #endregion

        #region Commands
        /// <summary>
        /// Retrieves <see cref="SelectedRendition"/>'s file content and saves it to a
        /// location the user picks.
        /// </summary>
        public AsyncRelayCommand RetrieveFileCommand { get; }

        /// <summary>
        /// Opens the file last saved by <see cref="RetrieveFileCommand"/> in its default
        /// Windows viewer.
        /// </summary>
        public RelayCommand ViewFileCommand { get; }

        /// <summary>
        /// Requests navigating to Archiving with the currently-loaded document ready to
        /// edit (raises <see cref="EditInArchivingRequested"/>); MainViewModel is what
        /// actually performs the navigation.
        /// </summary>
        public RelayCommand EditInArchivingCommand { get; }
        #endregion

        #region Events
        /// <summary>
        /// Raised when <see cref="EditInArchivingCommand"/> is invoked, carrying the
        /// currently-loaded document's ID.
        /// </summary>
        public event EventHandler<string> EditInArchivingRequested;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the DocumentDetailViewModel class
        /// </summary>
        /// <param name="connection">The shared connection state.</param>
        /// <param name="log">The shared output log.</param>
        public DocumentDetailViewModel(ConnectionViewModel connection, LogViewModel log)
        {
            this.connection = connection;
            this.log = log;

            RetrieveFileCommand = new AsyncRelayCommand(_ => RetrieveFile(), _ => !IsBusy && SelectedRendition != null);
            ViewFileCommand = new RelayCommand(_ => ViewFile(), _ => HasSavedFile);
            EditInArchivingCommand = new RelayCommand(_ => EditInArchivingRequested?.Invoke(this, DocumentId), _ => HasDocument);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Loads the document identified by <paramref name="id"/>: its metadata and
        /// revision/rendition tree (defaulting to the first revision's first rendition).
        /// Does NOT fetch file content, see <see cref="RetrieveFileCommand"/> for that, a
        /// separate, explicit action.
        /// </summary>
        /// <param name="id">The Document ID to load.</param>
        public async Task LoadDocument(string id)
        {
            IsBusy = true;

            Revisions.Clear();
            Renditions.Clear();
            SavedFilePath = null;

            try
            {
                if (!connection.IsConnected)
                {
                    log.Error("Cannot load document: not connected.");
                    return;
                }

                Metadata = await DocumentRetrieval.GetDocumentInfoAsync(id, connection.GetHttpClient());

                currentDocumentId = Metadata != null ? id : null;
                OnPropertyChanged(nameof(HasDocument));
                OnPropertyChanged(nameof(DocumentId));

                if (Metadata == null)
                {
                    log.Error($"No document found with ID [{id}].");
                    return;
                }

                var http = connection.GetHttpClient();
                var revisions = await DocumentRetrieval.GetDocumentRevisionsAsync(id, http);
                if (revisions != null)
                {
                    // FileTypeName isn't populated by GetDocumentRevisionsAsync itself
                    // (the REST API's own Rendition schema only carries fileTypeId, see
                    // RenditionInfo's own summary), resolved here instead so the View can
                    // show a readable name rather than a bare id.
                    var fileTypes = await OnBaseTaxonomy.GetFileTypesAsync(http);
                    foreach (var revision in revisions)
                    {
                        foreach (var rendition in revision.Renditions)
                        {
                            rendition.FileTypeName = fileTypes?.Find(f => f.Id == rendition.FileTypeId)?.Name;
                        }
                        Revisions.Add(revision);
                    }
                }

                // Defaults to the first revision (and, via SelectedRevision's own setter,
                // that revision's first rendition).
                SelectedRevision = Revisions.FirstOrDefault();

                log.Success($"Loaded document [{id}]: {Revisions.Count} revision(s).");
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

        #region Private Methods
        // Load Renditions for SelectedRevision, defaulting SelectedRendition to the first one
        private void LoadRenditions()
        {
            Renditions.Clear();

            if (SelectedRevision != null)
            {
                foreach (var rendition in SelectedRevision.Renditions) Renditions.Add(rendition);
            }

            SelectedRendition = Renditions.FirstOrDefault();
        }

        // Retrieve SelectedRendition's file content and save it to a user-chosen location
        private async Task RetrieveFile()
        {
            if (SelectedRendition == null || currentDocumentId == null) return;

            IsBusy = true;
            try
            {
                var documentId = currentDocumentId;
                var revisionId = SelectedRevision.Id;
                var fileTypeId = SelectedRendition.FileTypeId;
                var wantsPdf = PreferPdf;

                var file = await DocumentRetrieval.GetDocumentFileAsync(documentId, revisionId, fileTypeId, wantsPdf, connection.GetHttpClient());
                if (file?.Content == null)
                {
                    log.Error("No file content returned.");
                    return;
                }

                // See this class's own Training Notes on why SystemName, not a true
                // "FileExtension" field (RestApi.03's own RenditionInfo has none).
                var extension = wantsPdf ? "pdf" : await ResolveFileExtension(fileTypeId);

                var dialog = new SaveFileDialog
                {
                    FileName = $"{Metadata?.Name ?? documentId}.{extension}",
                    Filter = "All Files (*.*)|*.*"
                };

                if (dialog.ShowDialog() != true)
                {
                    log.Info("File save cancelled.");
                    return;
                }

                await File.WriteAllBytesAsync(dialog.FileName, file.Content);

                SavedFilePath = dialog.FileName;
                log.Success($"File saved to [{SavedFilePath}].");
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

        // Resolve a File Type id to a best-guess extension (its SystemName), for the save
        // dialog's default filename
        private async Task<string> ResolveFileExtension(string fileTypeId)
        {
            try
            {
                var fileTypes = await OnBaseTaxonomy.GetFileTypesAsync(connection.GetHttpClient());
                var fileType = fileTypes?.Find(f => f.Id == fileTypeId);
                return !string.IsNullOrEmpty(fileType?.SystemName) ? fileType.SystemName.ToLowerInvariant() : "dat";
            }
            catch (Exception)
            {
                return "dat";
            }
        }

        // Open the last-saved file in its default Windows viewer
        private void ViewFile()
        {
            try
            {
                Process.Start(new ProcessStartInfo(SavedFilePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                log.Error(ex);
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
