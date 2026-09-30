using MedConnect.Common.Constants;
using MedConnect.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Features.Audit;

public record AuditLogEntryDto(
    long Id,
    string? ActorUserId,
    string? ActorEmail,
    string Action,
    string? EntityType,
    string? EntityId,
    string? Details,
    DateTimeOffset PerformedAtUtc);

[ApiController]
[Route("api/v1/audit")]
[Authorize(Roles = Roles.Admin)]
public class AuditController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] int limit = 200)
    {
        limit = Math.Clamp(limit, 1, 500);

        var query = db.AuditLogs.AsNoTracking();
        if (!string.IsNullOrEmpty(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        if (!string.IsNullOrEmpty(entityId))
        {
            query = query.Where(a => a.EntityId == entityId);
        }

        var rows = await query.OrderByDescending(a => a.PerformedAtUtc)
            .Take(limit)
            .Select(a => new AuditLogEntryDto(
                a.Id, a.ActorUserId, a.ActorEmail, a.Action, a.EntityType, a.EntityId, a.Details, a.PerformedAtUtc))
            .ToListAsync();

        return Ok(rows);
    }
}
