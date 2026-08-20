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
using CSharp.Ch06.Supplemental.s05.ExceptionHandling.Models.Enumerations;
#endregion

namespace CSharp.Ch06.Supplemental.s05.ExceptionHandling.Models.Objects
{
    /// <summary>
    /// Define a custom exception (so we can avoid throwing System.Exception)
    /// </summary>
    [Serializable]
    public class TrainingException : Exception
    {
        #region Properties
        /// <summary>
        /// Error classification
        /// </summary>
        public ErrorType ErrorType { get; set; }

        /// <summary>
        /// Data type of caught exception
        /// </summary>
        public string ExceptionType { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create and initialize a new instance of the TrainingException class
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Previous exception</param>
        /// <param name="errorType">Error classification</param>
        public TrainingException(string message, Exception innerException = null, ErrorType errorType = ErrorType.General) : base(message, innerException)
        {
            ErrorType = errorType;
            ExceptionType = "TrainingException";
        }

        /// <summary>
        /// Create and initialize a new instance of the TrainingException class
        /// </summary>
        /// <param name="ex">Caught exception</param>
        public TrainingException(Exception ex) : base(ex.Message, ex.InnerException)
        {
            ErrorType = ErrorType.General;
            ExceptionType = ex.GetType().Name;
        }

        // This constructor is needed for serialization when an exception propagates from a remoting server to the client.
        protected TrainingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            // Deserialize custom properties if needed
            ErrorType = (ErrorType)info.GetValue(nameof(ErrorType), typeof(ErrorType));
            ExceptionType = info.GetString(nameof(ExceptionType));
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
