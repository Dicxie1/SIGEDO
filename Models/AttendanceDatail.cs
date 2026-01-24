using System.ComponentModel.DataAnnotations;

namespace Asistencia.Models;

public class AttendanceDetail
{
    [Key]
    public int AttendaceDetailsId {get; set;}
    [Required]
    public int AttendanceId {get; set;}
    public Attendance? Attendance {get; set;} 
    [Required]
    public int EnrollmentId {get; set;}

    public Enrollment? Enrollment {get; set;}
    public decimal HoursAttended {get; set;} = 0.0M; 
    [Required]
    [StringLength(1)]
    public string Status {get; set;} = string.Empty;
    [StringLength(255)]
    public string Observation {get; set;} = string.Empty;
}
