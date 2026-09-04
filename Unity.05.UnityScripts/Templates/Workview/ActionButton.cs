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
    /// Template WorkView script, action button event.
    /// </summary>
    public class ActionButton : IWorkViewExecuteActionButtonScript
    {
        /// <summary>
        /// Called when a WorkView action button is executed.
        /// </summary>
        /// <param name="unityApplication">Unity API Application Object</param>
        /// <param name="args">Event arguments</param>
        public void OnWorkViewExecuteActionButtonScript(Application unityApplication, WorkViewExecuteActionButtonScriptEventArgs args)
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
