using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudFolio.Models;

namespace StudFolio.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }

            if (!await context.Users.AnyAsync(u => u.Email == "support@studfolio.if.ua"))
            {
                var ownerUser = new User
                {
                    Lastname = "Система",
                    Firstname = "StudFolio",
                    Email = "support@studfolio.if.ua",
                    Avatar = "/favicon.svg",
                    Role = "owner",
                    CreationTime = DateTime.UtcNow
                };

                var hasher = new PasswordHasher<User>();
                ownerUser.Password = hasher.HashPassword(ownerUser, "StudFolioSecure2026!");

                context.Users.Add(ownerUser);
                await context.SaveChangesAsync();
            }
        }
    }
}