namespace MedConnect.Features.Identity.Dtos;

/// Patients authenticate with their NHIS number; staff authenticate with email.
/// One of Email or NHISNumber must be provided. Code is only needed when the
/// account has two-factor authentication enabled.
public record LoginDto(string? Email, string? NHISNumber, string Password, string? Code = null);