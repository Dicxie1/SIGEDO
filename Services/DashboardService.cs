using Asistencia.Data;
using Asistencia.Models;
using Asistencia.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Asistencia.Services
{
    public class DashboardService
    {
        private readonly ApplicationDbContext _context;
        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync()
        {
            var courses = await GetActiveCoursesAsync();
            return new DashboardViewModel
            {
                CountCourseActive = courses.Count,
                studentCount = await _context.Students.CountAsync(),
                CourseActive = courses,
                PendingTaskCount = await GetPendingGradesCountAsync(), // Tareas sin calificar
                PedingAssigmentItems = await GetPedingAssigmentItem(),
                TotalStudenActive = await GetTotalUniqueActiveStudentAsync(),
                StudentAtRiskAttendance = await GetStudentsAtRiskAttendanceCountAsync(), // KPI 15% Inasistencia
                TodaySchedule =  await GetScheduleItemViewModelAsync(),
                WeeklyAgenda = await GetRemainingWeekAgendaAsync(),
                CurrentClass = await GetCurrentClassViewModelAsync()
            };
        }

        public async Task<List<CourseListItemViewModel>> GetActiveCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Enrollments)
                .Include(c => c.Attendances)
                .Where(c => c.isActive)
                .Select(c => new CourseListItemViewModel
                {
                    CourseID = c.IdCourse,
                    CourseName = c.Subject!.SubjetName,
                    CountEnrrolle = c.Enrollments.Count,
                    ProgressPercentage = c.TotalHours > 0 
                        ? ((double)c.Attendances.Sum(d => d.TotalHours) * 100 / c.TotalHours) 
                        : 0,
                }).ToListAsync();
        }

        public async Task<int> GetPendingGradesCountAsync()
        {
            // Cuenta estudiantes activos que tienen tareas asignadas pero sin nota registrada (o nota null)
            return await _context.Enrollments
                .Where(e => e.Course!.isActive && e.Status == EnrollmentStatus.Active)
                .SelectMany(e => e.Course!.AcademicTerms.SelectMany(t => t.Assignments),
                    (enrollment, assignment) => new { enrollment, assignment })
                .Where(x => !_context.StudentGrades.Any(sg =>
                    sg.EnrollmentId == x.enrollment.EnrollmentId &&
                    sg.AssignmentId == x.assignment.AssignmentId &&
                    sg.Score != null))
                .CountAsync();
        }

        public async Task<int> GetStudentsAtRiskAttendanceCountAsync()
        {
            // KPI: Estudiantes con >= 15% de inasistencia (basado en horas faltadas vs horas totales del curso)
            const decimal threshold = 0.15M;
            
            return await _context.Enrollments
                .Where(e => e.Status == EnrollmentStatus.Active && e.Course!.isActive)
                .Select(e => new
                {
                    TotalCourseHours = (decimal)e.Course!.TotalHours,
                    // Horas faltadas = Sumatoria de (Horas de la sesión - Horas asistidas)
                    HoursMissed = _context.AttendancesDetails
                        .Where(ad => ad.EnrollmentId == e.EnrollmentId)
                        .Sum(ad => ad.Attendance!.TotalHours - ad.HoursAttended)
                })
                .Where(x => x.TotalCourseHours > 0 && x.HoursMissed / x.TotalCourseHours >= threshold)
                .CountAsync();
        }

        public async Task<List<PedingAssigmentItem>> GetPedingAssigmentItem()
        {
            return await _context.Assignments
                .Where(a => a.AcademicTerm!.Course!.isActive)
                .Select(a => new PedingAssigmentItem
                {
                    CourseId = a.AcademicTerm!.Course!.IdCourse,
                    AssignmentId = a.AssignmentId,
                    CourseName = a.AcademicTerm!.Course!.Subject!.SubjetName,
                    Title = a.Title,
                    Pending = a.AcademicTerm.Course.Enrollments
                        .Count(e => e.Status == EnrollmentStatus.Active &&
                        !e.Grades.Any(g => g.AssignmentId == a.AssignmentId))
                })
                .Where(x => x.Pending >= 1)
                .OrderByDescending(x => x.Pending)
                .ToListAsync();
        }

        public async Task<int> GetTotalUniqueActiveStudentAsync()
        {
            return await _context.Enrollments
                .Where(e => e.Course!.isActive && e.Status == EnrollmentStatus.Active)
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync();
        }

        public async Task<List<ScheduleItemViewModel>> GetScheduleItemViewModelAsync()
        {
            var today = DateTime.Today.DayOfWeek;
            var now = DateTime.Now.TimeOfDay;
            return await _context.Courses
                .Include(c => c.Schedules)
                .Include(c => c!.Subject)
                .Where(c => c.isActive && c.Schedules.Any(s => s.DayOfWeek == ((int)today) && s.StartTime > now))
                .Select(c => new ScheduleItemViewModel
                {
                    SubjectName = c.Subject!.SubjetName,
                    Location = c.Classroom!.ClassroomName,
                    BorderColorClass = "border-primary",
                    StartTime = c.Schedules
                        .Where(s => s.DayOfWeek == ((int)today) && s.StartTime > now)
                        .OrderBy(s => s.StartTime)
                        .Select(s => s.StartTime)
                        .FirstOrDefault()

                }).ToListAsync();
                
        }
        public async Task<List<WeeklyAgendaViewModel>> GetRemainingWeekAgendaAsync()
        {
            await Task.Delay(150);
            var today = DateTime.Today;
            int daysUntilSunday = ((int)DayOfWeek.Sunday - (int)today.DayOfWeek + 7) % 7;
            var endOfWeek = today.AddDays(daysUntilSunday).AddDays(1).AddTicks(-1);
            var teacherEvent = await _context.TeacherEvents
                .AsNoTracking()
                .Where(e => e.StartDateTime >= today && e.StartDateTime <= endOfWeek)
                .ToListAsync();
            var weekAgenda = teacherEvent
                .GroupBy(e => e.StartDateTime.Date)
                .OrderBy(g => g.Key)
                .Select(group => new WeeklyAgendaViewModel
                {
                    Day = group.Key.DayOfWeek,
                    Activities = group
                        .OrderBy(e => e.StartDateTime.TimeOfDay)
                        .Select(e => new AgendanItemDetailViewModel
                        {
                            Title = e.Title,
                            Hour = e.StartDateTime.TimeOfDay,
                            Location = string.IsNullOrEmpty(e.Description) ? "Por Definir" : e.Description,
                            Type = MapColorToAgendaType(e.ColorTheme)
                        }).ToList()
                }).ToList();
            
            return weekAgenda;
        }
        public async Task<CurrentClassViewModel> GetCurrentClassViewModelAsync()
        {
            var today = DateTime.Today.DayOfWeek;
            var now = DateTime.Now.TimeOfDay;
            var currentClass = await _context.Courses
                .Where(c => c.isActive && c.Schedules.Any(s => s.DayOfWeek == ((int)today))) 
                .Select( c => new
                {
                    Course = c,
                    MinStartTime = c.Schedules.Where(s => s.DayOfWeek == ((int)today) ).Min( s => s.StartTime),
                    MaxEndTime = c.Schedules.Where(s => s.DayOfWeek == ((int)today)).Max(s => s.EndTime)
                } )
                .Where(x => now >= x.MinStartTime && now <= x.MaxEndTime)
                .Select(x => new CurrentClassViewModel
                {
                   CourseId = x.Course.IdCourse,
                   CourseName = x.Course.Subject!.SubjetName,
                   Location = x.Course.Classroom!.ClassroomName,
                   StartTime = x.Course.Schedules.First(s => s.DayOfWeek == ((int)today) && s.StartTime <= now && s.EndTime >= now).StartTime,
                   EndTime = x.Course.Schedules.First(s => s.DayOfWeek == ((int)today) && s.StartTime <= now && s.EndTime >= now).EndTime,
                   StudentCount = x.Course.Enrollments.Count(e => e.Status == EnrollmentStatus.Active)
                }).FirstOrDefaultAsync();
            return currentClass;
        }
        private AgendaItemType MapColorToAgendaType(string colorTheme)
        {
            return colorTheme switch
            {
                "#198754" => AgendaItemType.Meeting,  // Verde = Reunión
                "#ffc107" => AgendaItemType.Meeting,  // Amarillo = Capacitación (Si tienes un Enum de Training, úsalo aquí)
                "#0d6efd" => AgendaItemType.Class,    // Azul = Clase Regular
                _ => AgendaItemType.Meeting           // Gris / Otro
            };
        }
    }
}
