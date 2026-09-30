namespace MedConnect.Features.Identity.Dtos;

public record TwoFactorSetupDto(
    string Secret,
    string OtpauthUri,
    bool Enabled);