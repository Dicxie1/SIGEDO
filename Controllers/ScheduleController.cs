using Microsoft.AspNetCore.Mvc;
using Asistencia.Data;
using Microsoft.EntityFrameworkCore;
using Asistencia.Models.ViewModels;
namespace Asistencia.Controllers;

public class ScheduleController : Controller
{
    private readonly ApplicationDbContext _context;
    public ScheduleController(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IActionResult> Details(string classroomId)
    {
        var classroom = await _context.Classrooms.FindAsync(classroomId);
        if (classroom == null) return NotFound();
        var schedules = await _context.Schedules
            .Include(s => s.Course)
            .ThenInclude(sc => sc!.Subject)
            .Where(s => s.ClassroomId == classroomId).ToListAsync();
        var model = new ClassroomScheduleDto
        {
            ClassroomId = classroom.ClassroomId,
            ClassroomName = classroom.ClassroomName,
            Sessions  =  schedules.Select(s => new ClassSession
            {
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                CourseName = s?.Course?.Subject?.SubjetName,
                ColorHex = GetColorForCourse( s!.ClassroomId!)
            }).ToList(),
        };
        return PartialView("", model);
    }
    private string GetColorForCourse(string courseId)
    {
        string [] colors = {"primary", "success", "danger", "info", "dark"};
        return colors[ int.Parse(courseId) % colors.Length];
    }

    private async Task<bool> IsClassroomAvailable(string classroomId, int day, TimeSpan start, TimeSpan end)
    {
        bool existOverlap = await _context.Schedules.AnyAsync( s=> 
            s.ClassroomId == classroomId &&
            s.DayOfWeek == day &&
            (
                (start >= s.StartTime && start < s.EndTime) ||
                (end > s.StartTime && end <= s.EndTime) ||
                (start <= s.StartTime && end >= s.EndTime)
            )
        );
        return !existOverlap;
    }
}