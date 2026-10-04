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
using System.Text.RegularExpressions;
using LessonRunner.Core.Models;
using Markdig;
using Markdig.Syntax;
#endregion

namespace LessonRunner.Core.Parsing;

/// <summary>
/// Parses a chapter's Lessons/ directory into an ordered list of LessonSteps.
/// Each .md file in the directory becomes one step, sorted by filename
/// (which uses a numeric prefix to guarantee order).
/// </summary>
public static class LessonStepParser
{
    #region Fields
    // Markdig pipeline for parsing Markdown into an AST. We only need the default pipeline here.
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().Build();
    #endregion

    #region Methods
    /// <summary>
    /// Discovers and parses all lesson step files under the given Lessons/ directory.
    /// Returns an empty list if the directory doesn't exist (projects without guided
    /// mode support yet simply produce no steps).
    /// </summary>
    public static IReadOnlyList<LessonStep> ParseDirectory(string lessonsDirectory)
    {
        if (!Directory.Exists(lessonsDirectory))
            return [];

        return [.. Directory
            .EnumerateFiles(lessonsDirectory, "*.md", SearchOption.TopDirectoryOnly)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .Select(ParseFile)
            .OfType<LessonStep>()];
    }

    /// <summary>
    /// Parses a single lesson step .md file. Returns null if the file cannot
    /// be parsed (missing code block, malformed frontmatter, etc.) so the
    /// caller can skip it rather than throw.
    /// </summary>
    public static LessonStep? ParseFile(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        var content = File.ReadAllText(filePath);

        var frontmatter = ExtractFrontmatter(content, out var body);
        var sourceCode  = ExtractCodeBlock(body);

        if (string.IsNullOrWhiteSpace(sourceCode))
            return null;

        return new LessonStep
        {
            Title           = GetString(frontmatter, "title"),
            Chapter         = GetInt(frontmatter, "chapter"),
            Index           = GetInt(frontmatter, "index"),
            Cumulative      = GetBool(frontmatter, "cumulative"),
            Dependencies    = GetStringList(frontmatter, "dependencies"),
            DefaultArgs     = GetString(frontmatter, "defaultArgs"),
            LaunchMode      = GetString(frontmatter, "launchMode").Equals(
                                  "external", StringComparison.OrdinalIgnoreCase)
                              ? LaunchMode.External : LaunchMode.InProcess,
            TargetFramework = GetString(frontmatter, "targetFramework", "net48"),
            SourceCode      = sourceCode,
            SourceFile      = filePath,
        };
    }
    #endregion

    #region Helper Functions (Frontmatter Extraction)
    // Matches a YAML frontmatter block delimited by --- at the very start of the file.
    private static readonly Regex FrontmatterRegex =
        new(@"^---\s*\n(.*?)\n---\s*\n", RegexOptions.Singleline | RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    // Extracts the frontmatter key-value pairs from the content, returning them in a dictionary.
    private static Dictionary<string, string> ExtractFrontmatter(string content, out string body)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var match = FrontmatterRegex.Match(content);

        if (!match.Success)
        {
            body = content;
            return result;
        }

        body = content[match.Length..];

        foreach (var line in match.Groups[1].Value.Split('\n'))
        {
            var colon = line.IndexOf(':');
            if (colon < 0) continue;

            var key   = line[..colon].Trim();
            var value = line[(colon + 1)..].Trim().Trim('"');

            if (!string.IsNullOrEmpty(key))
                result[key] = value;
        }

        return result;
    }
    #endregion

    #region Helper Functions (Code Block Extraction)
    /// <summary>
    /// Extracts the source from the first ```csharp fenced code block in the body.
    /// Markdig parses the document; we walk its AST rather than regex-matching
    /// raw text so embedded backticks in comments don't confuse the extraction.
    /// </summary>
    private static string? ExtractCodeBlock(string body)
    {
        var document = Markdown.Parse(body, Pipeline);

        var codeBlock = document
            .OfType<FencedCodeBlock>()
            .FirstOrDefault(b =>
                string.Equals(b.Info, "csharp", StringComparison.OrdinalIgnoreCase));

        if (codeBlock is null)
            return null;

        // FencedCodeBlock.Lines holds the raw source lines without the fence markers.
        var lines = codeBlock.Lines;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < lines.Count; i++)
            sb.AppendLine(lines.Lines[i].ToString());

        return sb.ToString().TrimEnd();
    }
    #endregion

    #region Helper Functions (Frontmatter Value Helpers)
    // Retrieves a string value from the frontmatter dictionary, returning a default if not found.
    private static string GetString(Dictionary<string, string> fm, string key, string defaultValue = "")
        => fm.TryGetValue(key, out var v) ? v : defaultValue;

    // Retrieves an integer value from the frontmatter dictionary, returning 0 if not found or invalid.
    private static int GetInt(Dictionary<string, string> fm, string key)
        => fm.TryGetValue(key, out var v) && int.TryParse(v, out var n) ? n : 0;

    // Retrieves a boolean value from the frontmatter dictionary, returning false if not found or invalid.
    private static bool GetBool(Dictionary<string, string> fm, string key)
        => fm.TryGetValue(key, out var v) && bool.TryParse(v, out var b) && b;

    // Parses a simple YAML inline sequence: ["a.md", "b.md"] or [] or a.md
    private static IReadOnlyList<string> GetStringList(Dictionary<string, string> fm, string key)
    {
        if (!fm.TryGetValue(key, out var raw) || string.IsNullOrWhiteSpace(raw))
            return [];

        // Strip surrounding brackets if present
        raw = raw.Trim('[', ']').Trim();
        if (string.IsNullOrEmpty(raw))
            return [];

        return [.. raw
            .Split(',')
            .Select(s => s.Trim().Trim('"', '\''))
            .Where(s => !string.IsNullOrEmpty(s))];
    }
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
