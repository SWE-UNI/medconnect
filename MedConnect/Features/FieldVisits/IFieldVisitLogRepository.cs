using MedConnect.Domain;

namespace MedConnect.Features.FieldVisits;

public interface IFieldVisitLogRepository
{
    Task<List<FieldVisitLog>> GetByCHWUserIdAsync(string chwUserId);
    Task<FieldVisitLog> AddAsync(FieldVisitLog log);
}
