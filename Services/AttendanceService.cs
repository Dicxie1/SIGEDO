using Asistencia.Data;
using Asistencia.Documents.Attendance.Models;
using Asistencia.Documents.Gradebook;
using Asistencia.Extensions;
using Asistencia.Models;
using Asistencia.Models.DTOs;
using Asistencia.Models.Enums;
using Microsoft.EntityFrameworkCore;
namespace Asistencia.Services
{
    public class AttendanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        public AttendanceService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
                 .OrderBy(e => e.Student!.LastName) // Orden Alfabético
                 .ThenBy(e => e.Student!.Name)
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
        public async Task<byte[]> GetAttendanceExcelAsync(int courseId)
        {
            // 1. Obtener los datos crudos de la base de datos (PostgreSQL)
            var course = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Enrollments)
                    .ThenInclude( c => c.Student)
                .Include(c => c.Attendances.OrderBy(s => s.Date))
                    .ThenInclude(s => s.AttendanceDetails)
                        .ThenInclude(ad => ad.Enrollment)
                .AsNoTracking() // Fundamental para que sea rápido y consuma poca RAM
                .FirstOrDefaultAsync(c => c.IdCourse == courseId);

            if (course == null) throw new Exception("Curso no encontrado");

            // 2. Mapear al DTO neutral
            var sheetData = new AttendanceSheetData
            {
                CourseName = course?.Subject?.SubjetName ?? "N/A",
                CourseHour = course.TotalHours,
                LogoPath = Path.Combine(_env.WebRootPath, "img", "logo.png"),
                SessionDates = course.Attendances.Select(s => s.Date).ToList()
            };

            var studentsSorted = course.Enrollments.OrderBy(s => s.Student.LastName).ToList();

            foreach (var student in studentsSorted)
            {
                var row = new StudentAttendanceRow
                {
                    Carnet = student.Student.Id, // Asegúrate que la propiedad se llame así
                    FullName = $"{student.Student.LastName}, {student.Student.Name}",
                    TotalAbsences = 0
                };

                // Procesar las asistencias de este estudiante
                foreach (var session in course.Attendances)
                {
                    var attendanceRecord = session.AttendanceDetails.FirstOrDefault(a => a.Enrollment.StudentId == student.StudentId);
                    decimal sessionTotalHour = session.TotalHours;
                    decimal absentHourThisSession = 0;
                    if (attendanceRecord != null)
                    {
                        string status = attendanceRecord.Status.Trim() switch
                        {
                            "P" => "P",
                            "J" => "J",
                            "A" => "A",
                            "T" => "T",
                            _ => "-"
                        };
                        row.Attendances.Add(session.Date, status);
                        decimal attended = attendanceRecord.HoursAttended;
                        absentHourThisSession = sessionTotalHour - attended;
                        if (!string.IsNullOrEmpty(attendanceRecord.Status) && attendanceRecord.Status == "A")
                        {
                            row.TotalAbsences++;
                        }
                    }
                    else
                    {
                        row.Attendances.Add(session.Date, "-");
                        absentHourThisSession = sessionTotalHour;
                    }
                    if(absentHourThisSession > 0)
                    {
                        row.TotalAbsences += absentHourThisSession;
                    }
                }
                if(sheetData.CourseHour > 0)
                {
                    row.AbsencePercentage =  (row.TotalAbsences / sheetData.CourseHour) * 100;
                }

                sheetData.Students.Add(row);
            }

            // 3. Delegar la creación del archivo a la clase especialista
            var excelGenerator = new ExcelAttendanceGenerator();
            byte[] excelBytes = excelGenerator.GenerateSheet(sheetData);

            return excelBytes;
        }
    }
    
}
