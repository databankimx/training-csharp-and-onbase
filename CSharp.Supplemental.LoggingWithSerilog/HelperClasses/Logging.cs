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

using Microsoft.Extensions.Configuration;
using Serilog;
using CSharp.Supplemental.LoggingWithSerilog.Models;

namespace CSharp.Supplemental.LoggingWithSerilog.HelperClasses;

/// <summary>
/// Logging utility using Serilog
/// </summary>
public static class Logging
{
    #region Constructor
    /// <summary>
    /// Initialize when class is loaded for the first time
    /// </summary>
    static Logging()
    {
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables();

        var config = builder.Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .Enrich.FromLogContext()
            .CreateLogger();
    }
    #endregion

    #region Extension Methods
    /// <summary>
    /// Write trace-level message to log file
    /// </summary>
    /// <param name="message">Log Message</param>
    /// <param name="args">Arguments for `Format` function</param>
    public static void Trace(this string message, params object[] args)
        => Log.Logger.Verbose(message, args);

    /// <summary>
    /// Write trace message to log file
    /// </summary>
    /// <param name="message">Log Message</param>
    /// <param name="args">Arguments for `Format` function</param>
    public static void Debug(this string message, params object[] args)
        => Log.Logger.Debug(message, args);

    /// <summary>
    /// Write informational message to log file
    /// </summary>
    /// <param name="message">Log Message</param>
    /// <param name="args">Arguments for `Format` function</param>
    public static void Info(this string message, params object[] args)
        => Log.Logger.Information(message, args);

    /// <summary>
    /// Write warning message to log file
    /// </summary>
    /// <param name="message">Log Message</param>
    /// <param name="args">Arguments for `Format` function</param>
    public static void Warn(this string message, params object[] args)
        => Log.Logger.Warning(message, args);

    /// <summary>
    /// Write error message to log file
    /// </summary>
    /// <param name="message">Log Message</param>
    /// <param name="args">Arguments for `Format` function</param>
    public static void Error(this string message, params object[] args)
        => Log.Logger.Error(message, args);

    /// <summary>
    /// Write fatal error message to log file
    /// </summary>
    /// <param name="message">Log Message</param>
    /// <param name="args">Arguments for `Format` function</param>
    public static void FatalError(this string message, params object[] args)
        => Log.Logger.Fatal(message, args);

    /// <summary>
    /// Log error information to console and log file
    /// </summary>
    /// <param name="ex">Exception to process</param>
    public static void HandleException(this Exception? ex)
    {
        while (ex != null)
        {
            string name = ex.GetType().Name;
            var dex = ex as DatabankException;
            Log.Logger.Error("{exType}{props}: {msg}\n\nStack Trace:\n{stackTrace}",
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

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
