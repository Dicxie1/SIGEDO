using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
namespace Asistencia.Models;

public class Course
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdCourse {get; set;}
    [Required]
    public string SubjectId {get; set;} = string.Empty;

    public Subject? Subject {get; set;} 
    [Required]
    public string ClassroomId {get; set;} = string.Empty;
    public Classroom? Classroom {get; set;}
    [Required]
    public int Year{get; set;} 
    [Required]
    public string Semester {get; set;} = string.Empty;
    [Required]
    public int Capacity {get; set;}
    [Required]
    public string Shift {get; set;} = string.Empty;
    [Required]
    public int Credits {get; set;}
    [Required]
    public int HoursPerWeek {get; set;}
    [Required]
    public int TotalHours {get; set;}
    public DateTime StartDate {get; set;}
    public DateTime EndDate {get; set;}    
    public bool isActive {get; set;}
    [StringLength(20)]
    public string ColorTheme { get; set; } = "primary";
    
    public ICollection<Attendance> Attendances {get; set;} = new List<Attendance>();
    public ICollection<Enrollment> Enrollments {get; set;} = new List<Enrollment>();
    public ICollection<Schedule> Schedules {get; set;} = new List<Schedule>();
    public ICollection<AcademicTerm> AcademicTerms {get; set;} = new List<AcademicTerm>();
    public virtual ICollection<AttentionRecord> AttentionRecords {get; set;} = new List<AttentionRecord>(); 
    
}