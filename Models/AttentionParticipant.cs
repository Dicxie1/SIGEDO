using System.ComponentModel.DataAnnotations;
namespace Asistencia.Models;
public class AttentionParticipant
{
    [Key]
    public int Id { get; set; }

    // 1. Enlace al Reporte Padre
    public int AttentionRecordId { get; set; }
    public virtual AttentionRecord? AttentionRecord { get; set; }

    // 2. ENLACE A LA MATRÍCULA (Lo que solicitaste)
    // Esto conecta específicamente a ESTE alumno en ESTE curso
    public int EnrollmentId { get; set; }
    public virtual Enrollment? Enrollment { get; set; }
}