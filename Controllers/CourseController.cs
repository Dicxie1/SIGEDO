using Asistencia.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Asistencia.Models.ViewModels;
using Asistencia.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Asistencia.Models.DTOs;
using QRCoder.Extensions;
using Asistencia.Extensions;
using Asistencia.Services;
namespace Asistencia.Controllers;

public class CourseController : Controller
{
    private readonly ApplicationDbContext _context;
    public CourseController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        return View();
    }
    public async Task<IActionResult> Admin()
    {
        // Lista las carrera 
        var career =  await _context.Careers
            .ToListAsync();
        ViewBag.CourseList = new SelectList(career, "CareerId", "Name");

        var courses = await _context.Courses
            .Include(c => c.Subject)
            .Include(c => c.Classroom)
            .ToListAsync();
        // ViewModels
        var viewModel = new CourseListViewModel
        {
            Courses = courses,
            ActiveCourses =  courses
                .Where(c => c.isActive == true )
                .ToList(),
            Classrooms =  await _context.Classrooms.ToListAsync()
        };
        var classrooms = await _context.Classrooms
            .OrderBy(c => c.ClassroomName)
            .ToListAsync();
        ViewBag.ClassroomList = new SelectList(classrooms, "ClassroomId", "ClassroomName");
        return View(viewModel);
    }
    [HttpGet("Course/{courseId}/Details")]
    public async Task<IActionResult> Details(int courseId)
    {
        var _syllabusService = new SyllabusService(_context);
        // Carga todo los datos de curso
        // registrode de asignatura, carrera, aula, matricula, parciales, tareas, estudiantes, calificacion  
        var course = await _context.Courses
            .Include( c => c.Subject)
                .ThenInclude(s => s!.Career)
            .Include(c => c.Classroom)
            .Include(c => c.Enrollments)
                .ThenInclude(st => st.Student)
            .Include(c => c.AcademicTerms)
            .ThenInclude(t => t.Assignments)
            .ThenInclude(w => w.Grades)
                .FirstOrDefaultAsync( m=> m.IdCourse == courseId);
        var subject = await _context.Subjects
            .FindAsync(course!.SubjectId);
        ViewBag.CourseId = courseId;
        if(course == null) {return View("CourseNotFound");}
        //lista de estudiantes matriculado
        var enrolledStudents = _context.Enrollments
            .Include(e => e.Student).
            Where(e => e.IdCourse == courseId)
            .Select( e => new EnrolledStudentDto
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId!,
                StudentInitials = Utils.GetInitials(e.Student!.Name ?? "N", e.Student!.LastName ?? "N"),
                FullName = e.Student!.Name + " " + e.Student!.LastName,
                Email = e.Student!.Email!,
                Cellphone = e.Student!.Cellphone!,
                Sexo = e.Student!.Sexo ,
                Ethnic = e.Student!.Ethnic! ,
            });
        //Cantidad de estudiantes del sexo Masculino
        int male = enrolledStudents.Count(m => m.Sexo == Sex.Masculino);
        //Cantidad de estudiantes del sexo Femenino
        int female = enrolledStudents.Count(f => f.Sexo == Sex.Femenino);
        // Etnia de los estudiantes matriculado
        var students = course.Enrollments.Select(e => e.Student).ToList();

        int totalStudents = course.Enrollments.Count;

        var etchnicityGroupRaw = enrolledStudents
            .GroupBy(s => s.Ethnic ).Select( g => new
            {
                Name = g.Key,
                Count = g.Count()
            }).ToList();
        var ethnicityStats = etchnicityGroupRaw
            .Select( item => new EthnicityStatDto
            {
                Name = item.Name.ToString(),
                Count = item.Count,
                Percentage = totalStudents > 0 ? ((decimal)item.Count / totalStudents) * 100 : 0,
                ColorClass = item.Name switch
                {
                    EthnicGroup.Miskito  => "bg-success",   // Verde
                    EthnicGroup.Creole   => "bg-info",      // Azul
                    EthnicGroup.Garifuna => "bg-warning",   // Amarillo
                    EthnicGroup.Mayangna => "bg-danger",    // Rojo
                    EthnicGroup.Mestizo  => "bg-secondary", // Gris
                    EthnicGroup.Rama     => "bg-primary",   // Azul oscuro
                    _ => "bg-light text-dark border"        // Defecto
                }
            }).ToList();
        // datos de asistencias por seccion
        var attendanceHitory = await _context.Attendances
            .Where(a => a.IdCourse == course.IdCourse)
            .OrderByDescending(a => a.Date)
            .Select( a=> new AttendanceHistoryDto
            {
                Id = a.AttendanceId,
                Date = a.Date,
                Topic = a.Topic ?? "Sin especificar",
                TotalStudents = a.AttendanceDetails.Count(),
                PresentCount = a.AttendanceDetails.Count(d => d.Status == "P"),
                AbsentCount = a.AttendanceDetails.Count(d => d.Status == "A"),
                LateCount = a.AttendanceDetails.Count(d => d.Status == "T"),
                JustifiedCount = a.AttendanceDetails.Count(d => d.Status == "J")
            }).ToListAsync();
        // ordena la asistencia de la primara a la ultima
        int totalSession = attendanceHitory.Count;
        for(int i = 0; i < totalSession; i++)
        {
            attendanceHitory[i].AttendancesNumber =  totalSession - i;
        }
        var attendances  = await _context.Attendances
            .Include(a => a.AttendanceDetails)
            .Where(a => a.IdCourse == course.IdCourse)
            .OrderBy(a => a.Date)
            .AsNoTracking()
            .ToListAsync();
        var statAttendance = new AttendaceStatsViewBox();
        foreach( var atted in attendances)
        {
            statAttendance.AttendaceDates.Add(atted.Date.ToString("dd/MM"));
            int presentCout = atted.AttendanceDetails
                .Count(d => d.Status == "P" || d.Status == "T");
            statAttendance.PresentPerSession.Add(presentCout);
            int absentCount = atted.AttendanceDetails
                .Count(d => d.Status == "A" || d.Status == "J" );
            statAttendance.AbsentsPerSession.Add(absentCount);
        }
        var allDetail = attendances.SelectMany(s => s.AttendanceDetails).ToList();

        statAttendance.TotalPresents = allDetail.Count(a => a.Status == "P");
        statAttendance.TotalAbsents = allDetail.Count(a => a.Status == "A");
        statAttendance.TotalJustified = allDetail.Count(a => a.Status == "J");
        statAttendance.TotalLates = allDetail.Count(a => a.Status == "T");
        //GradeBook
        var terms = await _context.AcademicTerms
            .Include(t=> t.Assignments)
                .ThenInclude(t => t.Grades)
            .Where(t => t.CourseId == course.IdCourse)
            .OrderBy(a => a.TermId)
            .AsNoTracking()
            .ToListAsync();
        
        var enrollmentGrade = await _context.Enrollments
            .Where(e => e.IdCourse == course.IdCourse)
            .Include(e => e.Student)
            .Include(e => e.Grades )
            .OrderBy(e => e.Student.LastName)
            .Include(e => e.Course)
                .ThenInclude(s =>s.Subject)
            .ToListAsync();

        var studentRows = enrollmentGrade.Select(e => CalculateStudentRow(e, course.AcademicTerms.ToList())).ToList();
        var gradeBook =new GradebookViewModel
        {
            CourseId = course.IdCourse,
            CourseName = course!.Subject!.SubjetName ?? "S/N", // Asumiendo que Course tiene propiedad Name
            
            // A. Mapear Encabezados (Columnas)
            Terms = terms.Select(t => new TermHeaderDto
            {
                TermId = t.TermId,
                Name = t.Name,
                Weight = t.WeightOnFinalGrade,
                Assignments = t.Assignments.Select(a => new AssignmentHeaderDto
                {
                    AssignmentId = a.AssignmentId,
                    Title = a.Title,
                    MaxPoints = a.MaxPoints,
                    IsExam = a.IsExam
                }).ToList()
            }).ToList(),
            // B. Mapear Filas (Estudiantes)
            Students = studentRows
        };
        
        var viewModel = new CourseDetailsViewModel
        {
            CourseId = courseId,
            CourseName = subject!.SubjetName!,
            CareerName = subject!.Career!.Name,
            CourseCode = subject!.SubjectId!,
            CreditCourse = subject!.Credits!,
            AcademicYear = subject.AcademiYear,
            TeacherName = "Dicxie Danuard Madrigal",
            ClassroomName = course.Classroom!.ClassroomName,
            TotalEnrolledStudents = enrolledStudents.Count(),
            MaxCapacity = course.Capacity,
            Year = course!.Year,
            EnrolledStudents = await enrolledStudents.ToListAsync(),
            attendanceHistories = attendanceHitory,
            Stats = statAttendance,
            TotalCourseHours = course.TotalHours,
            CompletedHours = attendances.Sum(a => a.TotalHours),
            MaleCount = male,
            FemaleCount = female,
            EthnicityStat = ethnicityStats,
            Gradebook = gradeBook,
            SyllabusItems = await _syllabusService.GetByCourseAsync(courseId),
            AssignmentLists = new AssignmentListViewModel
    {
        CourseId = course.IdCourse,
        CourseName = course?.Subject.SubjetName ?? "",
        Terms = terms.Select((t, index) => new TermItemListDto
        {
            TermId = t.TermId,
            Name = t.Name,
            Weight = t.WeightOnFinalGrade,
            IsActive = index == 0, // El primero estará abierto
            Assignments = t.Assignments.Select(a => new AssignmentItemDto
            {
                AssignmentId = a.AssignmentId,
                Title = a.Title,
                IsExam = a.IsExam,
                MaxPoints = a.MaxPoints,
                DueDate = a.DueDate,
                TotalStudents = totalStudents,
                GradedCount = a.Grades.Count(g => g?.Score != null) // Ajusta según tu lógica de nota
            }).ToList()
        }).ToList()
    }
        };
        return View(viewModel);
    }
    
    [HttpGet("/Course/{courseId}/Attendance")]
    public async Task<IActionResult> Imprint(string courseId)
    {
        var course = await _context.Courses
            .Include(c => c.Classroom)
            .Include(c => c.Subject)
            .ThenInclude(c => c!.Career)
            .Include(e => e.Enrollments)
                .ThenInclude(s => s.Student)
                .Where(c => c.IdCourse.ToString() == courseId)
                .FirstOrDefaultAsync();
        
         var header = new CourseAttendance
         {
            Career = course!.Subject!.Career!.Name,
            Regimen = "Semestral",
            Modality = "Presencial",
            AcademicYear = course.Subject!.AcademiYear,
            Year = course.Year,
            Semester = course.Subject!.Semester!,
            Shift = "Matutino",
            Section = course.Classroom!.ClassroomName!,
            Grade = "Pregrado",
            Campus = "URACCAN - Recinto Bilwi",
            TeacherName = "Dicxie Danuard Madrigal",
            CourseCode = course.Subject!.SubjectId!,
            CourseName = course.Subject!.SubjetName!
         };

         var body = await _context.Enrollments
            .Include(e => e.Student)
            .Where(e => e.IdCourse == course.IdCourse)
            .Select( e => new StudentInfo
            {
                Code = e.StudentId!,
                FullName = e.Student!.Name + " " + e.Student!.LastName,
                AttendanceStatus = "Presente",
                HoursAttended = 0,
                Note = ""
            }).ToListAsync();

        var document = new AttendanceDocument(header, body);
         string pdfFileName = $"Attendance_{course.Subject!.SubjectId}_Course_{course.IdCourse}.pdf";
        // RETORNAR EL ARCHIVO AL NAVEGADOR
        var stream = new MemoryStream();
        document.GeneratePdf(stream);
        stream.Position = 0;
        byte[] file =document.GeneratePdf();
        //return File(stream, "application/pdf", pdfFileName);
        return File(file, "application/pdf");
    }
    [HttpPost]

    public async Task<JsonResult> RegisterCourse([FromBody] CourseRegistrationDto model)
    {
        if(model == null) return Json(new {success = false, message ="Datos vacios backend"});
        if(!ModelState.IsValid)
        {
            return Json( new{success= false, message = "Modelo inválido"});
        }
        
        if(model.Schedules == null || !model.Schedules.Any())
            return Json( new{success= false, message = "Debe de asignar al menos una horario."});
        if(string.IsNullOrEmpty(model.ClassroomId)) return Json(new {success= false, message="Codigo de aula vacio"});
        Console.WriteLine($"el classroomId {model.ClassroomId}");
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            var subject = await _context.Subjects.FindAsync(model.SubjectId);
            if(subject == null) throw new Exception("Asignatura no encontrada");
            var newCourse = new Course
            {
                SubjectId = model.SubjectId ,
                Semester = model.Semester,
                Year = model.Year,
                Shift = model.Shift,
                Credits = model.Credits,
                TotalHours = model.TotalHours,
                HoursPerWeek = model.HoursPerWeek,
                ClassroomId = model.ClassroomId ,
                Capacity = model.Capacity,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                isActive = model.IsActive
            };
            _context.Courses.Add(newCourse);
            await _context.SaveChangesAsync(); // Genera el CourseId
            if(model.Schedules != null && model.Schedules.Any())
            {
                // Registra el horarios
            foreach(var item in model.Schedules)
            {
                var schedule = new Schedule
                {
                    IdCourse = newCourse.IdCourse,
                    ClassroomId = model.ClassroomId,
                    DayOfWeek = item.DayOfWeek,
                    StartTime = item.StartTime,
                    EndTime = item.EndTime
                };
                _context.Schedules.Add(schedule);
            }
            await _context.SaveChangesAsync();
            }
            transaction.Commit();
            return Json(new {success = true, message = "Curso programado Con éxito"});
        }
        catch(Exception ex)
        {
            // Falla la transacción, deshacemos todo los cambios
            transaction.Rollback();
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }
    [HttpPost]
    public async Task<JsonResult> Delete(string StudentId, int IdCourse)
    {
        if(StudentId== null) return Json(new {success= true, message= "Codigo incorrecto o vacio"});
        if (ModelState.IsValid)
        {
            Enrollment studentEnrollment = await _context.Enrollments
                .Where(s => s.StudentId == StudentId && s.IdCourse == IdCourse).FirstAsync();
            if(studentEnrollment == null) return Json( new{success= false, message = "El no Existe o fue eliminado"});
            try
            {
                _context.Enrollments.Remove(studentEnrollment);
            await _context.SaveChangesAsync();
            return Json( new{success = true, message= "se ha elimado correctamente"});
            }catch(Exception ex)
            {
                return Json(new {success = false, message= ex.ToString()});
            }
        }
        else
        {
            return Json(new {success= false, message ="erro al Eliminar"});
        }

    }
    [HttpGet("/Course/{idcourse}/ActivityScore")]
    public async Task<IActionResult> ActivityScore(int idcourse)
    {
        if(idcourse == 0) { return View("CourseNotFound"); }    
        var course = await _context.Courses
        .FindAsync(idcourse);
        if(course == null) { return View("CourseNotFound"); }
        var viewModel = new CourseActivityDetailsViewModel
        {
            CourseId = course.IdCourse,
            CourseName = (await _context.Subjects.FindAsync(course.SubjectId))!.SubjetName!,
            ActivityID = idcourse,
            ActivityName = "Parcial 1",
            ActivityType = "Examen",
            ActivityDate = DateTime.Now,
            MaxScore = 100,
            StudentScore = 85.5
        };
        return View("ActivityScore", viewModel);
    }
    [HttpPost]
    public async Task<IActionResult> EnrollStudents([FromBody] EnrollmentBatchDto model)
    {
        if(model.Students ==null  || !model.Students.Any())
        {
            return Json(new { success = false, message = "no se proporcionaron estudiantes para matricular"});
        }
        var nuevos = new List<string>();
        var duplicados = new List<string>();
        foreach(var studentCode in model.Students)
        {
            bool isEnrrolled = await  _context.Enrollments
                .AnyAsync(e => e.IdCourse == model.CourseId && e.StudentId == studentCode);
            if(isEnrrolled)
            {
                duplicados.Add(studentCode);
                continue;
            }
            
            var enrollment = new Enrollment
                {
                    IdCourse = model.CourseId,
                    StudentId = studentCode,
                    EnrollmentDate = DateTime.Now
                };
                _context.Enrollments.Add(enrollment);
                nuevos.Add(studentCode);
        }
        if(nuevos.Any())
        {
            await _context.SaveChangesAsync();
        }
        if(!nuevos.Any() && duplicados.Any())
        {
            return Json(new { success = false, allDuplicated = true,message = "Todos los estudiantes ya están matriculados", duplicados});
        }
        if(duplicados.Any())
        {
            return Json(new { success = true, 
            message = $"Se Matricularon ${nuevos.Count} estudiantes. duplicados ${duplicados.Count}", duplicados});
        }
        
        return Json(new { success = true, message = "Estudiantes matriculados con éxito"});
    }
    [HttpGet]
    public async Task<IActionResult> EnrolledStudentsRows(int courseId)
{
    var course = await _context.Courses
            .Include( c => c.Subject)
            .Include(c => c.Classroom)
            .Include(c => c.Enrollments)
                .ThenInclude(st => st.Student)
                .FirstOrDefaultAsync( m=> m.IdCourse == courseId);
        var subject = await _context.Subjects
            .FindAsync(course!.SubjectId);
        ViewBag.CourseId = courseId;
        if(course == null) {return View("CourseNotFound");}

        var enrolledStudents = _context.Enrollments
            .Include(e => e.Student).
            Where(e => e.IdCourse == courseId && e.Status ==EnrollmentStatus.Active)
            .Select( e => new EnrolledStudentDto
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId!,
                FullName = e.Student!.Name + " " + e.Student!.LastName,
                Email = e.Student!.Email!,
                Cellphone = e.Student!.Cellphone!,
                Sexo = e.Student!.Sexo,
                Ethnic = e.Student!.Ethnic! ,
            });

        var viewModel = new CourseDetailsViewModel
        {
            CourseId = courseId,
            CourseName = subject!.SubjetName!,
            CourseCode = subject!.SubjectId!,
            CreditCourse = subject!.Credits!,
            AcademicYear = subject.AcademiYear,
            TeacherName = "Dicxie Danuard Madrigal",
            ClassroomName = course.Classroom!.ClassroomName,
            TotalEnrolledStudents = enrolledStudents.Count(),
            MaxCapacity = course.Capacity,
            Year = course!.Year,
            EnrolledStudents = await enrolledStudents.ToListAsync(),
            Shift =course.Shift
        };

    return PartialView("_EnrolledStudentRows", viewModel);
}
    [HttpPost]
    public async Task<IActionResult> SaveAttendance([FromBody] AttendanceRegisterDto model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "Datos de asistencia inválidos: " + string.Join(", ", errors) });
        }
        try
        {
            // Validar que el curso exista
            var course = await _context.Courses.FindAsync(model.IdCourse);
            if (course == null)
            {
                return Json(new { success = false, message = "Curso no encontrado" });
            }
            // Validar que haya al menos un estudiante en la asistencia
            if (model.AttendanceDetail == null || !model.AttendanceDetail.Any())
            {
                return Json(new { success = false, message = "No se proporcionaron datos de asistencia de estudiantes" });
            }
            // Validar que las horas sean positivas
            if (model.Hours.TotalHours <= 0)
            {
                return Json(new { success = false, message = "Las horas de la sesión deben ser mayores que cero" });
            }
            // Crear el registro de asistencia
            var attendance = new Attendance
            {
                IdCourse = model.IdCourse,
                Date = model.Date,
                Topic = model.Topic,
                TotalHours = (decimal)model.Hours.TotalHours
            };
            // Agregar los detalles de asistencia de cada estudiante
            foreach (var detail in model.AttendanceDetail)
            {
                // Validar que el estudiante exista y esté matriculado en el curso
                var enrollmentExists = await _context.Enrollments.AnyAsync(s => s.EnrollmentId == detail.EnrollmentId);
                if (!enrollmentExists)
                {
                    return Json(new { success = false, message = $"Estudiante {detail.EnrollmentId} no encontrado" });
                }
                var isEnrolled = await _context.Enrollments.AnyAsync(e => e.IdCourse == model.IdCourse && e.EnrollmentId == detail.EnrollmentId);
                if (!isEnrolled)
                {
                    return Json(new { success = false, message = $"Estudiante {detail.EnrollmentId} no está matriculado en este curso" });}
                // Validar que el estado de asistencia sea válido
                if (!new[] { "P", "A", "T", "J" }.Contains(detail.Status))
                {
                    return Json(new { success = false, message = $"Estado de asistencia inválido: {detail.Status}" });
                }
                // Validar que las horas asistidas no excedan las horas totales de la sesión
                if (detail.HoursAttended > model.Hours.TotalHours)
                {
                    return Json(new { success = false, message = $"Horas asistidas no pueden exceder las horas totales de la sesión" });
                }
                var attendanceDetail = new AttendanceDetail
                {
                    EnrollmentId = detail.EnrollmentId,
                    Status = detail.Status,
                    HoursAttended = (decimal)detail.HoursAttended,
                    Observation = detail.Observation
                };
                attendance.AttendanceDetails.Add(attendanceDetail);
            }
            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Asistencia guardada correctamente" });
        }
        catch (Exception  ex) when(ex is DbUpdateException || ex is Exception)
        {
             // Verificamos si el mensaje de la excepción interna contiene el índice único
            if (ex.InnerException?.Message.Contains("uq_course_attendance_date") == true)
            {
                return Json(new {
                    success = false,
                    message = "Ya existe asistencia registrada para este curso en esa fecha."
                });
            }
            return Json(new { success = false, message = "Error al guardar la asistencia: " + ex.Message });
        }
    }
    [HttpGet]
    public async Task<JsonResult> CheckSessionExists(int courseId, DateOnly date)
    {
        // Verificamos si ya hay un registro de asistencia para ese curso en esa fecha exacta
        var exists = await _context.Attendances
            .AnyAsync(a => a.IdCourse == courseId && a.Date == date);

    return Json(new { exists });
    }
    /// <summary>
    /// Metodo que calcula el poncentaje de asistencias de la matricula basado en <b>horas asistencias</b> / <b>horas total de curso</b> 
    /// </summary>
    /// <param name="enrollmenId">Matricula</param>
    /// <returns> int: promedio de asistencia </returns>
    public async Task<int> AttendancePercetageAsync(int enrollmenId)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync( e => e.EnrollmentId == enrollmenId);
            if(enrollment == null) return 0;
            int courseId = enrollment.EnrollmentId;
            decimal totalCourseHours = await _context.Attendances
                .Where(a => a.IdCourse == courseId)
                .SumAsync( a => (decimal) a.TotalHours);
            if(totalCourseHours == 0) return 0;
            decimal attendanHours =  await _context.AttendancesDetails
                .Where( e =>
                    e.EnrollmentId == enrollmenId && (e.Status.Contains("P") || e.Status.Contains("T") ))
                    .SumAsync(d => d.HoursAttended);
            return (int) Math.Round((attendanHours * 100) /totalCourseHours);
    }
    
    // Lógica Central de Cálculo Matemático
    private  StudentRowDto CalculateStudentRow(Enrollment enrollment, List<AcademicTerm> terms)
    {
        var gradesDict = enrollment.Grades.ToDictionary(g => g.AssignmentId, g => g.Score);
        double grandTotal = 0;
        int activeTermsCount = terms.Count; // Divisor para promedio simple

        foreach (var term in terms)
        {
            // Sumar tareas de este corte
            double termSum = term.Assignments
                .Where(a => gradesDict.ContainsKey(a.AssignmentId))
                .Sum(a => gradesDict[a.AssignmentId]);
            
            grandTotal += termSum;
        }

        // FÓRMULA: Promedio Simple de Cortes (Sumatoria / Cantidad)
        double finalGrade = activeTermsCount > 0 ? (grandTotal / activeTermsCount) : 0;

        return new  StudentRowDto
        {
            EnrollmentId = enrollment.EnrollmentId,
            StudentFullName = $"{enrollment.Student.LastName}, {enrollment.Student.Name}",
            StudentId = enrollment.Student.Id,
            StudentInitials = $"{enrollment.Student.Name[0]}{enrollment.Student.LastName[0]}",
            Grades = gradesDict,
            FinalGrade = Math.Round(finalGrade, 2) // Redondeo a 2 decimales
        };
    }
}