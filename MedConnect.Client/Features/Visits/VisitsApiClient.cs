using MedConnect.Client.Services;

namespace MedConnect.Client.Features.Visits;

public class VisitsApiClient(HttpClient http)
{
    public Task<List<VisitDto>> GetByPatientAsync(int patientId) =>
        SafeApi.GetListAsync<VisitDto>(http, $"api/v1/visits/patient/{patientId}");

    public Task<ApiResult<VisitDto>> CreateAsync(CreateVisitDto dto) =>
        SafeApi.PostAsync<CreateVisitDto, VisitDto>(http, "api/v1/visits", dto);
}