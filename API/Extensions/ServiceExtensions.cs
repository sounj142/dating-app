using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using API.Helpers;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using API.SignalR;
using System;

namespace API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DataContext>(options => 
            {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                string connStr;

                // Depending on if in development or production, use either Heroku-provided
                // connection string, or development connection string from env var.
                if (env == "Development")
                {
                    // Use connection string from file.
                    connStr = configuration.GetConnectionString("DefaultConnection");
                }
                else
                {
                    // Use connection string provided at runtime by Heroku.
                    var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

                    // Parse connection URL to connection string for Npgsql
                    connUrl = connUrl.Replace("postgres://", string.Empty);
                    var pgUserPass = connUrl.Split("@")[0];
                    var pgHostPortDb = connUrl.Split("@")[1];
                    var pgHostPort = pgHostPortDb.Split("/")[0];
                    var pgDb = pgHostPortDb.Split("/")[1];
                    var pgUser = pgUserPass.Split(":")[0];
                    var pgPass = pgUserPass.Split(":")[1];
                    var pgHost = pgHostPort.Split(":")[0];
                    var pgPort = pgHostPort.Split(":")[1];

                    connStr = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};SSL Mode=Require;TrustServerCertificate=True";
                }

                // Whether the connection string came from the local development configuration file
                // or from the environment variable from Heroku, use it to set up your DbContext.
                options.UseNpgsql(connStr);
            });
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            services.AddSingleton<IPresenceTracker, PresenceTracker>();
            services.AddScoped<LogUserActivityActionFilter>();
            services.AddScoped<ClientInformation>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));

            return services;
        }

        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddIdentityCore<AppUser>(options =>
                {
                    if (configuration.GetValue<bool>("AllowWeakPassword"))
                    {
                        options.Password.RequireDigit = false;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequireUppercase = false;
                        options.Password.RequireLowercase = false;
                        options.Password.RequiredUniqueChars = 0;
                        options.Password.RequiredLength = 4;
                    }
                    options.SignIn.RequireConfirmedAccount = false;
                })
                .AddRoles<AppRole>()
                .AddRoleManager<RoleManager<AppRole>>()
                .AddSignInManager<SignInManager<AppUser>>()
                .AddRoleValidator<RoleValidator<AppRole>>()
                .AddEntityFrameworkStores<DataContext>();


            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuerSigningKey = true,
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["TokenKey"])),
                       ValidateIssuer = false,
                       ValidateAudience = false
                   };

                   options.Events = new JwtBearerEvents
                   {
                       OnMessageReceived = context =>
                       {
                           if (context.Request.Path.StartsWithSegments("/hubs"))
                           {
                               var accessToken = context.Request.Query["access_token"];
                               if (!string.IsNullOrEmpty(accessToken))
                               {
                                   context.Token = accessToken;
                               }
                           }
                           return Task.CompletedTask;
                       }
                   };
               });

            services
                .AddAuthorization(options => 
                {
                    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                    options.AddPolicy("RequireModerateRole", policy => policy.RequireRole("Admin", "Moderator"));
                });

            return services;
        }
    }
}
