using WebApplication1.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Data.DBContexts
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserAccess> Accesses { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartDetail> CartDetails { get; set; }
        public DbSet<Rate> Rates { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<AuthToken> AuthTokens { get; set; }

        public DataContext(DbContextOptions options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("site");

            modelBuilder.Entity<UserAccess>()
                .HasOne(ua => ua.Role)
                .WithMany()
                .HasForeignKey(ua => ua.RoleId);

            modelBuilder.Entity<Entities.UserRole>().HasData(
                new Entities.UserRole
                {
                    Id = Guid.Parse("D1A3D3A4-3A3D-4D1A-3A3D-4D1A3D3A4D1A"),
                    Name = "Admin",
                    Description = "Адміністратор",
                    CanCreate = true,
                    CanRead = true,
                    CanUpdate = true,
                    CanDelete = true,
                    IsEmployee = true
                },
                new Entities.UserRole
                {
                    Id = Guid.Parse("D1A3D3A4-3A3D-4D1A-3A3D-4D1A3D3A4D2A"),
                    Name = "User",
                    Description = "Користувач",
                    CanCreate = false,
                    CanRead = false,
                    CanUpdate = false,
                    CanDelete = false,
                    IsEmployee = false
                },
                new Entities.UserRole
                {
                    Id = Guid.Parse("D1A3D3A4-3A3D-4D1A-3A3D-4D1A3D3A4D3A"),
                    Name = "Employee",
                    Description = "Працівник",
                    CanCreate = false,
                    CanRead = false,
                    CanUpdate = false,
                    CanDelete = false,
                    IsEmployee = true
                }
            );

            modelBuilder.Entity<AuthToken>().HasKey(at => at.Jti);

            modelBuilder.Entity<AuthToken>()
                .HasOne(at => at.UserAccess)
                .WithMany()
                .HasPrincipalKey(u => u.Id)
                .HasForeignKey(at => at.Sub);

            modelBuilder.Entity<Rate>()
                .HasOne(r => r.User)
                .WithMany(u => u.Rates);

            modelBuilder.Entity<Rate>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Rates);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Accesses)
                .WithOne(ua => ua.User)
                .HasForeignKey(ua => ua.UserId);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products);

            modelBuilder.Entity<Promotion>()
                .HasMany(pm => pm.Products)
                .WithMany(p => p.Promotions)
                .UsingEntity(pmp => pmp.ToTable("ProductPromotion"));

            modelBuilder.Entity<UserAccess>().HasIndex(a => a.Login).IsUnique();

            modelBuilder.Entity<User>().HasIndex(u => u.Slug).IsUnique();
            modelBuilder.Entity<Product>().HasIndex(p => p.Slug);
            modelBuilder.Entity<Category>().HasIndex(c => c.Slug);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts);

            modelBuilder.Entity<CartDetail>()
                .HasOne(cd => cd.Product)
                .WithMany();

            modelBuilder.Entity<CartDetail>()
                .HasOne(cd => cd.Cart)
                .WithMany(c => c.CartDetails);

            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = Guid.Parse("C4971B90-C145-411D-A35A-0EC565423DB7"),
                    Name = "Скло",
                    Description = "Товари та вироби зі скла",
                    ImagesCsv = "glass.jpg",
                    Slug = "glass"
                },
                new Category
                {
                    Id = Guid.Parse("F889F0B9-ABB3-434B-93F4-1BA9331196BB"),
                    Name = "Офіс",
                    Description = "Офісні та настільні товари",
                    ImagesCsv = "office.jpg",
                    Slug = "office"
                },
                new Category
                {
                    Id = Guid.Parse("283E039B-C71D-46A7-B69F-F8BB7AEB1A5F"),
                    Name = "Каміння",
                    Description = "Вироби з натурального та штучного камінняня",
                    ImagesCsv = "stone.jpg",
                    Slug = "stone"
                },
                new Category
                {
                    Id = Guid.Parse("112F9ACE-B5E9-4C38-8FA5-3D6AD440D090"),
                    Name = "Дерево",
                    Description = "Товари та вироби з деревини",
                    ImagesCsv = "wood.jpg",
                    Slug = "wood"
                }
                );
        }
    }
}
