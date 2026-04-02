using System.Reflection.Metadata;

namespace Asistencia.Models.Gamification
{
    public class GameSession
    {
        public int GameSessionId { get; set; }
        public int QuizId { get; set; }
        public string PIN { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int CurrentQuestionIndex { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
        public DateTime CurrentQuestionStartTime { get; set; }
        public bool RandomizeQuestions { get; set; }   
        public List<GamePlayer> Players { get; set; } = new List<GamePlayer>();
        public Quiz Quiz { get; set; } 
    }
}
