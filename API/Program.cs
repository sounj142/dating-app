using API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            InitializeStandardTimeZone(host);

            await SeedOrMigrationDatabase(host);

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<Startup>();
               });

        private static async Task SeedOrMigrationDatabase(IHost host)
        {
            using var scope = host.Services.CreateScope();
            try
            {
                using var dbContext = scope.ServiceProvider.GetService<DataContext>();

                // run migrations
                await dbContext.Database.MigrateAsync();

                // seed data if the environment is Development
                var environment = scope.ServiceProvider.GetService<IWebHostEnvironment>();
                if (environment.IsDevelopment())
                {
                    var seedData = new SeedData();
                    await seedData.SeedUsers(dbContext);
                }
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
                logger.LogError(ex, "An error occured during seeding/migration");
                
                throw;
            }
        }

        private static void InitializeStandardTimeZone(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var configuration = scope.ServiceProvider.GetService<IConfiguration>();
            SystemInfo.ReadStandardTimezoneFromAppseting(configuration);
        }
    }
}
