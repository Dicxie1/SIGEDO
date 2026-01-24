using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asistencia.Models;
/// <summary>
///     Esta clase representa la Tabla de asisntencia se encarga de registrar las asistencias de los 
///     estudiantes en un determinado curso
/// </summary>
public class Attendance
{
    [Key]
    public int AttendanceId {get; set;}
    [Required]
    public DateOnly Date {get; set;}
    [Required]
    public int IdCourse {get; set;}
    public Course? Course {get; set;} 
    [StringLength(200)]
    public string Topic {get; set;} = string.Empty;
    [Required]
    public decimal TotalHours {get; set;}

    public ICollection<AttendanceDetail> AttendanceDetails {get; set;}  = new List<AttendanceDetail>();
}