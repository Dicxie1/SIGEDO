namespace Asistencia.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class AcademicTerm
{
    [Key]
    public int TermId { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty; // Ej: "I Corte Evaluativo"

    // CONFIGURACIÓN DE PESOS (Deben sumar lógicamente 100 en la UI)
    
    // Cuánto vale este corte del total del curso (Ej: 30%)
    [Range(0, 100)]
    public int WeightOnFinalGrade { get; set; } 

    // Cuánto vale el acumulado DENTRO de este corte (Ej: 60%)
    [Range(0, 100)]
    public int AccumulatedWeight { get; set; }

    // Cuánto vale el examen DENTRO de este corte (Ej: 40%)
    [Range(0, 100)]
    public int ExamWeight { get; set; }

    // RELACIONES
    public int CourseId { get; set; }
    [ForeignKey("CourseId")]
    public Course? Course { get; set; }
    public DateTime EndDate {get; set;}
    public DateTime StartDate {get; set;}

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    // PROPIEDAD COMPUTADA (Opcional, útil para validaciones)
    [NotMapped]
    public bool IsValidConfiguration => (AccumulatedWeight + ExamWeight) == 100;
}