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

namespace CSharp.SharedLibrary.Models
{
    /// <summary>
    /// Defines a custom exception class for reporting
    /// </summary>
    public class DatabankException : Exception
    {
        #region Properties
        /// <summary>
        /// Exception Type Name
        /// </summary>
        public string ExceptionType { get; set; } = "DatabankException";
        #endregion

        #region Constructors
        /// <summary>
        /// Create and initialize a new instance of the DatabankException class
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Inner exception</param>
        public DatabankException(string message, Exception innerException = null) : base(message, innerException) { }

        /// <summary>
        /// Create and initialize a new instance of the DatabankException class
        /// </summary>
        /// <param name="ex">Other exception type</param>
        public DatabankException(Exception ex) : base(ex.Message, ex.InnerException)
        {
            ExceptionType = ex.GetType().Name;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Write the error information (including all inner exceptions) to the console
        /// </summary>
        public void Log()
        {
            Console.WriteLine($"\n{ExceptionType}: {Message}\n\nStack Trace:\n{StackTrace}");
            var ex = InnerException;
            while (ex != null)
            {
                Log(ex);

                ex = ex.InnerException;
            }
        }

        /// <summary>
        /// Write a single exception's information to the console
        /// </summary>
        public static void Log(Exception ex)
        {
            Console.WriteLine($"\n{ex.GetType().Name}: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
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
