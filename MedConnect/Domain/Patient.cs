namespace MedConnect.Domain;

public class Patient
{
    public int PatientId { get; set; }
    public required string NHISNumber { get; set; }
    public required string FullName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public required string ContactInfo { get; set; }
    public string? BloodType { get; set; }

    public bool? DataSharingConsent { get; set; }
    public DateTimeOffset? ConsentUpdatedAt { get; set; }

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public ICollection<Visit> Visits { get; set; } = [];
    public ICollection<Referral> Referrals { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<FieldVisitLog> FieldVisitLogs { get; set; } = [];
    public ICollection<VitalSign> VitalSigns { get; set; } = [];
}
