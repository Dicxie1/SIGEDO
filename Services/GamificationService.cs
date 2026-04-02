using Asistencia.Data;
using Asistencia.Models.DTOs;
using Asistencia.Models.Gamification;
using Microsoft.EntityFrameworkCore;
namespace Asistencia.Services
{
    public class GamificationService
    {
        public readonly ApplicationDbContext _context;
        public GamificationService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<string> CreateSessionAsync(int quizId, bool randomizeQuestion)
        {
            var quizExist = await _context.Quizzes.AnyAsync(q => q.QuizId == quizId);
            if (!quizExist)
            {
                throw new ArgumentException("El Cuestionario seleccionado no Existe");
            }
            string generatedPin;
            bool isPinInUse;
            do
            {
                generatedPin = Random.Shared.Next(100000, 999999).ToString();
                isPinInUse = await _context.GameSessions.AnyAsync(s => s.PIN == generatedPin);
            } while (isPinInUse);
            var newSession = new GameSession
            {
                QuizId = quizId,
                PIN = generatedPin,
                IsActive = true,
                CurrentQuestionIndex = 0,
                CreatedAt = DateTime.Now,
                RandomizeQuestions = randomizeQuestion
            };
            _context.GameSessions.Add(newSession);
            await _context.SaveChangesAsync();
            return generatedPin;
        }
        public async Task<GameSession> GetActiveSessionByPinAsync(string pin)
        {
            if (string.IsNullOrEmpty(pin)) return null;
            return await _context.GameSessions
               .Include(s => s.Quiz)
               .Include(s => s.Players)
               .FirstOrDefaultAsync(s => s.PIN == pin && s.IsActive);
        }
        public async Task<List<Quiz>> GetQuizAsync()
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                .ToListAsync();
        }
        public async void SaveQuizAsync(Quiz quiz)
        {
            _context.Quizzes.Add(quiz);
            _context.SaveChanges();
        }
        public async Task<Quiz> GetQuizAsync(int id)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.AnserOptions)
                .FirstOrDefaultAsync(q => q.QuizId == id);
        }
        public void RemoveQuestion(Question question)
        {
            _context.Questions.Remove(question);
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task<PaginatedQuizResult> GetQuizzesPaginatedAsync(string search, int page, int pageSize)
        {
            var query = _context.Quizzes.Include(q => q.Questions).AsQueryable();
            // 1. Filtro
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(q => q.Title.ToLower().Contains(search.ToLower()));
            }
            int totalItems = await query.CountAsync();
            bool hasMore = (page * pageSize) < totalItems;

            // 3. Paginación y Mapeo al DTO
            var quizzes = await query
                .OrderByDescending(q => q.QuizId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(q => new QuizDto
                {
                    QuizId = q.QuizId,
                    Title = q.Title,
                    QuestionCount = q.Questions.Count
                })
                .ToListAsync();
            return new PaginatedQuizResult
            {
                Quizzes = quizzes,
                HasMore = hasMore
            };
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

            return false; // No se encontró o ya estaba inactiva        }
        }
    }

}
