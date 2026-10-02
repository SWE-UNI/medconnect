using MedConnect.Data;
using MedConnect.Domain;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Features.Divisions;

public class DivisionRepository(ApplicationDbContext db) : IDivisionRepository
{
    public Task<List<Division>> GetAllAsync() =>
        db.Divisions.AsNoTracking().OrderBy(d => d.Name).ToListAsync();

    public Task<Division?> GetByIdAsync(int id) =>
        db.Divisions.AsNoTracking().FirstOrDefaultAsync(d => d.DivisionId == id);

    public Task<bool> ExistsByNameAsync(string name) =>
        db.Divisions.AsNoTracking().AnyAsync(d => d.Name == name);

    public async Task<Division> AddAsync(Division division)
    {
        db.Divisions.Add(division);
        await db.SaveChangesAsync();
        return division;
    }
}
