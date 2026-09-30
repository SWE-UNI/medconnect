using MedConnect.Data;
using MedConnect.Domain;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Features.Visits;

public class VisitRepository(ApplicationDbContext db) : IVisitRepository
{
    public Task<List<Visit>> GetByPatientIdAsync(int patientId) =>
        db.Visits.AsNoTracking()
            .Include(v => v.Facility)
            .Where(v => v.PatientId == patientId)
            .OrderByDescending(v => v.Date)
            .ToListAsync();

    public async Task<Visit> AddAsync(Visit visit)
    {
        db.Visits.Add(visit);
        await db.SaveChangesAsync();
        await db.Entry(visit).Reference(v => v.Facility).LoadAsync();
        return visit;
    }
}
