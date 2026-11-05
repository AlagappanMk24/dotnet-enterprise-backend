namespace EnterprisePatterns.Application.Exceptions;

/// <summary>A requested resource does not exist.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Custom exception hierarchy (explicit failure modes).<br/>
/// <b>Purpose:</b> Mapped to HTTP 404 by the API middleware.<br/>
/// <b>Why It Is Used:</b> Handlers express intent without knowing anything about HTTP (layer separation).
/// </remarks>
public sealed class NotFoundException(string entityName, object key)
    : Exception($"{entityName} with id '{key}' was not found.")
{ }