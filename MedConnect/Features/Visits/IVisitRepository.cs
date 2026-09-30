using MedConnect.Domain;

namespace MedConnect.Features.Visits;

public interface IVisitRepository
{
    Task<List<Visit>> GetByPatientIdAsync(int patientId);
    Task<Visit> AddAsync(Visit visit);
}
