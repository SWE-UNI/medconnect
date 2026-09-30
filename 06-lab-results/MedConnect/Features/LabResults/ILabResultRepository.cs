using MedConnect.Domain;

namespace MedConnect.Features.LabResults;

public interface ILabResultRepository
{
    Task<(List<LabResult> Items, int TotalCount)> GetByPatientIdAsync(int patientId, int pageNumber, int pageSize);
    Task<LabResult?> GetByIdAsync(int id);
    Task<LabResult> AddAsync(LabResult labResult);
    Task<LabResult?> CompleteAsync(int id, string resultText, string? referenceRange, string reviewerUserId);
}