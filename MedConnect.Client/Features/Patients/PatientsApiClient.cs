using MedConnect.Client.Services;

namespace MedConnect.Client.Features.Patients;

public class PatientsApiClient(HttpClient http)
{
    public Task<PatientDto?> GetMineAsync() =>
        SafeApi.GetNullableAsync<PatientDto>(http, "api/v1/patients/me");

    public Task<PatientDto?> GetByIdAsync(int id) =>
        SafeApi.GetNullableAsync<PatientDto>(http, $"api/v1/patients/{id}");

    public Task<List<PatientDto>> GetAllAsync() =>
        SafeApi.GetListAsync<PatientDto>(http, "api/v1/patients");

    public Task<PatientConsentDto?> GetConsentAsync(int id) =>
        SafeApi.GetNullableAsync<PatientConsentDto>(http, $"api/v1/patients/{id}/consent");

    public Task<ApiResult<PatientConsentDto>> SetConsentAsync(int id, bool consent) =>
        SafeApi.PutAsync<object, PatientConsentDto>(http, $"api/v1/patients/{id}/consent", new { consent });

    public Task<ApiResult<PatientDto>> CreateAsync(CreatePatientDto dto) =>
        SafeApi.PostAsync<CreatePatientDto, PatientDto>(http, "api/v1/patients", dto);

    public Task<ApiResult<PatientDto>> UpdateAsync(int id, CreatePatientDto dto) =>
        SafeApi.PutAsync<object, PatientDto>(http, $"api/v1/patients/{id}", dto);
}