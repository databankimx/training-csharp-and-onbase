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
using Hyland.Unity.CodeAnalysis;
using Unity._05.UnityScripts.Templates.HelperLibrary;
#endregion

namespace Unity._05.UnityScripts.Templates.Workflow
{
    #region Training Notes
    /*
     * *Migration Note: this is the "canonical" fully-worked-out template in this whole
     * project, InitializeScript/HandleException/FinalizeScript is a genuinely reusable
     * pattern worth recognizing: consistent diagnostics logging (verbosity switches based
     * on Production vs. Test), error messages written both to a workflow property AND the
     * document history, and a HandleException that walks the full InnerException chain
     * rather than only reporting the outermost message. Every other file in this project
     * is a bare stub by comparison, on purpose, this one is meant to be the pattern to
     * copy from when actually filling one in.
     */
    #endregion

    /// <summary>
    /// Sample Template for Workflow Unity Script
    /// </summary>
    [SuppressRule(RuleNames.OB2006_PASS_EXCEPTION_OBJECT_TO_DIAGNOSTICS_WRITE_METHODS)]
    public class WorkflowScript : IWorkflowScript
    {
        #region User-Editable Script Settings
        // Script name for logging
        private const string ScriptName = "Workflow Script Template";

        // Diagnostics verbosity
        private const Diagnostics.DiagnosticsLevel TestDiagLevel = Diagnostics.DiagnosticsLevel.Verbose;
        private const Diagnostics.DiagnosticsLevel ProdDiagLevel = Diagnostics.DiagnosticsLevel.Warning;

        // Workflow property to store error messages
        private const string ErrorProperty = "UnityError";

        // When true, error message will be stored to the document history
        private const bool LogErrorToDocHistory = true;

        /*
         * Add any other values that might need to be modified by the end user
         * e.g.: connections strings, keyword names, etc.
         */
        #endregion

        /* Developer Warning!
         * End-User/SE: Do not edit beyond this point
         */

        #region Private Globals
        // Unity API Application Object
        private Application unity;

        // Workflow Event Arguments (for access to property bag)
        private WorkflowEventArgs wfArgs;

        // Active document in workflow
        private Document doc;

        /*
         * Add any other global variables you might require
         * e.g. database connection/command, etc.
         */
        #endregion

        #region IWorkflowScript
        /// <summary>
        /// Sample Implementation of Workflow Script<see cref="IWorkflowScript"/>
        /// </summary>
        /// <param name="app">Unity API Application Object<see cref="Application"/></param>
        /// <param name="args">Workflow Event Arguments<see cref="WorkflowEventArgs"/></param>
        public void OnWorkflowScriptExecute(Application app, WorkflowEventArgs args)
        {
            try
            {
                InitializeScript(app, args);

                /*
                 * Add your code here:
                 * Consider making each process a method and calling here
                 */
            }
            catch (Exception ex)
            {
                // Logs the exception (including inner exceptions) to the diagnostics console,
                // workflow property bag, and document history (if enabled)
                // From the HelperLibrary library script
                ex.HandleException(unity, wfArgs, ErrorProperty, LogErrorToDocHistory, doc);
            }
            finally
            {
                FinalizeScript();
            }
        }
        #endregion

        #region Helper Functions
        // Initialize global variables for script start
        private void InitializeScript(Application app, WorkflowEventArgs args)
        {
            try
            {
                unity = app;
                unity.Diagnostics.Level = unity.SystemProperties.IsProduction ? ProdDiagLevel : TestDiagLevel;
                unity.Diagnostics.WriteIf(Diagnostics.DiagnosticsLevel.Info, $"Start Script - {ScriptName}");
                wfArgs = args;
                wfArgs.ScriptResult = true;
                if (args.SessionPropertyBag.ContainsKey(ErrorProperty)) args.SessionPropertyBag.Remove(ErrorProperty);
                doc = wfArgs.Document;
                unity.Diagnostics.WriteIf(Diagnostics.DiagnosticsLevel.Verbose, $"Processing Document - [{doc.ID}]");
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error initializing script!", ex);
            }
        }

        // Clean up global variables for script end
        private void FinalizeScript()
        {
            try
            {
                unity.Diagnostics.WriteIf(Diagnostics.DiagnosticsLevel.Info, $"End Script - {ScriptName}");
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error finalizing script!", ex);
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
