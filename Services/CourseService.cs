using Asistencia.Data;
using Asistencia.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Asistencia.Services
{
    public class CourseService
    {
        private readonly ApplicationDbContext _context;
        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Career>> GetCareerList()
        {
            return await _context.Careers.ToListAsync();
        }
        public async Task<SelectList> GetCarrerSelectList()
        {
            var carrer = await GetCareerList();
            return new SelectList(carrer, "CareerId", "Name");
        }

        public async Task<SelectList> GetAcademicPeriodSelectList()
        {
            var acadmicPeriods = await _context.AcademicPeriods.ToListAsync();
            return new SelectList(acadmicPeriods, "", "");
        }

        public async Task<bool> ActivePeriodAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1 find the period the user wants to activate
                var periodToActivate = await _context.AcademicPeriods.FindAsync(id);
                if (periodToActivate == null) return false;
                // 2. Find any currently active period
                var currentActivePeriod = await _context.AcademicPeriods
                        .Where(x => x.Status == EnumPeriodStatus.Active && x.AcademicPeriodId != id)
                        .ToListAsync();
                // 3. Set existing period to Closed
                foreach (var activateperiod in currentActivePeriod)
                {
                    activateperiod.Status = EnumPeriodStatus.Closed;
                }
                periodToActivate.Status = EnumPeriodStatus.Active;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return false;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
