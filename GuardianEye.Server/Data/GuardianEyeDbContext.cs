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
    public DbSet<TimeRequest> TimeRequests { get; set; }
    public DbSet<SessionHistory> SessionHistories { get; set; }

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

        modelBuilder.Entity<TimeRequest>(entity =>
        {
            entity.HasIndex(tr => tr.StudentId);
            entity.HasIndex(tr => tr.Status);
            entity.HasOne(tr => tr.Student)
                .WithMany(u => u.TimeRequests)
                .HasForeignKey(tr => tr.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(tr => tr.ApprovedBy)
                .WithMany()
                .HasForeignKey(tr => tr.ApprovedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SessionHistory>(entity =>
        {
            entity.HasIndex(sh => sh.UserId);
            entity.HasIndex(sh => sh.LoginTime);
            entity.HasOne(sh => sh.User)
                .WithMany(u => u.SessionHistories)
                .HasForeignKey(sh => sh.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(sh => sh.TimeRequest)
                .WithMany()
                .HasForeignKey(sh => sh.TimeRequestId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
