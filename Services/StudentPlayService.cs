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
               .FirstOrDefaultAsync(s => s.PIN == pin && s.IsActive == true);
        }
        public async Task<bool> EndGameSessionAsync(string pin)
        {
            // Validación rápida para no golpear la base de datos innecesariamente
            if (string.IsNullOrEmpty(pin)) return false;

            // Buscamos la sesión activa
            var session = await _context.GameSessions
                .FirstOrDefaultAsync(s => s.PIN == pin && s.IsActive);

            // Si existe, la apagamos
            if (session != null)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }

            return false; // No se encontró o ya estaba inactiva
        }
    }
}
