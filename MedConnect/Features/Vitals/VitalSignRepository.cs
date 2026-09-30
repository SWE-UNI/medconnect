using MedConnect.Data;
using MedConnect.Domain;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Features.Vitals;

public class VitalSignRepository(ApplicationDbContext db) : IVitalSignRepository
{
    public Task<List<VitalSign>> GetByPatientIdAsync(int patientId) =>
        db.VitalSigns.AsNoTracking()
            .Include(v => v.RecordedByUser)
            .Where(v => v.PatientId == patientId)
            .OrderByDescending(v => v.RecordedAt)
            .ToListAsync();

    public async Task<VitalSign> AddAsync(VitalSign vitalSign)
    {
        db.VitalSigns.Add(vitalSign);
        await db.SaveChangesAsync();
        await db.Entry(vitalSign).Reference(v => v.RecordedByUser).LoadAsync();
        return vitalSign;
    }
}
