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
using Serilog;
using Serilog.Core;
#endregion

namespace OnBase.Preprocessor.HelperClasses.Extensions
{
    /// <summary>
    /// Performs logging to file (and console if in interactive mode). Same public surface as the
    /// log4net-backed version this replaces - Initialize() takes the place of setting a log4net
    /// ILog instance, everything that calls LogWriter.Log() didn't need to change at all.
    /// </summary>
    public static class LogWriter
    {
        #region Properties
        /// <summary>
        /// When true, trace logging is enabled
        /// </summary>
        public static bool DebugMode { get; set; }

        /// <summary>
        /// When true, log to console as well as log file
        /// </summary>
        public static bool Interactive { get; set; }
        #endregion

        #region Private Members
        // Serilog logger instance - configured once via Initialize(), rather than log4net's
        //   XML <log4net> config section, which this project no longer has
        private static Logger logger;
        #endregion

        #region Constants
        // Shorthand substitute for new-line
        private static readonly string Nl = Environment.NewLine;
        #endregion

        #region Public Methods
        /// <summary>
        /// Configure the Serilog file sink. Call once, before any Log() call, with the path
        /// read from PreprocessorSettings.LogFilePath.
        /// </summary>
        /// <param name="logFilePath">File path Serilog should write log entries to</param>
        public static void Initialize(string logFilePath)
        {
            logger = new LoggerConfiguration()
                .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        /// <summary>
        /// Log the provided message
        /// </summary>
        /// <param name="message">Text to be logged</param>
        /// <param name="isError">If true, log error regardless of DebugMode setting</param>
        /// <param name="forceLog">If true, log trace regardless of DebugMode setting</param>
        public static void Log(string message, bool isError = false, bool forceLog = false)
        {
            if (Interactive) Console.WriteLine(message);
            if (!isError)
            {
                if (DebugMode || forceLog) logger?.Debug(message);
                return;
            }
            logger?.Error(message);
        }

        /// <summary>
        /// Process exception and log error information (including all inner exceptions)
        /// </summary>
        /// <param name="ex">Caught exception</param>
        public static void Log(this Exception ex)
        {
            while (ex != null)
            {
                string message = $"{ex.GetType().Name}: {ex.Message}{(DebugMode ? $"{Nl}{Nl}Stack Trace:{Nl}{ex.StackTrace}" : "")}";
                Log(message, true);
                ex = ex.InnerException;
            }
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
