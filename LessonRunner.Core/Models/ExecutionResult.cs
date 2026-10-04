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

namespace LessonRunner.Core.Models;

/// <summary>
/// The result of executing a LessonStep via ILessonRunner.
/// </summary>
public class ExecutionResult
{
    #region Properties
    /// <summary>
    /// Combined stdout captured from the snippet's execution.
    /// </summary>
    public string Output { get; init; } = string.Empty;

    /// <summary>
    /// Any error output -- compile errors, runtime exceptions, or stderr.
    /// Empty on a clean run.
    /// </summary>
    public string Error { get; init; } = string.Empty;

    /// <summary>
    /// True if the snippet compiled and ran without errors.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Wall-clock time the execution took.
    /// </summary>
    public TimeSpan Elapsed { get; init; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates an <see cref="ExecutionResult"/> representing a successful execution.
    /// </summary>
    /// <param name="output">Output text produced by the execution.</param>
    /// <param name="elapsed">Total time taken by the execution.</param>
    /// <returns>An <see cref="ExecutionResult"/> with <c>Output</c> set to <paramref name="output"/>, <c>Elapsed</c> set to
    /// <paramref name="elapsed"/>, and <c>Success</c> set to <see langword="true"/>.</returns>
    public static ExecutionResult Succeeded(string output, TimeSpan elapsed) =>
        new() { Output = output, Success = true, Elapsed = elapsed };

    /// <summary>
    /// Creates an <see cref="ExecutionResult"/> representing a failed operation.
    /// </summary>
    /// <param name="error">Error message describing the failure.</param>
    /// <param name="elapsed">Time elapsed before the operation failed.</param>
    /// <returns>An <see cref="ExecutionResult"/> with <c>Success</c> set to <c>false</c>, and the <c>Error</c> and
    /// <c>Elapsed</c> values set to the provided arguments.</returns>
    public static ExecutionResult Failed(string error, TimeSpan elapsed) =>
        new() { Error = error, Success = false, Elapsed = elapsed };
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
