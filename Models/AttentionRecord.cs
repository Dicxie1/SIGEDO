using System.ComponentModel.DataAnnotations;
namespace Asistencia.Models;
public class AttentionRecord
{
    [Key]
    public int AttentionRecordId { get; set; }

    // Contexto: ¿En qué curso sucedió?
    public int CourseId { get; set; }
    public virtual Course? Course { get; set; }

    // Auditoría
    public DateTime Date { get; set; } = DateTime.Now;
    public string CreatedByUserId { get; set; }  = string.Empty; // El docente

    // Datos del Reporte
    public AttentionCategory Category { get; set; } // Académico, Conducta...
    public AttentionPriority Priority { get; set; } // Baja, Media, Alta
    
    public AttentionStatus Status {get; set;}
    [Required]
    public string Observation { get; set; }

    // RELACIÓN: Un reporte tiene uno o muchos participantes (matriculados)
    public virtual ICollection<AttentionParticipant> Participants { get; set; }
}

public enum AttentionCategory
{
    Academic = 1,       // Académico
    Behavioral = 2,     // Conducta
    Attendance = 3,     // Asistencia/Tardanzas
    EmotionalHealth = 4 // Emocional/Salud
}

public enum AttentionPriority
{
    Low = 1,    // Baja (Informativo)
    Medium = 2, // Media (Seguimiento)
    High = 3    // Alta (Urgente)
}

public enum AttentionStatus
{
    Pending = 1, // Pendiente / Abierto
    Resolved = 2 // Resuelto / Cerrado
}