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
    [HttpPost]
    [Route("Student/AgregarMasivo")] // Keeps your existing route mapping intact
    public async Task<IActionResult> ImportBulkStudents([FromBody] List<Student> students)
    {
        // 1. Validate that the payload is not null or empty
        if (students == null || !students.Any())
        {
            return BadRequest(new { message = "The student list is empty or the data format is invalid." });
        }

        // 2. Validate data annotations against the Student model structure
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Some records fail to meet the required validation criteria." });
        }

        // Using database transaction to guarantee atomicity (All-or-Nothing execution)
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var duplicateIds = new List<string>();

            foreach (var student in students)
            {
                // 3. Primary Key duplication safeguard for PostgreSQL
                bool recordExists = await _context.Students.AnyAsync(s => s.Id == student.Id);

                if (recordExists)
                {
                    duplicateIds.Add(student.Id);
                    continue; // Skip current record to prevent execution breakdown
                }

                // Data sanitization
                student.Name = student.Name?.Trim();
                student.LastName = student.LastName?.Trim();
                student.Email = student.Email?.Trim();

                // Queue for EF tracking insertion
                await _context.Students.AddAsync(student);
            }

            // 4. Halt execution if all entries in the CSV already exist in the database
            if (duplicateIds.Count == students.Count)
            {
                return Conflict(new { message = "All student entries provided in the file are already registered." });
            }

            // 5. Execute batch save modifications to PostgreSQL
            await _context.SaveChangesAsync();

            // Commit transaction smoothly
            await transaction.CommitAsync();

            // 6. Return successful response with fine-grained duplication feedback
            if (duplicateIds.Any())
            {
                return Ok(new
                {
                    message = $"Successfully imported {students.Count - duplicateIds.Count} students. {duplicateIds.Count} existing records were skipped.",
                    duplicates = duplicateIds
                });
            }

            return Ok(new { message = "All students were imported successfully." });
        }
        catch (Exception ex)
        {
            // Revert any pending database writes safely if a connection issue occurs
            await transaction.RollbackAsync();

            // Keep track of the internal exception details
            return StatusCode(500, new { message = "An internal error occurred while processing the batch request.", detail = ex.Message });
        }
    }
    [HttpPost]
    [Route("Student/CheckExistingIds")]
    public async Task<IActionResult> CheckExistingIds([FromBody] List<string> studentIds)
    {
        if (studentIds == null || !studentIds.Any())
        {
            return Ok(new List<string>()); // Devuelve lista vacía si no hay nada que validar
        }

        // Busca en PostgreSQL todos los IDs que coincidan con la lista enviada
        var existingIds = await _context.Students
            .Where(s => studentIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();

        return Ok(existingIds);
    }
}
