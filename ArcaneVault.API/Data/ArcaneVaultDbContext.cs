// Name: [Your Name] | Admin No: [Your Admin No] | Tutorial Group: [Your Group]

using ArcaneVault.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault.API.Data
{
    public class ArcaneVaultDbContext : DbContext
    {
        public ArcaneVaultDbContext(DbContextOptions<ArcaneVaultDbContext> options) 
            : base(options)
        {
        }

        public DbSet<ArcaneVaultUser> ArcaneVaultUsers { get; set; } = null!;
        public DbSet<ArcaneVaultUserRole> ArcaneVaultUserRoles { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<CollectionItem> CollectionItems { get; set; } = null!;
        public DbSet<CollectionItemCategory> CollectionItemCategories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ArcaneVaultUser primary key
            modelBuilder.Entity<ArcaneVaultUser>()
                .HasKey(u => u.UserName);

            // Configure ArcaneVaultUserRole primary key
            modelBuilder.Entity<ArcaneVaultUserRole>()
                .HasKey(r => r.RoleId)
                .HasName("PK_ArcaneVaultUserRoles");

            // Configure Category primary key
            modelBuilder.Entity<Category>()
                .HasKey(c => c.CategoryCode);

            // Configure CollectionItem primary key
            modelBuilder.Entity<CollectionItem>()
                .HasKey(i => i.ItemId);

            // Configure User -> Role relationship (many-to-one)
            modelBuilder.Entity<ArcaneVaultUser>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure User -> CollectionItems relationship (one-to-many)
            modelBuilder.Entity<CollectionItem>()
                .HasOne(i => i.User)
                .WithMany(u => u.CollectionItems)
                .HasForeignKey(i => i.UserName)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure composite primary key for CollectionItemCategory
            modelBuilder.Entity<CollectionItemCategory>()
                .HasKey(cic => new { cic.ItemId, cic.CategoryCode });

            // Configure CollectionItemCategory -> CollectionItem relationship
            modelBuilder.Entity<CollectionItemCategory>()
                .HasOne(cic => cic.Item)
                .WithMany(i => i.CollectionItemCategories)
                .HasForeignKey(cic => cic.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure CollectionItemCategory -> Category relationship
            modelBuilder.Entity<CollectionItemCategory>()
                .HasOne(cic => cic.Category)
                .WithMany(c => c.CollectionItemCategories)
                .HasForeignKey(cic => cic.CategoryCode)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed default roles
            modelBuilder.Entity<ArcaneVaultUserRole>().HasData(
                new ArcaneVaultUserRole { RoleId = 1, RoleName = "User" },
                new ArcaneVaultUserRole { RoleId = 2, RoleName = "Staff" }
            );
        }
    }
}
