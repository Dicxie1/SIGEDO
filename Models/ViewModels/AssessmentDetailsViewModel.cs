
namespace Asistencia.Models.ViewModels;
public class AssessmentDetailsViewModel
{
    // Metadatos de la Tarea
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int TermId { get; set; }
    public string TermName { get; set; } = string.Empty;
    // -- Assigment Details
    public int AssignmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public double MaxPoints { get; set; }
    public bool IsExam { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate {get; set;}


    // Estadísticas rápidas
    public double AverageScore { get; set; }
    public int GradedCount { get; set; }
    public int TotalStudents { get; set; }
    public int PendingCount {get; set;}
    public double HighestScore {get; set;}
    public int FailedCount {get; set;}

    // Lista de Estudiantes
    public List<StudentGradeItemDto> Students { get; set; }
}

public class StudentGradeItemDto
{
    public int EnrollmentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public double? Score { get; set; } // Puede ser null si no ha calificado
}