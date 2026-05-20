using Microsoft.EntityFrameworkCore;
using GuardianEye.Server.Models;

namespace GuardianEye.Server.Data;

public class GuardianEyeDbContext : DbContext
{
    public GuardianEyeDbContext(DbContextOptions<GuardianEyeDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<ActiveSession> ActiveSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Username).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).HasMaxLength(20);
        });

        modelBuilder.Entity<ActiveSession>(entity =>
        {
            entity.HasIndex(s => s.Token).IsUnique();
            entity.HasIndex(s => s.UserId);
            entity.HasOne(s => s.User)
                .WithMany(u => u.ActiveSessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
