using Microsoft.AspNetCore.Identity;
namespace Asistencia.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Campus { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
