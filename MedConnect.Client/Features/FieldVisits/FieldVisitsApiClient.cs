using MedConnect.Client.Services;

namespace MedConnect.Client.Features.FieldVisits;

public class FieldVisitsApiClient(HttpClient http)
{
    public Task<List<FieldVisitLogDto>> GetMineAsync() =>
        SafeApi.GetListAsync<FieldVisitLogDto>(http, "api/v1/field-visits/mine");

    public Task<ApiResult<FieldVisitLogDto>> CreateAsync(CreateFieldVisitLogDto dto) =>
        SafeApi.PostAsync<CreateFieldVisitLogDto, FieldVisitLogDto>(http, "api/v1/field-visits", dto);
}