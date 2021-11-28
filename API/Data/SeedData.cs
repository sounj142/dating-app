using API.Entities;
using API.Extensions;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace API.Data
{
    public class SeedData
    {
        public async Task SeedUsers(DataContext dataContext)
        {
            if (await dataContext.Users.AnyAsync()) return;

            const string PASSWORD = "password";

            var usersDataString = await File.ReadAllTextAsync("Data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<AppUser[]>(usersDataString);

            foreach(var user in users)
            {
                using var hmac = new HMACSHA512();
                user.PasswordHash = hmac.ComputePasswordHash(PASSWORD);
                user.PasswordSalt = hmac.Key;

                dataContext.Users.Add(user);
            }

            await dataContext.SaveChangesAsync();
        }
    }
}
