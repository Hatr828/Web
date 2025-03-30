using Microsoft.EntityFrameworkCore;
using WebApplication1.Services;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using WebApplication1.Data.DBContexts;
using WebApplication1.Services.kdf;
using WebApplication1.Services.Hash;
using WebApplication1.Services.Random;
using WebApplication1.Middleware.Auth;
using WebApplication1.Services.Storage;
using WebApplication1.Data;


namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:3000");
                    });
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddSingleton<ITimeDate, TimeDate>();
            builder.Services.AddSingleton<IRandomService, AbcRandomService>();
            builder.Services.AddSingleton<IHashService, Md5HashService>();
            builder.Services.AddSingleton<IKdfService, PbKdf1Service>();
            builder.Services.AddSingleton<IStorageService, LocalStorageService>();
            builder.Services.AddScoped<DataAccessor>();
            builder.Services.AddScoped<UserService>();

            builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseSession();

            app.MapStaticAssets();

            app.UseAuthSession();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
