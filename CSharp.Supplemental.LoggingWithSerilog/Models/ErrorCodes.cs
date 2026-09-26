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

using System.ComponentModel;

namespace CSharp.Supplemental.LoggingWithSerilog.Models;

#region Enumeration Values
/// <summary>
/// Enumeration of the error codes possible
/// </summary>
public enum ErrorCodes
{
    [Description("There were no error.")]
    None = 0,
    [Description("There was a non standard error please check the exception for more details!")]
    General = -1,
    [Description("The input file provided was not valid and could not be found!")]
    InvalidInputFile = -2,
    [Description("There was an error accessing the Output folder \\ File!")]
    OutputFileError = -3,
    [Description("There is missing configuration in the app.config file!")]
    MissingConfiguration = -4,
    [Description("The command line must contain the input and output file paths!")]
    MissingArgument = -5,
    [Description("Error Processing the input file!")]
    ProcessingError = -6,
    [Description("The input file provided was not in the correct format!")]
    InvalidInputFileFormat = -7,
    [Description("The existing folder already exists and cannot be overwritten!")]
    FolderExist = -8,
    [Description("The specified file was not found!")]
    FileNotFount = -9,
    [Description("An object was null when the application attempted to access it!")]
    NullReference = -10,
    [Description("The license code for the product is invalid!")]
    InvalidLicense = -99
}
#endregion

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
