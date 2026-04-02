using Asistencia.Hubs;
using Asistencia.Models.Gamification;
using Asistencia.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Net;
using System.Net.Sockets;


namespace Asistencia.Controllers
{
    public class StudentPlayController : Controller
    {
        private readonly StudentPlayService _service;
        private readonly IHubContext<QuizHub> _hubContext;
        public StudentPlayController(StudentPlayService service, IHubContext<QuizHub> hubContext)
        {
            _service = service;
            _hubContext = hubContext;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Quiz");
        }
        public async Task<IActionResult> TeacherHost(string pin)
        {
            string hotsname = Dns.GetHostName();
            var address = Dns.GetHostAddresses(hotsname);
            var ipaddress = address .Where(i => i.AddressFamily == AddressFamily.InterNetwork)
            .FirstOrDefault()!.ToString();
            var port = HttpContext.Request.Host.Port;
            string path = Url.Action("StudentJoin", new { pin}) ?? string.Empty;
            
            string url = $"http://{ipaddress}:{5078}{path}";
            byte[] img = new Extensions.Utils().GenerarCodigoQR(url);
            ViewBag.url = url;
            ViewBag.QrCode = $"data:image/png;base64,{Convert.ToBase64String(img)}";
            GameSession gameSession = await _service.GetGameSessionAsync(pin);
            if (gameSession == null) return NotFound("La sala existe o ya terminó");
            return View(gameSession);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> JoinGame(string pin, string nickname)
        {
            // 1. Validaciones básicas
            if (string.IsNullOrWhiteSpace(pin) || string.IsNullOrWhiteSpace(nickname))
            {
                TempData["Error"] = "Debes ingresar el PIN y tu nombre.";
                return RedirectToAction("StudentJoin", "StudentPlay");
            }

            // 2. Usar el servicio para verificar si la sala existe y está activa
            var session = await _service.GetActiveSessionByPinAsync(pin);

            if (session == null)
            {
                // Si el PIN está mal o el profe ya cerró la sala
                TempData["Error"] = "No reconocemos ese PIN. Verifica la pantalla.";
                return RedirectToAction("StudentJoin");
            }
            var questions = session.Quiz?.Questions?.Count; 
            if(session.CurrentQuestionIndex >= questions -1)
            {
                TempData["Error"] = "La partida está en su fase final o ya terminó. ¡Llegaste un poco tarde!";
                return RedirectToAction("StudentJoin");
            }
            // 3. Todo está correcto. Redirigir a la vista donde se juega (StudentPlay)
            return RedirectToAction("StudentPlay", new { pin = pin, nickname = nickname });
        }

        [HttpGet]
        [AllowAnonymous]
        [HttpGet]
        // Permite que los estudiantes jueguen sin iniciar sesión en el sistema
        public async Task<IActionResult> StudentPlay(string pin, string nickname)
        {
            // 1. Doble verificación: Evitar que alguien entre escribiendo la URL directamente
            if (string.IsNullOrWhiteSpace(pin) || string.IsNullOrWhiteSpace(nickname))
            {
                TempData["Error"] = "Por favor, ingresa desde la pantalla principal.";
                return RedirectToAction("StudentJoin");
            }

            // 2. Verificar que la sala sigue activa (por si el profesor la cerró de golpe)
            var session = await _service.GetActiveSessionByPinAsync(pin);

            if (session == null)
            {
                TempData["Error"] = "La sala ya fue cerrada por el profesor.";
                return RedirectToAction("StudentJoin");
            }

            // 3. Empaquetar los datos para la Vista
            // Usamos ViewBag porque solo necesitamos pasar estas dos cadenas de texto 
            // para que JavaScript las lea y se conecte al Hub de SignalR.
            ViewBag.Pin = pin;
            ViewBag.Nickname = nickname;

            // 4. Mostrar la interfaz de los botones
            return View();
        }
        [AllowAnonymous]
        public IActionResult StudentJoin(string pin)
        {
            ViewBag.Pin = string.IsNullOrEmpty(pin) ? "" : pin;
            return View();
        }
        public async Task<IActionResult> EndGameSession(string pin)
        {
            // 1. Delegamos el trabajo pesado al servicio
            bool result = await _service.EndGameSessionAsync(pin);

            // 2. Preparamos el mensaje de retroalimentación
            if (result)
            {
                await _hubContext.Clients.Group(pin).SendAsync("GameEndedByHost");
                TempData["Success"] = "¡La partida ha finalizado y la sesión se cerró correctamente!";
            }
            else
            {
                TempData["Error"] = "No se pudo cerrar la sesión o el PIN ya estaba inactivo.";
            }

            // 3. Redirigimos al menú principal
            return RedirectToAction(nameof(Index));
        }

    }

}
