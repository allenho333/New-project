using System.ComponentModel.DataAnnotations;
using InterviewShowcase.Api.Models;

namespace InterviewShowcase.Api.Contracts;

public record CreateProjectRequest(
    [property: Required, MinLength(3), MaxLength(100)] string Name,
    [property: Required, MinLength(10), MaxLength(300)] string Description,
    ProjectStatus Status);

public record UpdateProjectRequest(
    [property: Required, MinLength(3), MaxLength(100)] string Name,
    [property: Required, MinLength(10), MaxLength(300)] string Description,
    ProjectStatus Status);

public record ProjectResponse(Guid Id, string Name, string Description, ProjectStatus Status, DateTime CreatedOnUtc);
