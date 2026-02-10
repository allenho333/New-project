namespace InterviewShowcase.Api.Models;

public class TaskItemEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; }
    public TaskState State { get; set; }
    public DateOnly DueDate { get; set; }
    public DateTime CreatedOnUtc { get; set; }

    public Guid ProjectId { get; set; }
    public ProjectEntity? Project { get; set; }
}
