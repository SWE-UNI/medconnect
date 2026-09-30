namespace MedConnect.Client.Features.Vitals;

public record VitalSignDto(
    int VitalSignId,
    int PatientId,
    string RecordedByName,
    DateTime RecordedAt,
    int HeartRate,
    int BloodPressureSystolic,
    int BloodPressureDiastolic,
    double? TemperatureCelsius,
    int? OxygenSaturation,
    int AlertLevel);
