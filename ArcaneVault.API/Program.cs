// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using ArcaneVault.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace ArcaneVault.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configuredConnection = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=ArcaneVault.db";
            var connectionString = configuredConnection.Contains("Data Source=ArcaneVault.db", StringComparison.OrdinalIgnoreCase)
                ? $"Data Source={Path.Combine(builder.Environment.ContentRootPath, "ArcaneVault.db")}"
                : configuredConnection;

            builder.Services.AddDbContext<ArcaneVaultDbContext>(options =>
                options.UseSqlite(connectionString));

            var jwtKey = builder.Configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                jwtKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                builder.Configuration["Jwt:Key"] = jwtKey;
            }

            if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
            {
                throw new InvalidOperationException("Jwt:Key must contain at least 32 bytes.");
            }
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ArcaneVaultDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                await context.Database.EnsureCreatedAsync();
                await DatabaseSeeder.SeedAsync(context, app.Configuration, logger);
            }

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
