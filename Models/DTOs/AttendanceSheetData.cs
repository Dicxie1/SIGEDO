namespace Asistencia.Models.DTOs
{
    public class AttendanceSheetData
    {
        public string CourseName { get; set; }
        public decimal CourseHour { get; set; }
        public string LogoPath { get; set; }
        public List<DateOnly> SessionDates { get; set; } = new List<DateOnly>();
        public int Hour { get; set; }
        public List<StudentAttendanceRow> Students { get; set; } = new List<StudentAttendanceRow>();
    }
    public class StudentAttendanceRow
    {
        public string Carnet { get; set; }
        public string FullName { get; set; }
        // Un diccionario para buscar rápido: Fecha -> Estado ("P" o "A")
        public Dictionary<DateOnly, string> Attendances { get; set; } = new Dictionary<DateOnly, string>();
        public decimal TotalAbsences { get; set; }
        public decimal AbsencePercentage { get; set; }
    }
}
