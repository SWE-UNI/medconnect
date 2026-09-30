namespace MedConnect.Features.Identity.Dtos;

public record RegisterStaffDto(
    string Email,
    string Password,
    string FullName,
    string Role,
    int? FacilityId,
    int? DivisionId);