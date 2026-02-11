using Microsoft.AspNetCore.Identity;
using Asistencia.Models; // Asegúrate de usar tu namespace correcto

namespace Asistencia.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            // Obtener los servicios necesarios
            var userManager = service.GetService<UserManager<ApplicationUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();

            // 1. Crear Roles si no existen
            await CreateRoleAsync(roleManager, "Admin");
            await CreateRoleAsync(roleManager, "Docente");
            await CreateRoleAsync(roleManager, "Estudiante");

            // 2. Crear el Usuario Admin por defecto
            var adminEmail = "admin@uraccan.edu.ni";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = "admin", // El usuario para el login
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Administrador del Sistema", // Propiedad personalizada tuya
                    Campus = "Bilwi", // Asumiendo que 1 es Bilwi o Sede Central
                    IsActive = true
                };

                // CRÍTICO: La contraseña debe cumplir tus reglas (Mayúscula, minúscula, número, símbolo)
                var result = await userManager.CreateAsync(newAdmin, "Uraccan.2026!");

                if (result.Succeeded)
                {
                    // Asignarle el rol de Admin
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }

        private static async Task CreateRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}