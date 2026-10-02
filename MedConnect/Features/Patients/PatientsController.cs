using System.Security.Claims;
using MedConnect.Common.Constants;
using MedConnect.Features.Patients.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedConnect.Features.Patients;

[ApiController]
[Route("api/v1/patients")]
[Authorize]
public class PatientsController(PatientService service) : ControllerBase
{
    // AppointmentStaff (not just ClinicalStaff) because Receptionists need to
    // find a patient to book an appointment for, even though they can't see
    // clinical records (visits/referrals/vitals) for that patient.
    [HttpGet]
    [Authorize(Roles = Roles.AppointmentStaff)]
    public async Task<IActionResult> GetAll() => Ok(await service.GetAllAsync());

    [HttpGet("me")]
    public async Task<IActionResult> GetMine()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var patient = userId is null ? null : await service.GetByUserIdAsync(userId);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (User.IsInRole(Roles.Patient))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var owned = await service.GetEntityByIdAsync(id);
            if (owned is null || owned.UserId != userId)
            {
                return Forbid();
            }
        }

        var patient = await service.GetByIdAsync(id);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpPost]
    [Authorize(Roles = Roles.ClinicalStaff)]
    public async Task<IActionResult> Create(CreatePatientDto dto)
    {
        var patient = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = patient.PatientId }, patient);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(int id, CreatePatientDto dto)
    {
        var (succeeded, error, patient) = await service.UpdateAsync(id, dto);
        if (succeeded)
        {
            return Ok(patient);
        }

        return error == "not-found" ? NotFound() : BadRequest(new { error });
    }

    [HttpGet("{id:int}/consent")]
    public async Task<IActionResult> GetConsent(int id)
    {
        if (User.IsInRole(Roles.Patient))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var owned = await service.GetEntityByIdAsync(id);
            if (owned is null || owned.UserId != userId)
            {
                return Forbid();
            }
        }
        else if (!User.IsInRole(Roles.ClinicalStaff))
        {
            // Only the owning patient and clinical staff may view consent.
            return Forbid();
        }

        var consent = await service.GetConsentAsync(id);
        return consent is null ? NotFound() : Ok(consent);
    }

    [HttpPut("{id:int}/consent")]
    public async Task<IActionResult> SetConsent(int id, SetConsentDto dto)
    {
        if (User.IsInRole(Roles.Patient))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var owned = await service.GetEntityByIdAsync(id);
            if (owned is null || owned.UserId != userId)
            {
                return Forbid();
            }
        }
        else
        {
            // Consent is patient-driven; staff cannot give consent on their behalf.
            return Forbid();
        }

        var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var (succeeded, error, consent) = await service.SetConsentAsync(id, dto.Consent, actorUserId);
        return succeeded ? Ok(consent) : NotFound();
    }
}
