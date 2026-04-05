using Asistencia.Models.ViewModels;

namespace Asistencia.Models;

public class DashboardViewModel
{
    public int PendingTaskCount { get; set; }
    public double AverageAttendancePercentage { get; set; }
    public int? CountCourseActive {get; set;}
    public int? studentCount {get; set;}
    public int TotalStudenActive { get; set; }
    public int StudentAtRiskAttendance { get; set; }
    public CurrentClassViewModel CurrentClass { get; set; } = new CurrentClassViewModel();
    public List<PedingAssigmentItem> PedingAssigmentItems { get; set; } = new List<PedingAssigmentItem>();
    public List<CourseListItemViewModel> CourseActive { get; set; } = new();
    public List<ScheduleItemViewModel> TodaySchedule { get; set; } = new();
    public List<WeeklyAgendaViewModel> WeeklyAgenda { get; set; } = new();
}

public class ScheduleItemViewModel
{
    public string SubjectName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; } // Ej: 10:00 AM
    public string Location { get; set; } = string.Empty; // Ej: "Laboratorio 1"
    public string BorderColorClass { get; set; } = "border-primary"; // Para mantener el diseño visual (primary, info, etc)
}

public class PedingAssigmentItem
{
    public int CourseId { get; set; }
    public int AssignmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int? Pending { get; set; }
}

public enum AgendaItemType { Class, Meeting, ProfesionalDevelopment,Other}

public class WeeklyAgendaViewModel
{
    public DayOfWeek Day { get; set; }
    public string DayName => TranslateDayName(Day);
    public List<AgendanItemDetailViewModel> Activities { get; set; } = new List<AgendanItemDetailViewModel>();

    private string TranslateDayName(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => "Lunes",
        DayOfWeek.Tuesday => "Martes",
        DayOfWeek.Wednesday => "Miércoles",
        DayOfWeek.Thursday => "Jueves",
        DayOfWeek.Friday => "Viernes",
        DayOfWeek.Saturday => "Sábado",
        DayOfWeek.Sunday => "Domingo",
        _ => day.ToString()
    };
}

public class AgendanItemDetailViewModel
{
    public string Title { get; set; } = string.Empty;
    public TimeSpan Hour { get; set; }
    public string Location { get; set; } = string.Empty;
    public AgendaItemType Type { get; set; }
    public string IconClass => Type switch
        {
            AgendaItemType.Class => "fa-chalkboard-teacher",
            AgendaItemType.Meeting => "fa-users",
            AgendaItemType.ProfesionalDevelopment => "fa-graduation-cap",
            _ => "fa-calendar-alt"
        };
}
public class CurrentClassViewModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int StudentCount { get; set; }
}