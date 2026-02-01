using Asistencia.Data;
using Asistencia.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Asistencia.Services
{
    public class ClassroomService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="context"></param>
        public ClassroomService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Classroom>> GetClassroomsAsync()
        {
            return await _context.Classrooms.ToListAsync();
        }
        public async Task<List<SelectListItem>> GetClassroomsList()
        {
            var classroom = await _context.Classrooms
                    .AsNoTracking()
                    .OrderBy(x=> x.Location)
                    .ThenBy(x => x.ClassroomName)
                    .Select(x => new { x.ClassroomId, x.ClassroomName, x.Location })
                    .ToListAsync();
            // group class by location
            var groupList = classroom
                .GroupBy(a => a.Location)
                .Select(g => new SelectListGroup { Name = g.Key })
                .ToList();
            var selectList = new List<SelectListItem>();
            foreach(var group in groupList)
            {
                foreach(var item in classroom.Where(x=> x.Location == group.Name))
                {
                    selectList.Add(new SelectListItem
                    {
                        Value = item.ClassroomId?.ToString(),
                        Text = item.ClassroomName,
                        
                    });
                }
            }
            return selectList;
        }
    }

    class SelectListGroup
    {
        public string Name { get; set; } = string.Empty;
    }
}
