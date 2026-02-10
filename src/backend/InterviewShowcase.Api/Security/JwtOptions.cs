namespace InterviewShowcase.Api.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "InterviewShowcase.Api";
    public string Audience { get; set; } = "InterviewShowcase.Web";
    public string Key { get; set; } = "ChangeThisDevelopmentOnlyKey_AtLeast32Chars";
    public int ExpirationMinutes { get; set; } = 60;
}
