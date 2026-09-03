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
using Hyland.Unity;
using Unity._00.CommonFunctionality.Models.Enumerations;
using Unity._00.CommonFunctionality.Models.Objects;
#endregion

namespace Unity._00.CommonFunctionality.HelperClasses.Extensions
{
    /// <summary>
    /// Extension Methods for converting among data types
    /// </summary>
    public static class TypeConversionExtensions
    {
        #region Public Methods
        /// <summary>
        /// Convert string to OnBase LicenseType
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <returns>OnBase LicenseType</returns>
        public static LicenseType ToLicenseType(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return LicenseType.Default;
            return value.Substring(0, 1).ToLower() switch
            {
                "q" or "m" => LicenseType.QueryMetering,
                "e" => LicenseType.EnterpriseCoreAPI,
                _ => LicenseType.Default,
            };
        }

        /// <summary>
        /// Convert string to file format ID
        /// </summary>
        /// <param name="value">String to evaluate</param>
        /// <returns>OnBase file format ID</returns>
        public static long ToFileTypeId(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DatabankException("Cannot identify file type without an extension!");

            // Handle formats without associated extensions
            if (value.ToLower().EndsWith("form"))
            {
                return value.ToLower().Substring(0, 1) switch
                {
                    "u" => (long)FileFormat.UnityForm,
                    "v" => (long)FileFormat.VirtualForm,
                    _ => (long)FileFormat.EForm,
                };
            }

            var sb = new System.Text.StringBuilder(value);
            while (sb.Length < 3) sb.Append("-");

            return sb.ToString().ToLower().Substring(0, 3) switch
            {
                "txt" or "ctx" => (long)FileFormat.Text,
                "tif" or "jpg" or "jpe" or "gif" or "png" or "bmp" or "img" => (long)FileFormat.Image,
                "pcl" => (long)FileFormat.Pcl,
                "doc" or "dot" => (long)FileFormat.Word,
                "xls" or "xlt" => (long)FileFormat.Excel,
                "ppt" or "pps" => (long)FileFormat.PowerPoint,
                "rtf" => (long)FileFormat.RichText,
                "pdf" => (long)FileFormat.Pdf,
                "htm" => (long)FileFormat.Html,
                "avi" => (long)FileFormat.Avi,
                "mov" => (long)FileFormat.QuickTime,
                "wav" => (long)FileFormat.Wav,
                "xml" => (long)FileFormat.Xml,
                "msg" => (long)FileFormat.Outlook,
                "hl7" => (long)FileFormat.Hl7,
                "eml" => (long)FileFormat.Email,
                "zip" or "7z-" or "rar" => (long)FileFormat.Zip,
                _ => (long)FileFormat.Undefined,
            };
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
