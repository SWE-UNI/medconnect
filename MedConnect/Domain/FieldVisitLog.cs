using MedConnect.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MedConnect.Domain;

public class FieldVisitLog
{
    [Key]
    public int LogId { get; set; }

    public required string CHWUserId { get; set; }
    public ApplicationUser CHWUser { get; set; } = null!;

    public int? PatientId { get; set; }
    public Patient? Patient { get; set; }

    public required string HouseholdData { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime VisitDate { get; set; }

    public PregnancyStatus? PregnancyStatus { get; set; }
    public int? GestationalAgeWeeks { get; set; }
    public int? AntenatalVisits { get; set; }
    public int? ChildAgeMonths { get; set; }
    public decimal? ChildWeightKg { get; set; }
    public bool? ImmunizationsUpToDate { get; set; }
    public string? DangerSigns { get; set; }
}
