using Asistencia.Data;
using Asistencia.Documents.Attendance.Models;
using Asistencia.Documents.FullReport.Models;
using Asistencia.Models;
using Asistencia.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Asistencia.Services;

public class ReportTermService
{
    private readonly ApplicationDbContext _context;
    private readonly AttendanceService _attendanceService;

    public ReportTermService(ApplicationDbContext context, AttendanceService attendanceService )
    {
        _context = context;
        _attendanceService = attendanceService;
    }

    public async Task<ProgrammaticProgressViewModel> GetTermProgressAsync(int courseId, int termId)
    {
        // =========================================================================
        // PASO 1: OBTENCIÓN DE DATOS
        // =========================================================================
        
        // A. Obtener el Corte (Necesitamos Fechas y Tareas)
        var term = await _context.AcademicTerms
            .Include(t => t.Course)
                .ThenInclude(c => c.Subject)
                .ThenInclude(s => s.Career)
            .Include(t => t.Assignments).AsNoTracking()
            .FirstOrDefaultAsync(t => t.TermId == termId);

        if (term == null) return null;

        // B. Obtener TODAS las matrículas históricas del curso
        var allEnrollments = await _context.Enrollments
            .Where(e => e.IdCourse == courseId)
            .Include(e => e.Student)
            .Include(e => e.Grades)
            .ToListAsync();

        // =========================================================================
        // PASO 2: LÓGICA TEMPORAL (TIME TRAVEL)
        // =========================================================================

        // A. MATRÍCULA INICIAL
        // Estudiantes que estaban inscritos en algún momento dentro del rango del corte.
        // Condición: Se matricularon ANTES del fin del corte Y (Siguen activos O se retiraron DESPUÉS del inicio).
        var initialGroup = allEnrollments.Where(e =>
            e.EnrollmentDate.Date <= term.EndDate.Date &&
            (e.WithdrawDate == null || e.WithdrawDate.Value.Date >= term.StartDate.Date)
        ).ToList();

        // B. MATRÍCULA FINAL (Universo Activo)
        // Estudiantes que llegaron "vivos" al cierre del corte.
        // Condición: Siguen activos hoy O se retiraron DESPUÉS de que el corte terminó.
        var finalGroup = initialGroup.Where(e =>
            e.WithdrawDate == null || e.WithdrawDate.Value.Date > term.EndDate.Date
        ).ToList();


        // =========================================================================
        // PASO 3: CLASIFICACIÓN ACADÉMICA
        // =========================================================================
        
        // Identificar IDs de tareas que son EXÁMENES en este corte
        var examAssignmentIds = term.Assignments
            .Where(a => a.IsExam)
            .Select(a => a.AssignmentId)
            .ToList();

        var approvedList = new List<Student>();
        var failedList = new List<Student>();
        var notExaminedList = new List<Student>();

        foreach (var enrollment in finalGroup)
        {
            // A. Detección de "No Examinado"
            // Si el corte tiene exámenes, verificamos si el alumno tiene alguna nota registrada en ellos.
            bool isExamined = true;

            if (examAssignmentIds.Any())
            {
                // ¿Tiene alguna nota en grades cuyo AssignmentId esté en la lista de exámenes?
                bool hasExamGrade = enrollment.Grades
                    .Any(g => examAssignmentIds.Contains(g.AssignmentId));

                if (!hasExamGrade) isExamined = false;
            }

            // B. Clasificación
            if (!isExamined)
            {
                notExaminedList.Add(enrollment.Student);
            }
            else
            {
                // Calcular Sumatoria de Puntos SOLO de este corte
                // Sumamos todas las tareas (Examen + Acumulados) que pertenecen a este term
                double termSum = 0;
                
                foreach (var assignment in term.Assignments)
                {
                    var grade = enrollment.Grades
                        .FirstOrDefault(g => g.AssignmentId == assignment.AssignmentId);
                    
                    if (grade != null) termSum += grade.Score ?? 0;
                }

                // Umbral de Aprobación (60 Puntos)
                if (termSum >= 60)
                    approvedList.Add(enrollment.Student);
                else
                    failedList.Add(enrollment.Student);
            }
        }
        // =========================================================================
        // PASO 4: LLENADO DEL VIEWMODEL
        // =========================================================================

        var model = new ProgrammaticProgressViewModel
        {
            CourseName = term.Course?.Subject?.SubjetName ?? "Curso Desconocido",
            CarrerName = term.Course?.Subject?.Career?.Name ?? "Desconocido",
            semester = term.Course.Semester,
            AcademicYear = GetAcademicYear(term.Course.Subject.Semester),
            TermName = term.Name,
            TermId = termId
        };

        // 1. Matrícula Inicial
        model.Initial.Male = initialGroup.Count(e => e.Student.Sexo == Sex.Masculino);
        model.Initial.Female = initialGroup.Count(e => e.Student.Sexo == Sex.Femenino);
        // El .Total se calcula solo en la clase CountStat

        // 2. Matrícula Final
        model.Final.Male = finalGroup.Count(e => e.Student.Sexo == Sex.Masculino);
        model.Final.Female = finalGroup.Count(e => e.Student.Sexo == Sex.Femenino);

        // 3. No Examinados (Solo Cantidad)
        model.NotExamined.Male = notExaminedList.Count(s => s.Sexo == Sex.Masculino);
        model.NotExamined.Female = notExaminedList.Count(s => s.Sexo == Sex.Femenino);

        // 4. Aprobados (Cantidad y %)
        model.Approved.Male = approvedList.Count(s => s.Sexo == Sex.Masculino);
        model.Approved.Female = approvedList.Count(s => s.Sexo == Sex.Femenino);
        
        model.ApprovedPct.Male = CalculatePercentage(model.Approved.Male, model.Final.Male);
        model.ApprovedPct.Female = CalculatePercentage(model.Approved.Female, model.Final.Female);
        model.ApprovedPct.Total = CalculatePercentage(model.Approved.Total, model.Final.Total);

        // 5. Reprobados (Cantidad y %)
        model.Failed.Male = failedList.Count(s => s.Sexo == Sex.Masculino);
        model.Failed.Female = failedList.Count(s => s.Sexo == Sex.Femenino);
        
        model.FailedPct.Male = CalculatePercentage(model.Failed.Male, model.Final.Male);
        model.FailedPct.Female = CalculatePercentage(model.Failed.Female, model.Final.Female);
        model.FailedPct.Total = CalculatePercentage(model.Failed.Total, model.Final.Total);

        return model;
    }

    // Helper para calcular Hombres/Mujeres/Porcentajes
// =========================================================================
    // HELPER: CÁLCULO DE PORCENTAJES SEGURO
    // =========================================================================
    private double CalculatePercentage(int part, int totalUniverse)
    {
        if (totalUniverse == 0) return 0;
        return Math.Round(((double)part / totalUniverse) * 100, 2);
    }

    private string GetAcademicYear(string semester) => semester switch
    {
        "I" or "II" => "Primer Año",
        "III" or "IV" => "Segundo Año",
        "V" or "VI" => "Tercer Año",
        "VII" or "VIII" => "Cuarto Año",
        "IX" or "X" => "Quinto Año",
        "XI" or "XII" => "Sexto Año",
        _ => ">Desconocido"
    };
    public async Task<FullAcademicReportViewModel> GetCourseAnaliticReport(int courseId) 
    {
        var terms = await _context.AcademicTerms
        .Include(t => t.Assignments)
            .ThenInclude(t => t.Grades)
        .Where(t => t.CourseId == courseId)
        .OrderBy(a => a.TermId)
        .AsNoTracking()
        .ToListAsync();
        var enrollmentGrade = await _context.Enrollments
        .Where(e => e.IdCourse == courseId)
        .Include(e => e.Student)
        .Include(e => e.Grades)
        .OrderBy(e => e.Student!.LastName)
        .ToListAsync();
        var studentRows = enrollmentGrade.Select(e => CalculateStudentRow(e, terms)).ToList();
        var gradeBook = new GradebookViewModel
        {
            CourseId = courseId,
            CourseName = _context.Courses.Where(x => x.IdCourse == courseId).Select(x => x.Subject!.SubjetName).FirstOrDefault() ?? "S/N",
            Terms = terms.Select(t => new TermHeaderDto
            {
                TermId = t.TermId,
                Name = t.Name,
                Weight = t.WeightOnFinalGrade,
                Assignments = t.Assignments.OrderBy(e => e.IsExam).ThenBy(e => e.DueDate).Select(a => new AssignmentHeaderDto
                {
                    AssignmentId = a.AssignmentId,
                    Title = a.Title,
                    MaxPoints = a.MaxPoints,
                    IsExam = a.IsExam
                }).ToList()
            }).ToList(),
            Students = studentRows
        };
        var programmaticProgressMap = new Dictionary<int, ProgrammaticProgressViewModel>();
        foreach (var term in terms)
        {
            var progress = await GetTermProgressAsync(courseId, term.TermId);
            if (progress != null)
            {
                programmaticProgressMap.Add(term.TermId, progress);
            }
        }
        AttendanceReportModel attendanceReport = await _attendanceService.GetReportDataAsync(courseId);
        if (attendanceReport == null) throw new Exception("Asistencia vacio");
        List<AttentionRecordRowViewModel> atenciones = await _context.AttentionRecords
        .Where(ar => ar.CourseId == courseId)
        .Select(ar => new AttentionRecordRowViewModel
        {
            DateStr = ar.Date.ToString("dd/MM/yyyy"),
            Observation = ar.Observation,
            Category = ar.Category.ToString(), // O mapeo amigable a español
            Priority = ar.Priority.ToString(),
            Status = ar.Status.ToString(),
            StudentNames = ar.Participants.Select(p => $"{p.Enrollment!.Student!.Name} {p.Enrollment.Student.LastName}").ToList()
        }).ToListAsync();
        var full = new FullAcademicReportViewModel
        {
            ProgrammaticProgress = programmaticProgressMap!,
            Attendance = attendanceReport,
            AttentionRecord = atenciones,
            GradeBook = gradeBook,
            Syllabus = await _context.SyllabusItems.Where(c => c.CourseId == courseId).AsNoTracking().ToListAsync()
        };
        return full;
    }
    private StudentRowDto CalculateStudentRow(Enrollment enrollment, List<AcademicTerm> terms)
    {
        var gradesDict = enrollment.Grades.ToDictionary(g => g.AssignmentId, g => g.Score ?? 0);
        double grandTotal = 0;
        int activeTermsCount = terms.Count; // Divisor para promedio simple

        foreach (var term in terms)
        {
            // Sumar tareas de este corte
            double termSum = term.Assignments
                .Where(a => gradesDict.ContainsKey(a.AssignmentId))
                .Sum(a => gradesDict[a.AssignmentId]);

            grandTotal += termSum;
        }

        // FÓRMULA: Promedio Simple de Cortes (Sumatoria / Cantidad)
        double finalGrade = activeTermsCount > 0 ? (grandTotal / activeTermsCount) : 0;

        return new StudentRowDto
        {
            EnrollmentId = enrollment.EnrollmentId,
            StudentFullName = $"{enrollment!.Student!.LastName}, {enrollment.Student.Name}",
            StudentId = enrollment.Student.Id,
            StudentInitials = $"{enrollment?.Student?.Name?.FirstOrDefault()}{enrollment?.Student?.LastName?.FirstOrDefault()}",
            Grades = gradesDict,
            FinalGrade = Math.Round(finalGrade, 2) // Redondeo a 2 decimales
        };
    }
}