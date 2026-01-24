namespace Asistencia.Models.DTOs;

public class CourseRegistrationDto
{
    // Datos Básicos
    /// <summary>
    /// Codigo de asignatura 
    /// </summary>
    public string SubjectId {get;set;} = string.Empty;
    public string Semester {get; set;} = string.Empty;
    public int Year{get; set;} 
    public string Shift {get; set;} = string.Empty;
    public int Credits {get; set;}
    public int TotalHours {get; set;} 
    public int HoursPerWeek {get; set;}
    /// <summary>
    /// Codigo de Aula
    /// </summary>
    public string ClassroomId {get; set;} = string.Empty;
    /// <summary>
    /// Cantidad de estudiantes del curso
    /// </summary>
    public int Capacity {get; set;}
    /// <summary>
    /// Fecha de inicio de curso
    /// </summary>
    public DateTime StartDate {get; set;}
    /// <summary>
    /// Fecha final de curso
    /// </summary>
    public DateTime EndDate {get; set;}
    /// <summary>
    /// Estado de curso activo o desactivado
    /// </summary>
    public bool IsActive {get; set;}
    public List<ScheduleDto> Schedules {get; set;} = new List<ScheduleDto>();
}

public class ScheduleDto
{
    public int DayOfWeek {get; set;}
    public TimeSpan StartTime {get; set;}
    public TimeSpan EndTime {get; set;}

}