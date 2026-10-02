using MedConnect.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MedConnect.Hubs;

[Authorize]
public class ReferralHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var facilityId = Context.User?.FindFirst("FacilityId")?.Value;
        if (!string.IsNullOrEmpty(facilityId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, FacilityGroup(facilityId));
        }

        await base.OnConnectedAsync();
    }

    // Admin accounts have no FacilityId claim (they're facility-agnostic), so they
    // don't auto-join a group on connect. This lets the Referral Tracker's facility
    // picker join the selected facility's group on demand so Admin still gets live
    // updates for whichever facility they're currently viewing.
    public async Task JoinFacilityGroup(int facilityId)
    {
        if (Context.User?.IsInRole(Roles.Admin) != true)
        {
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, FacilityGroup(facilityId.ToString()));
    }

    public static string FacilityGroup(string facilityId) => $"facility-{facilityId}";
}
