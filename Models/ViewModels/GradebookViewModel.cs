public class GradebookViewModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    
    // Encabezados (La estructura de cortes y tareas)
    public List<TermHeaderDto>? Terms { get; set; }
    
    // Filas (Los estudiantes y sus notas ya mapeadas)
    public List<StudentRowDto>? Students { get; set; }
}

public class TermHeaderDto
{
    public int TermId { get; set; }
    public string Name { get; set; }= string.Empty;
    public int Weight { get; set; } // Peso del corte (ej: 30%)
    public List<AssignmentHeaderDto>? Assignments { get; set; }
}

public class AssignmentHeaderDto
{
    public int AssignmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public double MaxPoints { get; set; }
    public bool IsExam { get; set; }
}

public class StudentRowDto
{
    public string StudentInitials {get; set;} = string.Empty;
    public int EnrollmentId { get; set; } // ID de matrícula
    public string StudentFullName { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    
    // Diccionario clave: AssignmentId, Valor: Nota
    public Dictionary<int, double>? Grades { get; set; } 
    
    // Totales calculados (para mostrar carga inicial)
    public double FinalGrade { get; set; }
}