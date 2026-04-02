namespace Asistencia.Models;

public class DashboardViewModel
{
    public int? CountCourseActive {get; set;}
    public int? studentCount {get; set;}
    public List<Course> Courses { get; set; } = new List<Course>();
}