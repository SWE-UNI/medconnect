using System.Security.Claims;
using MedConnect.Common.Constants;
using MedConnect.Features.Patients;
using MedConnect.Features.Visits.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedConnect.Features.Visits;

[ApiController]
[Route("api/v1/visits")]
[Authorize]
public class VisitsController(VisitService service, PatientService patientService) : ControllerBase
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
    public async Task<IActionResult> Create(CreateVisitDto dto)
    {
        var visit = await service.CreateAsync(dto);
        return Ok(visit);
    }
}
