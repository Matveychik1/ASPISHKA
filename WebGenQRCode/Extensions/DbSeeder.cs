using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebGenQRCode.Constans;
using WebGenQRCode.Data;
using WebGenQRCode.Data.Entities.Identity;
using WebGenQRCode.Interfaces;
using WebGenQRCode.Models.Seeder;
using static System.Net.Mime.MediaTypeNames;

namespace WebGenQRCode.Extensions;

public class DbSeeder(IServiceProvider serviceProvider) : IDbSeeder
{
    //This - Розширення класу WebApplication
    //метод бцде запускатися у окремому потоці тому що він асинхроний
    public async Task SeedData()
    {
        using var scope = serviceProvider.CreateScope();
        //Цей об'єкт буде верта посилання на конткетс, який зараєстрвоано в Progran.cs
        var context = scope.ServiceProvider.GetRequiredService<AppQrDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<RoleEntity>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserEntity>>();
        var imageService = scope.ServiceProvider.GetRequiredService<IImageService>();

        context.Database.Migrate();

        if (!context.Roles.Any())
        {
            foreach (var roleName in Roles.ListRoles())
            {
                await roleManager.CreateAsync(new RoleEntity { Name = roleName });
            }
        }

        if (!context.Users.Any())
        {
            var curDir = Directory.GetCurrentDirectory();
            var jsonFile = Path.Combine(curDir, "Helpers", "JsonData", "Users.json");
            if (File.Exists(jsonFile))
            {
                var jsonData = await File.ReadAllTextAsync(jsonFile);
                try
                {
                    int i = 1;
                    //Список користувачів які ми отримали із файлу
                    var users = JsonSerializer.Deserialize<List<SeederUserModel>>(jsonData);
                    foreach(var user in users)
                    {
                        var entity = new UserEntity
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            UserName = user.Email
                        };

                        if (!string.IsNullOrEmpty(user.Email))
                        {
                            entity.Image = await imageService.SaveImageFromUrlAsync(user.Image);
                        }

                        var result = await userManager.CreateAsync(entity, user.Password);
                        if(result.Succeeded)
                        {
                            foreach (var role in user.Roles)
                                await userManager.AddToRoleAsync(entity, role);
                        }
                        Console.WriteLine($"Додано {i} користувачів");
                        i++;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Виклик помилки при Seed Useras", ex.Message); //показує помилку
                }
            }
        }
    }
}
