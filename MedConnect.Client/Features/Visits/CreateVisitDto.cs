namespace MedConnect.Client.Features.Visits;

public record CreateVisitDto(int PatientId, int FacilityId, DateTime Date, string Diagnosis, string? PrescriptionRef);
