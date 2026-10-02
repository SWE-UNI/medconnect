using MedConnect.Domain;

namespace MedConnect.Features.Divisions;

public interface IDivisionRepository
{
    Task<List<Division>> GetAllAsync();
    Task<Division?> GetByIdAsync(int id);
    Task<bool> ExistsByNameAsync(string name);
    Task<Division> AddAsync(Division division);
}
