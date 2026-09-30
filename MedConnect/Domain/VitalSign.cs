using MedConnect.Domain.Enums;

namespace MedConnect.Domain;

public class VitalSign
{
    public int VitalSignId { get; set; }

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public required string RecordedByUserId { get; set; }
    public ApplicationUser RecordedByUser { get; set; } = null!;

    public DateTime RecordedAt { get; set; }
    public int HeartRate { get; set; }
    public int BloodPressureSystolic { get; set; }
    public int BloodPressureDiastolic { get; set; }
    public double? TemperatureCelsius { get; set; }
    public int? OxygenSaturation { get; set; }
    public AlertLevel AlertLevel { get; set; } = AlertLevel.Normal;
}
