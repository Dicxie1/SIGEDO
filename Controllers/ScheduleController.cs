using Microsoft.AspNetCore.Mvc;
using Asistencia.Services;
using Asistencia.Models.DTOs;
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterEvent(CreateEventDto eventDto)
    {
        if (!ModelState.IsValid)
        {
           return BadRequest(ModelState);
        }
        bool success = await _service.RegisterEventAsync(eventDto);
        if (success)        {
            return Ok(new 
                { 
                    success = true, 
                    message = "¡La actividad se agendó correctamente!" 
                });
        }
        else
        {
            return BadRequest(new 
                { 
                    success = false, 
                    message = "No se pudo registrar la actividad. Verifica las fechas o intenta más tarde." 
                });
        }
    }
    [HttpGet]
    public async Task<IActionResult> GetTeachers(string term = "")
    {
        // 1. Llamas a tu servicio para buscar los docentes.
        // (Asumo que crearás este método en ScheduleService. Si no pasas 'term', devuelve todos o un top 20).
        var teachers = await _service.SearchTeachersAsync(term);

        // 2. Mapeamos la lista a la estructura exacta que Select2 exige (id, text)
        var select2Data = teachers.Select(t => new
        {
            id = t.TeacherID,
            // Asumiendo que tu modelo Teacher tiene FirstName y LastName
            text = $"{t.FirstName} {t.LastName}"
        }).ToList();

        // 3. Retornamos el JSON encapsulado en "results"
        return Json(new { results = select2Data });
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTeacherEvent(int id)
    {
        if (id <= 0) return BadRequest(new { success = false, message = "ID no válido." });

        bool success = await _service.DeleteTeacherEventAsync(id);

        if (success)
        {
            return Ok(new { success = true, message = "La actividad fue eliminada correctamente." });
        }
        else
        {
            return BadRequest(new { success = false, message = "No se pudo eliminar. Es posible que el evento ya no exista." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTeacherEvent(UpdateEventDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Datos del formulario inválidos." });
        }

        bool success = await _service.UpdateTeacherEventAsync(dto);

        if (success)
        {
            return Ok(new { success = true, message = "¡Actividad modificada con éxito!" });
        }
        else
        {
            return BadRequest(new { success = false, message = "Ocurrió un error al intentar actualizar la base de datos." });
        }
    }
}

    