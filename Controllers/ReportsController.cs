using Asistencia.Documents.Attendance;
using Asistencia.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;

namespace Asistencia.Controllers
{
    [Authorize(Roles = "Admin, Docente")] // Seguridad: Solo personal autorizado
    public class ReportsController : Controller
    {
        private readonly AttendanceService _reportService;
        private readonly ILogger<ReportsController> _logger;

        // Inyección de Dependencias
        public ReportsController(AttendanceService reportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        // GET: Reports/DownloadAttendancePdf?courseId=5
        [HttpGet]
        public async Task<IActionResult> DownloadAttendancePdf(int courseId)
        {
            try
            {
                // 1. Obtener los Datos (El Servicio hace el trabajo sucio)
                var reportModel = await _reportService.GetReportDataAsync(courseId);

                // 2. Generar el Documento QuestPDF
                var document = new AttendanceDocument(reportModel);

                // Generar los bytes del PDF
                byte[] pdfBytes = document.GeneratePdf();

                // 3. Definir un nombre de archivo amigable
                // Ej: Asistencia_IngSoftware_20240212.pdf
                string cleanCourseName = reportModel.CourseName.Replace(" ", "_");
                string fileName = $"Asistencia_{cleanCourseName}_{DateTime.Now:yyyyMMdd}.pdf";

                // 4. Retornar el Archivo al navegador
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando reporte de asistencia para el curso {CourseId}", courseId);

                // Si algo falla, mostramos un mensaje o redirigimos con error
                // Opción A: Mensaje simple
                return Content($"Error al generar el reporte: {ex.Message}");

                // Opción B: Redirigir con alerta (si usas TempData/Toastr)
                // TempData["Error"] = "No se pudo generar el reporte.";
                // return RedirectToAction("Index", "Dashboard");
            }
        }

        // (Opcional) Visualizar en el navegador en lugar de descargar
        [HttpGet]
        public async Task<IActionResult> PreviewAttendancePdf(int courseId)
        {
            try
            {
                var reportModel = await _reportService.GetReportDataAsync(courseId);
                var document = new AttendanceDocument(reportModel);
                byte[] pdfBytes = document.GeneratePdf();

                // Al no pasarle el 'fileName', el navegador intentará abrirlo en una pestaña nueva (Preview)
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }
    }
}
