using MedConnect.Client.Services;

namespace MedConnect.Client.Features.LabResults;

public class LabResultsApiClient(HttpClient http)
{
    public Task<PagedResult<LabResultDto>> GetByPatientAsync(int patientId, int pageNumber = 1, int pageSize = 100) =>
        SafeApi.GetPagedAsync<LabResultDto>(http, $"api/v1/labresults/patient/{patientId}?pageNumber={pageNumber}&pageSize={pageSize}");

    public Task<ApiResult<LabResultDto>> CreateAsync(CreateLabResultDto dto) =>
        SafeApi.PostAsync<CreateLabResultDto, LabResultDto>(http, "api/v1/labresults", dto);

    public Task<ApiResult<LabResultDto>> CompleteAsync(int id, CompleteLabResultDto dto) =>
        SafeApi.PostAsync<CompleteLabResultDto, LabResultDto>(http, $"api/v1/labresults/{id}/complete", dto);
}