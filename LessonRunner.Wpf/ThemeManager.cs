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
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Xml;
#endregion

namespace LessonRunner.Wpf;

#region Supporting Types
/// <summary>
/// Represents the available application UI themes.
/// </summary>
/// <remarks>Use <see cref="AppTheme.Dark"/> for a dark appearance and <see cref="AppTheme.Light"/> for a light
/// appearance.</remarks>
public enum AppTheme { Dark, Light }
#endregion

/// <summary>
/// Manages application-wide theme switching between Dark and Light modes.
/// Applies colours to both the AvalonEdit source pane and the
/// MarkdownRenderer static properties so the lesson pane updates too.
/// </summary>
public static class ThemeManager
{
    #region Properties
    /// <summary>
    /// Gets the application's current theme.
    /// </summary>
    /// <remarks>Initialized to <see cref="AppTheme.Dark"/>.</remarks>
    public static AppTheme Current { get; private set; } = AppTheme.Dark;
    #endregion

    #region Public Methods
    /// <summary>
    /// Applies the specified theme to the editor and application resources.
    /// </summary>
    /// <remarks>Sets the current theme before applying dark or light resources.</remarks>
    /// <param name="theme">The theme to apply.</param>
    /// <param name="editor">The editor instance to update with theme-specific settings.</param>
    /// <param name="appResources">The application resource dictionary to update with theme-specific resources.</param>
    public static void Apply(AppTheme theme, TextEditor editor,
        ResourceDictionary appResources)
    {
        Current = theme;

        if (theme == AppTheme.Dark)
            ApplyDark(editor, appResources);
        else
            ApplyLight(editor, appResources);
    }
    #endregion

    #region Dark Theme Application
    // Apply dark theme settings to the editor and application resources.
    private static void ApplyDark(TextEditor editor, ResourceDictionary res)
    {
        // App-wide colour tokens
        #pragma warning disable S1192 // Ignore "string literals should not be duplicated" for theme colours
        SetColor(res, "BackgroundColor",  "#1E1E1E");
        SetColor(res, "PanelColor",       "#252526");
        SetColor(res, "BorderColor",      "#3F3F46");
        SetColor(res, "ForegroundColor",  "#D4D4D4");
        SetColor(res, "AccentColor",      "#007ACC");
        SetColor(res, "SuccessColor",     "#4EC9B0");
        SetColor(res, "ErrorColor",       "#F44747");
        SetColor(res, "MutedColor",       "#858585");
        SetColor(res, "SelectionColor",   "#264F78");
        #pragma warning restore S1192
        RefreshBrushes(res);

        // Editor
        editor.Background          = Brush("#1E1E1E");
        editor.Foreground          = Brush("#D4D4D4");
        editor.LineNumbersForeground = Brush("#858585");
        editor.SyntaxHighlighting  = LoadXshd("CSharp-Dark");

        // Markdown renderer
        MarkdownRenderer.ProseBackground = Brush("#1E1E1E");
        MarkdownRenderer.ProseColor   = Brush("#D4D4D4");
        MarkdownRenderer.CodeBgColor  = Brush("#1A1A2E");
        MarkdownRenderer.CodeFgColor  = Brush("#9CDCFE");
        MarkdownRenderer.HeadingColor = Brush("#569CD6");
        MarkdownRenderer.MutedColor   = Brush("#858585");
        MarkdownRenderer.RuleBrush    = Brush("#3F3F46");
    }
    #endregion

    #region Light Theme Application
    // Apply light theme settings to the editor and application resources.
    private static void ApplyLight(TextEditor editor, ResourceDictionary res)
    {
        SetColor(res, "BackgroundColor",  "#FFFFFF");
        SetColor(res, "PanelColor",       "#F3F3F3");
        SetColor(res, "BorderColor",      "#CCCEDB");
        SetColor(res, "ForegroundColor",  "#1E1E1E");
        SetColor(res, "AccentColor",      "#007ACC");
        SetColor(res, "SuccessColor",     "#007F5F");
        SetColor(res, "ErrorColor",       "#CC0000");
        SetColor(res, "MutedColor",       "#717171");
        SetColor(res, "SelectionColor",   "#ADD6FF");
        RefreshBrushes(res);

        editor.Background           = Brush("#FFFFFF");
        editor.Foreground           = Brush("#1E1E1E");
        editor.LineNumbersForeground = Brush("#717171");
        editor.SyntaxHighlighting   = LoadXshd("CSharp-Light");

        MarkdownRenderer.ProseBackground = Brush("#FFFFFF");
        MarkdownRenderer.ProseColor   = Brush("#1E1E1E");
        MarkdownRenderer.CodeBgColor  = Brush("#F5F5F5");
        MarkdownRenderer.CodeFgColor  = Brush("#0451A5");
        MarkdownRenderer.HeadingColor = Brush("#0451A5");
        MarkdownRenderer.MutedColor   = Brush("#717171");
        MarkdownRenderer.RuleBrush    = Brush("#CCCEDB");
    }
    #endregion

    #region Helper Functions
    // Sets a color resource in the provided ResourceDictionary if the key exists.
    private static void SetColor(ResourceDictionary res, string key, string hex)
    {
        if (res.Contains(key))
            res[key] = ColorFromHex(hex);
    }

    /// <summary>
    /// Brushes are SolidColorBrush instances whose Color property is
    /// data-bound to the Color resources. Replacing the Color resource
    /// alone doesn't update them in WPF's resource system, so we replace
    /// the brush instances directly as well.
    /// </summary>
    private static void RefreshBrushes(ResourceDictionary res)
    {
        foreach (var key in res.Keys.OfType<string>().Where(k => k.EndsWith("Brush")).ToList())
        {
            var colorKey = key.Replace("Brush", "Color");
            if (res.Contains(colorKey) && res[colorKey] is Color c)
                res[key] = new SolidColorBrush(c);
        }
    }

    // Loads an XSHD syntax highlighting definition from embedded resources.
    private static IHighlightingDefinition LoadXshd(string name)
    {
        var resourceName = $"LessonRunner.Wpf.Highlighting.{name}.xshd";
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource not found: {resourceName}");
        using var reader = XmlReader.Create(stream);
        return HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }

    // Converts a hex color string to a Color object.
    private static Color ColorFromHex(string hex) =>
        (Color)ColorConverter.ConvertFromString(hex);

    // Creates a SolidColorBrush from a hex color string.
    private static SolidColorBrush Brush(string hex) =>
        new(ColorFromHex(hex));
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
