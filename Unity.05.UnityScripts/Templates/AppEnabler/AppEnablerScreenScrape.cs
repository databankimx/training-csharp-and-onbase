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
using Hyland.Unity.ApplicationEnabler;
#endregion

namespace Unity._05.UnityScripts.Templates.AppEnabler
{
    /// <summary>
    /// Template Application Enabler script, screen-scrape event.
    /// </summary>
    public class AppEnablerScreenScrape : IApplicationEnablerScrapeEventScript
    {
        /// <summary>
        /// Called when the Application Enabler scrapes a screen.
        /// </summary>
        /// <param name="app">Unity API Application Object</param>
        /// <param name="args">Event arguments</param>
        public void OnExecute(Application app, ApplicationEnablerScrapeEventArgs args)
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
