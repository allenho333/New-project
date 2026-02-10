using System.Security.Cryptography;

namespace InterviewShowcase.Api.Security;

public static class PasswordService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 120_000;

    public static (byte[] hash, byte[] salt) CreateHash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return (hash, salt);
    }

    public static bool Verify(string password, byte[] storedHash, byte[] storedSalt)
    {
        var attemptedHash = Rfc2898DeriveBytes.Pbkdf2(password, storedSalt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return CryptographicOperations.FixedTimeEquals(attemptedHash, storedHash);
    }
}
