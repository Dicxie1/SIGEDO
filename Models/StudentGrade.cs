namespace Asistencia.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class StudentGrade
{
    [Key]
    public int GradeId { get; set; }

    [Range(0, 500)]
    public double Score { get; set; } // Lo que sacó el alumno (Ej: 8.5)

    [StringLength(200)]
    public string? Feedback { get; set; } // Comentario del docente

    public DateTime GradedDate { get; set; } = DateTime.Now;

    // RELACIONES
    public int AssignmentId { get; set; }
    [ForeignKey("AssignmentId")]
    public Assignment? Assignment { get; set; }

    public int EnrollmentId { get; set; }
    [ForeignKey("EnrollmentId")]
    public Enrollment? Enrollment { get; set; }
}