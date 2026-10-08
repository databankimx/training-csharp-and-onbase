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
using System.Text;
using System.Windows;
using System.Windows.Media;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

// Explicit aliases to resolve the Block/Inline ambiguity between
// System.Windows.Documents and Markdig.Syntax / Markdig.Syntax.Inlines.
// WPF types are aliased; Markdig types keep their short names via the usings above.
using WpfBlock    = System.Windows.Documents.Block;
using WpfInline   = System.Windows.Documents.Inline;
using WpfList     = System.Windows.Documents.List;
using WpfListItem = System.Windows.Documents.ListItem;
using WpfParagraph = System.Windows.Documents.Paragraph;
using WpfSection  = System.Windows.Documents.Section;
using WpfSpan     = System.Windows.Documents.Span;
using WpfRun      = System.Windows.Documents.Run;
#endregion

namespace LessonRunner.Wpf;

/// <summary>
/// Converts a Markdig AST into a WPF FlowDocument so Lesson.md renders
/// as formatted text rather than raw markdown. Lives in the WPF project
/// because it depends on System.Windows.Documents and System.Windows.Media,
/// which are only available in a -windows TFM.
/// </summary>
public static class MarkdownRenderer
{
    #region Fields
    // The Markdig pipeline used to parse the markdown into an AST.
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the font family used for prose text.
    /// </summary>
    /// <remarks>Defaults to Segoe UI.</remarks>
    public static FontFamily ProseFont     { get; set; } = new("Segoe UI");

    /// <summary>
    /// Gets or sets the font family used for monospaced text.
    /// </summary>
    /// <remarks>The default value is "Consolas, Courier New".</remarks>
    public static FontFamily MonospaceFont { get; set; } = new("Consolas, Courier New");

    /// <summary>
    /// Gets or sets the base font size used as the default text size.
    /// </summary>
    public static double BaseFontSize  { get; set; } = 13.5;

    /// <summary>
    /// Gets or sets the brush used to render prose text.
    /// </summary>
    /// <remarks>The default value is a SolidColorBrush initialized with RGB (0xD4, 0xD4, 0xD4).</remarks>
    public static Brush  ProseColor      { get; set; } = new SolidColorBrush(Color.FromRgb(0xD4, 0xD4, 0xD4));

    /// <summary>
    /// Gets or sets the brush used for the document background.
    /// </summary>
    public static Brush  ProseBackground { get; set; } = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E));

    /// <summary>
    /// Gets or sets the brush used for the code background color.
    /// </summary>
    public static Brush  CodeBgColor   { get; set; } = new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x2E));

    /// <summary>
    /// Gets or sets the brush used for the code foreground color.
    /// </summary>
    /// <remarks>The default value is a SolidColorBrush initialized to RGB (156, 220, 254).</remarks>
    public static Brush  CodeFgColor   { get; set; } = new SolidColorBrush(Color.FromRgb(0x9C, 0xDC, 0xFE));

    /// <summary>
    /// Gets or sets the brush used to render heading text.
    /// </summary>
    /// <remarks>The default value is a SolidColorBrush with RGB components 0x56, 0x9C, 0xD6.</remarks>
    public static Brush  HeadingColor  { get; set; } = new SolidColorBrush(Color.FromRgb(0x56, 0x9C, 0xD6));

    /// <summary>
    /// Gets or sets the brush used for muted visual elements.
    /// </summary>
    public static Brush  MutedColor    { get; set; } = new SolidColorBrush(Color.FromRgb(0x85, 0x85, 0x85));

    /// <summary>
    /// Gets or sets the brush used to render rules.
    /// </summary>
    /// <remarks>The default value is a SolidColorBrush initialized with RGB (63, 63, 70).</remarks>
    public static Brush  RuleBrush     { get; set; } = new SolidColorBrush(Color.FromRgb(0x3F, 0x3F, 0x46));
    
    /// <summary>
    /// Gets or sets the brush used to render blockquotes.
    /// </summary>
    public static Brush  QuoteColor    { get; set; } = new SolidColorBrush(Color.FromRgb(0x6A, 0x6A, 0x6A));
    #endregion

    #region Methods
    /// <summary>
    /// Renders Markdown text into a WPF <see cref="System.Windows.Documents.FlowDocument"/> using the configured
    /// Markdown pipeline and document styling.
    /// </summary>
    /// <param name="markdown">Markdown content to render.</param>
    /// <returns>A <see cref="System.Windows.Documents.FlowDocument"/> containing blocks generated from the parsed Markdown
    /// input.</returns>
    public static System.Windows.Documents.FlowDocument Render(string markdown)
    {
        var doc = Markdown.Parse(markdown, Pipeline);

        var flow = new System.Windows.Documents.FlowDocument
        {
            FontFamily    = ProseFont,
            FontSize      = BaseFontSize,
            Foreground    = ProseColor,
            Background    = ProseBackground,
            PagePadding   = new Thickness(16),
            LineHeight    = double.NaN,
            TextAlignment = TextAlignment.Left,
        };

        foreach (var block in doc)
            flow.Blocks.Add(RenderBlock(block));

        return flow;
    }
    #endregion

    #region Block Rendering
    // Renders a Markdig Block into a WPF Block. This method uses pattern matching to determine the specific type of block and delegates to the appropriate rendering method.
    private static WpfBlock RenderBlock(Block block) => block switch
    {
        HeadingBlock h       => RenderHeading(h),
        FencedCodeBlock fc   => RenderFencedCode(fc),
        CodeBlock cb         => RenderIndentedCode(cb),
        ListBlock lb         => RenderList(lb),
        ThematicBreakBlock   => RenderRule(),
        QuoteBlock qb        => RenderQuote(qb),
        ParagraphBlock pb    => RenderParagraph(pb),
        _                    => new WpfParagraph()
    };

    // Renders a Markdig HeadingBlock into a WPF Paragraph with appropriate styling based on the heading level. The font size and margin are adjusted according to the heading level, and the text color is set to the configured HeadingColor.
    private static WpfParagraph RenderHeading(HeadingBlock h)
    {
        double size = h.Level switch
        {
            1 => BaseFontSize * 1.7,
            2 => BaseFontSize * 1.4,
            3 => BaseFontSize * 1.2,
            _ => BaseFontSize * 1.05
        };

        var p = new WpfParagraph
        {
            Foreground = HeadingColor,
            FontSize   = size,
            FontWeight = FontWeights.SemiBold,
            Margin     = new Thickness(0, h.Level == 1 ? 8 : 16, 0, 6),
        };

        if (h.Inline is not null)
            foreach (var inline in h.Inline)
                p.Inlines.Add(RenderInline(inline));

        return p;
    }

    // Renders a Markdig FencedCodeBlock into a WPF Section containing a Paragraph with monospaced font and code styling. The code content is extracted from the FencedCodeBlock's lines, and the Paragraph is styled with background color, foreground color, padding, and line height to visually represent a code block.
    private static WpfSection RenderFencedCode(FencedCodeBlock fc)
    {
        var lines = fc.Lines;
        var sb    = new StringBuilder();
        for (int i = 0; i < lines.Count; i++)
            sb.AppendLine(lines.Lines[i].ToString());

        var para = new WpfParagraph
        {
            FontFamily = MonospaceFont,
            FontSize   = BaseFontSize - 0.5,
            Foreground = CodeFgColor,
            Background = CodeBgColor,
            Padding    = new Thickness(12, 8, 12, 8),
            Margin     = new Thickness(0, 6, 0, 6),
            LineHeight = BaseFontSize * 1.6,
        };
        para.Inlines.Add(new WpfRun(sb.ToString().TrimEnd()));

        var section = new WpfSection { Margin = new Thickness(0) };
        section.Blocks.Add(para);
        return section;
    }

    // Renders a Markdig CodeBlock (indented code) into a WPF Paragraph with monospaced font and code styling. The code content is extracted from the CodeBlock's lines, and the Paragraph is styled with background color, foreground color, padding, and margin to visually represent an indented code block.
    private static WpfParagraph RenderIndentedCode(CodeBlock cb)
    {
        var lines = cb.Lines;
        var sb    = new StringBuilder();
        for (int i = 0; i < lines.Count; i++)
            sb.AppendLine(lines.Lines[i].ToString());

        var p = new WpfParagraph
        {
            FontFamily = MonospaceFont,
            FontSize   = BaseFontSize - 0.5,
            Foreground = CodeFgColor,
            Background = CodeBgColor,
            Padding    = new Thickness(12, 8, 12, 8),
            Margin     = new Thickness(0, 6, 0, 6),
        };
        p.Inlines.Add(new WpfRun(sb.ToString().TrimEnd()));
        return p;
    }

    // Renders a Markdig ListBlock into a WPF List with appropriate marker style, margin, and padding. Each ListItemBlock is rendered into a WPF ListItem, and its child blocks are added to the ListItem's Blocks collection.
    private static WpfList RenderList(ListBlock lb)
    {
        var list = new WpfList
        {
            MarkerStyle = lb.IsOrdered
                ? TextMarkerStyle.Decimal
                : TextMarkerStyle.Disc,
            Margin  = new Thickness(0, 4, 0, 4),
            Padding = new Thickness(24, 0, 0, 0),
        };

        foreach (var item in lb.OfType<ListItemBlock>())
        {
            var li = new WpfListItem { Margin = new Thickness(0, 2, 0, 2) };
            foreach (var child in item)
                li.Blocks.Add(RenderBlock(child));
            list.ListItems.Add(li);
        }

        return list;
    }

    // Renders a horizontal rule (thematic break) into a WPF BlockUIContainer containing a Rectangle with appropriate styling.
    private static System.Windows.Documents.BlockUIContainer RenderRule()
    {
        var line = new System.Windows.Shapes.Rectangle
        {
            Height              = 1,
            Fill                = RuleBrush,
            Margin              = new Thickness(0, 10, 0, 10),
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        return new System.Windows.Documents.BlockUIContainer(line);
    }

    // Renders a Markdig QuoteBlock into a WPF Section with muted text color and margin. Each child block of the QuoteBlock is rendered and added to the Section's Blocks collection.
    private static WpfSection RenderQuote(QuoteBlock qb)
    {
        var section = new WpfSection
        {
            Foreground = MutedColor,
            Margin     = new Thickness(16, 4, 0, 4),
        };
        foreach (var child in qb)
            section.Blocks.Add(RenderBlock(child));
        return section;
    }

    // Renders a Markdig ParagraphBlock into a WPF Paragraph with appropriate margin. Each inline element of the ParagraphBlock is rendered and added to the Paragraph's Inlines collection.
    private static WpfParagraph RenderParagraph(ParagraphBlock pb)
    {
        var p = new WpfParagraph { Margin = new Thickness(0, 4, 0, 8) };
        if (pb.Inline is not null)
            foreach (var inline in pb.Inline)
                p.Inlines.Add(RenderInline(inline));
        return p;
    }
    #endregion

    #region Inline Rendering
    // Renders a Markdig Inline element into a WPF Inline element. Different types of inlines such as literal text, emphasis, code, line breaks, HTML, and links are handled appropriately.
    private static WpfInline RenderInline(Inline inline) => inline switch
    {
        LiteralInline li   => new WpfRun(li.Content.ToString()),
        EmphasisInline em  => RenderEmphasis(em),
        CodeInline ci      => RenderInlineCode(ci),
        LineBreakInline    => new System.Windows.Documents.LineBreak(),
        HtmlInline         => new WpfRun(),
        LinkInline lk      => RenderLink(lk),
        _                  => new WpfRun()
    };

    // Renders a Markdig EmphasisInline into a WPF Span with appropriate font weight or style based on the delimiter count. The child inlines of the EmphasisInline are rendered and added to the Span's Inlines collection.
    private static WpfInline RenderEmphasis(EmphasisInline em)
    {
        var span = new WpfSpan();
        foreach (var child in em)
            span.Inlines.Add(RenderInline(child));

        if (em.DelimiterCount == 2)
            span.FontWeight = FontWeights.Bold;
        else
            span.FontStyle = FontStyles.Italic;

        return span;
    }

    // Renders a Markdig CodeInline into a WPF Run with monospaced font, adjusted font size, and configured foreground and background colors to visually represent inline code.
    private static WpfInline RenderInlineCode(CodeInline ci) =>
        new WpfRun(ci.Content.ToString())
        {
            FontFamily = MonospaceFont,
            FontSize   = BaseFontSize - 1,
            Foreground = CodeFgColor,
            Background = CodeBgColor,
        };

    // Renders a Markdig LinkInline into a WPF Span with underline text decoration and configured heading color. The child inlines of the LinkInline are rendered and added to the Span's Inlines collection.
    private static WpfInline RenderLink(LinkInline lk)
    {
        var span = new WpfSpan
        {
            TextDecorations = System.Windows.TextDecorations.Underline,
            Foreground      = HeadingColor,
        };
        foreach (var child in lk)
            span.Inlines.Add(RenderInline(child));
        return span;
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
