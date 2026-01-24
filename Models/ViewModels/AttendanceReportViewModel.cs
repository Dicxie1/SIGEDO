using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Asistencia.Models.ViewModels;

public class AttendanceReportViewModel
{
    public List<SessionHeaderDto> Sessions {get; set;} = new List<SessionHeaderDto>();
    public List<StudentAttendanceRowDto> studentAttendanceRowDtos {get; set;} = new List<StudentAttendanceRowDto>();
}

public class SessionHeaderDto
{
    public int AttedanceId {get; set;}
    [DisplayFormat(DataFormatString ="{0:dd/MM}")]
    public DateTime Date {get; set;}
}

public class StudentAttendanceRowDto
{
    public string? StudentId {get; set;}
    public string? FullName {get; set;}
    public Dictionary<int, bool> AttendanceStatus {get; set;} = new Dictionary<int, bool>();
    public int TotalPresent => AttendanceStatus.Count(a => a.Value == true);
    public int TotalSeccions => AttendanceStatus.Count();

    public double AttendacePercentage => TotalSeccions == 0 ? 0 : (double) TotalPresent / TotalSeccions * 100;
}