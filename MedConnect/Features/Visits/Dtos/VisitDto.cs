namespace MedConnect.Features.Visits.Dtos;

public record VisitDto(
    int VisitId,
    int PatientId,
    int FacilityId,
    string FacilityName,
    DateTime Date,
    string Diagnosis,
    string? PrescriptionRef);
