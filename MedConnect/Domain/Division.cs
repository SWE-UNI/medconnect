namespace MedConnect.Domain;

public class Division
{
    public int DivisionId { get; set; }
    public required string Name { get; set; }

    public ICollection<ApplicationUser> Doctors { get; set; } = [];
}
