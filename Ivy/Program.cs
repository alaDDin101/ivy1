using Application.Interfaces.IRepositories.Patient;
using Ivy.Infrastructure.Persistence;
using Ivy.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ivy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<IvyContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("IvyConnection")));

            builder.Services.AddScoped<IPatientRepository, PetientRepository>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
