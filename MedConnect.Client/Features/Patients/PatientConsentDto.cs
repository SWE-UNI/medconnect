namespace MedConnect.Client.Features.Patients;

public record PatientConsentDto(bool Consent, DateTimeOffset? UpdatedAt);