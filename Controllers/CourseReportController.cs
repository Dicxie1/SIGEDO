namespace Asistencia.Controllers;

using Asistencia.Data;
using Asistencia.Documents.FullReport.Models;
using Asistencia.Documents.ProgrammaticProgress;
using Asistencia.Models;
using Asistencia.Models.Analytics;
using Asistencia.Models.ViewModels;
using Asistencia.Services;
using Asistencia.Services.Analytics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System.Threading.Tasks;

public class CourseReportController : Controller
{
    private readonly ReportTermService _reportService;
    private readonly ApplicationDbContext _context; // Solo para llenar el Select de Cortes
    private readonly AcademicRagService _ragService;
    public CourseReportController(ReportTermService reportService, ApplicationDbContext context, AcademicRagService ragService)
    {
        _reportService = reportService;
        _context = context;
        _ragService = ragService;
    }
    [HttpGet("/Course/{courseId}/ProgrammaticProgress/{termId}")]
    public async Task<IActionResult> ProgrammaticProgress(int courseId, int? termId)
    {
        Console.WriteLine($" id resivido: {termId}");
        var termExists = await _context.AcademicTerms
                .AnyAsync(t => t.TermId == termId &&
                   t.CourseId == courseId);
        if (!termExists)
        {
            return NotFound();
        }
        // 1. MANEJO DEL CORTE POR DEFECTO
        // Si el usuario viene del menú principal, 'termId' será null.
        // Buscamos el primer corte cronológico o el activo.
        if (termId == null)
            {
                var defaultTerm = await _context.AcademicTerms
                    .Where(t => t.CourseId == courseId)
                    .OrderBy(t => t.StartDate) // Ordenar por fecha para obtener el 1ro
                    .FirstOrDefaultAsync();

                if (defaultTerm != null)
                {
                    termId = defaultTerm.TermId;
                    Console.WriteLine($"Configuracion {termId}");
                }
                else
                {
                    // Caso Borde: El curso se creó pero no tiene cortes definidos.
                    // Redirigimos o mostramos error.
                    TempData["Error"] = "Este curso no tiene cortes académicos configurados.";
                    Console.WriteLine("Este curso no tiene cortes academicos configurado");
                    return RedirectToAction("Index", "House", new { id = courseId });
                }
            }

            // 2. OBTENER DATOS DEL SERVICIO
            var model = await _reportService.GetTermProgressAsync(courseId, termId.Value);

            if (model == null)
            {
                Console.WriteLine("No se control datos para el corte \n\n\n");
                return NotFound("No se encontraron datos para el corte solicitado.");
            }

            // 3. PREPARAR DATOS PARA LA VISTA (Dropdown de Selección)
            // Cargamos la lista de todos los cortes de este curso para llenar el <select>
            ViewBag.Terms = await _context.AcademicTerms
                .Where(t => t.CourseId == courseId)
                .OrderBy(t => t.StartDate)
                .Select(t => new { t.TermId, t.Name })
                .ToListAsync();

            // Pasamos el CourseId para mantenerlo en los enlaces o formularios
            ViewBag.CourseId = courseId;

            return View(model);
        }
    [HttpGet]    
    public async Task<ActionResult> Imprimir(int courseId, int termId)
    {
        var model = await _reportService.GetTermProgressAsync(courseId, termId);

        if (model == null)
        {
            return NotFound("No se encontraron datos para generar el reporte.");
        }

        var document = new ProgrammatiProgressDoc(model);
        string pfgFileName = $"pp_{1}";
        var stream = new MemoryStream();
        document.GeneratePdf(stream);
        stream.Position = 0;
        byte[] file = document.GeneratePdf();
        return File(file, "application/pdf");
    }
    [HttpGet("/Course/{courseId}/AnalyticReport")]
    public async Task<IActionResult> AnalyticReport(int courseId)
    {
        FullAcademicReportViewModel reportData = await _reportService.GetCourseAnaliticReport(courseId);
        if (reportData == null) return BadRequest();

        var viewModel = new AcademicPreviewViewModel
        {
            CourseId = courseId,
            CourseName = reportData.ProgrammaticProgress?.FirstOrDefault().Value?.CourseName ?? "N/A",
            MarkdownContent = ""
        };
        return View(viewModel);
    }
    [HttpGet("/Course/{courseid}/AnalyticPreviewReport")]
    public async Task<IActionResult> AnaliticPreview(int courseid)
    {
        FullAcademicReportViewModel reportData = await _reportService.GetCourseAnaliticReport(courseid);
        if (reportData == null) return BadRequest();
        ViewBag.Instruction = _ragService.FormatDataToContext(reportData);
        return View();
    }
    [HttpGet("/Course/{courseid}/StreamAnalyticReport")]
    public async Task StreamAnalyticReport(int courseid)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");
        Response.Headers.Append("X-Accel-Buffering", "no");
        FullAcademicReportViewModel reportViewModel = await _reportService.GetCourseAnaliticReport(courseid);
        if (reportViewModel == null) return;
        await _ragService.GenerateAcademicAnaliticsStreamAsync(reportViewModel, Response.Body);
    }
}