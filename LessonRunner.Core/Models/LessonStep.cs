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

#region Supporting Types
/// <summary>
/// Specifies how an application or component is launched.
/// </summary>
/// <remarks>Use <c>InProcess</c> to run within the current process, or <c>External</c> to start a separate
/// process.</remarks>
public enum LaunchMode { InProcess, External }
#endregion

/// <summary>
/// Represents a single mini-program within a lesson, parsed from a
/// chapter's Lessons/ directory. Each .md file maps to one LessonStep.
/// </summary>
public class LessonStep
{
    #region Properties
    /// <summary>
    /// Display title from the file's YAML frontmatter.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Chapter number from frontmatter.
    /// </summary>
    public int Chapter { get; init; }

    /// <summary>
    /// Sort order within the chapter, from frontmatter.
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// True when steps are cumulative -- each file represents the complete
    /// accumulated state of the program at that checkpoint, not an isolated
    /// snippet. The runner uses this to decide whether to show a diff view
    /// or a full listing (e.g. Chapter 1 is cumulative, Chapter 2 is not).
    /// </summary>
    public bool Cumulative { get; init; }

    /// <summary>
    /// Filenames of other steps in the same chapter that this step depends
    /// on (e.g. "06-Enums.md" for a step that reuses the Months enum). The
    /// runner uses this to decide what supporting code to carry into Roslyn
    /// compilation alongside this step's own source.
    /// </summary>
    public IReadOnlyList<string> Dependencies { get; init; } = [];

    /// <summary>
    /// Default command-line arguments pre-filled in the args bar when this
    /// step is selected. Space-separated, same format as the args bar input.
    /// Empty means no args are pre-filled.
    /// </summary>
    public string DefaultArgs { get; init; } = string.Empty;

    /// <summary>
    /// Whether this step runs in-process via Roslyn or as an external
    /// subprocess. External is used for steps that need COM interop,
    /// WinForms, or other dependencies not available to in-memory compilation.
    /// </summary>
    public LaunchMode LaunchMode { get; init; } = LaunchMode.InProcess;

    /// <summary>
    /// The complete, compilable C# source extracted from the fenced code block.
    /// </summary>
    public string SourceCode { get; init; } = string.Empty;

    /// <summary>
    /// Absolute path to the .md file this step was parsed from.
    /// </summary>
    public string SourceFile { get; init; } = string.Empty;

    /// <summary>
    /// Target framework for Roslyn compilation. Defaults to "net48" to match
    /// the solution-wide standard; override per-step via frontmatter for any
    /// chapter that targets a newer runtime.
    /// </summary>
    public string TargetFramework { get; init; } = "net48";
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
