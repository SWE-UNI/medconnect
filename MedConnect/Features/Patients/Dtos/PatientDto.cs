namespace MedConnect.Features.Patients.Dtos;

public record PatientDto(
    int PatientId,
    string NHISNumber,
    string FullName,
    DateOnly DateOfBirth,
    string ContactInfo,
    string? BloodType);
