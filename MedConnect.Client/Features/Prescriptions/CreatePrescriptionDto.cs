namespace MedConnect.Client.Features.Prescriptions;

public record CreatePrescriptionDto(
    int PatientId,
    int VisitId,
    string Medication,
    string? Dosage,
    string? Frequency,
    int? DurationDays,
    string? Instructions);