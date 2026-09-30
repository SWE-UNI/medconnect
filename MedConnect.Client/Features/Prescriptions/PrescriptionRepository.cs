using MedConnect.Data;
using MedConnect.Domain;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Features.Prescriptions;

public class PrescriptionRepository(ApplicationDbContext db) : IPrescriptionRepository
{
    public Task<List<Prescription>> GetByPatientIdAsync(int patientId) =>
        db.Prescriptions.AsNoTracking()
            .Include(p => p.PrescribedByUser)
            .Include(p => p.Visit)
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.PrescribedAt)
            .ToListAsync();

    public async Task<Prescription> AddAsync(Prescription prescription)
    {
        db.Prescriptions.Add(prescription);
        await db.SaveChangesAsync();
        await db.Entry(prescription).Reference(p => p.PrescribedByUser).LoadAsync();
        await db.Entry(prescription).Reference(p => p.Visit).LoadAsync();
        return prescription;
    }
}