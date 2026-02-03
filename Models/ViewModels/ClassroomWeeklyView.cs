namespace Asistencia.Models.ViewModels
{
    public class ClassroomWeeklyView
    {
        public string ClassroomName { get; set; } = string.Empty;
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public List<ClassroomCalendarEventViewModel> Events { get; set; } = new List<ClassroomCalendarEventViewModel>();
    }
}
