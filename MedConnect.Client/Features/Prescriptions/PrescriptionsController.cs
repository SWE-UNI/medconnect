using System.Security.Claims;
using MedConnect.Common.Constants;
using MedConnect.Features.Patients;
using MedConnect.Features.Prescriptions.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedConnect.Features.Prescriptions;

[ApiController]
[Route("api/v1/prescriptions")]
[Authorize]
public class PrescriptionsController(PrescriptionService service, PatientService patientService) : ControllerBase
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

    [HttpPost]
    [Authorize(Roles = Roles.ClinicalStaff)]
    public async Task<IActionResult> Create(CreatePrescriptionDto dto)
    {
        var created = await service.CreateAsync(dto, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return CreatedAtAction(nameof(GetByPatient), new { patientId = dto.PatientId }, created);
    }
}