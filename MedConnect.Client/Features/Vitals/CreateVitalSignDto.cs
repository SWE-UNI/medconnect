namespace MedConnect.Client.Features.Vitals;

public record CreateVitalSignDto(
    int PatientId,
    int HeartRate,
    int BloodPressureSystolic,
    int BloodPressureDiastolic,
    double? TemperatureCelsius,
    int? OxygenSaturation);
