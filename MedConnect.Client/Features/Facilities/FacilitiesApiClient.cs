using MedConnect.Client.Services;

namespace MedConnect.Client.Features.Facilities;

public class FacilitiesApiClient(HttpClient http)
{
    public Task<List<FacilityDto>> GetAllAsync() =>
        SafeApi.GetListAsync<FacilityDto>(http, "api/v1/facilities");

    public Task<ApiResult<FacilityDto>> CreateAsync(CreateFacilityDto dto) =>
        SafeApi.PostAsync<CreateFacilityDto, FacilityDto>(http, "api/v1/facilities", dto);

    public Task<ApiResult<bool>> DeleteAsync(int id) =>
        SafeApi.DeleteStatusAsync(http, $"api/v1/facilities/{id}");
}