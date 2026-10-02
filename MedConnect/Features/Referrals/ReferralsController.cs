using System.Security.Claims;
using MedConnect.Common.Constants;
using MedConnect.Features.Patients;
using MedConnect.Features.Referrals.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedConnect.Features.Referrals;

[ApiController]
[Route("api/v1/referrals")]
[Authorize]
public class ReferralsController(ReferralService service, PatientService patientService) : ControllerBase
{
    [HttpGet("patient/{patientId:int}")]
    public async Task<IActionResult> GetByPatient(int patientId)
    {
        if (User.IsInRole(Roles.Patient))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var owned = await patientService.GetEntityByIdAsync(patientId);
            if (owned is null || owned.UserId != userId)
            {
                return Forbid();
            }
        }

        return Ok(await service.GetByPatientIdAsync(patientId));
    }

    [HttpGet("facility/{facilityId:int}/incoming")]
    [Authorize(Roles = Roles.ClinicalStaff)]
    public async Task<IActionResult> GetIncoming(int facilityId)
    {
        if (!User.IsInRole(Roles.Admin))
        {
            var claimFacilityId = User.FindFirstValue("FacilityId");
            if (claimFacilityId != facilityId.ToString())
            {
                return Forbid();
            }
        }

        return Ok(await service.GetIncomingByFacilityIdAsync(facilityId));
    }

    [HttpPost]
    [Authorize(Roles = Roles.ClinicalStaff)]
    public async Task<IActionResult> Create(CreateReferralDto dto)
    {
        var created = await service.CreateAsync(dto, User.FindFirstValue(ClaimTypes.NameIdentifier));
        return CreatedAtAction(nameof(GetByPatient), new { patientId = dto.PatientId }, created);
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = Roles.ClinicalStaff)]
    public async Task<IActionResult> UpdateStatus(int id, UpdateReferralStatusDto dto)
    {
        var referral = await service.UpdateStatusAsync(id, dto, User.FindFirstValue(ClaimTypes.NameIdentifier));
        return referral is null ? NotFound() : Ok(referral);
    }
}
