using MedConnect.Data;
using MedConnect.Domain;

namespace MedConnect.Features.Identity;

public static class DivisionSeeder
{
    // Standard set of medical specializations a Doctor can be assigned to.
    // Admins can add more via POST /api/v1/divisions as new ones come up.
    private static readonly string[] Defaults =
    [
        "General Practice / Family Medicine",
        "Internal Medicine",
        "Pediatrics",
        "Obstetrics & Gynecology",
        "General Surgery",
        "Orthopedic Surgery",
        "Cardiology",
        "Dermatology",
        "Neurology",
        "Psychiatry",
        "Radiology",
        "Anesthesiology",
        "Emergency Medicine",
        "Ophthalmology",
        "Otolaryngology (ENT)",
        "Urology",
        "Oncology",
        "Pulmonology",
        "Gastroenterology",
        "Endocrinology",
        "Nephrology",
        "Infectious Disease",
        "Pathology",
        "Public Health & Preventive Medicine",
        "Dental & Oral Health",
        "Physiotherapy & Rehabilitation",
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var existing = db.Divisions.Select(d => d.Name).ToHashSet();

        foreach (var name in Defaults)
        {
            if (!existing.Contains(name))
            {
                db.Divisions.Add(new Division { Name = name });
            }
        }

        await db.SaveChangesAsync();
    }
}
