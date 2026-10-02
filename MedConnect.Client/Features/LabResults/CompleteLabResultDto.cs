namespace MedConnect.Client.Features.LabResults;

public record CompleteLabResultDto(
    string ResultText,
    string? ReferenceRange = null);