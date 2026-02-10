using System.ComponentModel.DataAnnotations;
using InterviewShowcase.Api.Models;

namespace InterviewShowcase.Api.Contracts;

public record CreateTaskRequest(
    [property: Required, MinLength(3), MaxLength(120)] string Title,
    TaskPriority Priority,
    TaskState State,
    DateOnly DueDate,
    Guid ProjectId);

public record UpdateTaskRequest(
    [property: Required, MinLength(3), MaxLength(120)] string Title,
    TaskPriority Priority,
    TaskState State,
    DateOnly DueDate,
    Guid ProjectId);

public record TaskResponse(Guid Id, string Title, TaskPriority Priority, TaskState State, DateOnly DueDate, Guid ProjectId, DateTime CreatedOnUtc);

public record DashboardSummary(int Projects, int Tasks, int CompletedTasks, int InProgressTasks, int UpcomingTasks);
