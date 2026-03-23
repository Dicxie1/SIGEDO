using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace Asistencia.Models.Gamification
{
    public class Question
    {
        public int QuestionId { get; set; }
        public int QuizId { get; set; }
        public string Text { get; set; }
        public int TimeLimitSecond { get; set; }
        public int Point { get; set; }
        public List<AnwserOption>? AnserOptions { get; set; } = new List<AnwserOption>();
        [NotMapped]
        public int CorrectAnswerIndex {  get; set; }
    }
}
