using MedConnect.Domain;

namespace MedConnect.Features.Vitals;

public interface IVitalSignRepository
{
    Task<List<VitalSign>> GetByPatientIdAsync(int patientId);
    Task<VitalSign> AddAsync(VitalSign vitalSign);
}
