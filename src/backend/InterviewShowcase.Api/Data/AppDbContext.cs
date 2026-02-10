using InterviewShowcase.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InterviewShowcase.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();
    public DbSet<TaskItemEntity> Tasks => Set<TaskItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .Property(u => u.Email)
            .HasMaxLength(255);

        modelBuilder.Entity<ProjectEntity>()
            .Property(p => p.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<ProjectEntity>()
            .Property(p => p.Description)
            .HasMaxLength(300);

        modelBuilder.Entity<ProjectEntity>()
            .HasOne(p => p.Owner)
            .WithMany(u => u.Projects)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskItemEntity>()
            .Property(t => t.Title)
            .HasMaxLength(120);

        modelBuilder.Entity<TaskItemEntity>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
