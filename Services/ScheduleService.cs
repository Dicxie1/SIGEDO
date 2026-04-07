using Asistencia.Data;
using Asistencia.Models;
using Asistencia.Models.DTOs;
using Asistencia.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asistencia.Services
{
    public class ScheduleService
    {
        private readonly ApplicationDbContext _context;
        public ScheduleService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<ClassroomScheduleDto> GetClassroomSchedule(string classroomId)
        {
            var classroom = await _context.Classrooms.FindAsync(classroomId);
            var schedules = await _context.Schedules
                .Include(s => s.Course)
                .ThenInclude(sc => sc!.Subject)
                .Where(s => s.ClassroomId == classroomId).ToListAsync();
            var model = new ClassroomScheduleDto
            {
                ClassroomId = classroom.ClassroomId,
                ClassroomName = classroom.ClassroomName,
                Sessions = schedules.Select(s => new ClassSession
                {
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    CourseName = s?.Course?.Subject?.SubjetName,
                    ColorHex = GetColorForCourse(s!.ClassroomId!)
                }).ToList(),
            };
            return model;
        }
        public async Task<List<object>> GetCalendarEvents(DateTime start, DateTime end)
        {
            var events = new List<object>();

            // 1. Fetch Teacher Events
            var teacherEvents = await _context.TeacherEvents
                .Where(e => e.StartDateTime >= start && e.StartDateTime <= end)
                .ToListAsync();

            foreach (var te in teacherEvents)
            {
                events.Add(new
                {
                    id = $"e{te.TeacherEventId}",
                    title = te.Title,
                    start = te.StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = te.EndDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    color = te.ColorTheme,
                    description = te.Description,
                    allDay = false
                });
            }

            // 2. Fetch recurring Schedules and project them into the range
            var schedules = await _context.Schedules
                .Include(s => s.Course)
                .ThenInclude(c => c!.Subject)
                .Include(s => s.AcademicPeriod)
                .Where(s => s.AcademicPeriod.Status == EnumPeriodStatus.Active)
                .ToListAsync();

            for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                int dayOfWeek = (int)date.DayOfWeek;
                if (dayOfWeek == 0) dayOfWeek = 7; // Adjust Sunday if needed (ASP.NET 0=Sunday, Schedule 1=Monday)

                var daySchedules = schedules.Where(s => s.DayOfWeek == dayOfWeek).ToList();

                foreach (var s in daySchedules)
                {
                    // Ensure the date is within the academic period
                    var startPeriod = s.AcademicPeriod.StartPeriod.ToDateTime(TimeOnly.MinValue);
                    var endPeriod = s.AcademicPeriod.EndPeriod.ToDateTime(TimeOnly.MaxValue);

                    if (date >= startPeriod && date <= endPeriod)
                    {
                        events.Add(new
                        {
                            id = $"c{s.ScheduleId}-{date:yyyyMMdd}",
                            title = s.Course?.Subject?.SubjetName ?? "Clase",
                            start = date.Add(s.StartTime).ToString("yyyy-MM-ddTHH:mm:ss"),
                            end = date.Add(s.EndTime).ToString("yyyy-MM-ddTHH:mm:ss"),
                            color = "#0d6efd",
                            description = $"Aula: {s.ClassroomId}",
                            allDay = false
                        });
                    }
                }
            }

            return events;
        }

        private string GetColorForCourse(string courseId)
        {
            string[] colors = { "primary", "success", "danger", "info", "dark" };
            return colors[int.Parse(courseId) % colors.Length];
        }
        public async Task<bool> IsClassroomAvailable(string classroomId, int day, TimeSpan start, TimeSpan end)
        {
            bool existOverlap = await _context.Schedules.AnyAsync(s =>
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
        public async Task<List<object>> GetClassroomAsync()
        {
            var list = new List<object>();
            var schedules = await _context.Schedules
                .Include(s => s.Classroom)
                .Include(s => s.AcademicPeriod)
                .Include(s => s.Teacher)
                .Include(s => s.Course)
                .ThenInclude(s => s.Subject)
                .Where(s => s.Course.isActive).ToListAsync();
            var groupedSchedules = schedules
            .GroupBy(s => new { s.IdCourse, s.DayOfWeek }) // Agrupamos por Curso y Día
            .Select(g => new
            {
            Course = g.First().Course,
            Classroom = g.First().Classroom, // Asumimos que no cambian de aula tras el receso
            Teacher = g.First().Teacher,
            AcademicPeriod = g.First().AcademicPeriod,
            DayOfWeek = g.Key.DayOfWeek,

            // Calculamos los extremos del bloque unificado
            MinStartTime = g.Min(s => s.StartTime),
            MaxEndTime = g.Max(s => s.EndTime),

            // Guardamos un ID de referencia para el calendario
            BaseScheduleId = g.First().ScheduleId
            }).ToList();


            foreach (var block in groupedSchedules)
            {
                if (block.AcademicPeriod == null) continue;

                DateTime startRange = block.AcademicPeriod.StartPeriod.ToDateTime(TimeOnly.MinValue);
                DateTime endRange = block.AcademicPeriod.EndPeriod.ToDateTime(TimeOnly.MinValue);

                // OPTIMIZACIÓN: Buscamos el primer día que coincida con el día de la clase
                DateTime currentDate = startRange;
                while ((int)currentDate.DayOfWeek != block.DayOfWeek && currentDate <= endRange)
                {
                    currentDate = currentDate.AddDays(1);
                }

                // Una vez que encontramos el primer lunes (por ejemplo), damos saltos de 7 en 7 días
                // Esto reduce enormemente el procesamiento de los ciclos.
                while (currentDate <= endRange)
                {
                    list.Add(new
                    {
                        id = $"sch-{block.BaseScheduleId}-{currentDate:yyyyMMdd}",
                        title = block.Course?.Subject?.SubjetName ?? "Clase",

                        // Aplicamos las horas extremas calculadas en la agrupación
                        start = currentDate.Add(block.MinStartTime).ToString("yyyy-MM-ddTHH:mm:ss"),
                        end = currentDate.Add(block.MaxEndTime).ToString("yyyy-MM-ddTHH:mm:ss"),

                        extendedProps = new
                        {
                            classroom = block.Classroom?.ClassroomName,
                            teacher = $"{block.Teacher?.FirstName} {block.Teacher?.LastName}"
                        },
                        color = "#3788d8",
                        allDay = false
                    });

                    // Saltamos directamente a la próxima semana
                    currentDate = currentDate.AddDays(7);
                }
            }
            return list;
        }
        public async Task<bool> RegisterEventAsync(CreateEventDto dto  )
        {
            if(dto.StartDateTime >= dto.EndDateTime)
            {
                throw new ArgumentException("La fecha de fin debe ser posterior a la fecha de inicio.");
            }
            var newEvent = new TeacherEvent
            {
                TeacherId = dto.TeacherID,
                Title = dto.Title,
                ColorTheme = dto.ColorTheme,
                IsAllDay = dto.IsAllDay,
                StartDateTime = dto.StartDateTime,
                EndDateTime = dto.EndDateTime,
                Description = dto.Description
            };
            try
            {
                await _context.TeacherEvents.AddAsync(newEvent);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }catch(Exception ex)
            {
                // Aquí podrías loguear el error o manejarlo según tus necesidades
                Console.WriteLine($"Error al registrar evento: {ex.Message}");
                return false;
            }
        }
    }
}
