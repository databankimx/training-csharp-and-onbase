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

namespace Unity._05.UnityScripts.Templates.ClientSide
{
    /// <summary>
    /// Template global client-side script, runs for all client operations rather than
    /// being tied to a specific document type/workflow.
    /// </summary>
    public class GlobalClientScript : IGlobalClientScript
    {
        /// <summary>
        /// Called when the global client-side script executes.
        /// </summary>
        /// <param name="app">Unity API Application Object</param>
        /// <param name="args">Event arguments</param>
        public void OnGlobalClientScriptExecute(Application app, GlobalClientEventArgs args)
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
