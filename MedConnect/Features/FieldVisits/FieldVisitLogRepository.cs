using MedConnect.Data;
using MedConnect.Domain;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Features.FieldVisits;

public class FieldVisitLogRepository(ApplicationDbContext db) : IFieldVisitLogRepository
{
    public Task<List<FieldVisitLog>> GetByCHWUserIdAsync(string chwUserId) =>
        db.FieldVisitLogs.AsNoTracking()
            .Include(l => l.CHWUser)
            .Where(l => l.CHWUserId == chwUserId)
            .OrderByDescending(l => l.VisitDate)
            .ToListAsync();

    public async Task<FieldVisitLog> AddAsync(FieldVisitLog log)
    {
        db.FieldVisitLogs.Add(log);
        await db.SaveChangesAsync();
        await db.Entry(log).Reference(l => l.CHWUser).LoadAsync();
        return log;
    }
}
