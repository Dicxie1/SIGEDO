using Asistencia.Data;
using Asistencia.Models;
using Asistencia.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Asistencia.Services;

namespace Asistencia.Controllers; 

public class SyllabusController : Controller
{
    private readonly SyllabusService _service;
    private readonly ApplicationDbContext _context;

    public SyllabusController(SyllabusService service, ApplicationDbContext context)
    {
        _service = service;
        _context = context;
    }

    // GET: Vista Principal
    public async Task<IActionResult> Index(int courseId)
    {
        var items = await _service.GetByCourseAsync(courseId);
        ViewBag.CourseId = courseId;
        ViewBag.CourseName = _context.Courses.Find(courseId)?.Subject?.SubjetName;
        
        return View(items);
    }
    // POST: Guardar Cambios (AJAX)
    [HttpPost]
    public async Task<IActionResult> SaveAll(int courseId, [FromBody] List<SyllabusDto> items)
    {
        if(!ModelState.IsValid) return Json(new {success= false, message = "Datos invalidos"});
        try
        {
            await _service.SaveBatchAsync(courseId, items);
            return Json(new { success = true, message = "Planificación guardada correctamente." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error al guardar: " + ex.Message });
        }
    }

    // POST: Eliminar fila (AJAX)
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteItemAsync(id);
        return Json(new { success = true });
    }
    [HttpPost]
    public async Task<IActionResult> ImportFromWord(IFormFile file, int courseId)
    {
        if(file == null  || file.Length == 0)
            return Json(new {success = false, message= "Documento Vacio"});
        var isvalid = _service.isValid(file.OpenReadStream());
        if (isvalid == -1)
        {
            return Json(new {success= false, message= " Tabla Vacio"});
        }
        else
        {
        var syllabusItems =  _service.ReadWord(file.OpenReadStream(), courseId);
        if(syllabusItems.Count == 0) return Json(new {success= false, message= "Tabla Sin Datos"});
        _context.SyllabusItems.AddRange(syllabusItems);
        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "Registrado correctamente" });
        }
    }
    [HttpPost]
    public IActionResult PreviewFromWord(IFormFile file, int courseid)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "Archivo vacío" });

        var preview = _service.PreviewWord(file.OpenReadStream());

        return Json(new
        {
            success = true,
            total = preview.Count,
            valid = preview.Count(x => x.IsValid),
            invalid = preview.Count(x => !x.IsValid),
            rows = preview
        });
    }
    
}