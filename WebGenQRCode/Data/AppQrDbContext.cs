using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebGenQRCode.Data.Entities;
using WebGenQRCode.Data.Entities.Identity;

namespace WebGenQRCode.Data;

public class AppQrDbContext : IdentityDbContext<UserEntity, RoleEntity, int>
{
    public AppQrDbContext(DbContextOptions<AppQrDbContext> options)
        : base(options)
    { }

    public DbSet<QrCodeEntity> QrCodes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //identity
        modelBuilder.Entity<UserRoleEntity>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRoleEntity>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);
    }
}