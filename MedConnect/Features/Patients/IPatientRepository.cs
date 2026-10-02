using MedConnect.Domain;

namespace MedConnect.Features.Patients;

public interface IPatientRepository
{
    Task<List<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(int id);
    Task<Patient?> GetByUserIdAsync(string userId);
    Task<Patient> AddAsync(Patient patient);
    Task<Patient?> UpdateAsync(int id, Patient updated);
    Task<Patient?> SetConsentAsync(int id, bool consent, DateTimeOffset at);
}
