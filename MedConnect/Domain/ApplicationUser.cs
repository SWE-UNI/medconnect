using Microsoft.AspNetCore.Identity;

namespace MedConnect.Domain;

public class ApplicationUser : IdentityUser
{
    public required string FullName { get; set; }

    public int? FacilityId { get; set; }
    public Facility? Facility { get; set; }

    /// Only meaningful for users in the Doctor role.
    public int? DivisionId { get; set; }
    public Division? Division { get; set; }

    /// Base32 TOTP secret stored when the user enables two-factor authentication.
    public string? AuthenticatorKey { get; set; }

    public ICollection<FieldVisitLog> FieldVisitLogs { get; set; } = [];
    public ICollection<Appointment> CreatedAppointments { get; set; } = [];
}
