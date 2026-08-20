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
using System.Diagnostics;
using CSharp.Ch06.Supplemental.s05.ExceptionHandling.Models.Objects;
using log4net;
using log4net.Appender;

#endregion

namespace CSharp.Ch06.Supplemental.s05.ExceptionHandling.HelperClasses.Extensions
{
    /// <summary>
    /// Provides logging functionality
    /// </summary>
    public static class Logging
    {
        #region Properties
        /// <summary>
        /// Log4Net logging object
        /// </summary>
        public static ILog Logger { get; set; }
        #endregion

        #region Public Extension Methods
        /// <summary>
        /// Log trace information to console and log file
        /// </summary>
        /// <param name="message"></param>
        public static void TraceLog(this string message)
        {
            Console.WriteLine(message);
            Logger.Debug(message);
        }

        /// <summary>
        /// Log error information to console and log file
        /// </summary>
        /// <param name="ex">Exception to process</param>
        public static void HandleException(this Exception ex)
        {
            string nl = Environment.NewLine;

            // We will loop through all of the inner exceptions
            while (ex != null)
            {
                string errMessage = $"{ex.GetType().Name}{(ex.GetType().Name == "TrainingException" ? $" ({((TrainingException)ex).ExceptionType}) - ({((TrainingException)ex).ErrorType})" : "")}: {ex.Message}{nl}{nl}Stack Trace:{nl}{ex.StackTrace}";
                Console.WriteLine(errMessage);
                Logger.Error(errMessage);

                if (ex.InnerException == null)
                {
                    // In a real-world example, we might want to do something extra with the lowest inner exception
                    // Because this is the most meaningful one
                }

                ex = ex.InnerException;
            }
        }

        /// <summary>
        /// Launch the log file in Windows for viewing
        /// </summary>
        public static void ViewLog()
        {
            var appenders = Logger.Logger.Repository.GetAppenders();
            foreach (var appender in appenders)
            {
                if (appender.GetType() != typeof(FileAppender)) continue;
                var fileAppender = (FileAppender)appender;
                Process.Start(fileAppender.File);
                break;
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
