// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using ArcaneVault.API.Models;
using ArcaneVault.API.Security;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ArcaneVault.API.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(
            ArcaneVaultDbContext context,
            IConfiguration configuration,
            ILogger logger)
        {
            if (!await context.Categories.AnyAsync())
            {
                context.Categories.AddRange(
                    new Category { CategoryCode = "CARDS", CategoryName = "Trading Cards" },
                    new Category { CategoryCode = "COINS", CategoryName = "Coins and Currency" },
                    new Category { CategoryCode = "FIGURES", CategoryName = "Figures and Statues" }
                );
            }

            await SeedDemoUserAsync(
                context,
                userName: "staff",
                email: "staff@arcanevault.local",
                roleId: 1,
                configuredPassword: configuration["SeedAccounts:StaffPassword"],
                logger);

            await SeedDemoUserAsync(
                context,
                userName: "collector",
                email: "collector@arcanevault.local",
                roleId: 2,
                configuredPassword: configuration["SeedAccounts:UserPassword"],
                logger);

            await context.SaveChangesAsync();
        }

        private static async Task SeedDemoUserAsync(
            ArcaneVaultDbContext context,
            string userName,
            string email,
            int roleId,
            string? configuredPassword,
            ILogger logger)
        {
            var user = await context.ArcaneVaultUsers
                .SingleOrDefaultAsync(existingUser => existingUser.UserName == userName);

            if (user is not null)
            {
                if (!string.IsNullOrWhiteSpace(configuredPassword))
                {
                    user.PasswordHash = PasswordService.HashPassword(configuredPassword);
                }

                return;
            }

            var password = configuredPassword;
            if (string.IsNullOrWhiteSpace(password))
            {
                password = CreateTemporaryPassword();
                logger.LogWarning(
                    "Generated temporary password for demo account {UserName}: {Password}",
                    userName,
                    password);
            }

            context.ArcaneVaultUsers.Add(new ArcaneVaultUser
            {
                UserName = userName,
                Email = email,
                PasswordHash = PasswordService.HashPassword(password),
                IsDeleted = false,
                RoleId = roleId
            });
        }

        private static string CreateTemporaryPassword()
        {
            return $"{Convert.ToHexString(RandomNumberGenerator.GetBytes(12))}aA1!";
        }
    }
}
