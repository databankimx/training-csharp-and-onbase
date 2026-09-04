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
using Hyland.Unity.Workflow.BusinessRulesEngine;
#endregion

#region Notes
// The Business Rules Engine was retired after v22.1, so this template is provided for reference only.
#endregion

#region Warning Suppression
#pragma warning disable CS0618 // Type or member is obsolete
#endregion

namespace Unity._05.UnityScripts.Templates.Workflow
{
    /// <summary>
    /// Template Business Rules Engine script, get/set variable value.
    /// </summary>
    public class BusinessRulesEngine : IBusinessRulesEngineScript
    {
        /// <summary>
        /// Called to get the value of a Business Rules Engine variable.
        /// </summary>
        /// <param name="app">The application instance.</param>
        /// <param name="name">The variable name.</param>
        /// <param name="args">The event arguments.</param>
        /// <returns>The variable's value.</returns>
        public VariableValue OnGetVariableValue(Application app, string name, BusinessRulesEngineEventArgs args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called to set the value of a Business Rules Engine variable.
        /// </summary>
        /// <param name="app">The application instance.</param>
        /// <param name="name">The variable name.</param>
        /// <param name="oldValue">The variable's previous value.</param>
        /// <param name="newValue">The variable's new value.</param>
        /// <param name="args">The event arguments.</param>
        public void OnSetVariableValue(Application app, string name, VariableValue oldValue, VariableValue newValue,
            BusinessRulesEngineEventArgs args)
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
