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
using System.Reflection;

namespace CSharp.Supplemental.LoggingWithSerilog.Models;

/// <summary>
/// Class to get the description tag of the Error Code value.
/// </summary>
public static class ErrorCodeLookup
{
    #region Extension Methods
    /// <summary>
    /// Method to get the description tag of the Error Code value.
    /// </summary>
    /// <param name="value">Error code to analyze</param>
    /// <returns>Description attribute of error code</returns>
    public static string? GetDescription(ErrorCodes value)
    {
        FieldInfo? field = value.GetType().GetField(value.ToString());
        if (field != null)
        {
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                return attr.Description;
        }

        return null;
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
