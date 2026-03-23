using Asistencia.Models.Gamification;
using Asistencia.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Sockets;


namespace Asistencia.Controllers
{
    public class StudentPlayController : Controller
    {
        private readonly StudentPlayService _service;
        public StudentPlayController(StudentPlayService service)
        {
            _service = service;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> TeacherHost(string pin)
        {
            string hotsname = Dns.GetHostName();
            var address = Dns.GetHostAddresses(hotsname);
            var ipaddress = address .Where(i => i.AddressFamily == AddressFamily.InterNetwork)
            .FirstOrDefault()!.ToString();
            var port = HttpContext.Request.Host.Port;
            string path = Url.Action("StudentPlay", new { pin}) ?? string.Empty;
            
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

            // 3. Todo está correcto. Redirigir a la vista donde se juega (StudentPlay)
            // Pasamos el PIN y el Nickname por la URL o usando TempData
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
        public IActionResult StudentJoin()
        {
            return View();
        }
    }

}
