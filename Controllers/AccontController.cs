using Asistencia.Models;
using Asistencia.Models.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace Asistencia.Controllers
{
    public class AccontController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccontController> _logger;
        public AccontController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccontController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }
        [HttpGet]
        [AllowAnonymous] // Importante: Permitir acceso a no autenticados
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken] // Protección contra ataques CSRF
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return Json(new {success= false, message = "Datos vacio" });
            }

            // 1. Buscar usuario primero para verificar si existe o está activo
            var user = await _userManager.FindByNameAsync(model.Email);

            if (user != null && !user.IsActive)
            {
                ViewData["Error"] = "Su cuenta ha sido desactivada. Contacte a RRHH.";
                return Json(new { success = false, message = "Su cuenta ha sido desactivada. Contacte a RRHH." });
            }

            // 2. Intentar Login (PasswordSignInAsync maneja el hashing automáticamente)
            // El parámetro 'lockoutOnFailure: true' activa el bloqueo tras 5 intentos fallidos
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Usuario {model.Email} inició sesión.");

                // Redirigir de forma segura (evita Open Redirect Vulnerability)
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Json( new {suceess = false,  returnUrl });
                }
                else
                {
                    return Json(new {success = true, message = "acceder", url = "/Home/" });
                }
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning($"Cuenta de usuario {model.Email} bloqueada por intentos fallidos.");
                ViewData["Error"] = "Cuenta bloqueada temporalmente por seguridad. Intente en 15 minutos.";
                return Json(new {success= false, message = $"Cuenta de usuario {model.Email} bloqueada por intentos fallidos." });
            }
            else
            {
                // Login fallido genérico (No decir si falló usuario o contraseña por seguridad)
                ViewData["Error"] = "Credenciales institucionales incorrectas.";
                return Json(new { success = false, message = "Credenciales institucionales incorrectas." });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuario cerró sesión.");
            return RedirectToAction("Login", "Account");
        }
    }
}
