namespace Asistencia.Models.ViewModels
{
    public class CourseListItemViewModel
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int CountEnrrolle { get; set; }
        public double ProgressPercentage { get; set; }

    }
}
