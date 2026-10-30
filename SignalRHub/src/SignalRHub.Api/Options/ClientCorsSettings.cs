using System.ComponentModel.DataAnnotations;

namespace SignalRHub.Api.Options;

/// <summary>Settings from the "Cors" section of appsettings.</summary>
public sealed class ClientCorsSettings
{
    public const string SectionName = "Cors";

    /// <summary>Name of the CORS policy used by UseCors.</summary>
    public const string PolicyName = "SignalRClients";

    /// <summary>
    /// Websites allowed to call this API and open the hub, for example "http://localhost:4200".
    /// Must list exact origins: a wildcard cannot be combined with credentials.
    /// </summary>
    [Required, MinLength(1)]
    public string[] AllowedOrigins { get; set; } = [];
}
