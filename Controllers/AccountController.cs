using Asistencia.Models;
using Asistencia.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Asistencia.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Homne");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View("Login");
        }
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl )
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                ViewData["Error"] = "Modelo Invalid";
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            _logger.LogInformation($" el CORREO: {user.Email}, Usuarios: {user.UserName} contraseña: {model.Password}");
            if(user != null && !user.IsActive )
            {
                ViewData["Error"] = "Su cuenta ha sido desactivada. Contacte a RRHH.";
                
                return View(model);
            }
            var result = await _signInManager.PasswordSignInAsync(user!.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);
            if(result.Succeeded)
            {
                _logger.LogInformation($"Usuario {model.Email } inicio Sessión");
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning($"Cuenta de usuario {model.Email} bloqueada por intentos fallidos.");
                ViewData["Error"] = "Cuenta bloqueada temporalmente por seguridad. Intente en 15 minutos.";
                return View(model);
            }
            else
            {
                // Login fallido genérico (No decir si falló usuario o contraseña por seguridad)
                ViewData["Error"] = "Credenciales institucionales incorrectas.";
                return View(model);
            }
        }
        public IActionResult AccessDenied()
        {

            return RedirectToAction("Index", "Home");
        }
        public IActionResult Profile()
        {
            return View();
        }
        [HttpPost] // IMPORTANTE: Solo permitir POST para evitar ataques CSRF
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // 1. Borrar la cookie de autenticación
            await _signInManager.SignOutAsync();

            // 2. Log para auditoría
            _logger.LogInformation("El usuario cerró sesión.");

            // 3. Redirigir. 
            // Opción A: Ir al Home
            // return RedirectToAction("Index", "Home");

            // Opción B (Recomendada para sistemas): Ir de nuevo al Login
            return RedirectToAction("Login", "Account");
        }
    }
}
