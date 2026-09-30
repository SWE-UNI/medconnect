namespace MedConnect.Features.Vitals.Dtos;

public record CreateVitalSignDto(
    int PatientId,
    int HeartRate,
    int BloodPressureSystolic,
    int BloodPressureDiastolic,
    double? TemperatureCelsius,
    int? OxygenSaturation);
