using Microsoft.AspNetCore.Mvc;
using Asistencia.Services;
namespace Asistencia.Controllers;

public class ScheduleController : Controller
{
    private ScheduleService _service;
    public ScheduleController(ScheduleService service)
    {
        _service = service;
    }
    public IActionResult Index()
    {
        return View();
    }
    public async Task<IActionResult> Details(string classroomId)
    {
        var model = await _service.GetClassroomSchedule(classroomId);
        return PartialView("", model);
    }

    private async Task<bool> IsClassroomAvailable(string classroomId, int day, TimeSpan start, TimeSpan end)
    {
        bool existOverlap = await _service.IsClassroomAvailable(classroomId, day, start, end);
        return !existOverlap;
    }
    [HttpGet]
    public async Task<IActionResult> GetEvents(DateTime? start, DateTime? end)
    {
        // Si no se pasan fechas, usamos el mes actual por defecto
        var startDate = start ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var endDate = end ?? startDate.AddMonths(1).AddDays(-1);

        try
        {
            var events = await _service.GetCalendarEvents(startDate, endDate);

            // Si no hay eventos reales aún, podemos dejar los mock como ejemplo comentados
            // o simplemente retornar la lista vacía o real
            if (events.Count == 0 && !start.HasValue)
            {
                var hoy = DateTime.Today;
                var mockEvents = new List<object>
                 {
                    new { id = "m1", title = "Pensamiento Lógico", start = hoy.AddDays(1).ToString("yyyy-MM-ddT08:00:00"), end = hoy.AddDays(1).ToString("yyyy-MM-ddT10:00:00"), color = "#0d6efd", description = "Laboratorio 1" },
                    new { id = "m2", title = "Desarrollo Móvil (Flutter)", start = hoy.AddDays(2).ToString("yyyy-MM-ddT13:00:00"), end = hoy.AddDays(2).ToString("yyyy-MM-ddT15:00:00"), color = "#0d6efd", description = "Laboratorio Mac" }
                 };
                return Json(mockEvents);
            }

            return Json(events);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener eventos", details = ex.Message });
        }
    }
    public async Task<IActionResult> GetClassroom()
    {
        return Json(new { data = await _service.GetClassroomAsync() });
    }
}

    