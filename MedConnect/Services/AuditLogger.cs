using MedConnect.Data;
using MedConnect.Domain;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Services;

/// Appends an immutable who/what/when row for security- and record-relevant
/// actions. Writes are independent of the caller's own unit of work so a
/// failing audit write can never fail the business operation.
public class AuditLogger(ApplicationDbContext db, ILogger<AuditLogger> logger)
{
    public async Task LogAsync(string? actorUserId, string action, string? entityType = null, string? entityId = null, string? details = null)
    {
        try
        {
            var actorEmail = actorUserId is null
                ? null
                : await db.Users.AsNoTracking()
                    .Where(u => u.Id == actorUserId)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();

            db.AuditLogs.Add(new AuditLogEntry
            {
                ActorUserId = actorUserId,
                ActorEmail = actorEmail,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                PerformedAtUtc = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write audit entry for {Action}", action);
        }
    }
}
