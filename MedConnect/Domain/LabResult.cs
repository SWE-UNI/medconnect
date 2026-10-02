using MedConnect.Domain.Enums;

namespace MedConnect.Domain;

public class LabResult
{
    public int LabResultId { get; set; }

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int FacilityId { get; set; }
    public Facility Facility { get; set; } = null!;

    public required string TestName { get; set; }
    public string? ResultText { get; set; }
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public LabResultStatus Status { get; set; } = LabResultStatus.Ordered;

    public required string OrderedByUserId { get; set; }
    public ApplicationUser OrderedByUser { get; set; } = null!;

    public string? ReviewedByUserId { get; set; }
    public ApplicationUser? ReviewedByUser { get; set; }

    public DateTime OrderedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}