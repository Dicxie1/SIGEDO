namespace Asistencia.Models.Dtos;

public class SyllabusDto
{
    public int SyllabusId { get; set; } // 0 si es nuevo, >0 si existe
    public int CourseId { get; set; }
    public DateTime Date { get; set; }
    public string? Objectives { get; set; }
    public string? Content { get; set; }
    public string? Strategies { get; set; }
    public string? Resources { get; set; }
    public string? Evaluations { get; set; }
    public string? Bibliography { get; set; }
}