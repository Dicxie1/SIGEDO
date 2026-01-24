using Asistencia.Data;
using Microsoft.AspNetCore.Mvc;
using Asistencia.Models;
using Asistencia.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Asistencia.Models.DTOs;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
namespace Asistencia.Controllers;

public class AttentionController : Controller
{
    readonly ApplicationDbContext _context;
    public AttentionController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpGet("Course/{courseid}/AttentionLog")]
    public async Task<IActionResult> AttentionLog(int courseid)
    {
        if(courseid == 0) return NotFound("Curson no encontrado");
        var course = await _context.Courses.FindAsync(courseid);
        // 1. Cargar Historial con relaciones
        var history = await _context.AttentionRecords
            .Include(r => r.Participants)
                .ThenInclude(p => p.Enrollment)
                .ThenInclude(p => p.Student)
            .Where(r => r.Course == course)
            .OrderByDescending(r => r.Date)
            .ToListAsync();
            if(course == null) BadRequest();
            var students = await _context.Enrollments
            .Where(e => e.Course == course)
            .OrderBy(e => e.Student!.LastName ?? "")
            .ThenBy(e => e.Student!.Name)
            .Select(e => new StudentSimpleDto
            {
                Id = e.EnrollmentId, // Usamos EnrollmentId, no StudentId
                Name = $"{e.Student!.LastName}, {e.Student.Name}"
            })
            .ToListAsync();
            var model = new CourseAttentionViewModel
        {
            CourseId = course.IdCourse,
            CourseName =  course?.Subject?.SubjetName ?? "",
            EnrolledStudents = students,
            
            // Estadísticas
            TotalIncidents = history.Count,
            BehavioralCount = history.Count(h => h.Category == AttentionCategory.Behavioral),
            AcademicCount = history.Count(h => h.Category == AttentionCategory.Academic),
            PendingCount = history.Count(h => h.Status == AttentionStatus.Pending),

            // Historial (DTO)
            History = history.Select(h => new AttentionRecordDto
            {
                Id = h.AttentionRecordId,
                Category = GetCategoryName(h.Category), // Método auxiliar para nombre bonito
                PriorityColor = GetPriorityColor(h.Priority),
                Observation = h.Observation,
                Date = h.Date,
                Status = h.Status,
                // Lógica de nombre inteligente:
                StudentName = h.Participants.Count == 1 
                    ? $"{h.Participants.First().Enrollment?.Student?.Name } {h.Participants?.First()?.Enrollment?.Student?.LastName}"
                    : $"Grupo ({h.Participants.Count} alumnos)"
            }).ToList()
        };
        return View("AttentionLog", model);
    }
    private string GetPriorityColor(AttentionPriority p) => p switch {
        AttentionPriority.High => "danger",
        AttentionPriority.Medium => "warning",
        _ => "success" // Low
    };
    private string GetCategoryName(AttentionCategory c) => c switch {
        AttentionCategory.Academic => "Académico",
        AttentionCategory.Behavioral => "Conducta",
        AttentionCategory.Attendance => "Asistencia",
        _ => "Salud/Otro"
    };
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> AddAttention([FromBody] CreateAttentionRecordDto model)
    {
        if(!ModelState.IsValid) return Json(new {success = false, message = "Datos incompletos o nulos"});
        if(model.EnrollmentIds == null || !model.EnrollmentIds.Any()) 
            return Json(new { success = false, message = "Debes seleccionar al menos un estudiante." });

        try
        {
            // PASO 1: Crear la Entidad Maestra (El Reporte)
        var record = new AttentionRecord
        {
            CourseId = model.CourseId,
            
            // Casteamos los enteros a sus Enums correspondientes
            Category = (AttentionCategory)model.Category,
            Priority = (AttentionPriority)model.Priority,
            
            Observation = model.Observation,
            CreatedByUserId = "Dicxie Madrigal",
            Date = DateTime.Now,
            Status = AttentionStatus.Pending
        };
        record.Participants = model.EnrollmentIds.Select(enrollmentId => new AttentionParticipant
        {
            EnrollmentId = enrollmentId
            // No es necesario setear AttentionRecordId, EF lo hace automático.
        }).ToList(); 
        // PASO 3: Guardar todo en una transacción atómica
        _context.AttentionRecords.Add(record);
        await _context.SaveChangesAsync();
        return Json(new {success=true,message="Guardado Correctamente" });
        }catch(Exception ex)
        {
            return Json(new {success = false, message =$"Error Interno: {ex}"});
        }
    }
    public async Task<JsonResult> GetDetails(int id )
    {
        var record = await _context.AttentionRecords
        .Include(r => r.Participants)
            .ThenInclude(p => p.Enrollment)
            .ThenInclude(e => e.Student)
        .FirstOrDefaultAsync(r => r.AttentionRecordId == id);
        if (record == null) 
            return Json(new { success = false, message = "Registro no encontrado." });
        
        return Json(new {
            success = true,
            data = new {
                id = record.AttentionRecordId,
                category = (int)record.Category,
                priority = (int)record.Priority,
                status = (int)record.Status,
                observation = record.Observation,
                // Enviamos solo los nombres para mostrarlos como "Badges" de solo lectura
                participants = record.Participants.Select(p => 
                    $"{p.Enrollment.Student.Name} {p.Enrollment.Student.LastName}"
                ).ToList()
            }
        });
    }
    // POST: Actualizar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromBody] UpdateAttentionRecordDto dto)
    {
        var record = await _context.AttentionRecords.FindAsync(dto.RecordId);
        if (record == null) return Json(new { success = false, message = "Registro no encontrado" });

        // Actualizar campos
        record.Category = (AttentionCategory)dto.Category;
        record.Priority = (AttentionPriority)dto.Priority;
        record.Status = (AttentionStatus)dto.Status;
        record.Observation = dto.Observation;
        
        // Opcional: Si se marca como resuelto, guardar fecha
        if (record.Status == AttentionStatus.Resolved && record.Date == DateTime.MinValue)
        {
            record.Date = DateTime.Now;
        }

        try
        {
            await _context.SaveChangesAsync();
            return Json(new { success = true, message="Registro actualizado correctamente" });
        }catch(Exception ex)
        {
            return Json(new {success= false, message= $"Error en la base de datos {ex} "});
        }
    }
    [HttpPost]
    [ValidateAntiForgeryToken] // Seguridad contra ataques CSRF
    public async Task<IActionResult> Resolve(int id)
    {
        try
        {
            var record = await _context.AttentionRecords.FindAsync(id);
            if (record == null) return Json(new {success= false, message="No se encontron Ninigun registro"});
            record.Status = AttentionStatus.Resolved;
            record.Date = DateTime.Now;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error interno: " + ex.Message });
        }
    }
    // GET: Datos para el modal (Correo + Info para plantilla)
    [HttpGet]
    public async Task<IActionResult> GetNotificationData(int id)
    {
        var record = await _context.AttentionRecords
            .Include(r => r.Participants)
                .ThenInclude(p => p.Enrollment)
                .ThenInclude(e => e.Student) // Asegúrate de tener el ParentEmail aquí
            .FirstOrDefaultAsync(r => r.AttentionRecordId == id);
        
        if (record == null) return Json(new { success = false, message = "Registro no encontrado" });

        // Tomamos el correo del primer estudiante (si es grupal, se podría iterar)
        var student = record.Participants.FirstOrDefault()?.Enrollment.Student;
        
        // Simulación de datos
        var info = new {
            studentName = $"{student.Name} {student.LastName}",
            parentEmail = student.Email, // O student.ParentEmail si tienes esa propiedad
            phoneNumber = student.Cellphone,
            category = record.Category.ToString(),
            priority = record.Priority.ToString(),
            observation = record.Observation,
            date = record.Date.ToString("dd/MM/yyyy")
        };

        return Json(new { success = true, info });
    }

    // POST: Enviar Correo
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendNotification([FromBody] NotificationDto dto)
    {
        // 1. Obtener registro y correo destino
        var record = await _context.AttentionRecords
            .Include(r => r.Participants).ThenInclude(p => p.Enrollment).ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(r => r.AttentionRecordId == dto.RecordId);

        if (record == null) return Json(new { success = false, message = "Error" });
        
        var emailTo = record.Participants.FirstOrDefault()?.Enrollment.Student.Email;

        if (string.IsNullOrEmpty(emailTo)) 
            return Json(new { success = false, message = "El estudiante no tiene correo registrado." });

        try 
        {
            // 2. LLAMADA AL SERVICIO DE EMAIL (Simulado aquí)
            // await _emailService.SendEmailAsync(emailTo, dto.Subject, dto.Message);
            
            // 3. ACTUALIZAR ESTADO EN BD
            //record.IsNotified = true;
            //record.NotificationDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        catch(Exception ex)
        {
            return Json(new { success = false, message = "Fallo al enviar correo: " + ex.Message });
        }
    }
    // Helper para limpiar el número
private string CleanPhoneNumber(string phone)
{
    if (string.IsNullOrEmpty(phone)) return "";
    // Eliminar guiones, espacios y paréntesis
    return new string(phone.Where(char.IsDigit).ToArray());
}
}