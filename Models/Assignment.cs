namespace Asistencia.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class Assignment
{
    [Key]
    public int AssignmentId { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty; // Ej: "Mapa Conceptual"

    [StringLength(250)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    // PUNTAJE
    [Range(0, 500)]
    public double MaxPoints { get; set; } // Nota máxima posible (Ej: 10 pts)

    // EL DISCRIMINADOR CLAVE
    // true = Es la nota del Examen (usa ExamWeight)
    // false = Es tarea del Acumulado (usa AccumulatedWeight)
    public bool IsExam { get; set; } 

    // RELACIONES
    public int TermId { get; set; }
    [ForeignKey("TermId")]
    public AcademicTerm? AcademicTerm { get; set; }

    // Las notas que los estudiantes sacaron en esta tarea
    public ICollection<StudentGrade> Grades { get; set; } = new List<StudentGrade>();
}
