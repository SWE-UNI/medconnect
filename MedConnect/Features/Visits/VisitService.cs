using MedConnect.Domain;
using MedConnect.Features.Visits.Dtos;

namespace MedConnect.Features.Visits;

public class VisitService(IVisitRepository repository)
{
    public async Task<List<VisitDto>> GetByPatientIdAsync(int patientId)
    {
        var visits = await repository.GetByPatientIdAsync(patientId);
        return visits.Select(ToDto).ToList();
    }

    public async Task<VisitDto> CreateAsync(CreateVisitDto dto)
    {
        var visit = await repository.AddAsync(new Visit
        {
            PatientId = dto.PatientId,
            FacilityId = dto.FacilityId,
            Date = dto.Date,
            Diagnosis = dto.Diagnosis,
            PrescriptionRef = dto.PrescriptionRef
        });
        return ToDto(visit);
    }

    private static VisitDto ToDto(Visit v) =>
        new(v.VisitId, v.PatientId, v.FacilityId, v.Facility.Name, v.Date, v.Diagnosis, v.PrescriptionRef);
}
