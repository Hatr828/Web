using WebApplication1.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Data.DBContexts
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserAccess> UsersAccess { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("site");

            modelBuilder.Entity<User>()
                .ToTable("Users", "site"); // Ensure EF recognizes the correct schema

            modelBuilder.Entity<UserAccess>()
                .HasIndex(a => a.Login)
                .IsUnique();

            modelBuilder.Entity<UserAccess>()
                .HasOne(a => a.User)
                .WithMany(u => u.Accesses)
                .HasPrincipalKey(u => u.Id)
                .HasForeignKey(a => a.UserId);
        }
    }
}
