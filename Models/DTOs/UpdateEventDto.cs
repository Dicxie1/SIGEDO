using System.ComponentModel.DataAnnotations;
namespace Asistencia.Models.DTOs
{
    public class UpdateEventDto
    {
        [Required]
        public int TeacherEventId { get; set; } // Viene del input oculto
        [Required(ErrorMessage = "El título es obligatorio")]
        public string Title { get; set; } = string.Empty;
        [Required(ErrorMessage = "Debe seleccionar un profesor")]
        public int TeacherID { get; set; }
        public string ColorTheme { get; set; } = string.Empty;
        public bool IsAllDay { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Propiedades para la recurrencia (por si decide hacerlo recurrente al editar)
        public bool IsRecurring { get; set; }
        public List<DayOfWeek> RecurringDays { get; set; } = new List<DayOfWeek>();
        public DateTime? RecurrenceEndDate { get; set; }
    }
}
