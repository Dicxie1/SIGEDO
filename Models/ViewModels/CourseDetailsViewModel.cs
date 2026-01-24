
using System.Drawing;

namespace Asistencia.Models.ViewModels;

public class CourseDetailsViewModel
{
    // Informacion del curso 
    public int CourseId {get; set;} = 0;
    public string CareerName {get; set;} = string.Empty;
    public string CourseName {get; set;} = string.Empty;
    public string CourseCode {get; set;} = string.Empty;
    public int CreditCourse {get; set;}
    public int AcademicYear {get; set;}
    public string? ClassroomName {get; set;}
    public string TeacherName {get; set;} = string.Empty;
    public int MaleCount {get; set;}
    public int FemaleCount{get; set;}
    // Lista de estudiantes inscritos
    public List<EnrolledStudentDto> EnrolledStudents {get; set;} = [];
    public List<AttendanceHistoryDto> attendanceHistories {get; set;} = [];
    public AttendaceStatsViewBox Stats {get; set;} = new AttendaceStatsViewBox();
    public List<EthnicityStatDto> EthnicityStat {get; set;} = new List<EthnicityStatDto>();
    public GradebookViewModel? Gradebook {get; set;} 
    public AssignmentListViewModel AssignmentLists {get; set;} = new AssignmentListViewModel();
    public List<SyllabusItem> SyllabusItems {get; set;} = new List<SyllabusItem>();

    // Cantidad de estudiantes inscritos
    public int TotalEnrolledStudents {get; set;} = 0;
    //Capacidad maxima del curso
    public int MaxCapacity {get; set;} = 0;
    // Año Lectivo
    public int Year {get; set;}
    public string Shift {get; set;} = string.Empty;
    public int TotalCourseHours {get; set;}
    public decimal CompletedHours {get; set;}

}

public class EnrolledStudentDto
{
    public int EnrollmentId {get; set;}
    public string StudentId {get; set;} = string.Empty;
    public string StudentInitials {get; set;} = string.Empty;
    public string FullName {get; set;} = string.Empty;
    public string Email{get; set;} = string.Empty;
    public int Cellphone {get; set;} = 0;
    public Sex Sexo {get; set;} = Sex.Femenino;
    public EthnicGroup Ethnic {get; set;} = EthnicGroup.Mestizo;
    public EnrollmentStatus Status {get; set;}
}
public class AttendanceHistoryDto
{
    public int Id {get; set;}
    public int AttendancesNumber {get; set;}
    public DateOnly Date {get; set;}
    public string Topic {get; set;} = string.Empty;
    public int TotalStudents {get; set;}
    public int PresentCount {get; set;}
    public int LateCount {get; set;}
    public int AbsentCount {get; set;}
    public int JustifiedCount {get; set;}
    public int AttentancePercetage => TotalStudents == 0 ? 0: 
        (int) Math.Round((double) (PresentCount + LateCount ) / TotalStudents) * 100;
}
public class AttendanceRegisterDto
{
    public int IdCourse {get; set;}
    public DateOnly Date {get; set;}
    public string Topic {get; set;} = string.Empty;
    public TimeSpan Hours {get; set;}
    public List<AttendenceDetailsDto> AttendanceDetail {get; set;} =  new List<AttendenceDetailsDto>();
}
public class AttendenceDetailsDto{
    public int EnrollmentId {get; set;}
    public string StudentInitials {get; set;} = string.Empty;
    public string Status {get; set;} = string.Empty;
    public int HoursAttended {get; set;}
    public string Observation {get; set;}  =string.Empty;
}

public class AttendanceUpdateDto
{
    public int IdCourse {get; set;}
    public int AttendanceId{get; set;}
    public DateOnly Date {get; set;}
    public string Topic {get; set;} = string.Empty;
    public decimal Hours {get; set;}
    public List<AttendenceUpdateDetailsDto> AttendanceDetail {get; set;} =  new List<AttendenceUpdateDetailsDto>();
}

public class AttendenceUpdateDetailsDto{
    public int EnrollmentId {get; set;} 
    public string StudentId {get; set;} = string.Empty;
    public string StudentFullName {get; set;} = string.Empty;
    public string StudentInitials {get; set;} = string.Empty;
    public string Status {get; set;} = string.Empty;
    public decimal HoursAttended {get; set;}
    public string Observation {get; set;}  =string.Empty;
}

public class AttendaceStatsViewBox
{
    public List<string> AttendaceDates {get; set;} = new List<string>();
    public List<int> PresentPerSession{get; set;} = new List<int>();
    public List<int> AbsentsPerSession {get; set;} = new List<int>();

    public int TotalPresents {get; set;}
    public int TotalAbsents {get; set;}
    public int TotalLates{get; set;}
    public int TotalJustified {get; set;}
}

public class AttendanceViewDto{
    public int AttendanceId{get; set;}
    public int CourseId {get; set;}
    public string CourseName{get; set;}  = string.Empty;
    public DateOnly Date {get; set;}
    public string Topic {get; set;} = string.Empty;
    public decimal TotalHours {get; set;}
    public int AttendancePercentage{get; set;}
    public AttendanceSummaryDto Summary {get; set;} = new AttendanceSummaryDto();
    public List<AttendanceViewDetailDto> AttendanceDetail {get; set;} = new List<AttendanceViewDetailDto>();
}
public class AttendanceSummaryDto
{
    public int TotalStudents { get; set; }
    public int PresentCount { get; set; }
    public int LateCount { get; set; }
    public int AbsentCount { get; set; }
    public int JustifiedCount { get; set; }
}
public class AttendanceViewDetailDto
{
    public string StudentName { get; set; } = string.Empty;
    public int EnrollmentCode { get; set; }
    public string StudentCode {get; set;} = string.Empty;
    public string StudentInitials { get; set; } = string.Empty; // Ej: "JP"
    public string Status { get; set; } = string.Empty; // P, A, T, J
    public decimal HoursAttended { get; set; }
    public string Observation { get; set; } = string.Empty;
}