using MedConnect.Client.Services;

namespace MedConnect.Client.Features.Audit;

public record AuditLogEntryDto(
    long Id,
    string? ActorUserId,
    string? ActorEmail,
    string Action,
    string? EntityType,
    string? EntityId,
    string? Details,
    DateTimeOffset PerformedAtUtc);

public class AuditApiClient(HttpClient http)
{
    public Task<List<AuditLogEntryDto>> GetAsync(string? entityType = null, string? entityId = null, int limit = 200)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query.Add($"entityType={Uri.EscapeDataString(entityType)}");
        }

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            query.Add($"entityId={Uri.EscapeDataString(entityId)}");
        }

        query.Add($"limit={limit}");

        return SafeApi.GetListAsync<AuditLogEntryDto>(http, $"api/v1/audit?{string.Join("&", query)}");
    }
}
