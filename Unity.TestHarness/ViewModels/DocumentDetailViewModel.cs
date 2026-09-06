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
using System.Linq;
using System.Threading.Tasks;
using Hyland.Unity;
using Microsoft.Win32;
using Unity._03.DocumentRetrieval.HelperClasses.OnBase;
using Unity._03.DocumentRetrieval.Models.Objects;
#endregion

namespace Unity.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: reworked while auditing this harness for logic that belongs in
     * the library rather than the UI layer, ahead of a planned web-portal reproduction of
     * this app. Revisions/Renditions used to be live Hyland.Unity Revision/Rendition
     * objects held directly as view model state, and bound to their raw properties from
     * XAML, which only works because a WPF app keeps one connected Unity session open for
     * its whole lifetime. They're now Unity.03.DocumentRetrieval's own RevisionInfo/
     * RenditionInfo DTOs (via the new GetDocumentRevisions method), a serializable shape
     * that fits a stateless request model too. currentDocument (the raw Document) is
     * still held, but ONLY as the thing needed to ask for a specific revision/rendition's
     * FILE by ID (see RetrieveFile below), never bound to or read from directly for
     * display, that's what Metadata/Revisions/Renditions are for now.
     *
     * SelectedRevision/SelectedRendition default to the FIRST item in whatever order
     * Revisions/Renditions come back in, matching "by default, select the first revision
     * and that revision's first rendition". File retrieval is a deliberately SEPARATE,
     * explicit action (RetrieveFileCommand), LoadDocument itself never fetches file
     * content, only metadata/links/the revision-rendition tree, keeping the initial
     * document-selection cheap even for large files.
     */
    #endregion

    /// <summary>
    /// The selected document's detail pane: metadata, keyword groups, its
    /// revision/rendition tree (defaulting to the first revision's first rendition),
    /// DocPop/UnityPop links, and file retrieval/viewing.
    /// </summary>
    public class DocumentDetailViewModel : ViewModelBase
    {
        #region Private Members
        private readonly ConnectionViewModel connection;
        private readonly LogViewModel log;
        private DocumentRetrieval retrieval;

        // Held only so RetrieveFile() can ask for a specific revision/rendition's file by
        // ID, never bound to or read from directly for display.
        private Document currentDocument;

        private DocumentInfo metadata;
        private DocumentLink links;
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
        public bool HasDocument => currentDocument != null;

        /// <summary>
        /// The loaded document's ID, or <see langword="null"/> if none is loaded. Used by
        /// <see cref="EditInArchivingCommand"/>.
        /// </summary>
        public long? DocumentId => currentDocument?.ID;

        /// <summary>
        /// The loaded document's metadata (keywords, keyword groups, and other display fields).
        /// </summary>
        public DocumentInfo Metadata
        {
            get => metadata;
            private set => SetField(ref metadata, value);
        }

        /// <summary>
        /// The loaded document's DocPop/UnityPop links.
        /// </summary>
        public DocumentLink Links
        {
            get => links;
            private set => SetField(ref links, value);
        }

        /// <summary>
        /// The loaded document's revisions.
        /// </summary>
        public ObservableCollection<RevisionInfo> Revisions { get; } = new ObservableCollection<RevisionInfo>();

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
        public ObservableCollection<RenditionInfo> Renditions { get; } = new ObservableCollection<RenditionInfo>();

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
        /// When true and <see cref="SelectedRendition"/>'s file type supports it,
        /// retrieval converts to PDF instead of the native format.
        /// </summary>
        public bool PreferPdf
        {
            get => preferPdf;
            set => SetField(ref preferPdf, value);
        }

        /// <summary>
        /// Whether <see cref="PreferPdf"/> is actually applicable to
        /// <see cref="SelectedRendition"/>'s file type.
        /// </summary>
        public bool CanPreferPdf => SelectedRendition != null && DocumentRetrieval.IsPdfConvertible(SelectedRendition.FileTypeId);

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
        /// Opens <see cref="Links"/>' DocPop URL in the default browser.
        /// </summary>
        public RelayCommand OpenDocPopCommand { get; }

        /// <summary>
        /// Opens <see cref="Links"/>' UnityPop URL in the default browser.
        /// </summary>
        public RelayCommand OpenUnityPopCommand { get; }

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
        public event EventHandler<long> EditInArchivingRequested;
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
            OpenDocPopCommand = new RelayCommand(_ => OpenUrl(Links?.DocPop), _ => !string.IsNullOrEmpty(Links?.DocPop));
            OpenUnityPopCommand = new RelayCommand(_ => OpenUrl(Links?.UnityPop), _ => !string.IsNullOrEmpty(Links?.UnityPop));
            EditInArchivingCommand = new RelayCommand(_ => EditInArchivingRequested?.Invoke(this, DocumentId ?? 0), _ => HasDocument);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Loads the document identified by <paramref name="id"/>: its metadata, links,
        /// and revision/rendition tree (defaulting to the first revision's first
        /// rendition). Does NOT fetch file content, see <see cref="RetrieveFileCommand"/>
        /// for that, a separate, explicit action.
        /// </summary>
        /// <param name="id">The Document ID to load.</param>
        public async Task LoadDocument(long id)
        {
            IsBusy = true;

            Revisions.Clear();
            Renditions.Clear();
            SavedFilePath = null;

            try
            {
                var app = connection.CurrentApplication;
                if (app == null)
                {
                    log.Error("Cannot load document: not connected.");
                    return;
                }

                retrieval = new DocumentRetrieval(app, new Metadata(app));

                currentDocument = await Task.Run(() => retrieval.GetDocumentById(id, app));
                OnPropertyChanged(nameof(HasDocument));
                OnPropertyChanged(nameof(DocumentId));

                if (currentDocument == null)
                {
                    log.Error($"No document found with ID [{id}].");
                    return;
                }

                Metadata = await Task.Run(() => retrieval.GetDocumentInfo(currentDocument));
                Links = await Task.Run(() => retrieval.GetDocumentLink(currentDocument));

                var revisions = await Task.Run(() => retrieval.GetDocumentRevisions(currentDocument));
                foreach (var revision in revisions) Revisions.Add(revision);

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
            if (SelectedRendition == null) return;

            IsBusy = true;
            try
            {
                var doc = currentDocument;
                var revisionId = SelectedRevision.Id;
                var fileTypeId = SelectedRendition.FileTypeId;
                var wantsPdf = PreferPdf;

                var file = await Task.Run(() => retrieval.GetDocumentFile(doc, revisionId, fileTypeId, wantsPdf));

                // FileExtension (not FileTypeName) is what actually distinguishes e.g.
                // DOC from DOCX, images, and similar cases a bare FileType doesn't capture.
                var extension = wantsPdf && DocumentRetrieval.IsPdfConvertible(fileTypeId) ? "pdf" : SelectedRendition.FileExtension;
                var dialog = new SaveFileDialog
                {
                    FileName = $"{Metadata?.Name ?? doc.ID.ToString()}.{extension}",
                    Filter = "All Files (*.*)|*.*"
                };

                if (dialog.ShowDialog() != true)
                {
                    log.Info("File save cancelled.");
                    return;
                }

                await Task.Run(() => System.IO.File.WriteAllBytes(dialog.FileName, file.Content));

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

        // Open a URL in the default browser
        private void OpenUrl(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return;
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
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
