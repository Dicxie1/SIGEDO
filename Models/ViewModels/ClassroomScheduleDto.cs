
namespace Asistencia.Models.ViewModels;

public class ClassroomScheduleDto
{
    public string? ClassroomId {get; set;}
    public string? ClassroomName {get; set;}
    public List<ClassSession>? Sessions {get; set;}
}

public class ClassSession
{
    public int DayOfWeek {get; set;}
    public TimeSpan StartTime {set; get;}
    public TimeSpan EndTime {get; set;}
    public string? CourseName {get;set;}
    public string? ColorHex {get;set;}
}