using DocumentFormat.OpenXml.InkML;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace Asistencia.Documents
{
    public static class QuestPdfMarkdownRenderer
    {
        public static void RenderMarkdown(this ColumnDescriptor column, string markdownText)
        {
            if (string.IsNullOrEmpty(markdownText)) return;
            var pipeline = new MarkdownPipelineBuilder().Build();
            var document = Markdown.Parse(markdownText, pipeline);
            foreach(var block in document)
            {
                switch(block)
                {
                    case HeadingBlock heading:
                        RenderHeading(column, heading);
                        break;
                    case ParagraphBlock paragraph:
                        RenderParagraph(column, paragraph);
                        break;
                    case ListBlock list:
                        RenderList(column, list);
                        break;
                    default:
                        break;

                }
            }
        }
        private static void RenderHeading(ColumnDescriptor column, HeadingBlock heading)
        {
            string text = heading.Inline?.FirstChild?.ToString() ?? "";
            column.Item().Padding(12).PaddingBottom(4).Row(row => {
                row.RelativeItem().Column(col =>
                {
                    float fontSize = 12;
                    string fontColor = Colors.Grey.Darken3; // Por defecto para H3

                    if (heading.Level == 1 || heading.Level == 2)
                    {
                        fontColor = Colors.Blue.Darken3;
                    }
                    col.Item().Text(targetText =>
                    {
                        if(heading.Inline != null)
                        {
                            ProcessInLines(targetText, heading.Inline, isBoldParent: true, fontSize: fontSize, fontColor: fontColor);
                        }
                    });
                    if (heading.Level == 1)
                    {
                        col.Item().PaddingTop(2).Height(1).Background(Colors.Grey.Lighten2);
                    }
                });
            });
        }
        private static void RenderParagraph(ColumnDescriptor column, ParagraphBlock paragraph )
        {
            string text = paragraph.Inline?.FirstChild?.ToString() ?? "";
            column.Item().PaddingBottom(8)
                .DefaultTextStyle(style =>
                style.LineHeight(1.3f)
                .FontSize(12)
                .FontColor(Colors.Grey.Darken4)
                )
                .Text(targetText =>
            {
                if(paragraph.Inline != null)
                {
                    ProcessInLines(targetText, paragraph.Inline);
                }
            });
                
        }
        private static void RenderList(ColumnDescriptor column, ListBlock list)
        {
            foreach(var item in list)
            {
                if( item is ListItemBlock listItem)
                {
                    var subBlock = listItem.FirstOrDefault() as ParagraphBlock;
                   
                    column.Item().PaddingLeft(15).PaddingBottom(4).Row(row =>
                    {
                        row.AutoItem().PaddingRight(6).Text("•")
                            .FontSize(12)
                            .Bold()
                            .FontColor(Colors.Blue.Medium);
                        row.RelativeItem()
                            .DefaultTextStyle(style => style.LineHeight(1.2f).FontSize(12).FontColor(Colors.Grey.Darken4))
                            .Text(targetText =>
                            {
                                if (subBlock?.Inline != null)
                                {
                                    ProcessInLines(targetText, subBlock.Inline);
                                }
                            });
                    });
                }
            }
            column.Item().PaddingBottom(6);
        }
        ///<summary>
        /// Recursive method translate markding fraqment (Span) to QuestPDF 
        ///</summary>
        private static void ProcessInLines(
            TextDescriptor target, 
            ContainerInline container, 
            bool isBoldParent = false, 
            bool isItalicParent = false,
            float? fontSize = null,
            string? fontColor= null)
        {
            foreach(var inline in container) 
            {
                switch (inline) 
                {
                    case LiteralInline literal:
                        var span = target.Span(literal.Content.ToString());
                        if (isBoldParent) span.Bold();
                        if (isItalicParent) span.Italic();
                        if (fontSize.HasValue) span.FontSize(fontSize.Value);
                        if (!string.IsNullOrEmpty(fontColor)) span.FontColor(fontColor);
                        break;
                    case CodeInline code:
                        var codeSpan = target.Span(code.Content);
                        codeSpan.FontFamily(Fonts.CourierNew)
                            .FontColor(Colors.DeepPurple.Medium)
                            .Bold()
                            .BackgroundColor(Colors.Grey.Lighten3);
                        if (fontSize != null) codeSpan.FontSize((fontSize ?? 12F) - 1F);
                        break;
                    case EmphasisInline emphasis:
                        bool isBold = emphasis.DelimiterCount >= 2 || isBoldParent;
                        bool isItalic = emphasis.DelimiterCount == 1 || isItalicParent;
                        ProcessInLines(target, emphasis, isBold, isItalic);
                        break;
                    case LinkInline link:
                        ProcessInLines(target, link, isBoldParent, isItalicParent, fontSize, fontColor);
                        break;
                    case LineBreakInline _:
                        target.EmptyLine();
                        break;
                    default:
                        if (inline is ContainerInline containerInline)
                        {
                            ProcessInLines(target, containerInline, isBoldParent, isItalicParent, fontSize, fontColor);
                        }
                        else 
                        {
                            var fallbackText = inline.ToString();
                            if (!string.IsNullOrEmpty(fallbackText))
                            {
                                var fallbackSpan = target.Span(fallbackText);
                                if (isBoldParent) fallbackSpan.Bold();
                                if (isItalicParent) fallbackSpan.Italic();
                            }
                        }
                        break;
                }
            }
        }
    }
}
