namespace MedConnect.Features.Identity.Dtos;

/// Self-service registration — patients only. Staff accounts must be created by
/// an Admin via the /auth/register-staff endpoint.
public record RegisterDto(
    string Email,
    string Password,
    string FullName,
    string? NHISNumber,
    DateOnly? DateOfBirth,
    string? ContactInfo);