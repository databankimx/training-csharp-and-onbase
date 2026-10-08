#region Copyright
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
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
using LessonRunner.Core.Models;
#endregion

namespace LessonRunner.Core.Execution;

/// <summary>
/// Abstraction over how a lesson step gets executed. Reference mode and
/// guided mode both produce output through this interface; the UI never
/// needs to know which implementation is active.
/// </summary>
public interface ILessonRunner
{
    #region Contract
    /// <summary>
    /// Runs the specified lesson step asynchronously, forwarding output, requesting input when needed, and honoring
    /// cancellation.
    /// </summary>
    /// <param name="step">The lesson step to execute.</param>
    /// <param name="onOutputLine">An optional callback invoked for each output line produced during execution.</param>
    /// <param name="onInputRequired">An optional callback that provides input text when execution requests input.</param>
    /// <param name="args">Optional command-line arguments passed to the execution context.</param>
    /// <param name="ownerHwnd">An optional owner window handle used for any UI that requires a parent window.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task that completes with the execution result for the lesson step.</returns>
    Task<ExecutionResult> RunAsync(
        LessonStep step,
        Action<string>? onOutputLine = null,
        Func<string, string>? onInputRequired = null,
        string[]? args = null,
        IntPtr ownerHwnd = default,
        CancellationToken cancellationToken = default);
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
