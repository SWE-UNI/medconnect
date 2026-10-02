using MedConnect.Domain.Enums;

namespace MedConnect.Domain;

public class Referral
{
    public int ReferralId { get; set; }

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int FromFacilityId { get; set; }
    public Facility FromFacility { get; set; } = null!;

    public int ToFacilityId { get; set; }
    public Facility ToFacility { get; set; } = null!;

    public ReferralStatus Status { get; set; } = ReferralStatus.Pending;
    public DateTime Timestamp { get; set; }
}
