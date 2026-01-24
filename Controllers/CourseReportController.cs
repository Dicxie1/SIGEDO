namespace Asistencia.Controllers;

using System.Threading.Tasks;
using Asistencia.Data;
using Asistencia.Models;
using Asistencia.Models.ViewModels;
using Asistencia.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


public class CourseReportController : Controller
{
    private readonly ReportTermService _reportService;
    private readonly ApplicationDbContext _context; // Solo para llenar el Select de Cortes
    public CourseReportController(ReportTermService reportService, ApplicationDbContext context)
    {
        _reportService = reportService;
        _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> ProgrammaticProgress(int courseId, int? termId)
    {
        Console.WriteLine($" id resivido: {termId}");
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
        

}