using Asistencia.Data;
using Asistencia.Models;
using Asistencia.Models.ViewModels;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Asistencia.Services
{
    public class ClassroomService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="context"></param>
        public ClassroomService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Classroom>> GetClassroomsAsync()
        {
            return await _context.Classrooms.ToListAsync();
        }
        public async Task<List<SelectListItem>> GetClassroomsList()
        {
            var classroom = await _context.Classrooms
                    .AsNoTracking()
                    .OrderBy(x => x.Location)
                    .ThenBy(x => x.ClassroomName)
                    .Select(x => new { x.ClassroomId, x.ClassroomName, x.Location })
                    .ToListAsync();
            // group class by location
            var groupList = classroom
                .GroupBy(a => a.Location)
                .Select(g => new SelectListGroup { Name = g.Key })
                .ToList();
            var selectList = new List<SelectListItem>();
            foreach (var group in groupList)
            {
                foreach (var item in classroom.Where(x => x.Location == group.Name))
                {
                    selectList.Add(new SelectListItem
                    {
                        Value = item.ClassroomId?.ToString(),
                        Text = item.ClassroomName,

                    });
                }
            }
            return selectList;
        }
        /// <summary>
        /// this method que event from a interval o Monday to sunday
        /// </summary>
        /// <param name="classroomId"></param>
        /// <param name="weekStart"></param>
        /// <param name="weekEnd"></param>
        /// <returns></returns>
        public async Task<ClassroomWeeklyView> GetClassroomWeekAsync(string classroomId, DateOnly weekStart, DateOnly weekEnd)
        {
            var recurringSchedule = await _context.Schedules
                .Include(s => s.Course)
                .ThenInclude(a => a!.Subject)
                .Include(c => c.Classroom)
                .Include(c => c.AcademicPeriod)
                .Where(s => s.ClassroomId == classroomId &&
                s.AcademicPeriod.Status == EnumPeriodStatus.Active).ToListAsync();
            var events = new List<ClassroomCalendarEventViewModel>();
            for (var date = weekStart; date <= weekEnd; date = date.AddDays(1))
            {
                int currentDayOfWeek = date.DayOfWeek == DayOfWeek.Saturday ? 6 : (int)date.DayOfWeek;
                var dailyMatches = recurringSchedule.Where(s =>
                    s.DayOfWeek == currentDayOfWeek &&
                    date >= DateOnly.FromDateTime(s.Course.StartDate) && date <= DateOnly.FromDateTime(s.Course.EndDate)
                    );
                foreach (var match in dailyMatches)
                {
                    events.Add(new ClassroomCalendarEventViewModel
                    {
                        Date = date.ToDateTime(TimeOnly.MinValue),
                        DayName = date.ToString("dddd", new CultureInfo("es-Es")),
                        StartTime = match.StartTime,
                        EndTime = match.EndTime,
                        CourseName = match?.Course?.Subject?.SubjetName,
                        ColorHex = match.Course.ColorTheme
                    });
                }
            }
            return new ClassroomWeeklyView
            {
                ClassroomName = recurringSchedule.FirstOrDefault()?.Classroom.ClassroomName ?? "Aula Sin Nombre Asignado",
                WeekStart = weekStart.ToDateTime(TimeOnly.MinValue),
                WeekEnd = weekEnd.ToDateTime(TimeOnly.MinValue),
                Events = events.OrderBy(e => e.Date)
                .ThenBy(e => e.StartTime).ToList()
            };
        }
        public async Task<ClassroomWeeklyView> Calendar(string classroomId, DateTime? date = null)
        {
            var targetDate = DateOnly.FromDateTime(date ?? DateTime.Now);
            int diff = (7 + (targetDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            var weekStart = targetDate.AddDays(-1 * diff);
            var weekEnd = weekStart.AddDays(6);
            return await GetClassroomWeekAsync(classroomId, weekStart, weekEnd);
        }
        class SelectListGroup
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}