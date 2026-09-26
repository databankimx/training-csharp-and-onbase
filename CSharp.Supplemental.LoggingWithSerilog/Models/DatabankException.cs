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

namespace CSharp.Supplemental.LoggingWithSerilog.Models;

/// <summary>
/// Custom exception class
/// </summary>
public class DatabankException : Exception
{
    #region Properties
    /// <summary>
    /// Exception data type
    /// </summary>
    public string ExceptionType { get; set; }

    /// <summary>
    /// Error condition
    /// </summary>
    public ErrorCodes ErrorType { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Create and initialize a new instance of the DatabankException class
    /// </summary>
    /// <param name="message">Exception Message</param>
    /// <param name="innerException">Inner Exception Object</param>
    /// <param name="errorType">Error Condition</param>
    public DatabankException(string message, Exception? innerException = null, ErrorCodes errorType = ErrorCodes.General) :
        base(message, innerException)
    {
        ExceptionType = "DatabankException";
        ErrorType = errorType;
    }

    /// <summary>
    /// Create and initialize a new instance of the DatabankException class from another Exception
    /// </summary>
    /// <param name="ex">Source Exception</param>
    /// <param name="errorType">Error Condition</param>
    public DatabankException(Exception ex, ErrorCodes errorType = ErrorCodes.General) : base(ex.Message, ex.InnerException)
    {
        ExceptionType = ex.GetType().Name;
        ErrorType = ExceptionType == "NullReferenceException"
            ? ErrorCodes.NullReference
            : errorType;
    }
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
