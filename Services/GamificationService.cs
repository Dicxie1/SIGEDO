using Asistencia.Data;
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
            } while(isPinInUse);
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
            return  await _context.Quizzes
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
    }

}
