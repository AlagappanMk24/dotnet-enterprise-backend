using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SignalRHub.Api.Options;

namespace SignalRHub.Api.Extensions;

/// <summary>
/// Helpers that exist only for local testing. Remove before deploying.
/// </summary>
public static class DevelopmentEndpoints
{
    /// <summary>
    /// Maps GET /dev/token?userId=alice&amp;role=Admin which returns a signed JWT, so the test page
    /// can log in without a real login system. Anyone who can reach this URL can become any user,
    /// which is why it is mapped in Development only.
    /// </summary>
    public static WebApplication MapDevelopmentEndpoints(this WebApplication app)
    {
        app.MapGet("/dev/token", (string userId, string? role, IOptions<JwtSettings> options) =>
        {
            var jwt = options.Value;

            // Claims inside the token: who the user is, and optionally their role.
            var claims = new List<Claim> { new("sub", userId) };

            if (!string.IsNullOrWhiteSpace(role))
                claims.Add(new Claim("role", role));

            // Sign with the same key the API uses to validate, so the token is accepted.
            var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = jwt.Issuer,
                Audience = jwt.Audience,
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    SecurityAlgorithms.HmacSha256)
            });

            return Results.Ok(new { token });
        })
        .AllowAnonymous();

        return app;
    }
}
