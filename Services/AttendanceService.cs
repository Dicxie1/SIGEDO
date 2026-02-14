using Asistencia.Data;
using Asistencia.Documents.Attendance.Models;
using Asistencia.Models;
using Asistencia.Models.Enums;
using Asistencia.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Asistencia.Services
{
    public class AttendanceService
    {
        private readonly ApplicationDbContext _context;
        public AttendanceService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<AttendanceReportModel> GetReportDataAsync(int courseId)
        {
            // 1. Obtener información del Curso (Cabecera del Reporte)
            var course = await _context.Courses
                .Include(c => c.Subject)        // Asignatura
                .Include(c => c.AcademicTerms)   // Semestre/Periodo
                .FirstOrDefaultAsync(c => c.IdCourse == courseId);

            if (course == null) throw new Exception("Curso no encontrado");

            // 2. Obtener TODAS las asistencias registradas para ese curso
            var enrollments = await _context.Enrollments
                 .Where(e => e.IdCourse == courseId)
                 .Include(e => e.AttendanceDetails)
                 .Include(e => e.Student) // Traemos al estudiante para obtener el Nombre
                 .Where(e => e.Status == EnrollmentStatus.Active) // Opcional: Solo activos
                 .OrderBy(e => e.Student.LastName) // Orden Alfabético
                 .ThenBy(e => e.Student.Name)
                 .ToListAsync();

            // 3. Obtener las fechas ÚNICAS de las clases (Columnas del reporte)
            // Ordenamos cronológicamente
            var sessions = await _context.Attendances
                .Where(a => a.IdCourse == courseId)
                .Include(a => a.AttendanceDetails)
                .ToListAsync();

            // 4. Obtener la lista MAESTRA de estudiantes inscritos
            // (Es mejor consultar 'Enrollments' para no omitir alumnos sin asistencia registrada)
            var distinctDates = sessions
                 .Select(a => a.Date)
                 .Distinct()
                 .OrderBy(d => d)
                 .ToList();


            // 5. Transformar la data (Cruzar Estudiantes vs Fechas)
            var studentRows = new List<StudentAttendance>();
            decimal totalCourseHours = sessions.Sum(a => a.TotalHours);
            foreach (var enrolled in enrollments)
            {
                var row = new StudentAttendance
                {
                    StudentName = $"{enrolled.Student.LastName}, {enrolled.Student.Name}", // Formato: Pérez, Juan
                    StudentId = enrolled.Student.Id, // Carnet
                    AttendanceLog = new Dictionary<DateOnly, string>(),
                    AttendancePercentage = 0
                };

                decimal presentCount = 0;
                int totalClasses = distinctDates.Count;
                decimal studentTotalHoursAttended = 0;
                // Para cada fecha que hubo clase, buscamos el estado del alumno
                foreach (var date in distinctDates)
                {
                    // A. Buscar la sesión de esa fecha (Maestro)
                    var session = sessions.FirstOrDefault(s => s.Date == date);

                    if (session != null)
                    {
                        // B. Buscar el detalle de este estudiante específico en esa sesión
                        // Asumimos que AttendanceDetail tiene EnrollmentId o StudentId
                        var detail = session.AttendanceDetails
                            .FirstOrDefault(d => d.EnrollmentId == enrolled.EnrollmentId);

                        if (detail != null)
                        {
                            AttendanceStatus statusEnum = MapToEnum(detail.Status);
                            // --- LÓGICA VISUAL (LETRA) ---
                            // Mostramos la letra, o si prefieres, las horas (ej: "2/3")
                            // Si usas el Enum Status, lo convertimos a letra
                            row.AttendanceLog.Add(date, statusEnum.ToLetter());

                            // --- LÓGICA MATEMÁTICA (ACUMULADOR) ---
                            // Sumamos las horas que el estudiante estuvo presente
                            // OJO: Solo sumamos si el estado cuenta como presente (P o T)
                            if (statusEnum.CountsAsPresent())
                            {
                                studentTotalHoursAttended += detail.HoursAttended;
                            }
                        }
                        else
                        {
                            // Hubo clase (Session existe) pero no hay detalle del alumno = Ausente
                            row.AttendanceLog.Add(date, "-");
                            // No sumamos nada a studentTotalHoursAttended
                        }
                    }
                }

                // Calcular %
                if (totalClasses > 0)
                {
                    decimal percentage = (studentTotalHoursAttended / totalCourseHours) * 100;

                    // Redondeamos y convertimos a double para el ViewModel
                    row.AttendancePercentage = (double)Math.Round(percentage, 0);
                }
                else
                {
                    // Si no se han impartido horas, el porcentaje es 100% o 0% según política
                    row.AttendancePercentage = 100;
                }

                studentRows.Add(row);
            }

            // 6. Construir el Modelo Final
            return new AttendanceReportModel
            {
                UniversityName = "URACCAN",
                Campus = "Sede Central",
                CourseName = course.Subject.SubjetName,
                ProfessorName =  "Dicxie Danuard Madrigal",
                Term =  "Período Actual",
                Dates = distinctDates,
                Students = studentRows
            };
        }
        // -------------------------------------------------------------------
        // MÉTODO AUXILIAR (TRADUCTOR STRING -> ENUM)
        // -------------------------------------------------------------------
        private AttendanceStatus MapToEnum(string statusDbValue)
        {
            if (string.IsNullOrEmpty(statusDbValue)) return AttendanceStatus.Absent;

            // Normalizamos a mayúsculas y quitamos espacios por si acaso
            string normalized = statusDbValue.Trim().ToUpper();

            // Ajusta estos casos según lo que realmente guardes en tu BD
            return normalized switch
            {
                "P" or "PRESENTE" or "PRESENT" => AttendanceStatus.Present,
                "A" or "AUSENTE" or "ABSENT" => AttendanceStatus.Absent,
                "T" or "TARDANZA" or "LATE" or "L" => AttendanceStatus.Late,
                "J" or "JUSTIFICADO" or "EXCUSED" => AttendanceStatus.Excused,
                "R" or "RETIRADO" => AttendanceStatus.Withdrawn,
                _ => AttendanceStatus.Absent // Valor por defecto si no reconoce el string
            };
        }
    }
    
}
