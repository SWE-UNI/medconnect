namespace MedConnect.Client.Features.LabResults;

public record CreateLabResultDto(
    int PatientId,
    int FacilityId,
    string TestName,
    string? Unit,
    string? ReferenceRange);