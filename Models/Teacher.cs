namespace Asistencia.Models
{
    public class Teacher
    {
        public int TeacherID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Sex Sex { get; set; }
    }   
}
