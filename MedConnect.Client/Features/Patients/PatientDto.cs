namespace MedConnect.Client.Features.Patients;

public record PatientDto(
    int PatientId, string NHISNumber, string FullName, DateOnly DateOfBirth, string ContactInfo, string? BloodType);
