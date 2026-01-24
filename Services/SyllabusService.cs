using Asistencia.Data;
using Asistencia.Models;
using Asistencia.Models.Dtos;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.EntityFrameworkCore;
using WPTableCell = DocumentFormat.OpenXml.Wordprocessing.TableCell;
using WPText = DocumentFormat.OpenXml.Wordprocessing.Text;
using WPTable = DocumentFormat.OpenXml.Wordprocessing.Table;
using WPTableRow = DocumentFormat.OpenXml.Wordprocessing.TableRow;
using System.Globalization;
using WPColor = DocumentFormat.OpenXml.Wordprocessing.Color;
using WPFontSize = DocumentFormat.OpenXml.Wordprocessing.FontSize;
using WPPAragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using System.Text;
using W = DocumentFormat.OpenXml.Wordprocessing;
using System.Xml.Linq;
using System.IO;
using System.Drawing.Printing;
using Microsoft.AspNetCore.Components.Web;

namespace Asistencia.Services;

public class SyllabusService
{
    private readonly ApplicationDbContext _context;

    public SyllabusService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Obtener lista para la vista
    public async Task<List<SyllabusItem>> GetByCourseAsync(int courseId)
    {
        return await _context.SyllabusItems
            .Include(x => x.Course)
                .ThenInclude(x => x.Subject)
            .Where(x => x.CourseId == courseId)
            .OrderBy(x => x.Date)
            .ToListAsync();
    }

    // Guardar lista completa (Upsert: Update or Insert)
    public async Task SaveBatchAsync(int courseId, List<SyllabusDto> items)
    {
        foreach (var item in items)
        {
            if (item.SyllabusId == 0)
            {
                // NUEVO
                var newItem = new SyllabusItem
                {
                    CourseId = courseId,
                    Date = item.Date,
                    Objectives = item.Objectives,
                    Content = item.Content,
                    Strategies = item.Strategies,
                    Resources = item.Resources,
                    Evaluations = item.Evaluations,
                    Bibliography = item.Bibliography
                };
                _context.SyllabusItems.Add(newItem);
            }
            else
            {
                // EXISTENTE (Editar)
                var existing = await _context.SyllabusItems.FindAsync(item.SyllabusId);
                if (existing != null)
                {
                    existing.Date = item.Date;
                    existing.Objectives = item.Objectives;
                    existing.Content = item.Content;
                    existing.Strategies = item.Strategies;
                    existing.Resources = item.Resources;
                    existing.Evaluations = item.Evaluations;
                    existing.Bibliography = item.Bibliography;
                }
            }
        }
        await _context.SaveChangesAsync();
    }

    // Eliminar una fila individual
    public async Task DeleteItemAsync(int id)
    {
        var item = await _context.SyllabusItems.FindAsync(id);
        if (item != null)
        {
            _context.SyllabusItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
    
    public List<SyllabusItem> ReadWord(Stream stream, int courseId)
    {
        var list = new List<SyllabusItem>();
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart?.Document.Body;
        if (body == null) return list;

        var table = body.Elements<WPTable>().FirstOrDefault();
        if (table == null) return list;

        var rows = table.Elements<WPTableRow>().Skip(1).ToList();
        if (!rows.Any()) return list;

        foreach (var row in rows)
        {
            var cells = row.Elements<WPTableCell>().ToList();
            // 🔧 COMPLETAR HASTA 7 COLUMNAS
            while (cells.Count < 7)
            {
                cells.Add(new WPTableCell(
                    new Paragraph(new Run(new Text(string.Empty)))
                ));
            }
            if (cells.All(c => string.IsNullOrWhiteSpace(GetText(c))))
                continue;
            var texts = cells.Select(c => GetHtmlFromCell(c).Trim()).ToList();
        
            // FILA TOTALMENTE VACÍA
            if (texts.All(string.IsNullOrWhiteSpace))
            {
                Console.WriteLine("Fila ignorada: completamente vacía");
                continue;
            }
            // FECHA
            if (!TryParseDate(texts[0], out DateTime date))
            {
                Console.WriteLine($"Fila ignorada por fecha inválida: {texts[0]}");
                continue;
            }
            list.Add(new SyllabusItem
            {
                CourseId = courseId,
                Date = date,
                Objectives = texts[1],
                Content = texts[2],
                Strategies = texts[3],
                Resources = texts[4],
                Evaluations = texts[5],
                Bibliography = texts[6]
            });
        }
        return list;
    }
    public int isValid(Stream stream)
    {
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart?.Document.Body;

        var table = body?.Elements<WPTable>().FirstOrDefault();
        if (table == null) return -1;

        var rows = table.Elements<WPTableRow>();
        if (!rows.Any()) return -1;
        return 1;
    }
    private string GetText(WPTableCell cell)
    {
        return string.Join( " ", 
            cell.Descendants<WPText>().Select( t => t.Text)).Trim();
    }
private bool TryParseDate(string input, out DateTime date)
{
    string[] formats =
    {
        "dd/MM/yyyy",
        "d/M/yyyy",
        "dd-MM-yyyy",
        "yyyy-MM-dd",
        "d MMMM yyyy",
        "dd 'de' MMMM 'de' yyyy"
    };

    return DateTime.TryParseExact(
        input.Trim(),
        formats,
        new System.Globalization.CultureInfo("es-NI"), // o es-ES
        DateTimeStyles.None,
        out date
    );
}
public List<SyllabusPreviewRowDto> PreviewWord(Stream stream)
{
    var result = new List<SyllabusPreviewRowDto>();

    using var doc = WordprocessingDocument.Open(stream, false);
    var body = doc.MainDocumentPart?.Document.Body;
    if (body == null) return result;

    var table = body.Elements<WPTable>().FirstOrDefault();
    if (table == null) return result;

    var rows = table.Elements<WPTableRow>().Skip(1).ToList();
    int rowIndex = 1; // fila real en Word

    foreach (var row in rows)
    {
        var cells = row.Elements<WPTableCell>().ToList();

        while (cells.Count < 7)
            cells.Add(new WPTableCell(new Paragraph(new Run(new Text("")))));

        var texts = cells.Select(c => GetText(c).Trim()).ToList();
        if (cells.All(c => string.IsNullOrWhiteSpace(GetText(c))))
                continue;

        var preview = new SyllabusPreviewRowDto
        {
            RowNumber = rowIndex
        };

        if (texts.All(string.IsNullOrWhiteSpace))
        {
            preview.IsValid = false;
            preview.Error = "Fila vacía";
            result.Add(preview);
            rowIndex++;
            continue;
        }

        if (!TryParseDate(texts[0], out DateTime date))
        {
            preview.IsValid = false;
            preview.Error = $"Fecha inválida: {texts[0]}";
            result.Add(preview);
            rowIndex++;
            continue;
        }
        
        preview.IsValid = true;
        preview.Date = DateOnly.FromDateTime(date);
        preview.DateFormatted = preview.Date.ToString("dd/MM/yyyy");
        preview.Objectives   = texts[1];
        preview.Content      = texts[2];
        preview.Strategies   = texts[3];
        preview.Resources    = texts[4];
        preview.Evaluations  = texts[5];
        preview.Bibliography = texts[6];

        result.Add(preview);
        rowIndex++;
    }

    return result;
}

private string GetHtmlFromCell(WPTableCell cell)
{
    var sb = new StringBuilder();

    foreach (var para in cell.Elements<Paragraph>())
    {
        foreach (var run in para.Elements<Run>())
        {
            var text = run.GetFirstChild<Text>()?.Text;
            if (string.IsNullOrEmpty(text)) continue;

            var html = text;

            var props = run.RunProperties;
            if (props != null)
            {
                if (props.Bold != null)
                    html = $"<strong>{html}</strong>";

                if (props.Italic != null)
                    html = $"<em>{html}</em>";

                if (props.Underline != null)
                    html = $"<u>{html}</u>";
                var color = props.GetFirstChild<WPColor>();
                if (color != null)
                    html = $"<span style='color:#{color.Val}'>{html}</span>";
                var fontSize = props.GetFirstChild<WPFontSize>();
                if (fontSize?.Val != null && int.TryParse(fontSize.Val, out int halfPoints))
                {
                    int px = halfPoints / 2;
                    html = $"<span style='font-size:{px}px'>{html}</span>";
                }
            }
            sb.Append(html);
        }
        sb.Append("<br>");
    }
    return sb.ToString().Trim();
    }
}