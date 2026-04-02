using Asistencia.Models.Gamification;
using Microsoft.AspNetCore.Mvc;
using Asistencia.Services;
namespace Asistencia.Controllers
{
    public class GamificationController : Controller
    {
        private readonly GamificationService _service;
        public GamificationController( GamificationService service) {
            _service = service;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSession(int quizId, bool randomizeQuestions)
        {
            try
            {
                string pin = await _service.CreateSessionAsync(quizId, randomizeQuestions);
                return RedirectToAction("TeacherHost", "StudentPlay", new {pin = pin});
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocurrió un error al crear la sala.");
            }
        }
        [HttpGet]
        public async Task<IActionResult> TeacherHost(string pin)
        {
            var session = await _service.GetActiveSessionByPinAsync(pin);
            if (session == null) return NotFound("La sala no existe o ya ha sido cerrada.");
            return View(session);
        }
        [HttpPost]
        public async Task<IActionResult> EndGameSession(string pin)
        {
            var session = await _service.EndGameSessionAsync(pin);
            return RedirectToAction(nameof(Index));
        }
    }
}
