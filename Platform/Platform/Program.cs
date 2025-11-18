using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http.Features;
using Platform.Application.IRepos;
using Platform.Application.ServiceInterfaces;
using Platform.Application.Services;
using Platform.Core.Interfaces;
using Platform.Core.Interfaces.IRepos;
using Platform.Core.Models;
using Platform.Core.Interfaces.IUnitOfWork;
using Platform.Infrastructure.Data.DbContext;
using Platform.Infrastructure.Repositories;
using Platform.Infrastructure.UnitOfWork;
using System.Text;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using Platform.Infrastructure.Services;
using X.Paymob.CashIn;
using Platform.Application.Interfaces;
using Microsoft.Extensions.FileProviders;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;

namespace Platform
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Console.WriteLine($"[Debug] ContentRootPath = {builder.Environment.ContentRootPath}");

            // Ensure wwwroot exists before anything else
            var wwwRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
            if (!Directory.Exists(wwwRootPath))
            {
                Directory.CreateDirectory(wwwRootPath);
                Console.WriteLine($"[Info] Created missing folder: {wwwRootPath}");
            }

            builder.Environment.WebRootPath = wwwRootPath;

            // ✅ Configure Kestrel to use Let's Encrypt certificate

            //builder.WebHost.ConfigureKestrel(options =>
            //{
            //    // Allow large requests
            //    options.Limits.MaxRequestBodySize = long.MaxValue;

            //    // Path to your Let's Encrypt certificate
            //    var certPath = "/etc/letsencrypt/live/82-29-190-91.sslip.io/fullchain.pem";
            //    var keyPath = "/etc/letsencrypt/live/82-29-190-91.sslip.io/privkey.pem";

            //    if (File.Exists(certPath) && File.Exists(keyPath))
            //    {
            //        Console.WriteLine("[HTTPS] Using Let's Encrypt certificate.");
            //        options.ListenAnyIP(5000); // HTTP
            //        options.ListenAnyIP(5001, listenOptions =>
            //        {
            //            listenOptions.UseHttps(certPath, keyPath);
            //        });
            //    }
            //    else
            //    {
            //        Console.WriteLine("[HTTPS WARNING] Certificate not found, using HTTP only.");
            //        options.ListenAnyIP(5000);
            //    }
            //});

            // Add services
            builder.Services.AddControllers();

            // DbContext
            builder.Services.AddDbContext<CourseDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity
            builder.Services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<CourseDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddPaymobCashIn(conf =>
            {
                conf.ApiKey = builder.Configuration["Paymob:ApiKey"];
                conf.Hmac = builder.Configuration["Paymob:HmacKey"];
            });

            // Custom services
            builder.Services.AddScoped<IUnitOfWork, UnitOFWork>();
            builder.Services.AddScoped<IInstructorRepository, InstructorRepository>();
            builder.Services.AddScoped<IInstructorService, InstructorService>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IVideoService, VideoService>();
            builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
            builder.Services.AddScoped<IModuleService, ModuleService>();
            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IOtpService, OtpService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();

            builder.Services.AddMemoryCache();

            // Large file uploads config
            builder.Services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = long.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
                options.KeyLengthLimit = int.MaxValue;
                options.ValueCountLimit = int.MaxValue;
            });

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular",
                    policy => policy
                        .WithOrigins("https://classy-dolphin-f3f222.netlify.app", "http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
            });

            // JWT Auth
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
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });

            // Swagger / OpenAPI
            builder.Services.AddOpenApi();

            // FFmpeg setup
            var ffmpegPath = Path.Combine(AppContext.BaseDirectory, "FFmpeg");
            if (!Directory.Exists(ffmpegPath))
            {
                Directory.CreateDirectory(ffmpegPath);
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegPath);
            }
            FFmpeg.SetExecutablesPath(ffmpegPath);

            var app = builder.Build();

            // Middleware
            // Enable Swagger/OpenAPI for all environments
            // Swagger UI

            app.MapOpenApi();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/openapi/v1.json", "Neuro API v1");
            });

            // Static file serving
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "Images")),
                RequestPath = "/uploads"
            });

            app.UseHttpsRedirection();
            app.UseCors("AllowAngular");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStaticFiles();

            app.MapControllers();

            app.Run();
        }
    }
}
