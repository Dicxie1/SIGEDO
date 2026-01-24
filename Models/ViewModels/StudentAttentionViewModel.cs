using Asistencia.Models;

public class StudentAttentionViewModel
{
    // Contexto
    public int CourseId { get; set; }
    public string CourseName { get; set; }
    
    // Estadísticas (Header)
    public int TotalRecords { get; set; }      // Total registros
    public int AcademicCases { get; set; }     // Casos Académicos
    public int BehavioralCases { get; set; }   // Casos Conductuales
    public int PendingFollowUps { get; set; }  // Pendientes de seguimiento

    // Listas para el Formulario
    public List<StudentSimpleDto> Students { get; set; }
    public List<AttentionCategoryDto> Categories { get; set; } // Ej: Conducta, Académico, Asistencia
    
    // Historial Reciente
    public List<AttentionRecordDto> RecentHistory { get; set; }
}

public class StudentSimpleDto { public int Id { get; set; } public string Name { get; set; } }
public class AttentionCategoryDto { public int Id { get; set; } public string Name { get; set; } }
public class AttentionRecordDto 
{
    public int Id { get; set; }
    public string StudentName { get; set; } // O "Grupo (5 alumnos)"
    public string Category { get; set; }
    public DateTime Date { get; set; }
    public string Observation { get; set; }
    public string PriorityColor { get; set; } // "danger", "warning", "success"
    public AttentionStatus Status {get; set;}
}