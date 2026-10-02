using MedConnect.Data;
using MedConnect.Domain;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Features.Patients;

public class PatientRepository(ApplicationDbContext db) : IPatientRepository
{
    public Task<List<Patient>> GetAllAsync() =>
        db.Patients.AsNoTracking().OrderBy(p => p.FullName).ToListAsync();

    public Task<Patient?> GetByIdAsync(int id) =>
        db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.PatientId == id);

    public Task<Patient?> GetByUserIdAsync(string userId) =>
        db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task<Patient> AddAsync(Patient patient)
    {
        db.Patients.Add(patient);
        await db.SaveChangesAsync();
        return patient;
    }

    public async Task<Patient?> UpdateAsync(int id, Patient updated)
    {
        var patient = await db.Patients.FirstOrDefaultAsync(p => p.PatientId == id);
        if (patient is null)
        {
            return null;
        }

        patient.NHISNumber = updated.NHISNumber;
        patient.FullName = updated.FullName;
        patient.DateOfBirth = updated.DateOfBirth;
        patient.ContactInfo = updated.ContactInfo;
        patient.BloodType = updated.BloodType;

        // Keep the linked login account's display name in sync — it's what
        // actually shows in the header/greeting, separate from this record.
        if (patient.UserId is not null)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == patient.UserId);
            if (user is not null)
            {
                user.FullName = updated.FullName;
            }
        }

        await db.SaveChangesAsync();
        return patient;
    }

    public async Task<Patient?> SetConsentAsync(int id, bool consent, DateTimeOffset at)
    {
        var patient = await db.Patients.FirstOrDefaultAsync(p => p.PatientId == id);
        if (patient is null)
        {
            return null;
        }

        patient.DataSharingConsent = consent;
        patient.ConsentUpdatedAt = at;
        await db.SaveChangesAsync();
        return patient;
    }
}
