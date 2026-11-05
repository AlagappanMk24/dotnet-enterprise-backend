namespace EnterprisePatterns.Application.DTOs
{
    /// <summary>Address as exposed on the API boundary.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> Data Transfer Object (DTO) as immutable record.<br/>
    /// <b>Purpose:</b> Decouples the wire contract from the domain Address value object.<br/>
    /// <b>Why It Is Used:</b> Domain refactors do not break API clients (Dependency Inversion at the boundary).
    /// </remarks>
    public sealed record AddressDto(string Street, string City, string PostalCode, string Country);
}