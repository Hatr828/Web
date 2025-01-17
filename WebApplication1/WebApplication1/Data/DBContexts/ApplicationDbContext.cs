using ASP_P22.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ASP_P22.Data.DBContexts
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserAccess> UsersAccess { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("site");

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
