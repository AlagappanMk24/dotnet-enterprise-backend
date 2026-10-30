using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SignalRHub.Api.Constants;
using SignalRHub.Api.Options;

namespace SignalRHub.Api.Extensions;

/// <summary>JWT login setup for both normal API calls and the SignalR hub.</summary>
public static class AuthenticationExtensions
{
    /// <summary>Registers JWT bearer authentication and authorization.</summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        // Bind the "Jwt" section and fail at startup if a value is missing or the key is too short.
        services.AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // Configure the bearer handler after the settings above are available.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                // Keep claim names exactly as written in the token ("sub", "role"),
                // instead of .NET renaming them to long URI-style names.
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Issuer,                  // reject tokens from other issuers
                    ValidAudience = jwt.Audience,              // reject tokens meant for other apps
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),  // verify the signature
                    NameClaimType = "sub",                     // User.Identity.Name comes from "sub"
                    RoleClaimType = "role",                    // [Authorize(Roles = ...)] reads "role"
                    ClockSkew = TimeSpan.FromSeconds(30)       // small tolerance for clock differences
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ReadTokenFromQueryForHub
                };
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Browsers cannot set an Authorization header on a WebSocket, so SignalR sends the token
    /// as ?access_token=... . This copies it into the request, but only for the hub path,
    /// so normal API calls cannot pass tokens through the URL.
    /// </summary>
    private static Task ReadTokenFromQueryForHub(MessageReceivedContext context)
    {
        var accessToken = context.Request.Query["access_token"];

        if (!string.IsNullOrEmpty(accessToken) &&
            context.HttpContext.Request.Path.StartsWithSegments(HubRoutes.Notifications))
        {
            context.Token = accessToken;
        }

        return Task.CompletedTask;
    }
}
