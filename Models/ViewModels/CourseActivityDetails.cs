
namespace Asistencia.Models.ViewModels;
public class CourseActivityDetailsViewModel
{
    public int CourseId {get; set;} = 0;
    public string CourseName {get; set;} = string.Empty;
    public int ActivityID {get; set;} =0;
    public string ActivityType {get; set;} = string.Empty;
    public string ActivityName {get; set;} = string.Empty;
    public DateTime ActivityDate {get; set;}
    public int MaxScore {get; set;} = 0;
    public double StudentScore {get; set;}
}