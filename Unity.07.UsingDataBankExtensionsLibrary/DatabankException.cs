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
#endregion

namespace Unity._07.UsingDataBankExtensionsLibrary
{
    #region Training Notes
    /*
     * *Migration Note: this project has no natural dependency on either
     * Unity.00.CommonFunctionality or Unity.05.UnityScripts (its topic, the
     * DBIMX.Unity.Extensions library, is unrelated to either), so it gets its own
     * minimal DatabankException, matching the same self-contained approach those two
     * projects use for the same reason.
     */
    #endregion

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
