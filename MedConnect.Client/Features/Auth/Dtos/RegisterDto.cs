namespace MedConnect.Client.Features.Auth.Dtos;

public record RegisterDto(
    string Email,
    string Password,
    string FullName,
    string? NHISNumber,
    DateOnly? DateOfBirth,
    string? ContactInfo);