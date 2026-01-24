using System.ComponentModel.DataAnnotations;

namespace Asistencia.Models;
public class Career
{
    // Clave primaria (Primary Key - PK)
    [Key]
    
    public int CareerId { get; set; }
    // Nombre completo de la carrera (ej: "Ingeniería en Sistemas")
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    // Código abreviado (ej: "IS", "DER") - Se mapeará como UNIQUE
    public string Code { get; set; } = string.Empty;
    // Propiedad de navegación para la relación 1:N con Asignaturas (Subjects)
    public ICollection<Subject>? Subjects { get; set; } = new List<Subject>();
}