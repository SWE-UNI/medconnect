namespace MedConnect.Client.Features.Patients;

public record CreatePatientDto(
    string NHISNumber, string FullName, DateOnly DateOfBirth, string ContactInfo, string? BloodType);
