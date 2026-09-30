namespace MedConnect.Client.Features.Auth.Dtos;

public record TwoFactorSetupDto(string Secret, string OtpauthUri, bool Enabled);