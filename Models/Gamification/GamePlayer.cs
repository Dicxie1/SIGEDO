namespace Asistencia.Models.Gamification
{
    public class GamePlayer
    {
        public int GamePlayerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
        public int SessionId { get; set; }
        public string Nickname { get; set; } = string.Empty;
        public double TotalScore { get; set; } 

    }
}
