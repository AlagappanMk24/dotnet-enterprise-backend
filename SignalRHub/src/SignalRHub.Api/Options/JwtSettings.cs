using System.ComponentModel.DataAnnotations;

namespace SignalRHub.Api.Options;

/// <summary>
/// Settings from the "Jwt" section of appsettings.
/// The app refuses to start if one is missing (see ValidateOnStart in AuthenticationExtensions).
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>Who issued the token. Must match the "iss" claim.</summary>
    [Required] public string Issuer { get; set; } = string.Empty;

    /// <summary>Who the token is for. Must match the "aud" claim.</summary>
    [Required] public string Audience { get; set; } = string.Empty;

    /// <summary>Secret used to sign and verify tokens. At least 32 characters. Keep it out of source control.</summary>
    [Required, MinLength(32)] public string Key { get; set; } = string.Empty;
}
