namespace MedConnect.Features.Patients.Dtos;

public record CreatePatientDto(
    string NHISNumber,
    string FullName,
    DateOnly DateOfBirth,
    string ContactInfo,
    string? BloodType);
