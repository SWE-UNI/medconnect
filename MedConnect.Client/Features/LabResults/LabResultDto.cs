namespace MedConnect.Client.Features.LabResults;

public record LabResultDto(
    int LabResultId,
    int PatientId,
    int FacilityId,
    string FacilityName,
    string TestName,
    string? ResultText,
    string? Unit,
    string? ReferenceRange,
    string Status,
    string? OrderedByUserName,
    string? ReviewedByUserName,
    DateTime OrderedAt,
    DateTime? CompletedAt);