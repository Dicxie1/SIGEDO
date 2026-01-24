namespace Asistencia.Models.ViewModels;
public class AssignmentListViewModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; }
    public List<TermItemListDto> Terms { get; set; }
}

public class TermItemListDto
{
    public int TermId { get; set; }
    public string Name { get; set; } // Ej: "I Parcial"
    public int Weight { get; set; }  // Ej: 30
    public bool IsActive { get; set; } // Para abrir el acordeón por defecto
    public List<AssignmentItemDto> Assignments { get; set; }
}

public class AssignmentItemDto
{
    public int AssignmentId { get; set; }
    public string Title { get; set; }
    public bool IsExam { get; set; }
    public double MaxPoints { get; set; }
    public DateTime? DueDate { get; set; }
    
    // Estadísticas
    public int TotalStudents { get; set; }
    public int GradedCount { get; set; } // Cuántos tienen nota
    
    // Cálculos para la vista (Porcentajes)
    public int ProgressPercent => TotalStudents == 0 ? 0 : (int)((double)GradedCount / TotalStudents * 100);
}