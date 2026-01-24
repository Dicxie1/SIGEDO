using Microsoft.AspNetCore.Mvc;
using Asistencia.Models.DTOs;
using Asistencia.Models;
using Asistencia.Data;
using Asistencia.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace Asistencia.Controllers;

public class GradebookController : Controller
{
    readonly ApplicationDbContext _context;
    readonly IGradebookExportService _exportService;
    public GradebookController(ApplicationDbContext context, IGradebookExportService exportService)
    {
        _context = context;
        _exportService = exportService;
    }
    [HttpPost]
    public async Task<JsonResult> SaveGrade([FromBody] SaveGradeDto input)
    {
        if (input.Score < 0) 
            return Json(new { success = false, message = "Nota negativa no permitida." });
        try
        {
            // 1. Buscar si ya existe la nota
            var existingGrade = await _context.StudentGrades
            .FirstOrDefaultAsync(g => g.EnrollmentId == input.EnrollmentId && 
                                    g.AssignmentId == input.AssignmentId);

            if (existingGrade != null)
            {
                // A. ACTUALIZAR (UPDATE)
                existingGrade.Score = input.Score;
                existingGrade.GradedDate = DateTime.Now;
                _context.Update(existingGrade);
            }
            else
            {
                // B. INSERTAR NUEVA (CREATE)
                // Primero validamos integridad básica (que existan matrícula y tarea)
                // Esto es opcional si confías en el Frontend, pero recomendado por seguridad.
                bool existsEnrollment = await _context.Enrollments.AnyAsync(e => e.EnrollmentId == input.EnrollmentId);
                bool existsAssignment = await _context.Assignments.AnyAsync(a => a.AssignmentId == input.AssignmentId);

                if(!existsEnrollment || !existsAssignment)
                    return Json(new { success = false, message = "Datos de referencia inválidos." });

                var newGrade = new StudentGrade
                {
                    EnrollmentId = input.EnrollmentId,
                    AssignmentId = input.AssignmentId,
                    Score = input.Score,
                    GradedDate = DateTime.Now
                };
                _context.StudentGrades.Add(newGrade);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message ="Calificacion registrado" });
        }
        catch (Exception ex)
        {
            // Loguear error (ex.Message)
            return Json(new { success = false, message = "Error en servidor: " + ex.Message });
        }
    }
    public async Task<IActionResult> ExportToExcel(int id)
    {
        // 1. OBTENCIÓN DE DATOS (Responsabilidad del Controlador: Orquestar DB)
        var course = await _context.Courses
            .Include(c => c.Subject)
            .FirstOrDefaultAsync( c => c.IdCourse == id);
        if (course == null) return NotFound();

        var terms = await _context.AcademicTerms
            .Where(t => t.CourseId == id)
            .Include(t => t.Assignments)
            .OrderBy(t => t.TermId)
            .AsNoTracking()
            .ToListAsync();

        var enrollments = await _context.Enrollments
            .Where(e => e.IdCourse == id)
            .Include(e => e.Student)
            .Include(e => e.Grades)
            .OrderBy(e => e!.Student!.LastName ?? "")
            .AsNoTracking()
            .ToListAsync();

        // 2. GENERACIÓN DEL ARCHIVO (Responsabilidad del Servicio)
        byte[] fileContent = _exportService.GenerateExportReport(course, terms, enrollments);

        // 3. ENTREGA DEL RESULTADO (Responsabilidad del Controlador: HTTP)
        string fileName = $"Notas_{course.IdCourse}_{DateTime.Now:yyyyMMdd}.xlsx";
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
     public async Task<IActionResult> TestExcel(int id)
    {
        // 2. GENERACIÓN DEL ARCHIVO (Responsabilidad del Servicio)
        byte[] fileContent = _exportService.GenerateExportReport(null!, null!, null!);

        // 3. ENTREGA DEL RESULTADO (Responsabilidad del Controlador: HTTP)
        string fileName = $"Notas_{id}_{DateTime.Now:yyyyMMdd}.xlsx";
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

}