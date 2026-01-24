using Asistencia.Data;
using Asistencia.Models;
using Asistencia.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Asistencia.Controllers;

public class AttendanceController: Controller
{
    private readonly ApplicationDbContext _context;
    public AttendanceController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpPost]
    public IActionResult SavaSudentesAttendances()
    {

        return View();
    }
    [HttpGet("/Course/{courseid}/Attendance/{attendance}")]
    async public Task<IActionResult> AttendanceDetail(int courseid, int attendance)
    {
        if(courseid <= 0 || attendance <= 0) return BadRequest();
        var attendancesRecord =  await _context.Attendances
            .Include(a => a.Course)
                .ThenInclude(s => s!.Subject)
            .Include(a => a.AttendanceDetails)
                .ThenInclude(d => d.Enrollment)
                    .ThenInclude(c => c!.Student)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.IdCourse == courseid && a.AttendanceId == attendance );
        if(attendancesRecord == null) 
        {
            Console.WriteLine("DEBUG: No attendance record found for courseid: " + courseid + ", attendance: " + attendance);
            return NotFound();
        }

        Console.WriteLine("DEBUG: Attendance record found. CourseId: " + attendancesRecord.IdCourse + ", SubjectId: " + (attendancesRecord.Course?.SubjectId ?? "NULL"));

        // Debug: Check if the course and subject are loaded correctly
        if (attendancesRecord.Course == null)
        {
            Console.WriteLine("DEBUG: Course is null in attendance record");
            return NotFound("Course not found in attendance record");
        }
        
        if (attendancesRecord.Course.Subject == null)
        {
            Console.WriteLine("DEBUG: Subject is null in course. CourseId: " + attendancesRecord.Course.IdCourse);
            Console.WriteLine("DEBUG: Course SubjectId: " + attendancesRecord.Course.SubjectId);
            
            // Try to load the subject manually as a fallback
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.SubjectId == attendancesRecord.Course.SubjectId);
            
            if (subject != null)
            {
                attendancesRecord.Course.Subject = subject;
                Console.WriteLine("DEBUG: Subject loaded manually: " + subject.SubjetName);
            }
            else
            {
                Console.WriteLine("DEBUG: Subject not found in database with SubjectId: " + attendancesRecord.Course.SubjectId);
            }
        }
        
        int totalStudent = attendancesRecord.AttendanceDetails.Count;
        int presentCount = attendancesRecord.AttendanceDetails.Count( a => a.Status == "P");
        int absentCount = attendancesRecord.AttendanceDetails.Count( a => a.Status == "A");
        int latesCount = attendancesRecord.AttendanceDetails.Count( a => a.Status == "T");
        int justifiedCount = attendancesRecord.AttendanceDetails.Count( a => a.Status == "J");
        int percetage = totalStudent == 0 ? 0 : 
            (int)Math.Round(((double)(presentCount + latesCount) / totalStudent) * 100);
        var viewModel = new AttendanceViewDto
        {
            AttendanceId = attendancesRecord.AttendanceId,
            CourseId = attendancesRecord.IdCourse,
            CourseName = attendancesRecord.Course?.Subject?.SubjetName ?? "Asignatura no encontrada",
            Date = attendancesRecord.Date   ,
            Topic = attendancesRecord.Topic,
            TotalHours = attendancesRecord.TotalHours ,
            AttendancePercentage = percetage,
            Summary = new AttendanceSummaryDto
            {
                TotalStudents = totalStudent,
                PresentCount = presentCount,
                AbsentCount = absentCount,
                JustifiedCount = justifiedCount,
                LateCount = latesCount
            },
            AttendanceDetail = attendancesRecord.AttendanceDetails
                .Select(s => new AttendanceViewDetailDto
                {
                    EnrollmentCode = s!.Enrollment!.EnrollmentId  ,
                    StudentCode = s!.Enrollment!.Student!.Id ?? "0",
                    StudentName = $"{s.Enrollment.Student?.Name} {s.Enrollment.Student?.LastName}",
                    StudentInitials =  GetInitials(s.Enrollment.Student!.Name!, s.Enrollment.Student!.LastName!),
                    Status = s.Status,
                    HoursAttended = s.HoursAttended,
                    Observation = s.Observation
                }).OrderBy( s => s.StudentName)
                .ToList()
        } ;
        return View("_AttendanceDetail", viewModel);
    }
    
    // Método auxiliar para generar iniciales bonitas
    public string GetInitials(string name, string lastName)
    {
        char first = !string.IsNullOrEmpty(name) ? name[0] : '?';
        char second = !string.IsNullOrEmpty(lastName) ? lastName[0] : '?';
        return $"{first}{second}".ToUpper();
    }

    [HttpGet("/Course/{courseid}/Attendance/Edit/{attendance}")]
    public async Task<IActionResult> EditAttendance(int courseid, int attendance)
    {
        if(courseid <= 0 || attendance <= 0) return BadRequest();
        var attendances = _context.Attendances
            .Include(a => a.Course)
                .ThenInclude(c => c.Subject)
            .Include(da => da.AttendanceDetails)
                .ThenInclude(d => d.Enrollment)
                    .ThenInclude(s => s.Student)
            .FirstOrDefault(a => a.AttendanceId == attendance && a.IdCourse == courseid);
        if(attendances == null) return NotFound("Se encontro registros");
        var model = new AttendanceUpdateDto{
            IdCourse = attendances.IdCourse,
            AttendanceId = attendances.AttendanceId,
            Date = attendances.Date,
            Topic = attendances.Topic,
            Hours = attendances.TotalHours,
            AttendanceDetail = attendances.AttendanceDetails.Select( d => new AttendenceUpdateDetailsDto
            {
                EnrollmentId = d.Enrollment.EnrollmentId,
                StudentFullName = $"{d.Enrollment.Student.Name} {d.Enrollment.Student.LastName}",
                Status = d.Status,
                HoursAttended = d.HoursAttended,
                Observation = d.Observation
            }
            ).ToList()
        };
        return View("_AttendanceEdit", model);
    }
    [HttpPost]
    public async Task<IActionResult> UpdateAttedance([FromBody] AttendanceUpdateDto model)
    {
        if(!ModelState.IsValid) return BadRequest();
        var session = await _context.Attendances
            .Include(a => a.AttendanceDetails)
            .FirstOrDefaultAsync(a => a.AttendanceId == model.AttendanceId);
        if (session == null) return NotFound(new { success = false, message = $"Sesión {model.AttendanceId} no encontrada" });
        session.Date = model.Date;
        session.Topic = model.Topic;
        session.TotalHours = model.Hours;
        // 2. Actualizar detalles (Filas)
        foreach (var incomingDetail in model.AttendanceDetail)
        {
            // Buscamos el detalle existente en la BD que corresponde a este estudiante
            var dbDetail = session.AttendanceDetails
                .FirstOrDefault(d => d.EnrollmentId == incomingDetail.EnrollmentId);

            if (dbDetail != null)
            {
                dbDetail.Status = incomingDetail.Status;
                dbDetail.HoursAttended = incomingDetail.HoursAttended;
                dbDetail.Observation = incomingDetail.Observation;
            }
        }
        try
        {
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error al actualizar: " + ex.Message });
        }
    }
    public async Task<int> AttendancePercentageAsync(int enrollmentId)
    {
        var enrollment = await _context.Enrollments
            .Include( e => e.Course)
            .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
        int courseId = enrollment!.IdCourse;
        decimal attendanceHours =  await TotalAttendanceCourse(enrollmentId);
        decimal totalCourseHours = await TotalCourseHours(courseId);
        return (int) Math.Round((attendanceHours * 100) / totalCourseHours);
    }
    /// <summary>
    /// Metodo que calcula el total de las hora de un curso
    /// </summary>
    /// <param name="enrollmentId"> el codigo del curso que se quiere calcular la horas </param>
    /// <returns></returns>
    public async Task<decimal> TotalCourseHours(int IdCourse)
    {
        decimal total = await _context.Attendances
            .Where(a => a.IdCourse == IdCourse)
            .SumAsync(a => a.TotalHours);
        return total;
    }
    /// <summary>
    /// calcular el cantidad de horas recibida por un estudiante en particular
    /// </summary>
    /// <param name="enrollment">representa la matricula del estudiante en el curso</param>
    /// <returns></returns>
    public async Task<decimal> TotalAttendanceCourse(int enrollmentId)
    {
        return await _context.AttendancesDetails
            .Where(d => d.EnrollmentId == enrollmentId &&
                ( d.Status == "P"  || d.Status == "T"))
            .SumAsync(a => a.HoursAttended);
    }
    
}