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
using log4net;
using System;
using CSharp.Supplemental.LoggingWithLog4Net.Models;
#endregion

namespace CSharp.Supplemental.LoggingWithLog4Net.HelperClasses
{
    /// <summary>
    /// Log4Net Logging Utility Functions
    /// </summary>
    public static class Logging
    {
        #region Constants
        private const string LogType = "Text";
        #endregion

        #region Properties
        /// <summary>
        /// Log4Net Logging Utility
        /// </summary>
        public static ILog Logger { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initialize when class is loaded for the first time
        /// </summary>
        static Logging()
        {
            log4net.Config.XmlConfigurator.Configure();
            Logger = LogManager.GetLogger(LogType);
            Info("Log4Net Initialized - {0}", LogType);
        }
        #endregion

        #region Extension Methods
        /// <summary>
        /// Log trace information to log file
        /// </summary>
        /// <param name="message">Log Message</param>
        /// <param name="args">Arguments for `Format` function</param>
        public static void Trace(this string message, params object[] args)
            => Logger.DebugFormat(message, args);

        /// <summary>
        /// Log information messages to log file
        /// </summary>
        /// <param name="message">Log Message</param>
        /// <param name="args">Arguments for `Format` function</param>
        public static void Info(this string message, params object[] args)
            => Logger.InfoFormat(message, args);

        /// <summary>
        /// Log warning information to console and log file
        /// </summary>
        /// <param name="message">Log Message</param>
        /// <param name="args">Arguments for `Format` function</param>
        public static void Warn(this string message, params object[] args)
            => Logger.WarnFormat(message, args);

        /// <summary>
        /// Log error information to console and log file
        /// </summary>
        /// <param name="message">Log Message</param>
        /// <param name="args">Arguments for `Format` function</param>
        public static void Error(this string message, params object[] args)
            => Logger.ErrorFormat(message, args);

        /// <summary>
        /// Log fatal error information to console and log file
        /// </summary>
        /// <param name="message">Log Message</param>
        /// <param name="args">Arguments for `Format` function</param>
        public static void FatalError(this string message, params object[] args)
            => Logger.FatalFormat(message, args);

        /// <summary>
        /// Log error information to console and log file
        /// </summary>
        /// <param name="ex">Exception to process</param>
        public static void HandleException(this Exception ex)
        {
            // We will loop through all of the inner exceptions. Using `as` + a null check
            // here (rather than a direct cast) matters: an outer DatabankException can still
            // wrap an ordinary, non-DatabankException inner exception, and a direct cast would
            // throw an InvalidCastException partway through walking the chain.
            while (ex != null)
            {
                string name = ex.GetType().Name;
                var dex = ex as DatabankException;
                Logger.ErrorFormat("{0}{1}: {2}\n\nStack Trace:\n{3}",
                    name,
                    dex != null
                        ? string.Format(" ({0}) - ({1})", dex.ExceptionType, dex.ErrorType)
                        : "",
                    ex.Message,
                    ex.StackTrace);

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
