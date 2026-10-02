using MedConnect.Data;
using MedConnect.Domain;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Features.Facilities;

public class FacilityRepository(ApplicationDbContext db) : IFacilityRepository
{
    public Task<List<Facility>> GetAllAsync() =>
        db.Facilities.AsNoTracking().OrderBy(f => f.Name).ToListAsync();

    public Task<Facility?> GetByIdAsync(int id) =>
        db.Facilities.AsNoTracking().FirstOrDefaultAsync(f => f.FacilityId == id);

    public async Task<Facility> AddAsync(Facility facility)
    {
        db.Facilities.Add(facility);
        await db.SaveChangesAsync();
        return facility;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var facility = await db.Facilities.FindAsync(id);
        if (facility is null)
        {
            return false;
        }

        db.Facilities.Remove(facility);
        await db.SaveChangesAsync();
        return true;
    }
}
