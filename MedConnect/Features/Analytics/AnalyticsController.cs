using MedConnect.Common.Constants;
using MedConnect.Data;
using MedConnect.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Features.Analytics;

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

[ApiController]
[Route("api/v1/analytics")]
[Authorize(Roles = Roles.Admin)]
public class AnalyticsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);
        var weekFromNow = now.AddDays(7);

        var staffRoles = new[] { "CHW", "Doctor", "Nurse", "FacilityAdmin", "Admin", "Receptionist" };

        var summary = new AnalyticsSummaryDto(
            TotalPatients: await db.Patients.CountAsync(),
            TotalFacilities: await db.Facilities.CountAsync(),
            TotalStaff: await db.UserRoles.AsNoTracking()
                .Where(ur => db.Roles.Any(r => r.Id == ur.RoleId && staffRoles.Contains(r.Name!)))
                .Select(ur => ur.UserId)
                .Distinct()
                .CountAsync(),
            FieldVisitsLogged: await db.FieldVisitLogs.CountAsync(),
            VisitsLast30Days: await db.Visits.CountAsync(v => v.Date >= thirtyDaysAgo),
            AppointmentsUpcoming7Days: await db.Appointments.CountAsync(a => a.ScheduledTime >= now && a.ScheduledTime <= weekFromNow),
            PrescriptionsIssued: await db.Prescriptions.CountAsync(),
            LabResultsCompleted: await db.LabResults.CountAsync(l => l.Status == LabResultStatus.Completed),
            ReferralsInProgress: await db.Referrals.CountAsync(r => r.Status != ReferralStatus.Pending && r.Status != ReferralStatus.Cancelled));

        return Ok(summary);
    }

    [HttpGet("visits/recent")]
    public async Task<IActionResult> RecentVisits([FromQuery] int days = 14)
    {
        days = Math.Clamp(days, 7, 60);
        var start = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-(days - 1));

        var rows = await db.Visits.AsNoTracking()
            .Where(v => v.Date >= start.ToDateTime(TimeOnly.MinValue))
            .GroupBy(v => new { v.Date.Year, v.Date.Month, v.Date.Day })
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => new DateOnly(g.Key.Year, g.Key.Month, g.Key.Day), g => g.Count);

        var series = Enumerable.Range(0, days)
            .Select(i => start.AddDays(i))
            .Select(d => new DailyVisitDto(d, rows.GetValueOrDefault(d)))
            .ToList();

        return Ok(series);
    }
}
