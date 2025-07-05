using Domain.IdentityEntiities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    
    public class DataContext : IdentityDbContext
        {
            public DataContext(DbContextOptions<DataContext> options) : base(options)
            {
            }

            public DbSet<Permission> Permissions { get; set; }
            public DbSet<PermissionRole> PermissionRoles { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // Configure Permission entity
                modelBuilder.Entity<Permission>()
                    .HasKey(p => p.Id);

                // Configure PermissionRole entity
                modelBuilder.Entity<PermissionRole>()
                    .HasKey(pr => pr.Id);


                modelBuilder.Entity<PermissionRole>()
                    .HasOne(pr => pr.Role)
                    .WithMany()
                    .HasForeignKey(pr => pr.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }
    
}
