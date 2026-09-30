using MedConnect.Domain;

namespace MedConnect.Features.Prescriptions;

public interface IPrescriptionRepository
{
    Task<List<Prescription>> GetByPatientIdAsync(int patientId);
    Task<Prescription> AddAsync(Prescription prescription);
}