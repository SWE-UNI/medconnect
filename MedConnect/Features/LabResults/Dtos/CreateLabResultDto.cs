namespace MedConnect.Features.LabResults.Dtos;

public record CreateLabResultDto(
    int PatientId,
    int FacilityId,
    string TestName,
    string? Unit,
    string? ReferenceRange);