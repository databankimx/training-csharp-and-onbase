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
using System.IO;
using Hyland.Unity;
using Hyland.Unity.CodeAnalysis;
using DBIMX.Unity.Extensions;
#endregion

#region Suppress Warnings
#pragma warning disable S125  // Allow commented code in lesson files
#pragma warning disable S1192 // Allow repeating string literals in lesson files
#pragma warning disable S1481 // Unused variables demonstrate available methods in the DBIMX.Extensions library
#endregion

namespace Unity._07.UsingDataBankExtensionsLibrary
{
    #region Training Notes
    /*
     * *Migration Note: this project's whole purpose is demonstrating the
     * DBIMX.Unity.Extensions library's own convenience methods, worth reading
     * ExtensionsDemo() alongside its commented-out "plain Unity API" equivalents
     * (unity.FindDocumentType(...) vs. theApp.Core.DocumentTypes.Find(...), etc.), the
     * extension methods aren't doing anything a plain Unity API call couldn't, they're
     * just shorter and, in AddKeyword's case, add automatic truncation to the keyword
     * type's configured max length rather than requiring that check to be written out by
     * hand each time.
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
        private const string ScriptName = "Extensions Demo Script";

        // Diagnostics verbosity
        private const Diagnostics.DiagnosticsLevel TestDiagLevel = Diagnostics.DiagnosticsLevel.Verbose;
        private const Diagnostics.DiagnosticsLevel ProdDiagLevel = Diagnostics.DiagnosticsLevel.Warning;

        // Workflow property to store error messages
        private const string ErrorProperty = "UnityError";

        // When true, error message will be stored to the document history
        private const bool LogErrorToDocHistory = true;

        // The DBIMX.Extensions license key (specific to this script only)
        private const string LicenseKey = "781AA-51AEF-F33F5-74629";
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

                ExtensionsDemo();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                FinalizeScript();
            }
        }
        #endregion

        #region Helper Functions
        // Perform tasks using extensions library
        private void ExtensionsDemo()
        {
            unity.Diagnostics.WriteIf(Diagnostics.DiagnosticsLevel.Info, $"OnBase Version: {unity.ToVersionDetails()}");

            var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.txt");
            File.WriteAllText(filePath, "Pangaea shattered / humanity is scattered / separation for evermore");    // \m/

            var docType = unity.FindDocumentType("TST Document");
            // var docType = theApp.Core.DocumentTypes.Find("TST Document");
            if (docType == null) throw new DatabankException("Could not find document type TST Document");

            var fileType = unity.FindFileType("Text Report Format");
            // var fileType = theApp.Core.FileTypes.Find("Text Report Format");
            if (fileType == null) throw new DatabankException("Could not find file type Text Report Format");

            var storeProps = unity.Core.Storage.CreateStoreNewDocumentProperties(docType, fileType);
            var newDoc = unity.Core.Storage.StoreNewDocument(new[] { filePath }, storeProps);
            unity.Diagnostics.WriteIf(Diagnostics.DiagnosticsLevel.Verbose, $"New document stored with Document ID [{newDoc.ID}]");

            KeywordType keyType;
            if (newDoc.DocumentType.TryGetKeywordType("Description", out keyType))
            {
                var keyMod = newDoc.CreateKeywordModifier();
                string description = "THIS WAS UPDATED VIA THE UNITY API AND IS A REALLY " +
                                     "LONG LINE OF TEXT TO DEMONSTRATE HOW TRUNCATION WORKS IN THE ADDKEYWORD EXTENSION METHOD.";

                keyMod.AddKeyword("Description", description, true);

                //if (description.Length > keyType.DataLength)
                //    description = description.Substring(0, (int) keyType.DataLength);
                //keyMod.AddKeyword("Description", description);

                keyMod.ApplyChanges();
            }

            var descriptionKeyword = newDoc.KeywordRecords.GetFirstKeyword("Description");
            unity.Diagnostics.WriteIf(Diagnostics.DiagnosticsLevel.Info, $"The Description keyword value is [{descriptionKeyword}]");

            var values = newDoc.KeywordRecords.GetAllKeywordValues("Description");
            var myDescKeyword = newDoc.KeywordRecords.GetFirstKeywordHavingValue("Description", "MY DESCRIPTION");
            int descCount = newDoc.KeywordRecords.GetKeywordInstanceCount("Description");

            long id = newDoc.ID;
            unity.DeleteDocument(newDoc.ID);    // theApp.Core.Storage.DeleteDocument(newDoc);
            unity.Diagnostics.WriteIf(Diagnostics.DiagnosticsLevel.Verbose, $"Document with Document ID [{id}] was deleted");

            unity.Diagnostics.WriteIf(Diagnostics.DiagnosticsLevel.Verbose, $"Cleaning up temporary file [{filePath}]");
            if (File.Exists(filePath)) File.Delete(filePath);
        }

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

                License.Register(LicenseKey);
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

        // Process and log any errors
        private void HandleException(Exception ex)
        {
            // Make sure to process all the inner exceptions
            while (ex != null)
            {
                // Log error details to diagnostics console
                unity.Diagnostics.WriteIf(Diagnostics.DiagnosticsLevel.Error,
                    $"{ex.GetType().Name}: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
                // The last inner exception is the root error
                if (ex.InnerException == null)
                {
                    // Store error message to workflow property
                    wfArgs.SessionPropertyBag.Set(ErrorProperty, ex.Message);
                    // Store error message to document history
                    if (LogErrorToDocHistory) unity.Core.LogManagement.CreateDocumentHistoryItem(doc, ex.Message);
                }
                ex = ex.InnerException;
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
