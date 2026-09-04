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
    /// Template WorkView script, get-transform-classes event.
    /// </summary>
    public class GetTransformClass : IWorkViewGetTransformClassesScript
    {
        /// <summary>
        /// Called to determine which classes a WorkView object can transform to.
        /// </summary>
        /// <param name="unityApplication">Unity API Application Object</param>
        /// <param name="args">Event arguments</param>
        public void OnWorkViewGetTransformClasses(Application unityApplication, WorkViewGetTransformClassesEventArgs args)
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
