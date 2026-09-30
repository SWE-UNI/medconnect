namespace MedConnect.Client.Features.Auth.Dtos;

/// Patients sign in with NHIS number; staff sign in with email. Exactly one of
/// Email or NHISNumber should be set. Code is used when 2FA is enabled.
public record LoginDto(string? Email, string? NHISNumber, string Password, string? Code = null);