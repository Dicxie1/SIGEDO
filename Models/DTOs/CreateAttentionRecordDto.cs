namespace Asistencia.Models.DTOs;

public class CreateAttentionRecordDto
{
    public int CourseId { get; set; }
    public List<int> EnrollmentIds { get; set; } // Lista de IDs para soportar Grupal
    public AttentionCategory Category { get; set; }
    public AttentionPriority Priority { get; set; }
    public string Observation { get; set; } = string.Empty;
}