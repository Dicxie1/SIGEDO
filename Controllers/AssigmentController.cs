namespace  Asistencia.Controllers;
using Microsoft.AspNetCore.Mvc;
using Asistencia.Data;
using Asistencia.Models;
using Asistencia.Models.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;

public class AssigmentController : Controller
{
    readonly ApplicationDbContext _context;
    public AssigmentController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpPost("/Course/{courseid}/EvaluationConfig/AddAssigment")]
    public async Task<JsonResult> AddAssigment([FromBody] Assignment model)
    {
        if(!ModelState.IsValid) return Json(new {success = false, message= "Datos incompletos"});
        try
        {
            var term = await _context.AcademicTerms
            .Include(t => t.Assignments) // ¡Importante el Include!
            .FirstOrDefaultAsync(t => t.TermId == model.TermId);

            if (term == null) return Json(new { success = false, message = "Corte no encontrado." });
            // 2. Calcular cuánto se ha gastado hasta ahora
            // Filtramos por el mismo tipo que estamos intentando guardar (Examen o Acumulado)
            double currentUsedPoints = term.Assignments
                .Where(a => a.IsExam == model.IsExam) 
                .Sum(a => a.MaxPoints);
                // 3. Determinar el límite según el tipo
            double limit = model.IsExam ? term.ExamWeight : term.AccumulatedWeight;
            string typeName = model.IsExam ? "Examen" : "Acumulado";
            // 4. VALIDACIÓN MATEMÁTICA
            double newTotal = currentUsedPoints + model.MaxPoints;
            double remaining = limit - currentUsedPoints;
            if (newTotal > limit)
            {
                return Json(new { 
                    success = false, 
                    message = $"No puedes agregar {model.MaxPoints} pts. El {typeName} tiene un límite de {limit} pts y ya has ocupado {currentUsedPoints}. Solo te quedan {remaining} pts disponibles." 
                });
            }
            // 5. Si pasa la validación, guardar
            var assignment = new Assignment
            {
                TermId = model.TermId,
                Title = model.Title,
                MaxPoints = model.MaxPoints,
                IsExam = model.IsExam,
                DueDate = model.DueDate,
                Description = model.Description
            };
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();
            return Json(new {success= true, message = "Guardado exitosamente"});
        }catch(DbUpdateException ex)
        {
            if(ex.InnerException is Npgsql.NpgsqlException)
            {
                return Json(new {success= false, message= $"{ex.Data}"});
            }
            else
            {
                return Json(new {success = false, message = "Error al guardar"}); 
            }
        }
    }
    [HttpPost("/Course/{courseid}/Assignment/{assignmentid}")]
    public async Task<JsonResult> DeleteAssignment(int courseid, int assignmentid)
    {
        try
        {
            var assignment = await _context.Assignments
            .Include(a => a.Grades) // Incluimos las notas para verificar
            .Include(c => c.AcademicTerm)
                .ThenInclude( c => c.Course)
            .FirstOrDefaultAsync(a => a.AssignmentId == assignmentid);
        if (assignment == null)
        {
            return Json(new { success = false, message = $"La actividad no existe o ya fue eliminada.{assignmentid}" });
        }
        // 2. VALIDACIÓN DE INTEGRIDAD: ¿Tiene notas registradas?
        // Si ya hay alumnos calificados, NO debemos borrarla directamente
        if (assignment.Grades.Any())
        {
            return Json(new { 
                success = false, 
                message = $"No se puede eliminar '{assignment.Title}' porque ya existen {assignment.Grades.Count} calificaciones registradas. Debes eliminar las notas primero." 
            });
        }
        // 3. Eliminar
        _context.Assignments.Remove(assignment);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message ="eliminado correctamente" });
        }catch (Exception ex)
        {
            // Loguear el error real (ex.Message) en tu sistema de logs
            return Json(new { success = false, message = "Ocurrió un error en el servidor al intentar eliminar." });
        }

    }
    [HttpGet("/Course/{courseid}/Assigment/Grade/{id}")]
public async Task<IActionResult> AssessmentDetails(int id) // id = AssignmentId
{
    // 1. Cargar la Asignación y Curso
    var assignment = await _context.Assignments
        .Include(a => a.AcademicTerm)
        .ThenInclude(t => t.Course)
        .ThenInclude(s => s.Subject)
        .FirstOrDefaultAsync(a => a.AssignmentId == id);

    if (assignment == null) return NotFound();

    // 2. Cargar Estudiantes y Notas de ESTA tarea
    var enrollments = await _context.Enrollments
        .Where(e => e.IdCourse == assignment.AcademicTerm!.CourseId)
        .Include(e => e.Student)
        .Include(e => e.Grades.Where(g => g.AssignmentId == id)) // Filtramos solo esta nota
        .OrderBy(e => e!.Student!.LastName)
        .AsNoTracking()
        .ToListAsync();
        

    // 3. Construir ViewModel
    var gradedStudents = enrollments.Where(e => e.Grades.Any(g => g.AssignmentId == id)).ToList();
    var gradesValues = gradedStudents.Select(e => e.Grades.First(g => g.AssignmentId == id).Score).ToList();

    // Cálculo de estadísticas seguras (evitar división por cero)
    double avg = gradesValues.Any() ? gradesValues.Average() : 0;
    double max = gradesValues.Any() ? gradesValues.Max() : 0;

    // Definimos "En Riesgo" como nota menor al 60% del valor máximo de la tarea
    int failed = gradesValues.Count(score => score < (assignment.MaxPoints * 0.60));

    
    var model = new AssessmentDetailsViewModel
    {
        CourseId = assignment.AcademicTerm.CourseId,
        CourseName = assignment.AcademicTerm.Course.Subject.SubjetName,
        TermId = assignment.TermId,
        TermName = assignment.AcademicTerm.Name,
        AssignmentId = assignment.AssignmentId,
        Title = assignment.Title,
        MaxPoints = assignment.MaxPoints,
        IsExam = assignment.IsExam,
        Description = assignment.Description,
        
        TotalStudents = enrollments.Count,
        GradedCount = gradedStudents.Count,
        PendingCount = enrollments.Count - gradedStudents.Count,
        AverageScore = avg,
        HighestScore = max,
        FailedCount = failed,

        Students = enrollments.Select(e => new StudentGradeItemDto
        {
            EnrollmentId = e.EnrollmentId,
            StudentName = $"{e.Student.LastName}, {e.Student.Name}",
            StudentCode = e.Student.Id,
            Score = e.Grades.FirstOrDefault()?.Score // Puede ser null
        }).ToList()
    };
    return View("AssessmentDetails",model);
}
}
