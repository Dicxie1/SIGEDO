using System;
using System.Collections.Generic;
namespace Asistencia.Documents.Attendance.Models
{
    public class AttendanceReportModel
    {
        public string UniversityName { get; set; } = "URACCAN";
        public string Campus { get; set; }
        public int CourseId{ get; set; } 
        public string CourseName { get; set; }
        public string ProfessorName { get; set; }
        public string Term { get; set; } // Semestre
        public List<DateOnly> Dates { get; set; }
        public List<StudentAttendance> Students { get; set; }
    }
    public class StudentAttendance
    {
        public string StudentName { get; set; } = string.Empty;
        public string StudentId { get; set; } // Carnet
        // Diccionario: Fecha -> Estado (P=Presente, A=Ausente, J=Justificado, T=Tardanza)
        public Dictionary<DateOnly, string> AttendanceLog { get; set; } = new Dictionary<DateOnly, string>();
        public double AttendancePercentage { get; set; }
    }
}
