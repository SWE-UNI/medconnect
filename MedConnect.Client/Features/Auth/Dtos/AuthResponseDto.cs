namespace MedConnect.Client.Features.Auth.Dtos;

public record AuthResponseDto(
    string Token,
    DateTime ExpiresAtUtc,
    string Role,
    string FullName,
    bool TwoFactorRequired = false);