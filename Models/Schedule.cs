using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Asistencia.Models;
public class Schedule
{
    [Key]
    public int ScheduleId {get;set;}
    [Required]
    public string ClassroomId {get; set;} = string.Empty;
    public Classroom? Classroom {get; set;}
    [Required]
    public int IdCourse {get; set;}
    public Course? Course {get; set;}
    [Required]
    [Range(1,7, ErrorMessage ="El día debe de ser entre 1(Lunes) y 7(Domingo)")]
    public int DayOfWeek{get; set;}
    [Required]
    public TimeSpan StartTime {get; set;}
    [Required]
    public TimeSpan EndTime {get; set;}
    public int AcademicPeriodId { get; set; }
    public int? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    public AcademicPeriod AcademicPeriod { get; set; }
    [NotMapped]
    public string TimeRange => $"{StartTime:hh\\mm} - {EndTime:hh\\mm}";
}
