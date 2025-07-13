using Application.Interfaces.IRepositories.Clinic;
using Application.Interfaces.IRepositories.Doctor;
using Application.Interfaces.IRepositories.Patient;
using Application.Interfaces.IRepositories.City;
using Application.Interfaces.IRepositories.Specialty;
using Ivy.Infrastructure.Persistence;
using Ivy.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Repositories;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Application.Interfaces.IRepositories.ClinicEmployee;
using Infrastructure.Repositories.ClinicEmployee;
using Application.Interfaces.IRepositories.Appointment;
using Application.Interfaces.IRepositories;
using Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using Domain.IdentityEntiities;

namespace Ivy
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<IvyContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("IvyConnection")));

            builder.Services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders();
            
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });

            // Configure authorization with permission policies
            builder.Services.AddAuthorization(options =>
            {
                foreach (var permission in Permissions.GetAllPermissions())
                {
                    options.AddPolicy(permission, policy =>
                        policy.Requirements.Add(new PermissionRequirement(permission)));
                }
            });

            // Register authorization handler
            builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            // Register services
            builder.Services.AddScoped<IPatientRepository, PetientRepository>();
            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
            builder.Services.AddScoped<IClinicRepository, ClinicRepository>();
            builder.Services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
            builder.Services.AddScoped<ICityRepository, CityRepository>();
            builder.Services.AddScoped<IClinicEmployeeRepository, ClinicEmployeeRepository>();
            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            builder.Services.AddScoped<IPermissionService, PermissionService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { 
                    Title = "Ivy Health Management API", 
                    Version = "v1",
                    Description = "A comprehensive health management system API for managing patients, doctors, clinics, and appointments with role-based access control.",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "Ivy Health Management System",
                        Email = "support@ivy-health.com"
                    }
                });

                // Include XML documentation
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // 🔐 Add JWT Bearer Definition
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter your JWT token like this: **Bearer your_token_here**"
                });

            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });

            var app = builder.Build();

            // Seed roles and permissions
            await SeedRolesAndPermissions(app.Services);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            await app.RunAsync();
        }

        private static async Task SeedRolesAndPermissions(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            // Create roles
            string[] roleNames = { "admin", "doctor", "clinic-staff", "patient" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create permissions
            var permissions = Permissions.GetAllPermissions().ToList();
            foreach (var permissionName in permissions)
            {
                var existingPermission = await context.Permissions
                    .FirstOrDefaultAsync(p => p.Name == permissionName);

                if (existingPermission == null)
                {
                    context.Permissions.Add(new Permission
                    {
                        Name = permissionName
                    });
                }
            }

            await context.SaveChangesAsync();

            // Assign permissions to admin role
            var adminRole = await roleManager.FindByNameAsync("admin");
            if (adminRole != null)
            {
                var allPermissionIds = await context.Permissions.Select(p => p.Id).ToListAsync();
                
                var existingAssignments = await context.PermissionRoles
                    .Where(pr => pr.RoleId == adminRole.Id)
                    .Select(pr => pr.PermissionId)
                    .ToListAsync();

                var newPermissions = allPermissionIds.Except(existingAssignments);

                foreach (var permissionId in newPermissions)
                {
                    context.PermissionRoles.Add(new PermissionRole
                    {
                        RoleId = adminRole.Id,
                        PermissionId = permissionId
                    });
                }

                await context.SaveChangesAsync();
            }
        }

        private static string FormatPermissionDescription(string permissionName)
        {
            return permissionName.Replace("_", " ").ToUpper();
        }
    }
}
