namespace JazeeraBooking.Application.Common.Models;

/// <summary>
/// Bound from the "Jwt" config section. Lives in Application (not Infrastructure or Api)
/// because both the token *issuer* (AuthService, in Infrastructure) and the token *validator*
/// (JWT Bearer middleware, wired up in Api) need the same shape - putting it here avoids
/// either layer depending on the other just to share a settings class. SigningKey itself is
/// never given a default: appsettings.Development.json (gitignored) or environment variables
/// supply it, so nothing here accidentally ships with a real secret.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 60;
}
