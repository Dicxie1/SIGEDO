namespace Asistencia.Models.DTOs
{
    public class PaginatedQuizResult
    {
        public IEnumerable<QuizDto> Quizzes { get; set; } = new List<QuizDto>();
        public bool HasMore { get; set; }
    }
    public class QuizDto
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
    }
}
