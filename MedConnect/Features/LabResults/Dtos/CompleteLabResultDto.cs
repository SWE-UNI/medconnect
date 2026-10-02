namespace MedConnect.Features.LabResults.Dtos;

public record CompleteLabResultDto(
    string ResultText,
    string? ReferenceRange = null);