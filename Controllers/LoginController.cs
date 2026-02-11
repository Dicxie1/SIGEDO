using Microsoft.AspNetCore.Mvc;

namespace Asistencia.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
