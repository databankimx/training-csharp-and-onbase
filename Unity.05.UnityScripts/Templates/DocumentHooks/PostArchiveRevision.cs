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
using Hyland.Unity;
#endregion

namespace Unity._05.UnityScripts.Templates.DocumentHooks
{
    #region Training Notes
    /*
     * *Fixed*: the original version of this file implemented
     * IDocumentReindexPostArchiveEventScript, the SAME interface as PostArchiveReindex.cs
     * right next to it, despite this file's own name describing a REVISION event, not a
     * reindex event. Corrected to IDocumentRevisionPostArchiveEventScript. The matching
     * event args type name (DocumentRevisionPostArchiveEventArgs) follows this API's own
     * consistent I{Feature}Script / {Feature}EventArgs naming pattern; worth confirming
     * against the actual Unity API docs if this doesn't compile as-is.
     */
    #endregion

    /// <summary>
    /// Template document hook script, post-archive revision event.
    /// </summary>
    public class PostArchiveRevision : IDocumentFileImportPostArchiveAsRevisionEventScript
    {
        /// <summary>
        /// Executed after a document is archived as a revision. This is the place to add custom logic that should run after the revision has been created and archived.
        /// </summary>
        /// <param name="app">The application instance.</param>
        /// <param name="args">The event arguments.</param>
        public void OnItemExecute(Application app, DocumentFileImportPostArchiveAsRevisionEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
