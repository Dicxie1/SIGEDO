using System.ComponentModel.DataAnnotations;

namespace Asistencia.Models
{
    public class Teacher
    {

        [Key]
        public int TeacherID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Sex Sex { get; set; }

        public List<Schedule> Schedules { get; set; } = new List<Schedule>();
        public List<TeacherEvent> TeacherEvents { get; set; } = new List<TeacherEvent>();
        public List<Course> Courses { get; set; } = new List<Course>();
    }   
}
