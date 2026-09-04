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
using Hyland.Unity.WorkView;
using Application = Hyland.Unity.Application;
#endregion

namespace Unity._05.UnityScripts.Templates.Workview
{
    /// <summary>
    /// Template WorkView script, load-application-session event.
    /// </summary>
    public class LoadApplicationSession : IWorkViewLoadApplicationSessionScript
    {
        /// <summary>
        /// Called to load session-scoped data for a WorkView application.
        /// </summary>
        /// <param name="unityApplication">Unity API Application Object</param>
        /// <param name="args">Event arguments</param>
        public void OnWorkViewLoadApplicationSession(Application unityApplication, WorkViewLoadApplicationSessionEventArgs args)
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
