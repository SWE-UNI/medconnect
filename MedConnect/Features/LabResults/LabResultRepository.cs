using MedConnect.Data;
using MedConnect.Domain;
using MedConnect.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Features.LabResults;

public class LabResultRepository(ApplicationDbContext db) : ILabResultRepository
{
    public async Task<(List<LabResult> Items, int TotalCount)> GetByPatientIdAsync(int patientId, int pageNumber, int pageSize)
    {
        var query = db.LabResults.AsNoTracking()
            .Include(l => l.Facility)
            .Include(l => l.OrderedByUser)
            .Include(l => l.ReviewedByUser)
            .Where(l => l.PatientId == patientId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.OrderedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }

    public Task<LabResult?> GetByIdAsync(int id) =>
        db.LabResults.AsNoTracking()
            .Include(l => l.Facility)
            .Include(l => l.OrderedByUser)
            .Include(l => l.ReviewedByUser)
            .FirstOrDefaultAsync(l => l.LabResultId == id);

    public async Task<LabResult> AddAsync(LabResult labResult)
    {
        db.LabResults.Add(labResult);
        await db.SaveChangesAsync();
        await db.Entry(labResult).Reference(l => l.Facility).LoadAsync();
        await db.Entry(labResult).Reference(l => l.OrderedByUser).LoadAsync();
        return labResult;
    }

    public async Task<LabResult?> CompleteAsync(int id, string resultText, string? referenceRange, string reviewerUserId)
    {
        var result = await db.LabResults.SingleOrDefaultAsync(l => l.LabResultId == id);
        if (result is null)
        {
            return null;
        }

        result.ResultText = resultText;
        if (!string.IsNullOrWhiteSpace(referenceRange))
        {
            result.ReferenceRange = referenceRange;
        }
        result.Status = LabResultStatus.Completed;
        result.ReviewedByUserId = reviewerUserId;
        result.CompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await db.Entry(result).Reference(l => l.Facility).LoadAsync();
        await db.Entry(result).Reference(l => l.OrderedByUser).LoadAsync();
        await db.Entry(result).Reference(l => l.ReviewedByUser).LoadAsync();
        return result;
    }
}