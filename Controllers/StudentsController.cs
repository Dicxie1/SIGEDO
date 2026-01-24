using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asistencia.Data;
using Asistencia.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Asistencia.Controllers;

public class StudentsController : Controller
{
    private readonly ApplicationDbContext _context;
    public StudentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var students = await _context.Students
        .OrderBy(n => n.Name)
        .ToListAsync();
        return View(students);
    }

    public IActionResult Agregar()
    {
        LoadCombos();
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Agregar([Bind("Id,Name,LastName,Ethnic,Cellphone,Email,Sexo")] Student student)
    {
        if (ModelState.IsValid)
        {
            _context.Add(student);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Estudiantes agregado exitosamente";
            return RedirectToAction(nameof(Index));
        }
        LoadCombos();
        return View(student);
    }

    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null || _context.Students == null)
        {
            return NotFound();
        }

        var student = await _context.Students.FindAsync(id);
        if (student == null)
        {
            return NotFound();
        }
        LoadCombos();
        return View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Student student)
    {
        if (id != student.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(student);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Estudiante actualizado exitosamente";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(student.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        LoadCombos();
        return View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStudent(string id)
    {
        var student = await _context.Students.FindAsync(id);
        if (string.IsNullOrEmpty(id))
        {
            return Json(  new {succes = false, message = "Error en el ID", statusCode = 404});
        }
        if(student == null)
        {
            return Json(new { success = false, message = "Estudiante no encontrado" });
        }
        try 
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Estudiante eliminado exitosamente" });
        }
        catch (Exception)
        {
            return Json(new { success = false, message = "Error al eliminar en la base de datos" });
        }
    }
    private void LoadCombos()
    {
        ViewBag.SexList = new SelectList(
            Enum.GetValues(typeof(Sex))
            .Cast<Sex>()
            .Select(e => new SelectListItem
            {
                Value = ((int)e).ToString(),
                Text = e.GetDisplayName()
            }), "Value", "Text");

        ViewBag.EthnicGroupList = new SelectList(
            Enum.GetValues(typeof(EthnicGroup))
            .Cast<EthnicGroup>()
            .Select(e => new SelectListItem
            {
                Value = ((int)e).ToString(),
                Text = e.GetDisplayName()
            }), "Value", "Text");
    }

    [HttpGet("Course/Details/{courseId}/Student/{studentId}")]
    public async Task<IActionResult> StudentInCourse()
    {
        return View();
    }

    [HttpGet("/Course/Details/Students/List")]
    public async Task<IActionResult> ListStudentInCourse()
    {
        return View();
    }

    private bool StudentExists(string id)
    {
        return _context.Students.Any(e => e.Id == id);
    }
    public IActionResult List(string busqueda)
    {
        var students = _context.Students.AsQueryable();
        if(!string.IsNullOrEmpty(busqueda))
        {
            busqueda = busqueda.ToLower();

            var words = busqueda.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach(var word in words)
            {
                students = students
                .Where(s => s.Name!.ToLower().Contains(word)
                 || s.LastName!.ToLower().Contains(word) || s.Id!.ToLower().Contains(word))
                .OrderBy(n => n.Name);
            }
        }
        
        var result = students
            .OrderBy( s=> s.LastName)
            .OrderBy( n=> n.Name)
            .Take(20)
            .Select(s => new
            {
                StudentId = s.Id,
                FullName = s.Name + " " + s.LastName
            }).ToList();
        return Json(result);
    }
}
