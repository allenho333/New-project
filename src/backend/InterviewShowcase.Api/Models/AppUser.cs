namespace InterviewShowcase.Api.Models;

public class AppUser
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    public DateTime CreatedOnUtc { get; set; }

    public List<ProjectEntity> Projects { get; set; } = [];
}
