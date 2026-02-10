using InterviewShowcase.Api.Security;

namespace InterviewShowcase.Api.Tests;

public class PasswordServiceTests
{
    [Fact]
    public void CreateHash_AndVerify_WithMatchingPassword_ReturnsTrue()
    {
        var password = "MyStrongPassword!42";

        var (hash, salt) = PasswordService.CreateHash(password);

        var isValid = PasswordService.Verify(password, hash, salt);

        Assert.True(isValid);
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var (hash, salt) = PasswordService.CreateHash("CorrectPassword123!");

        var isValid = PasswordService.Verify("WrongPassword123!", hash, salt);

        Assert.False(isValid);
    }
}
