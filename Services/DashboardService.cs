using Asistencia.Data;
using Asistencia.Models;
using Microsoft.EntityFrameworkCore;

namespace Asistencia.Services
{
    public class DashboardService
    {
        private readonly ApplicationDbContext _context;
        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<DashboardViewModel> GetDashboardViewModelAsync()
        {
            var courses = await GetActiveCoursesAsync();
            int countActiveCourse = courses.Count();
            int countStudent = _context.Students.Count();
            return new DashboardViewModel
            {
                Courses = courses,
                CountCourseActive = countActiveCourse,
                studentCount = countStudent
            };
        }
        public async Task<List<Course>> GetActiveCoursesAsync()
        {
            return  _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Enrollments)
                .Where(c => c.isActive == true).ToList();
        }

    }
}
