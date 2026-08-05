// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using ArcaneVault.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault.API.Data
{
    /// <summary>
    /// ArcaneVaultDbContext - Entity Framework Core DbContext for ArcaneVault application.
    /// Handles database mapping, relationships, soft deletes via global query filters, and data seeding.
    /// </summary>
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

        /// <summary>
        /// OnModelCreating - Configures entity mappings, relationships, query filters, and seed data.
        /// 
        /// Key configurations:
        /// - Composite primary key for CollectionItemCategory (ItemId + CategoryCode)
        /// - Global query filters to exclude soft-deleted records (IsDeleted == true)
        /// - One-to-many relationships with cascade/restrict delete behavior
        /// - Seed data: Staff (RoleId=1) and User (RoleId=2) roles
        /// </summary>
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

            // Global query filter for ArcaneVaultUser - exclude soft-deleted records
            modelBuilder.Entity<ArcaneVaultUser>()
                .HasQueryFilter(u => !u.IsDeleted);

            // Global query filter for CollectionItem - exclude soft-deleted records
            modelBuilder.Entity<CollectionItem>()
                .HasQueryFilter(i => !i.IsDeleted);

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

            // Configure composite primary key for CollectionItemCategory (ItemId, CategoryCode)
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

            // Seed default master roles
            // Role 1: Staff - can manage categories and collections
            // Role 2: User - can only manage their own collections
            modelBuilder.Entity<ArcaneVaultUserRole>().HasData(
                new ArcaneVaultUserRole { RoleId = 1, RoleName = "Staff" },
                new ArcaneVaultUserRole { RoleId = 2, RoleName = "User" }
            );
        }
    }
}
