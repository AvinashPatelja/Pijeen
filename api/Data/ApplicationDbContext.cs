using Microsoft.EntityFrameworkCore;
using Pijeen.API.Models;

namespace Pijeen.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceLive> DeviceLive => Set<DeviceLive>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Users table config
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // Devices table config
        modelBuilder.Entity<Device>()
            .HasIndex(d => d.IMEI)
            .IsUnique();

        modelBuilder.Entity<Device>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(d => d.UserId);

        // DeviceLive table config
        modelBuilder.Entity<DeviceLive>()
            .HasIndex(dl => dl.IMEI)
            .IsUnique();

        modelBuilder.Entity<DeviceLive>()
            .HasOne<Device>()
            .WithMany()
            .HasForeignKey(dl => dl.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // AuditLog table config
        modelBuilder.Entity<AuditLog>()
            .HasOne<Device>()
            .WithMany()
            .HasForeignKey(al => al.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AuditLog>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(al => al.ActionBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
