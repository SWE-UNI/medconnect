using MedConnect.Data;
using MedConnect.Domain;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Features.Referrals;

public class ReferralRepository(ApplicationDbContext db) : IReferralRepository
{
    private static IQueryable<Referral> WithFacilities(ApplicationDbContext db) =>
        db.Referrals.Include(r => r.FromFacility).Include(r => r.ToFacility);

    public Task<List<Referral>> GetByPatientIdAsync(int patientId) =>
        WithFacilities(db).AsNoTracking()
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();

    public Task<List<Referral>> GetIncomingByFacilityIdAsync(int facilityId) =>
        WithFacilities(db).AsNoTracking()
            .Where(r => r.ToFacilityId == facilityId)
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();

    public Task<Referral?> GetByIdAsync(int id) =>
        WithFacilities(db).FirstOrDefaultAsync(r => r.ReferralId == id);

    public async Task<Referral> AddAsync(Referral referral)
    {
        db.Referrals.Add(referral);
        await db.SaveChangesAsync();
        await db.Entry(referral).Reference(r => r.FromFacility).LoadAsync();
        await db.Entry(referral).Reference(r => r.ToFacility).LoadAsync();
        return referral;
    }

    public async Task UpdateAsync(Referral referral)
    {
        db.Referrals.Update(referral);
        await db.SaveChangesAsync();
    }
}
