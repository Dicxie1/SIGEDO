using Asistencia.Data;
using Asistencia.Models;
using Asistencia.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace Asistencia.Controllers;
public class ClassroomController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ClassroomService _classroomService;
    public ClassroomController(ApplicationDbContext context, ClassroomService classroomService)
    {
        _context = context;
        _classroomService = classroomService;
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> AddClassroom([FromBody] Classroom classroom )
    {
        if (ModelState.IsValid)
        {
            try{
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();
            return Json(new {success = true, data = "Datos registrado"});
            }catch(DbUpdateException ex)
            {   
                string message = "Error al guardar los Datos";  
                if(ex.InnerException is PostgresException pgEx)
                {
                    switch (pgEx.SqlState)
                    {
                        case "23505": // llave Primaria duplicada
                            message = $"Ya existe un Registro con este Codigo: {classroom.ClassroomId}";
                            break;
                        case "23502":
                            message ="Faltan datos obligados";
                            break;
                    }
                }
                return Json(new {success = false, message});
            }
        }
        else if(classroom == null)
        {
            return Json(new {success = false, data = "Dato null"});
        }
        return Json(new {success = false, data = "Datos No registrado registrado"});
    }
    /// <summary>
    /// Controller /Classroom
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Index()
    {
        var classrooms = await _classroomService.GetClassroomsAsync();
        ViewBag.ClassroomList = await _classroomService.GetClassroomsList();
        string[] days = { "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado", "Domingo" };
        ViewBag.Days = days;
        return View(classrooms);
    }
    [HttpGet]
    public async Task<IActionResult> EditClassroom(string classroomId)
    {
        if(classroomId == null) return NotFound();
        var classroom = await _context.Classrooms
            .FindAsync(classroomId);
        if(classroom == null) return NotFound();
        return PartialView("_EditClassroom", classroom);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([Bind("")] Classroom classroom)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _context.Classrooms.Update(classroom);
                await _context.SaveChangesAsync();
                return Json(new {success = true, message ="Aula Actualizado"});
            }catch(Exception ex)
            {
                return StatusCode(500, $"Error Interno al intentar guarda en la base de datos ${ex.Message}");
            }
        }
        return Json(new {success= false, message = "Datos Invalido. Verifique los campos"});
    }
    public async Task<IActionResult> Schedule()
    {
        return View();
    }
    public async Task<JsonResult> IsClassroomAvaliable([FromBody]Schedule schedule)
    {
        return Json(new {success = false, message = "Caracteristica en Construcción"});
    }

    [HttpPost]
    public async Task<JsonResult> GetClassroomSchedule(string classroomId, string startDate)
    {

        if (string.IsNullOrEmpty(classroomId))
        {
            return Json(new { success = false, message = "El ID del aula es obligatorio." });
        }
        DateTime targetDate;
        if (DateOnly.TryParse(startDate, out var parsedDate))
        {
            targetDate = parsedDate.ToDateTime(TimeOnly.MinValue);
        }
        else
        {
            targetDate = DateTime.Now;
        }
        try
        {
            var scheduleView = await _classroomService.Calendar(classroomId, targetDate);
            var jsonResponse = new
            {
                success = true,
                classroomName = scheduleView.ClassroomName,
                weekStart = scheduleView.WeekStart.ToString("yyyy-MM-dd"),
                weekEnd = scheduleView.WeekEnd.ToString("yyyy-MM-dd"),
                events = scheduleView.Events.Select(e => new
                {
                    title = e.CourseName,
                    start = e.StartTime.ToString(@"hh\:mm"),
                    end = e.EndTime.ToString(@"hh\:mm"),
                    date = e.Date.ToString("yyyy-MM-dd"),
                    color = e.ColorHex ?? "#0D6EFD",
                    description = $"{e.CourseName}({e.StartTime:hh\\:mm} - {e.EndTime:hh\\:mm})",
                    teacher = "Dicxie Danuard Madrigal Brack"
                }) 
            };
            return Json(jsonResponse);
        }catch(Exception ex)
        {
            return Json(new {success = false, message = "Error en obtener horarios" + ex.Message});
        }
        /*else
        {
            var schedule = await _classroomService.Calendar(classroomId, DateTime.Now);
            return Json(new { success = true, message = $"Lista de clase: {schedule} ", data = schedule });
        }*/
    }
}