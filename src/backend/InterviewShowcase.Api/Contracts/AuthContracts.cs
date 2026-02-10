using System.ComponentModel.DataAnnotations;

namespace InterviewShowcase.Api.Contracts;

public record RegisterRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(8), MaxLength(128)] string Password);

public record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password);

public record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, Guid UserId, string Email);
