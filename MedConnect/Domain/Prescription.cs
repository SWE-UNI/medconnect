namespace MedConnect.Domain;

public class Prescription
{
    public int PrescriptionId { get; set; }

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int VisitId { get; set; }
    public Visit Visit { get; set; } = null!;

    public required string Medication { get; set; }
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public int? DurationDays { get; set; }
    public string? Instructions { get; set; }

    public required string PrescribedByUserId { get; set; }
    public ApplicationUser PrescribedByUser { get; set; } = null!;

    public DateTime PrescribedAt { get; set; }
}