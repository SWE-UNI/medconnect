using MedConnect.Domain;
using MedConnect.Features.Referrals.Dtos;
using MedConnect.Hubs;
using MedConnect.Services;
using Microsoft.AspNetCore.SignalR;

namespace MedConnect.Features.Referrals;

public class ReferralService(IReferralRepository repository, IHubContext<ReferralHub> hub, NotificationService notifications, AuditLogger audit)
{
    public async Task<List<ReferralDto>> GetByPatientIdAsync(int patientId)
    {
        var referrals = await repository.GetByPatientIdAsync(patientId);
        return referrals.Select(ToDto).ToList();
    }

    public async Task<List<ReferralDto>> GetIncomingByFacilityIdAsync(int facilityId)
    {
        var referrals = await repository.GetIncomingByFacilityIdAsync(facilityId);
        return referrals.Select(ToDto).ToList();
    }

    public async Task<ReferralDto> CreateAsync(CreateReferralDto dto, string? actorUserId = null)
    {
        var referral = await repository.AddAsync(new Referral
        {
            PatientId = dto.PatientId,
            FromFacilityId = dto.FromFacilityId,
            ToFacilityId = dto.ToFacilityId,
            Timestamp = DateTime.UtcNow
        });

        var result = ToDto(referral);
        await hub.Clients.Group(ReferralHub.FacilityGroup(referral.ToFacilityId.ToString()))
            .SendAsync("ReferralCreated", result);
        await notifications.SendReferralCreatedAsync(referral.PatientId, referral.ToFacility.Name);
        await audit.LogAsync(actorUserId, "ReferralCreated", "Referral", referral.ReferralId.ToString(), $"to facility {referral.ToFacilityId}");
        return result;
    }

    public async Task<ReferralDto?> UpdateStatusAsync(int id, UpdateReferralStatusDto dto, string? actorUserId = null)
    {
        var referral = await repository.GetByIdAsync(id);
        if (referral is null)
        {
            return null;
        }

        referral.Status = dto.Status;
        await repository.UpdateAsync(referral);

        var result = ToDto(referral);
        await hub.Clients.Group(ReferralHub.FacilityGroup(referral.ToFacilityId.ToString()))
            .SendAsync("ReferralStatusChanged", result);
        await hub.Clients.Group(ReferralHub.FacilityGroup(referral.FromFacilityId.ToString()))
            .SendAsync("ReferralStatusChanged", result);
        await notifications.SendReferralStatusChangedAsync(referral.PatientId, dto.Status.ToString());
        await audit.LogAsync(actorUserId, "ReferralStatusChanged", "Referral", referral.ReferralId.ToString(), dto.Status.ToString());
        return result;
    }

    private static ReferralDto ToDto(Referral r) => new(
        r.ReferralId,
        r.PatientId,
        r.FromFacilityId,
        r.FromFacility.Name,
        r.ToFacilityId,
        r.ToFacility.Name,
        r.Status,
        r.Timestamp);
}
