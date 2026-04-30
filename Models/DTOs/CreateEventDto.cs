using System.ComponentModel.DataAnnotations;
namespace Asistencia.Models.DTOs;
public class CreateEventDto
{
    [Required(ErrorMessage ="Es Necesario el TeacherID")]
    public int TeacherID {get; set;}
    [Required(ErrorMessage = "El título de la actividad es obligatorio.")]
    [StringLength(100, ErrorMessage = "El título no puede exceder los 100 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes seleccionar una categoría.")]
    public string ColorTheme { get; set; } = string.Empty;

    public bool IsAllDay { get; set; }

    [Required(ErrorMessage = "La fecha y hora de inicio son obligatorias.")]
    public DateTime StartDateTime { get; set; }

    [Required(ErrorMessage = "La fecha y hora de fin son obligatorias.")]
    public DateTime EndDateTime { get; set; }

        // El signo de interrogación (?) indica que es un campo opcional (Nullable)
    [StringLength(200, ErrorMessage = "La ubicación es demasiado larga.")]
    public string? Location { get; set; }

    [StringLength(1000, ErrorMessage = "La descripción no puede exceder los 1000 caracteres.")]
    public string? Description { get; set; }
    public bool IsRecurring { get; set; }
    public List<DayOfWeek> RecurringDays { get; set; } = new List<DayOfWeek>();
    public DateTime? RecurrenceEndDate { get; set; }
}