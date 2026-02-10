using InterviewShowcase.Api.Models;
using InterviewShowcase.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace InterviewShowcase.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        var (hash, salt) = PasswordService.CreateHash("Demo123!");
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "demo@interview.dev",
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedOnUtc = DateTime.UtcNow
        };

        var projectA = new ProjectEntity
        {
            Id = Guid.NewGuid(),
            Name = "Interview Prep App",
            Description = "Build a full-stack app to demonstrate engineering depth",
            Status = ProjectStatus.InProgress,
            CreatedOnUtc = DateTime.UtcNow.AddDays(-7),
            OwnerId = user.Id
        };

        var projectB = new ProjectEntity
        {
            Id = Guid.NewGuid(),
            Name = "CI/CD Pipeline",
            Description = "Automate test/build/deploy with GitHub Actions",
            Status = ProjectStatus.NotStarted,
            CreatedOnUtc = DateTime.UtcNow.AddDays(-1),
            OwnerId = user.Id
        };

        var tasks = new[]
        {
            new TaskItemEntity
            {
                Id = Guid.NewGuid(),
                Title = "Set up ASP.NET API",
                Priority = TaskPriority.High,
                State = TaskState.Done,
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-6)),
                CreatedOnUtc = DateTime.UtcNow.AddDays(-6),
                ProjectId = projectA.Id
            },
            new TaskItemEntity
            {
                Id = Guid.NewGuid(),
                Title = "Create React dashboard",
                Priority = TaskPriority.High,
                State = TaskState.InProgress,
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
                CreatedOnUtc = DateTime.UtcNow.AddDays(-2),
                ProjectId = projectA.Id
            },
            new TaskItemEntity
            {
                Id = Guid.NewGuid(),
                Title = "Draft Terraform modules",
                Priority = TaskPriority.Medium,
                State = TaskState.NotStarted,
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4)),
                CreatedOnUtc = DateTime.UtcNow,
                ProjectId = projectB.Id
            }
        };

        dbContext.Users.Add(user);
        dbContext.Projects.AddRange(projectA, projectB);
        dbContext.Tasks.AddRange(tasks);
        await dbContext.SaveChangesAsync();
    }
}
