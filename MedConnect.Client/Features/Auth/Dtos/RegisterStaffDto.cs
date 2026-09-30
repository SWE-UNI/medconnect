namespace MedConnect.Client.Features.Auth.Dtos;

public record RegisterStaffDto(
    string Email,
    string Password,
    string FullName,
    string Role,
    int? FacilityId,
    int? DivisionId);