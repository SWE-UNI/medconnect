namespace MedConnect.Features.Patients.Dtos;

public record PatientConsentDto(bool Consent, DateTimeOffset? UpdatedAt);