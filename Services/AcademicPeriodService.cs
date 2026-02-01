using Asistencia.Data;
using Asistencia.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.XPath;

namespace Asistencia.Services
{
    public class AcademicPeriodService
    {
        private readonly ApplicationDbContext _context;
        public AcademicPeriodService(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
