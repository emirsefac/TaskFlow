using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Entities;

namespace TaskFlow.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- User ----
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(200).IsRequired();
        });

        // ---- Project ----
        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(150).IsRequired();
            entity.HasOne(p => p.Owner)
                  .WithMany()
                  .HasForeignKey(p => p.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- ProjectMember (many-to-many ara tablosu) ----
        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasIndex(pm => new { pm.ProjectId, pm.UserId }).IsUnique();

            entity.HasOne(pm => pm.Project)
                  .WithMany(p => p.Members)
                  .HasForeignKey(pm => pm.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pm => pm.User)
                  .WithMany(u => u.ProjectMemberships)
                  .HasForeignKey(pm => pm.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- TaskItem ----
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.Property(t => t.Title).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(t => t.Priority).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(t => t.Project)
                  .WithMany(p => p.Tasks)
                  .HasForeignKey(t => t.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.AssignedUser)
                  .WithMany(u => u.AssignedTasks)
                  .HasForeignKey(t => t.AssignedUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ---- Comment ----
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.Property(c => c.Content).HasMaxLength(2000).IsRequired();

            entity.HasOne(c => c.TaskItem)
                  .WithMany(t => t.Comments)
                  .HasForeignKey(c => c.TaskItemId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.User)
                  .WithMany(u => u.Comments)
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
