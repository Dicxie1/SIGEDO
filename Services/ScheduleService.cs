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
        public async Task<List<CalendarEventDto>> GetClassroomAsync()
        {
            var list = new List<CalendarEventDto>();
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
                Shift = g.First().Course.Shift,
                // Calculamos los extremos del bloque unificado
                MinStartTime = g.Min(s => s.StartTime),
                MaxEndTime = g.Max(s => s.EndTime),
                Description = $"Aula / Laboratorio: {g.First().Classroom.ClassroomName}",

                // Guardamos un ID de referencia para el calendario
                BaseScheduleId = g.First().ScheduleId
            }).ToList();


            foreach (var block in groupedSchedules)
            {
                if (block.AcademicPeriod == null) continue;

                TimeSpan adjustedStartTime = block.MinStartTime;
                TimeSpan adjustedEndTime = block.MaxEndTime;
                bool isNotMorning = !string.Equals(block.Shift, "Matutino", StringComparison.OrdinalIgnoreCase);
                if (isNotMorning)
                {
                    if (adjustedStartTime.Hours < 12)
                    {
                        adjustedStartTime = adjustedStartTime.Add(TimeSpan.FromHours(12));
                    }
                    if (adjustedEndTime.Hours < 12)
                    {
                        adjustedEndTime = adjustedEndTime.Add(TimeSpan.FromHours(12));
                    }
                }
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
                    list.Add(new CalendarEventDto
                    {
                        id = $"sch-{block.BaseScheduleId}-{currentDate:yyyyMMdd}",
                        title = block.Course?.Subject?.SubjetName ?? "Clase",

                        // Aplicamos las horas extremas calculadas en la agrupación
                        start = currentDate.Add(adjustedStartTime).ToString("yyyy-MM-ddTHH:mm:ss"),
                        end = currentDate.Add(adjustedEndTime).ToString("yyyy-MM-ddTHH:mm:ss"),

                        extendedProps = new
                        {
                            classroom = block.Classroom?.ClassroomName,
                            teacher = $"{block.Teacher?.FirstName} {block.Teacher?.LastName}",
                            type = "class",
                            description = block.Description
                        },
                        color = "#3788d8",
                        allDay = false
                    });

                    // Saltamos directamente a la próxima semana
                    currentDate = currentDate.AddDays(7);
                }
            }
            var teacherEvents = await _context.TeacherEvents
                .Include(t => t.Teacher)
                .ToListAsync();
            foreach (var ev in teacherEvents)
            {
                list.Add(new CalendarEventDto
                {
                    // Prefijo 'evt-' para diferenciarlo de las clases al hacer clic
                    id = $"evt-{ev.TeacherEventId}",
                    title = ev.Title,

                    // FullCalendar acepta el formato ISO directo de DateTime
                    start = ev.StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = ev.EndDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),

                    extendedProps = new
                    {
                        classroom = "N/A", // Puedes cambiarlo si luego le agregas 'Location' al modelo
                        teacher = ev.Teacher != null ? $"{ev.Teacher.FirstName} {ev.Teacher.LastName}" : "Sin asignar",
                        description = ev.Description,
                        type = "event", // Para que JS sepa que es una reunión/capacitación
                        teacherId = ev.Teacher.TeacherID,
                    },
                    color = ev.ColorTheme, // Aquí usamos el color guardado en BD
                    allDay = ev.IsAllDay
                });
            }
            var orderedList = list.OrderBy(e => e.start).ToList();
            return list.OrderBy(s => s.start).ToList();
        }
        public async Task<bool> RegisterEventAsync(CreateEventDto dto)
        {
            if (dto.StartDateTime >= dto.EndDateTime)
            {
                throw new ArgumentException("La fecha de fin debe ser posterior a la fecha de inicio.");
            }
            var eventsToInsert = new List<TeacherEvent>();
            if (!dto.IsRecurring)
            {
                eventsToInsert.Add(new TeacherEvent
                {
                    TeacherId = dto.TeacherID,
                    Title = dto.Title,
                    ColorTheme = dto.ColorTheme,
                    IsAllDay = dto.IsAllDay,
                    StartDateTime = dto.StartDateTime,
                    EndDateTime = dto.EndDateTime,
                    Description = dto.Description
                });
            }
            else
            {
                if (dto.RecurrenceEndDate == null || !dto.RecurringDays.Any())
                {
                    throw new ArgumentException("Para eventos recurrentes debe seleccionar los días y una fecha de finalización.");
                }
                TimeSpan startTime = dto.StartDateTime.TimeOfDay;
                TimeSpan endTime = dto.EndDateTime.TimeOfDay;
                for (DateTime currentDate = dto.StartDateTime; currentDate <= dto.RecurrenceEndDate.Value.Date; currentDate.AddDays(1))
                {
                    if (dto.RecurringDays.Contains(currentDate.DayOfWeek))
                    {
                        eventsToInsert.Add(new TeacherEvent
                        {
                            TeacherId = dto.TeacherID,
                            Title = dto.Title,
                            ColorTheme = dto.ColorTheme,
                            IsAllDay = dto.IsAllDay,
                            StartDateTime = currentDate.Add(startTime),
                            EndDateTime = currentDate.Add(endTime),
                            Description = dto.Description
                        });
                    }
                }
            }
            try
            {
                await _context.TeacherEvents.AddRangeAsync(eventsToInsert);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                // Aquí podrías loguear el error o manejarlo según tus necesidades
                Console.WriteLine($"Error al registrar evento: {ex.Message}");
                return false;
            }
        }
        public async Task<List<Teacher>> SearchTeachersAsync(string term)
        {
            // 1. AsNoTracking: Le decimos a Entity Framework que no rastree estos objetos
            // en memoria porque solo los vamos a leer para enviarlos al Frontend. 
            // ¡Esto mejora el rendimiento y reduce el consumo de RAM!
            var query = _context.Teachers.AsNoTracking().AsQueryable();

            // Opcional pero recomendado: Filtrar solo profesores activos
            // query = query.Where(t => t.IsActive);

            // 2. Si el usuario escribió algo, aplicamos el filtro
            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.Trim().ToLower();

                // Buscamos coincidencias en el Nombre, el Apellido, o la unión de ambos 
                // (por si escriben "Ana Mar" en lugar de solo "Ana")
                query = query.Where(t =>
                    t.FirstName.ToLower().Contains(term) ||
                    t.LastName.ToLower().Contains(term) ||
                    (t.FirstName + " " + t.LastName).ToLower().Contains(term)
                );

                /* * TIP PARA POSTGRESQL AVANZADO:
                 * Si notas problemas de rendimiento porque .ToLower() evita el uso de índices normales,
                 * puedes usar el comodín ILIKE nativo de Npgsql (requiere usar el paquete de PostgreSQL):
                 * * query = query.Where(t => EF.Functions.ILike(t.FirstName, $"%{term}%") || ...);
                 */
            }

            // 3. Ejecutamos la consulta en la BD: Ordenamos y LIMITAMOS los resultados
            return await query
                .OrderBy(t => t.FirstName)
                .ThenBy(t => t.LastName)
                .Take(25) // Seguridad: Nunca devolvemos miles de registros, solo el Top 25
                .ToListAsync();
        }
    
    public async Task<bool> UpdateTeacherEventAsync(UpdateEventDto dto)
        {
            // Validar fechas
            if (dto.StartDateTime >= dto.EndDateTime)
            {
                throw new ArgumentException("La fecha de fin debe ser posterior a la de inicio.");
            }

            try
            {
                // 1. Obtener el evento original
                var existingEvent = await _context.TeacherEvents.FindAsync(dto.TeacherEventId);
                if (existingEvent == null) return false;

                // 2. Actualizar los campos básicos
                existingEvent.Title = dto.Title;
                existingEvent.ColorTheme = dto.ColorTheme;
                existingEvent.StartDateTime = dto.StartDateTime;
                existingEvent.EndDateTime = dto.EndDateTime;

                existingEvent.Description = dto.Description;

                // 3. Lógica Especial: Si decidió convertirlo en recurrente durante la edición
                var newRecurringEvents = new List<TeacherEvent>();

                if (dto.IsRecurring && dto.RecurrenceEndDate.HasValue && dto.RecurringDays.Any())
                {
                    TimeSpan startTime = dto.StartDateTime.TimeOfDay;
                    TimeSpan endTime = dto.EndDateTime.TimeOfDay;

                    // Comenzamos a crear copias a partir del DÍA SIGUIENTE al evento modificado
                    for (DateTime date = dto.StartDateTime.Date.AddDays(1); date <= dto.RecurrenceEndDate.Value.Date; date = date.AddDays(1))
                    {
                        if (dto.RecurringDays.Contains(date.DayOfWeek))
                        {
                            newRecurringEvents.Add(new TeacherEvent
                            {
                                TeacherId = existingEvent.TeacherId, // Mantiene el mismo profesor original
                                Title = dto.Title,
                                ColorTheme = dto.ColorTheme,
                                IsAllDay = existingEvent.IsAllDay,
                                StartDateTime = date.Add(startTime),
                                EndDateTime = date.Add(endTime),
                                Description = dto.Description
                            });
                        }
                    }
                }

                // 4. Guardar todo en una sola transacción
                _context.TeacherEvents.Update(existingEvent);

                if (newRecurringEvents.Any())
                {
                    await _context.TeacherEvents.AddRangeAsync(newRecurringEvents);
                }

                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error actualizando evento: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> DeleteTeacherEventAsync(int id)
        {
            try
            {
                // Buscamos el evento en la BD
                var teacherEvent = await _context.TeacherEvents.FindAsync(id);

                if (teacherEvent == null) return false; // Ya no existe

                _context.TeacherEvents.Remove(teacherEvent);
                var result = await _context.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar evento: {ex.Message}");
                return false;
            }
        }

    }
}
