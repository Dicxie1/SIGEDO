using Asistencia.Data;
using Asistencia.Models;
using Asistencia.Models.Gamification;
using Microsoft.EntityFrameworkCore;
namespace Asistencia.Services
{
    public class StudentPlayService
    {
        private readonly ApplicationDbContext _context;
        public StudentPlayService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<GameSession> GetGameSessionAsync(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin)) throw new ArgumentException("Pin vacio");
            GameSession? gameSession = await _context.GameSessions
                .Include(s => s.Quiz)
                .Include(s => s.Players)
                .FirstOrDefaultAsync(s => s.PIN == pin && s.IsActive);
            return gameSession;
        }
        public async Task<GameSession> GetActiveSessionByPinAsync(string pin)
        {
            if (string.IsNullOrEmpty(pin)) return null;
            return await _context.GameSessions
               .Include(s => s.Quiz)
               .Include(s => s.Players)
               .FirstOrDefaultAsync(s => s.PIN == pin && s.IsActive);
        }
    }
}
