using System.ComponentModel.DataAnnotations;
namespace Asistencia.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El usuario intitucional es requerido")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "La Contraseña es requerido")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Display(Name ="Recordame")]
        public bool RememberMe { get; set; } 
        public string? ReturnUrl { get; set; } = string.Empty;

    }
}
