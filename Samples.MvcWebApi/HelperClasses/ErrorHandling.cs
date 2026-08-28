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

#region Directives
using System;
using System.Collections.Generic;
using Serilog;
#endregion

namespace Samples.MvcWebApi.HelperClasses
{
    /// <summary>
    /// Error handling functionality
    /// </summary>
    public static class ErrorHandling
    {
        #region Properties
        /// <summary>
        /// Serilog logging utility
        /// </summary>
        public static ILogger Logger { get; set; } = Log.Logger;

        /// <summary>
        /// When true, include trace logging
        /// </summary>
        public static bool DebugMode { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Process and log errors
        /// </summary>
        /// <param name="ex">Caught Exception</param>
        /// <returns>List of error messages</returns>
        public static List<string> HandleException(this Exception ex)
        {
            string nl = Environment.NewLine;
            var errors = new List<string>();

            while (ex != null)
            {
                string message = $"{ex.GetType().Name}: {ex.Message}{(DebugMode ? $"{nl}{nl}Stack Trace:{nl}{ex.StackTrace}" : "")}";
                Logger?.Error(message);
                errors.Add(message);
                ex = ex.InnerException;
            }

            return errors;
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
