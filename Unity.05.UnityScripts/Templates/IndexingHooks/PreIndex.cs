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

namespace Unity._05.UnityScripts.Templates.IndexingHooks
{
    /// <summary>
    /// Template indexing hook script, pre-index event.
    /// </summary>
    public class PreIndex : IScanQueuePreIndexEventScript
    {
        /// <summary>
        /// Called before a document is indexed.
        /// </summary>
        /// <param name="app">Unity API Application Object</param>
        /// <param name="args">Event arguments</param>
        public void OnItemExecute(Application app, ScanQueuePreIndexEventArgs args)
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
