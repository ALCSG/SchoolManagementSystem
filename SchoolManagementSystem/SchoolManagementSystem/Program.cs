using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Data.Interface;
using SchoolManagementSystem.Data.Repository;
using SchoolManagementSystem.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SchoolManagementSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration
                .GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is missing.");

            builder.Services.AddDbContext<DataContext>(config =>
                config.UseSqlServer(connectionString));

            builder.Services.AddIdentity<AppUser, IdentityRole>(config =>
            {
                config.User.RequireUniqueEmail = true;
                config.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders();

            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];
            var jwtKey = builder.Configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtIssuer) ||
                string.IsNullOrWhiteSpace(jwtAudience) ||
                string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException(
                    "Jwt:Issuer, Jwt:Audience and Jwt:Key must be configured.");
            }

            if (Encoding.UTF8.GetByteCount(jwtKey) < 64)
            {
                throw new InvalidOperationException(
                    "Jwt:Key must contain at least 64 UTF-8 bytes.");
            }

            builder.Services
                .AddAuthentication()
                .AddJwtBearer(
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.MapInboundClaims = false;

                        options.TokenValidationParameters =
                            new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidIssuer = jwtIssuer,

                                ValidateAudience = true,
                                ValidAudience = jwtAudience,

                                ValidateLifetime = true,
                                RequireExpirationTime = true,

                                ValidateIssuerSigningKey = true,
                                RequireSignedTokens = true,

                                IssuerSigningKey = new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(jwtKey)),

                                ValidAlgorithms = new[]
                                {
                        SecurityAlgorithms.HmacSha256
                                },

                                NameClaimType = "name",
                                RoleClaimType = "role",

                                ClockSkew = TimeSpan.FromSeconds(30)
                            };
                    });

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            builder.Services.AddScoped<IClassGroupRepository, ClassGroupRepository>();
            builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
            builder.Services.AddScoped<IRoleHelper, RoleHelper>();
            builder.Services.AddScoped<IAlertRepository, AlertRepository>();
            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("Mail"));
            builder.Services.AddScoped<IMailHelper, MailHelper>();
            builder.Services.AddScoped<ISchoolSettingsRepository,SchoolSettingsRepository>();

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(builder.Configuration["Syncfusion:LicenseKey"]);


            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                if (!await context.Set<SchoolSettings>().AnyAsync())
                {
                    context.Set<SchoolSettings>().Add(new SchoolSettings
                    {
                        MaxAbsencePercentage = 10
                    });

                    await context.SaveChangesAsync();
                }

                var roleHelper = scope.ServiceProvider.GetRequiredService<IRoleHelper>();

                await roleHelper.SeedRolesAsync(AppRoles.All);

                var adminEmail = builder.Configuration["Admin:Email"];
                var adminPassword = builder.Configuration["Admin:Password"];

                if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
                {
                    app.Logger.LogWarning("Admin:Email / Admin:Password not configured. The administrator was not seeded.");
                }
                else
                {
                    var result = await roleHelper.SeedAdminAsync(adminEmail, adminPassword, "Admin", "Admin");

                    if (!result.Succeeded)
                    {
                        var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Could not seed the administrator: {errors}");
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error/500");
                app.UseHsts();
            }

            app.UseWhen(
                context => !context.Request.Path.StartsWithSegments("/api"),
                branch =>
                {
                    branch.UseStatusCodePagesWithReExecute("/Error/{0}");
                });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
