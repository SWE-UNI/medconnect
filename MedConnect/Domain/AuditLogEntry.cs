namespace MedConnect.Domain;

public class AuditLogEntry
{
    public long Id { get; set; }
    public string? ActorUserId { get; set; }
    public string? ActorEmail { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public DateTimeOffset PerformedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
