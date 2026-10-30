using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace SignalRHub.Api.Hubs;

/// <summary>
/// Tells SignalR which part of the JWT is the user id. After this, Clients.User("alice")
/// reaches every connection whose token has sub = "alice", and Context.UserIdentifier is that id.
/// We read "sub" directly because MapInboundClaims is turned off (claim names stay as written).
/// </summary>
public sealed class SubClaimUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        connection.User.FindFirst("sub")?.Value
        ?? connection.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;   // fallback for tokens that use the long claim name
}
