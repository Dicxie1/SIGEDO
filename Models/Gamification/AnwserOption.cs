using System.ComponentModel.DataAnnotations;

namespace Asistencia.Models.Gamification
{
    public class AnwserOption
    {
        [Key]
        public int AnwerOptionId { get; set; }
        public int QuestionId { get; set; }
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
        public string ColorCode { get; set; }
    }
}
