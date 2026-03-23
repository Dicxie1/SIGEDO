using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Asistencia.Data;
using Asistencia.Models.Gamification;
using System.Threading.Tasks;
using System.Linq;
namespace Asistencia.Hubs
{
    public class QuizHub : Hub
    {
        private readonly ApplicationDbContext _context;
        public QuizHub(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task JoinGame(string pin, string nikname)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, pin);
            await Clients.Group($"{pin}_Teacher").SendAsync($"PlayerJoin, {nikname}");
        }
        public async Task SendQuestion(string pin, object questionData)
        {
            await Clients.Group(pin).SendAsync("ReceiveQuestion", questionData);
        }
        public async Task SubmitAnwer(string pin, int anwerId, double timeElapsed)
        {
            await Clients.Group($"{pin}").SendAsync("AnserResived");
        }
        public async Task ShowLeaderboard(string pin, object leaderboardData)
        {
            await Clients.Group(pin).SendAsync("UpdateLeaderboarder");
        }
        public async Task JoinAsTeacher(string pin)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, pin);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{pin}_Teacher");
        }
        public async Task JoinAsStudent(string pin, string nickname)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, pin);
            var session = await _context.GameSessions
                .FirstOrDefaultAsync(s => s.PIN == pin && s.IsActive);
            if (session != null)
            {
                var players = new GamePlayer
                {
                    SessionId = session.GameSessionId,
                    Nickname = nickname,
                    ConnectionId = Context.ConnectionId,
                    TotalScore = 0
                };
                _context.GamePlayers.Add(players);
                await _context.SaveChangesAsync();
            }
            await Clients.Group($"{pin}_Teacher").SendAsync("PlayerJoined", nickname);
        }
        public async Task SendNextQuestion(string pin)
        {
            // 1. Buscamos la sesión activa y cargamos todo el cuestionario
            var session = await _context.GameSessions
                .Include(s => s.Quiz!)
                    .ThenInclude(q => q.Questions!)
                    .ThenInclude(q => q.AnserOptions) 
                .FirstOrDefaultAsync(s => s.PIN == pin && s.IsActive);

            if (session == null || session.Quiz == null || session.Quiz.Questions == null)
            {
                return; // Si algo es nulo, detenemos la ejecución por seguridad
            }
            // 2. Verificamos si ya se acabaron las preguntas
            if (session.CurrentQuestionIndex >= session.Quiz.Questions.Count)
            {
                var topPlayers = await _context.GamePlayers
                    .Where(p => p.SessionId == session.GameSessionId)
                    .OrderByDescending(p => p.TotalScore)
                    .Select(p => new
                    {
                        nickname = p.Nickname,
                        score = p.TotalScore
                    })
                    .ToListAsync();
                // Si ya no hay preguntas, avisamos que terminó el juego
                await Clients.Group($"{pin}_Teacher").SendAsync("ShowPodium", topPlayers);
                await Clients.Group(pin).SendAsync("GameOver");
                return;
            }

            // 3. Extraemos la pregunta actual usando el índice
            var currentQuestion = session.Quiz.Questions.ElementAt(session.CurrentQuestionIndex);

            // 4. Empaquetamos los datos que la pantalla gigante necesita mostrar
            // Usamos un objeto anónimo para no enviar datos sensibles (como cuál es la correcta) al JavaScript
            var questionDataForTeacher = new
            {
                text = currentQuestion.Text,
                timeLimit = currentQuestion.TimeLimitSecond,
                questionNumber = session.CurrentQuestionIndex + 1,
                totalQuestions = session.Quiz.Questions.Count,
                options = currentQuestion.AnserOptions.Select(o => new
                {
                    text = o.Text,
                    isCorrect = o.IsCorrect,
                    colorCode = o.ColorCode
                }).ToList()
            };
            session.CurrentQuestionIndex++;
            await _context.SaveChangesAsync();
            // 5. ¡BOMBARDEO DE MENSAJES SIGNALR!

            // A. Mandamos el texto de la pregunta SOLO a la pantalla del profesor
            await Clients.Group($"{pin}_Teacher").SendAsync("LoadQuestionUI", questionDataForTeacher);

            // B. Le decimos a TODOS los celulares del aula que muestren los 4 botones
            await Clients.Group(pin).SendAsync("ReceiveQuestion", session.CurrentQuestionIndex + 1);
            
        }
        public async Task SubmitAnswer(string pin, string nickname, int optionIndex, double timeElapsedSeconds)
        {
            var session = await _context.GameSessions
                .Include(s => s.Quiz)
                    .ThenInclude(q => q.Questions)
                    .ThenInclude(q => q.AnserOptions)
                .FirstOrDefaultAsync(s => s.PIN == pin && s.IsActive);

            if (session == null) return;

            // Obtenemos al jugador que presionó el botón usando su ID de conexión de SignalR
            var player = await _context.GamePlayers
                .FirstOrDefaultAsync(p => p.Nickname == nickname && p.SessionId == session.GameSessionId);

            if (player == null) return;

            int currentIndex = session.CurrentQuestionIndex - 1;
            if (currentIndex < 0 || currentIndex >= session.Quiz.Questions.Count) return;
            // Evaluamos la respuesta
            var currentQuestion = session.Quiz.Questions[currentIndex];

            // Asegurarnos de que el índice que mandó el celular (0 a 3) exista en la lista de opciones
            if (optionIndex >= 0 && optionIndex < currentQuestion.AnserOptions.Count)
            {
                var selectedOption = currentQuestion.AnserOptions[optionIndex];

                int pointsEarned = 0;

                if (selectedOption.IsCorrect)
                {
                    // LÓGICA DE GAMIFICACIÓN: Entre más rápido responde, más puntos gana
                    // Fórmula base: Puntos base * (Tiempo restante / Tiempo total)
                    double timeLeft = currentQuestion.TimeLimitSecond - timeElapsedSeconds;
                    if (timeLeft < 0) timeLeft = 0; // Por si hubo lag

                    // Calculamos el porcentaje de puntos que merece (mínimo se lleva la mitad si acertó)
                    double scoreMultiplier = 0.5 + (0.5 * (timeLeft / currentQuestion.TimeLimitSecond));
                    pointsEarned = (int)Math.Round(currentQuestion.Point * scoreMultiplier);

                    // Sumamos los puntos al perfil del estudiante
                    player.TotalScore += pointsEarned;
                    await _context.SaveChangesAsync();
                }

                // Le avisamos AL ESTUDIANTE en privado si acertó y cuántos puntos ganó
                await Clients.Caller.SendAsync("AnswerProcessed", selectedOption.IsCorrect, pointsEarned);
            }

            // Le avisamos AL PROFESOR que alguien respondió para que suba el contador en pantalla
            await Clients.Group($"{pin}_Teacher").SendAsync("AnswerReceived");
        }
        public async Task RevealStudentResults(string pin)
        {
            await Clients.Group(pin).SendAsync("ShowResultScreen");
        }


    }
}
