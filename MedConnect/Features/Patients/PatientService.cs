using MedConnect.Domain;
using MedConnect.Features.Patients.Dtos;
using MedConnect.Services;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Features.Patients;

public class PatientService(IPatientRepository repository, AuditLogger audit)
{
    public async Task<List<PatientDto>> GetAllAsync()
    {
        var patients = await repository.GetAllAsync();
        return patients.Select(ToDto).ToList();
    }

    public async Task<PatientDto?> GetByIdAsync(int id)
    {
        var patient = await repository.GetByIdAsync(id);
        return patient is null ? null : ToDto(patient);
    }

    public async Task<Patient?> GetEntityByIdAsync(int id) => await repository.GetByIdAsync(id);

    public async Task<PatientDto?> GetByUserIdAsync(string userId)
    {
        var patient = await repository.GetByUserIdAsync(userId);
        return patient is null ? null : ToDto(patient);
    }

    public async Task<PatientDto> CreateAsync(CreatePatientDto dto)
    {
        var patient = await repository.AddAsync(new Patient
        {
            NHISNumber = dto.NHISNumber,
            FullName = dto.FullName,
            DateOfBirth = dto.DateOfBirth,
            ContactInfo = dto.ContactInfo,
            BloodType = dto.BloodType
        });
        return ToDto(patient);
    }

    public async Task<(bool Succeeded, string? Error, PatientDto? Patient)> UpdateAsync(int id, CreatePatientDto dto)
    {
        try
        {
            var updated = await repository.UpdateAsync(id, new Patient
            {
                NHISNumber = dto.NHISNumber,
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                ContactInfo = dto.ContactInfo,
                BloodType = dto.BloodType
            });
            return updated is null ? (false, "not-found", null) : (true, null, ToDto(updated));
        }
        catch (DbUpdateException)
        {
            return (false, "That NHIS number is already registered to another patient.", null);
        }
    }

    public async Task<PatientConsentDto?> GetConsentAsync(int id)
    {
        var patient = await repository.GetByIdAsync(id);
        return patient is null ? null : new PatientConsentDto(patient.DataSharingConsent ?? false, patient.ConsentUpdatedAt);
    }

    public async Task<(bool Succeeded, string? Error, PatientConsentDto? Consent)> SetConsentAsync(int id, bool consent, string? actorUserId = null)
    {
        var updated = await repository.SetConsentAsync(id, consent, DateTimeOffset.UtcNow);
        if (updated is null)
        {
            return (false, "not-found", null);
        }

        await audit.LogAsync(actorUserId, "ConsentUpdated", "Patient", id.ToString(), consent ? "Granted" : "Revoked");
        return (true, null, new PatientConsentDto(consent, updated.ConsentUpdatedAt));
    }

    private static PatientDto ToDto(Patient p) =>
        new(p.PatientId, p.NHISNumber, p.FullName, p.DateOfBirth, p.ContactInfo, p.BloodType);
}
