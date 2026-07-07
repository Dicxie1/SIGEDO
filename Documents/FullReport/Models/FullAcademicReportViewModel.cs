using Asistencia.Models.ViewModels;
using Asistencia.Documents.Attendance.Models;
namespace Asistencia.Documents.FullReport.Models
{
    public class FullAcademicReportViewModel
    {
        public Dictionary<int, ProgrammaticProgressViewModel?>? ProgrammaticProgress { get; set; }
        public AttendanceReportModel? Attendance { get; set; }
        public List<AttentionRecordRowViewModel>? AttentionRecord { get; set;  }
        public GradebookViewModel GradeBook { get; set; }

    }
    public class AttentionRecordRowViewModel
    {
        public string DateStr { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Observation { get; set; } = string.Empty;
        public List<string> StudentNames { get; set; } = new();
    }
}
