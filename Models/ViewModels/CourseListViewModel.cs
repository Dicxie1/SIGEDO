namespace Asistencia.Models.ViewModels;

class CourseListViewModel
{
    public ICollection<Course>? ActiveCourses {get; set;}
    public ICollection<Course>? Courses{get; set;}
    public ICollection<Classroom>? Classrooms {get; set;}
}