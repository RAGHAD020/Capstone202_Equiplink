using EquipLink.ApplicationDbContext;
using EquipLink.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace EquipLink
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region SqlServer Connection
            builder.Services.AddDbContext<EquipmentDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("EquipmentDBConnection"));
                options.EnableSensitiveDataLogging(); // Add this for detailed SQL logging
            });
            #endregion

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<IFileUploadService, FileUploadService>();
            builder.Services.AddLogging();
            #region Add session support
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
                options.Cookie.SameSite = SameSiteMode.Strict;           // CSRF protection
            });
            #endregion

            #region Add Authentication Services
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.SlidingExpiration = true;
                });
            #endregion

            // REMOVED: No longer needed since we're using BCrypt.Net.BCrypt consistently
            // builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            //    app.UseStaticFiles(new StaticFileOptions()
            //    {
            //        FileProvider = new PhysicalFileProvider(
            //Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
            //        RequestPath = ""
            //    });
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Front}/{action=Home}/{id?}");

            app.Run();
        }
    }
}