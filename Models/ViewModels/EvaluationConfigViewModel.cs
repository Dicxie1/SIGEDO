namespace Asistencia.Models.ViewModels;
public class EvaluationConfigViewModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public List<AcademicTerm>? Terms { get; set; }
    
    // Calcula el total actual (Ej: 30+30+40 = 100)
    public int TotalWeightAllocated => Terms!.Sum(t => t.WeightOnFinalGrade) ;
}

