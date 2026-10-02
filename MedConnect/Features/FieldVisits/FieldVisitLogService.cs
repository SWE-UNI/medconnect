using MedConnect.Domain;
using MedConnect.Features.FieldVisits.Dtos;

namespace MedConnect.Features.FieldVisits;

public class FieldVisitLogService(IFieldVisitLogRepository repository)
{
    public async Task<List<FieldVisitLogDto>> GetByCHWUserIdAsync(string chwUserId)
    {
        var logs = await repository.GetByCHWUserIdAsync(chwUserId);
        return logs.Select(ToDto).ToList();
    }

    public async Task<FieldVisitLogDto> CreateAsync(CreateFieldVisitLogDto dto, string chwUserId)
    {
        var log = await repository.AddAsync(new FieldVisitLog
        {
            CHWUserId = chwUserId,
            PatientId = dto.PatientId,
            HouseholdData = dto.HouseholdData,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            VisitDate = dto.VisitDate,
            PregnancyStatus = (MedConnect.Domain.Enums.PregnancyStatus?)dto.PregnancyStatus,
            GestationalAgeWeeks = dto.GestationalAgeWeeks,
            AntenatalVisits = dto.AntenatalVisits,
            ChildAgeMonths = dto.ChildAgeMonths,
            ChildWeightKg = dto.ChildWeightKg,
            ImmunizationsUpToDate = dto.ImmunizationsUpToDate,
            DangerSigns = dto.DangerSigns
        });
        return ToDto(log);
    }

    private static FieldVisitLogDto ToDto(FieldVisitLog l) => new(
        l.LogId,
        l.CHWUserId,
        l.CHWUser.FullName,
        l.PatientId,
        l.HouseholdData,
        l.Latitude,
        l.Longitude,
        l.VisitDate,
        (int?)l.PregnancyStatus,
        l.GestationalAgeWeeks,
        l.AntenatalVisits,
        l.ChildAgeMonths,
        l.ChildWeightKg,
        l.ImmunizationsUpToDate,
        l.DangerSigns);
}
