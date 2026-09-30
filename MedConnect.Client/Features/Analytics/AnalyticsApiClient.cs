using MedConnect.Client.Services;

namespace MedConnect.Client.Features.Analytics;

public record AnalyticsSummaryDto(
    int TotalPatients,
    int TotalFacilities,
    int TotalStaff,
    int FieldVisitsLogged,
    int VisitsLast30Days,
    int AppointmentsUpcoming7Days,
    int PrescriptionsIssued,
    int LabResultsCompleted,
    int ReferralsInProgress);

public record DailyVisitDto(DateOnly Date, int Count);

public class AnalyticsApiClient(HttpClient http)
{
    public async Task<AnalyticsSummaryDto> GetSummaryAsync() =>
        await SafeApi.GetNullableAsync<AnalyticsSummaryDto>(http, "api/v1/analytics/summary")
        ?? new AnalyticsSummaryDto(0, 0, 0, 0, 0, 0, 0, 0, 0);

    public Task<List<DailyVisitDto>> GetRecentVisitsAsync(int days = 14) =>
        SafeApi.GetListAsync<DailyVisitDto>(http, $"api/v1/analytics/visits/recent?days={days}");
}
