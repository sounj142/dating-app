using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace API.Data
{
    public class SeedData
    {
        public async Task SeedUsers(UserManager<AppUser> userManager, bool isDevelopment)
        {
            if (await userManager.Users.AnyAsync()) return;

            const string PASSWORD = "password";

            // seed admin account
            var admin = new AppUser
            {
                UserName = "Admin",
                Gender = "male",
                KnownAs = "Administrator",
                DateOfBirth = new DateTime(1990, 1, 1),
                City = "Ho Chi Minh",
                Country = "Vietnam"
            };

            await userManager.CreateAsync(admin, PASSWORD);
            await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });

            // only seed these test data if the environment is Development
            if (isDevelopment)
            {
                var usersDataString = await File.ReadAllTextAsync("Data/UserSeedData.json");
                var users = JsonSerializer.Deserialize<AppUser[]>(usersDataString);

                foreach (var user in users)
                {
                    await userManager.CreateAsync(user, PASSWORD);
                    await userManager.AddToRoleAsync(user, "Member");
                }
            }
        }

        public async Task SeedRoles(RoleManager<AppRole> roleManager)
        {
            if (await roleManager.Roles.AnyAsync()) return;

            var roles = new AppRole[]
            {
                new AppRole { Name = "Member" },
                new AppRole { Name = "Admin" },
                new AppRole { Name = "Moderator"}
            };

            foreach (var role in roles) await roleManager.CreateAsync(role);
        }
    }
}
