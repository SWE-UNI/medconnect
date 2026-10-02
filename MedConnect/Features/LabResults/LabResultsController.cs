using System.Security.Claims;
using MedConnect.Common;
using MedConnect.Common.Constants;
using MedConnect.Features.LabResults.Dtos;
using MedConnect.Features.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedConnect.Features.LabResults;

[ApiController]
[Route("api/v1/labresults")]
[Authorize]
public class LabResultsController(LabResultService service, PatientService patientService) : ControllerBase
{
    [HttpGet("patient/{patientId:int}")]
    public async Task<IActionResult> GetByPatient(int patientId, int pageNumber = 1, int pageSize = 100)
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

        return Ok(await service.GetByPatientIdAsync(patientId, Paging.NormalizePageNumber(pageNumber), Paging.NormalizePageSize(pageSize)));
    }

    [HttpPost]
    [Authorize(Roles = Roles.ClinicalStaff)]
    public async Task<IActionResult> Create(CreateLabResultDto dto)
    {
        var created = await service.CreateAsync(dto, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return CreatedAtAction(nameof(GetByPatient), new { patientId = dto.PatientId }, created);
    }

    [HttpPost("{id:int}/complete")]
    [Authorize(Roles = Roles.ClinicalStaff)]
    public async Task<IActionResult> Complete(int id, CompleteLabResultDto dto)
    {
        var updated = await service.CompleteAsync(id, dto, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (updated is null)
        {
            return NotFound();
        }
        return Ok(updated);
    }
}