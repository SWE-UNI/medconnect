using MedConnect.Domain.Enums;

namespace MedConnect.Domain;

public class Facility
{
    public int FacilityId { get; set; }
    public required string Name { get; set; }
    public FacilityType Type { get; set; }
    public required string Location { get; set; }

    public ICollection<ApplicationUser> Users { get; set; } = [];
    public ICollection<Visit> Visits { get; set; } = [];
}
