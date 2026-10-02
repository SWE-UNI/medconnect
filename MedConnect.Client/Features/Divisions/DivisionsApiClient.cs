using MedConnect.Client.Services;

namespace MedConnect.Client.Features.Divisions;

public class DivisionsApiClient(HttpClient http)
{
    public Task<List<DivisionDto>> GetAllAsync() =>
        SafeApi.GetListAsync<DivisionDto>(http, "api/v1/divisions");

    public Task<ApiResult<DivisionDto>> CreateAsync(CreateDivisionDto dto) =>
        SafeApi.PostAsync<CreateDivisionDto, DivisionDto>(http, "api/v1/divisions", dto);
}