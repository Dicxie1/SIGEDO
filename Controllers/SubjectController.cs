using Microsoft.AspNetCore.Mvc;
using Asistencia.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Asistencia.Models;
namespace Asistencia.Controllers;
public class SubjectController : Controller
{
    private readonly ApplicationDbContext _context;

    public SubjectController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IActionResult> Index()
    {
        var careers = await _context.Careers
            .Include( c => c.Subjects)
            .ToListAsync();
        ViewBag.CareerList = new SelectList(careers, "CareerId", "Name");
        return View(careers);
    }
    public async Task<IActionResult> Admin()
    {
        var careers = await _context.Careers
            .Include( c => c.Subjects)
            .ToListAsync();
        ViewBag.CareerList = new SelectList(careers, "Id", "Name");
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSubject( [FromBody] Subject subject)
    {
        if (ModelState.IsValid)
        {
            _context.Add(subject);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Asignatura Registrado"});
        }
        return Json(new { success = false, message = $"Datos Incorrecto: SubjectId: {subject.SubjectId} SubjetName: {subject.SubjetName} " +
                                                    $"Semester: {subject.Semester} Academi: {subject.AcademiYear} Credists: {subject.Credits} " +
                                                    $"Career: {subject.CareerId} Area: {subject.Area}"});
    } 
    [HttpPost]
    public async Task<IActionResult> AddCareer([FromBody] Career career)
    {
        if(ModelState.IsValid)
        {
            _context.Add(career);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Carrera Registrado "});
        }
        return Json(new { success = false, message = "Datos Incorrecto"});
    } 
    
    public async Task<IActionResult> GetCourses(string subjectId)
    {
        var coursesList =  _context.Courses
            .Where(c => c.SubjectId == subjectId)
            .Include(e => e.Enrollments)
            .Select(c => new
            {
                courseId = c.IdCourse,
                year = c.Year,
                students = c.Enrollments.Count(),
                c.isActive

            });
        return Json(new {success = true, courses = coursesList});
    }
    [HttpGet]
    public async Task<JsonResult> GetSubjectByCareer(int careerId)
    {
        var subjects = await _context.Subjects
            .Include(c => c.Career)
            .Where( s =>  s.CareerId == careerId)
            .Select(s => new
            {
                id = s.SubjectId,
                name = s.SubjetName
            })
            .ToListAsync();
        if(subjects == null)
        {
            return Json( new {success = false, msg = "No existe carrera"});
        }
        return Json(new {success = true, data = subjects});
    }
    [HttpGet]
    public  async Task<IActionResult> GetSubjectCredit(string subjectId)
    {
        var subjectCredit = await _context.Subjects
            .Where(s => s.SubjectId == subjectId)
            .Select( e => new { Credit = e.Credits}).FirstOrDefaultAsync();
        if( subjectCredit?.Credit == 0) return Json(new {success = false, data = "N/D"});
        return Json(new {success = true, data = subjectCredit});
    }
    [HttpPost]
    [ValidateAntiForgeryToken] // Valida el token enviado en las cabeceras HTTP
    public async Task<IActionResult> EditSubject([FromBody] Subject model)
    {
        // 1. Validar que el modelo sea correcto según las data annotations
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Los datos proporcionados no son válidos." });
        }

        try
        {
            // 2. Buscar si la asignatura existe en la base de datos
            var subjectDb = await _context.Subjects
                .FirstOrDefaultAsync(s => s.SubjectId == model.SubjectId);

            if (subjectDb == null)
            {
                return Json(new { success = false, message = "La asignatura no fue encontrada." });
            }

            // 3. Actualizar las propiedades deseadas
            subjectDb.SubjetName = model.SubjetName;
            subjectDb.CareerId = model.CareerId;
            subjectDb.AcademiYear = model.AcademiYear; // O AcademiYear según tu entidad
            subjectDb.Semester = model.Semester;
            subjectDb.Area = model.Area;
            subjectDb.Credits = model.Credits;

            // 4. Guardar los cambios asíncronamente
            _context.Subjects.Update(subjectDb);
            await _context.SaveChangesAsync();

            // 5. Retornar respuesta de éxito para SweetAlert2
            return Json(new { success = true, message = "La asignatura se ha actualizado correctamente." });
        }
        catch (DbUpdateException ex)
        {
            // Errores de restricciones de clave foránea o base de datos
            return Json(new { success = false, message = "Error de consistencia en la base de datos: " + ex.InnerException?.Message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Ocurrió un error inesperado: " + ex.Message });
        }
    }
}