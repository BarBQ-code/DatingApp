using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public static class Seed
    {
        public static async Task SeedUsers(ApplicationDbContext context)
        {
            if (await context.Users.AnyAsync())
                return;

            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            foreach (var user in users)
            {
                using var hmac = new HMACSHA512();

                user.UserName = user.UserName.ToUpper();
                user.Password = hmac.ComputeHash(Encoding.UTF8.GetBytes("password"));
                user.PasswordSalt = hmac.Key;

                context.Users.Add(user);
            }
            await context.SaveChangesAsync();
        }
    }
}