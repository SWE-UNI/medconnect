using MedConnect.Domain;
using MedConnect.Features.Prescriptions.Dtos;
using MedConnect.Services;

namespace MedConnect.Features.Prescriptions;

public class PrescriptionService(IPrescriptionRepository repository, AuditLogger audit)
{
    public async Task<List<PrescriptionDto>> GetByPatientIdAsync(int patientId)
    {
        var prescriptions = await repository.GetByPatientIdAsync(patientId);
        return prescriptions.Select(ToDto).ToList();
    }

    public async Task<PrescriptionDto> CreateAsync(CreatePrescriptionDto dto, string prescribedByUserId)
    {
        var prescription = await repository.AddAsync(new Prescription
        {
            PatientId = dto.PatientId,
            VisitId = dto.VisitId,
            Medication = dto.Medication,
            Dosage = dto.Dosage,
            Frequency = dto.Frequency,
            DurationDays = dto.DurationDays,
            Instructions = dto.Instructions,
            PrescribedByUserId = prescribedByUserId,
            PrescribedAt = DateTime.UtcNow
        });
        await audit.LogAsync(prescribedByUserId, "PrescriptionCreated", "Prescription", prescription.PrescriptionId.ToString(), $"{dto.Medication} {dto.Dosage}");
        return ToDto(prescription);
    }

    private static PrescriptionDto ToDto(Prescription p) =>
        new(p.PrescriptionId, p.PatientId, p.VisitId, p.Medication, p.Dosage, p.Frequency,
            p.DurationDays, p.Instructions, p.PrescribedByUser.FullName, p.PrescribedAt);
}