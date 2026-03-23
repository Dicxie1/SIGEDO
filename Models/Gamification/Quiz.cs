namespace Asistencia.Models.Gamification
{
    public class Quiz
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public List<Question>? Questions { get; set; }
    }
}
