using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asistencia.Models;
/// <summary>
/// Clase que almacena las matricula de los estudiantes 
/// </summary>
public class Enrollment
{
    /// <summary>
    /// Propiead que representa la llave primarias de la tabla
    /// </summary>
    [Key]
    public int EnrollmentId {get; set;}
    /// <summary>
    /// Propiedad que representa  el codigo de curso 
    /// </summary>
    [Required]
    public int IdCourse {get; set;}
    public Course? Course {get; set;} 
    /// <summary>
    /// Propiedad que representa el numero de Carnet del estudiante 
    /// </summary>
    [Required]
    public string StudentId {get; set;} = string.Empty;
    public Student? Student {get; set;} 
    /// <summary>
    /// Datos que describe la fecha que se matriculo en estudiante al curso
    /// </summary>
    [Required]
    public DateTime EnrollmentDate {get; set;} = DateTime.Now;
    /// <summary>
    /// Representa el estado de la matricula: Activa, Retirada, Aprobada, cancelado
    /// </summary>
    public EnrollmentStatus Status {get; set;} = EnrollmentStatus.Active;
    /// <summary>
    /// Representa la fecha en la que el estudiante se retino del curso 
    /// </summary>
    public DateTime? WithdrawDate {get; set;}
    public ICollection<AttendanceDetail> AttendanceDetails {get; set;} = new List<AttendanceDetail>();
    
    public ICollection<AttentionParticipant> AttentionParticipants {get; set;} = new List<AttentionParticipant>();
    // Relación inversa: Una matrícula tiene muchas notas
    public ICollection<StudentGrade> Grades { get; set; } = new List<StudentGrade>();
}
/// <summary>
/// Enum que representa el estado de la matriculo de un estudiante 
/// </summary>
public enum EnrollmentStatus
{
    /// <summary>
    /// Esta valor representa que la matricula esta activa
    /// </summary>
    Active = 1, // Activo
    /// <summary>
    /// representa que se ha retirado 
    /// </summary>
    Withdrawn = 2, // Retirado
    Abandoned = 3, //Abandono
    Cancelled = 4, //cancelado
    Completed = 5 // Completado
}