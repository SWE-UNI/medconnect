namespace MedConnect.Features.Prescriptions.Dtos;

public record CreatePrescriptionDto(
    int PatientId,
    int VisitId,
    string Medication,
    string? Dosage,
    string? Frequency,
    int? DurationDays,
    string? Instructions);