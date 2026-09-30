namespace MedConnect.Client.Features.Auth.Dtos;

public record ForgotPasswordDto(string Identifier);
public record ForgotPasswordResponseDto(bool Succeeded, string Message, string? ResetToken = null);
public record ResetPasswordDto(string Identifier, string Token, string NewPassword);
