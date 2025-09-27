
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace Practica2.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            
            string[] roleNames = { "Broker", "Administrador" };
            
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Opcional: Crear un usuario Broker de prueba para iniciar sesión
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            string brokerEmail = "broker@inmobiliaria.com";
            string brokerPassword = "Contraseña123"; 

            if (await userManager.FindByEmailAsync(brokerEmail) == null)
            {
                var brokerUser = new IdentityUser { UserName = brokerEmail, Email = brokerEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(brokerUser, brokerPassword);
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(brokerUser, "Broker");
                }
            }
        }
    }
}