namespace InterviewShowcase.Api.Models;

public class ProjectEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; }
    public DateTime CreatedOnUtc { get; set; }

    public Guid OwnerId { get; set; }
    public AppUser? Owner { get; set; }

    public List<TaskItemEntity> Tasks { get; set; } = [];
}
