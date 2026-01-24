using System.ComponentModel.DataAnnotations;

namespace Asistencia.Models;

public class SyllabusItem
{
    [Key]
    public int SyllabusId { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
        // Datos del Syllabus
     public DateTime Date { get; set; }
    public string? Objectives { get; set; }
    public string? Content { get; set; }     // Unidad y Contenido
    public string? Strategies { get; set; }
    public string? Resources { get; set; }
    public string? Evaluations { get; set; }
     public string? Bibliography { get; set; }
        // Ordenamiento (opcional, para mantener el orden visual)
    public int OrderIndex { get; set; }
}