#region Copyright
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
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

#region Notes
/*
 * This is a template for a Helper Library in Unity. It provides a basic structure for creating reusable
 * helper methods and classes that can be utilized across different Unity scripts.
 * 
 * Usage:
 * - Add your helper methods and classes within the HelperLibrary class.
 * - Ensure that the namespace is correctly referenced in other scripts where you want to use the helper methods.
 * 
 * Library scripts can be referenced by any Unity script and do not require implementation of specific
 * interfaces like IWorkflowScript. They are meant to provide utility functions and shared logic.
 */
#endregion

#region Using Directives
using Hyland.Unity;
using System;
#endregion

namespace Unity._05.UnityScripts.Templates.HelperLibrary
{
    /// <summary>
    /// Provides a container for reusable helper functionality.
    /// </summary>
    /// <remarks>Use this type to group utility members shared across the library.</remarks>
    public static class HelperLibrary
    {
        #region Methods
        /// <summary>
        /// Processes an exception chain, writes each exception to diagnostics, and
        /// records the root error message to workflow state and optionally document history.
        /// </summary>
        /// <remarks>Iterates through all inner exceptions and logs each one.
        /// Only the final inner exception message is written to the workflow property bag and,
        /// when enabled, to document history.</remarks>
        /// <param name="ex">Exception to process, including any inner exceptions.</param>
        /// <param name="app">Unity API Application, allowing access to Unity's core functionality.</param>
        /// <param name="wfArgs">Workflow event arguments that provide access to the session property bag</param>
        /// <param name="errorProperty">Session property name used to store the root error message.</param>
        /// <param name="writeToDocHistory">Indicates whether to create a document history entry</param>
        /// <param name="doc">Document for the history entry when document history logging is enabled.</param>
        /// <exception cref="DatabankException">Thrown when no application instance is available.</exception>
        public static void HandleException(this Exception ex, Application app,
            WorkflowEventArgs wfArgs = null,
            string errorProperty = "UnityError",
            bool writeToDocHistory = false,
            Document doc = null)
        {
            if (app == null) throw new DatabankException("No app instance available.", ex);

            // Make sure to process all the inner exceptions
            while (ex != null)
            {
                // Log error details to diagnostics console
                app.Diagnostics.WriteIf(Diagnostics.DiagnosticsLevel.Error,
                    $"{ex.GetType().Name}: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
                // The last inner exception is the root error
                if (ex.InnerException == null)
                {
                    // Store error message to workflow property
                    wfArgs?.SessionPropertyBag.Set(errorProperty, ex.Message);

                    // Store error message to document history
                    if (writeToDocHistory && doc != null)
                        app.Core.LogManagement.CreateDocumentHistoryItem(doc, ex.Message);
                }
                ex = ex.InnerException;
            }
        }
        #endregion
    }

    /// <summary>
    /// A minimal, self-contained exception type, matching this training set's standard
    /// exception-handling convention.
    /// </summary>
    public class DatabankException : Exception
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the DatabankException class with a specified error message and an optional inner exception.
        /// </summary>
        /// <param name="message">Error message that explains the reason for the exception.</param>
        /// <param name="innerException">Exception that is the cause of the current exception, or <see langword="null"/> if no inner exception is specified.</param>
        public DatabankException(string message, Exception innerException = null) : base(message, innerException)
        {
            /* Intentional no-op: the base class handles the initialization. */
        }

        /// <summary>
        /// Initializes a new instance of the DatabankException class based on another
        /// exception, preserving its message and inner exception.
        /// </summary>
        /// <param name="ex">The exception to wrap in a DatabankException.</param>
        public DatabankException(Exception ex) : this(ex.Message, ex)
        {
            /* Intentional no-op: the base class handles the initialization. */
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
