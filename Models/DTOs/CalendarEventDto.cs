namespace Asistencia.Models.DTOs
{
    public class CalendarEventDto
    {
        public string id { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public string start { get; set; } = string.Empty;
        public string end { get; set; } = string.Empty;
        public string color { get; set; } = "#3788d8";
        public bool allDay { get; set; }

        // Usamos object aquí para mantener la flexibilidad de los datos extra
        public object? extendedProps { get; set; }
    }
}
