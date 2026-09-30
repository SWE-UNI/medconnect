namespace MedConnect.Domain;

public class Visit
{
    public int VisitId { get; set; }

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int FacilityId { get; set; }
    public Facility Facility { get; set; } = null!;

    public DateTime Date { get; set; }
    public required string Diagnosis { get; set; }
    public string? PrescriptionRef { get; set; }
}
