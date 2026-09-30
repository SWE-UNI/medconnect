using MedConnect.Common;
using MedConnect.Domain;
using MedConnect.Domain.Enums;
using MedConnect.Features.LabResults.Dtos;
using MedConnect.Services;

namespace MedConnect.Features.LabResults;

public class LabResultService(ILabResultRepository repository, NotificationService notifications, AuditLogger audit)
{
    public async Task<PagedResult<LabResultDto>> GetByPatientIdAsync(int patientId, int pageNumber, int pageSize)
    {
        var (results, totalCount) = await repository.GetByPatientIdAsync(patientId, pageNumber, pageSize);
        return new PagedResult<LabResultDto>(results.Select(ToDto).ToList(), pageNumber, pageSize, totalCount);
    }

    public async Task<LabResultDto?> GetByIdAsync(int id)
    {
        var result = await repository.GetByIdAsync(id);
        return result is null ? null : ToDto(result);
    }

    public async Task<LabResultDto> CreateAsync(CreateLabResultDto dto, string orderedByUserId)
    {
        var result = await repository.AddAsync(new LabResult
        {
            PatientId = dto.PatientId,
            FacilityId = dto.FacilityId,
            TestName = dto.TestName,
            Unit = dto.Unit,
            ReferenceRange = dto.ReferenceRange,
            OrderedByUserId = orderedByUserId,
            OrderedAt = DateTime.UtcNow,
            Status = LabResultStatus.Ordered
        });
        return ToDto(result);
    }

    public async Task<LabResultDto?> CompleteAsync(int id, CompleteLabResultDto dto, string reviewerUserId)
    {
        var result = await repository.CompleteAsync(id, dto.ResultText, dto.ReferenceRange, reviewerUserId);
        if (result is not null)
        {
            await notifications.SendLabResultReadyAsync(result.PatientId, result.TestName);
            await audit.LogAsync(reviewerUserId, "LabResultCompleted", "LabResult", result.LabResultId.ToString(), result.TestName);
        }

        return result is null ? null : ToDto(result);
    }

    private static LabResultDto ToDto(LabResult l) =>
        new(l.LabResultId, l.PatientId, l.FacilityId, l.Facility.Name, l.TestName, l.ResultText,
            l.Unit, l.ReferenceRange, l.Status.ToString(), l.OrderedByUser.FullName,
            l.ReviewedByUser?.FullName, l.OrderedAt, l.CompletedAt);
}