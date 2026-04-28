using Microsoft.AspNetCore.Identity;
using VasosInteligentes.Models;

namespace VasosInteligentes.Seeds;

public class IdentitySeeds
{
    public static async Task SeedRolesAndUser(
        IServiceProvider serviceProvider, string defaultPassword)
    {
        // Criação das roles (Administrador e Usuario)
        var roleManager = serviceProvider.GetRequiredService<RoleManager<Models.ApplicationRole>>();
        string[] roleNames = { "Administrador", "Usuario" };

        foreach (var roleName in roleNames)
        {
            // Verificar se já foi criado
            if (await roleManager.FindByNameAsync(roleName) == null)
            {
                // Se não encontrou será inserido
                var result = await roleManager.CreateAsync(new ApplicationRole { Name = roleName });

                if (result.Succeeded)
                {
                    Console.WriteLine($"Seed: Role {roleName} foi criada.");
                }
                else { return; }
            }
        }

        // Criar os usuários
        var userManager = serviceProvider.GetRequiredService<UserManager<Models.ApplicationUser>>();

        // Criar o Administrador
        if (await userManager.FindByEmailAsync("admin@admin.com") == null)
        {
            // Se não encontrar será inserido
            var adminUser = new ApplicationUser
            {
                UserName = "admin@admin.com",
                Email = "admin@admin.com",
                EmailConfirmed = true,
            };

            var resultAdmin = await userManager.CreateAsync(adminUser, defaultPassword);

            if (resultAdmin.Succeeded)
            {
                Console.WriteLine("Seed: Administrador foi criado.");
                await userManager.AddToRoleAsync(adminUser, "Administrador");
            }
            else { return; }
        }

        // Criar o Usuario comum
        if (await userManager.FindByEmailAsync("teste@usuario.com") == null)
        {
            // Se não encontrar será inserido
            var user = new ApplicationUser
            {
                UserName = "teste@usuario.com",
                Email = "teste@usuario.com",
                EmailConfirmed = true,
            };

            var resultUser = await userManager.CreateAsync(user, "Teste@123");

            if (resultUser.Succeeded)
            {
                Console.WriteLine("Seed: Usuário comum foi criado.");
                await userManager.AddToRoleAsync(user, "Usuario");
            }
            else { return; }
        }
    }
}
