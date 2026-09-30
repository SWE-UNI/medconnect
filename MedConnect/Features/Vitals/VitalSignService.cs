using MedConnect.Domain;
using MedConnect.Domain.Enums;
using MedConnect.Features.Vitals.Dtos;

namespace MedConnect.Features.Vitals;

public class VitalSignService(IVitalSignRepository repository)
{
    public async Task<List<VitalSignDto>> GetByPatientIdAsync(int patientId)
    {
        var vitalSigns = await repository.GetByPatientIdAsync(patientId);
        return vitalSigns.Select(ToDto).ToList();
    }

    public async Task<VitalSignDto> CreateAsync(CreateVitalSignDto dto, string recordedByUserId)
    {
        var vitalSign = await repository.AddAsync(new VitalSign
        {
            PatientId = dto.PatientId,
            RecordedByUserId = recordedByUserId,
            RecordedAt = DateTime.UtcNow,
            HeartRate = dto.HeartRate,
            BloodPressureSystolic = dto.BloodPressureSystolic,
            BloodPressureDiastolic = dto.BloodPressureDiastolic,
            TemperatureCelsius = dto.TemperatureCelsius,
            OxygenSaturation = dto.OxygenSaturation,
            AlertLevel = ComputeAlertLevel(dto)
        });
        return ToDto(vitalSign);
    }

    private static AlertLevel ComputeAlertLevel(CreateVitalSignDto dto)
    {
        var isCritical =
            dto.HeartRate is >= 130 or <= 40 ||
            dto.BloodPressureSystolic is >= 180 or <= 85 ||
            dto.BloodPressureDiastolic >= 120 ||
            dto.OxygenSaturation is <= 90;

        if (isCritical)
        {
            return AlertLevel.Critical;
        }

        var isWarning =
            dto.HeartRate is >= 100 or <= 50 ||
            dto.BloodPressureSystolic is >= 140 or <= 95 ||
            dto.BloodPressureDiastolic >= 90 ||
            dto.OxygenSaturation is <= 94;

        return isWarning ? AlertLevel.Warning : AlertLevel.Normal;
    }

    private static VitalSignDto ToDto(VitalSign v) => new(
        v.VitalSignId,
        v.PatientId,
        v.RecordedByUser.FullName,
        v.RecordedAt,
        v.HeartRate,
        v.BloodPressureSystolic,
        v.BloodPressureDiastolic,
        v.TemperatureCelsius,
        v.OxygenSaturation,
        (int)v.AlertLevel);
}
