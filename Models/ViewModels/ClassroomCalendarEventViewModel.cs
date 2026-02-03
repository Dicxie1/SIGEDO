namespace Asistencia.Models.ViewModels
{
    public class ClassroomCalendarEventViewModel
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string ColorHex { get; set; }

        public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;
        public int StartMinutesOffset => (int)StartTime.TotalMinutes;
    }
}
