using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InterviewShowcase.Api.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var rawConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=showcase;Username=showcase;Password=showcase";

        optionsBuilder.UseNpgsql(NormalizePostgresConnectionString(rawConnectionString));
        return new AppDbContext(optionsBuilder.Options);
    }

    private static string NormalizePostgresConnectionString(string raw)
    {
        if (raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(raw);
            var userInfoParts = uri.UserInfo.Split(':', 2, StringSplitOptions.None);
            var username = Uri.UnescapeDataString(userInfoParts[0]);
            var password = userInfoParts.Length > 1 ? Uri.UnescapeDataString(userInfoParts[1]) : string.Empty;
            var database = uri.AbsolutePath.TrimStart('/');

            var queryParams = ParseQuery(uri.Query);
            queryParams.TryGetValue("sslmode", out var sslMode);
            var port = uri.IsDefaultPort || uri.Port <= 0 ? 5432 : uri.Port;

            var parts = new List<string>
            {
                $"Host={uri.Host}",
                $"Port={port}",
                $"Database={database}",
                $"Username={username}",
                $"Password={password}"
            };

            if (!string.IsNullOrWhiteSpace(sslMode))
            {
                parts.Add($"SSL Mode={sslMode}");
            }

            return string.Join(';', parts);
        }

        return raw;
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(query))
        {
            return result;
        }

        var trimmed = query.TrimStart('?');
        var pairs = trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=', 2, StringSplitOptions.None);
            var key = Uri.UnescapeDataString(keyValue[0]);
            var value = keyValue.Length > 1 ? Uri.UnescapeDataString(keyValue[1]) : string.Empty;
            result[key] = value;
        }

        return result;
    }
}
