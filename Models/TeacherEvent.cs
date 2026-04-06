
namespace Asistencia.Models
{
    using System.ComponentModel.DataAnnotations;
    public class TeacherEvent
    {
        [Key]
        public int TeacherEventId { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public DateTime StartDateTime { get; set; }
        [Required]
        public DateTime EndDateTime { get; set; }
        public string ColorTheme { get; set; } = "#3788d8";
        

    }
}
