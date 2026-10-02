using MedConnect.Domain;

namespace MedConnect.Features.Facilities;

public interface IFacilityRepository
{
    Task<List<Facility>> GetAllAsync();
    Task<Facility?> GetByIdAsync(int id);
    Task<Facility> AddAsync(Facility facility);
    Task<bool> DeleteAsync(int id);
}
