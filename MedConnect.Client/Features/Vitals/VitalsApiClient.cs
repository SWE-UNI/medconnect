using MedConnect.Client.Services;

namespace MedConnect.Client.Features.Vitals;

public class VitalsApiClient(HttpClient http)
{
    public Task<List<VitalSignDto>> GetByPatientAsync(int patientId) =>
        SafeApi.GetListAsync<VitalSignDto>(http, $"api/v1/vitals/patient/{patientId}");

    public Task<ApiResult<VitalSignDto>> CreateAsync(CreateVitalSignDto dto) =>
        SafeApi.PostAsync<CreateVitalSignDto, VitalSignDto>(http, "api/v1/vitals", dto);
}