namespace MedConnect.Client.Features.Prescriptions;

public record PrescriptionDto(
    int PrescriptionId,
    int PatientId,
    int VisitId,
    string Medication,
    string? Dosage,
    string? Frequency,
    int? DurationDays,
    string? Instructions,
    string PrescribedByUserName,
    DateTime PrescribedAt);